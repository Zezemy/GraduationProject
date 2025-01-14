using Binance.Net.Interfaces.Clients;
using Signalizer.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using Signalizer.Entities.Interfaces;
using Signalizer.Entities.Strategies.Options;
using Signalizer.Entities.Enums;
using Signalizer.Context;
using Microsoft.EntityFrameworkCore;

namespace Signalizer.BackgroundServices
{
    internal sealed class MaCrossoverSignaller(
        IServiceScopeFactory serviceScopeFactory,
        IOptions<MaCrossoverWorkerOptions> options,
        ILogger<MaCrossoverSignaller> logger,
        IBinanceRestClient restClient)
        : BackgroundService
    {
        private readonly Random _random = new();
        private readonly MaCrossoverWorkerOptions _options = options.Value;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await GenerateSignal();

                await Task.Delay(_options.WorkInterval, stoppingToken);
            }
        }

        private async Task GenerateSignal()
        {
            using var scope = serviceScopeFactory.CreateScope();
            using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var strategies = context.SignalStrategies.Where(x => x.StrategyType == (int)StrategyTypes.MaCrossover).Include(b => b.TradingPair).ToList();

            foreach (var strategy in strategies)
            {
                try
                {
                    var props = Newtonsoft.Json.JsonConvert.DeserializeObject<MaCrossoverStrategyOptions>(strategy.Properties);
                    var symbol = strategy.TradingPair.Base + strategy.TradingPair.Quote;
                    var kLineInterval = (Binance.Net.Enums.KlineInterval)Enum.Parse(typeof(Binance.Net.Enums.KlineInterval), props.KLineInterval.ToString());
                    var kLines = await restClient.SpotApi.ExchangeData.GetKlinesAsync(symbol, kLineInterval, limit: props.LongPeriod);
                    var closePricesLongList = kLines.Data.TakeLast(props.LongPeriod).Select(x => x.ClosePrice);
                    var latestCloseTime = kLines.Data.TakeLast(1).Select(x => x.CloseTime.ToLocalTime()).FirstOrDefault();
                    //DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(latestCloseTime);
                    //DateTime latestUtcCloseTime = dateTimeOffset.UtcDateTime;

                    Models.TradingSignal dbSignal = new Models.TradingSignal();
                    dbSignal.SignalType = (int)MovingAverageCrossover(closePricesLongList, props.ShortPeriod, props.LongPeriod);
                    dbSignal.Symbol = symbol;
                    dbSignal.DateTime = latestCloseTime;
                    dbSignal.StrategyId = strategy.Id;
                    dbSignal.StrategyType = (int)StrategyTypes.MaCrossover;
                    dbSignal.Interval = strategy.Interval;
                    context.TradingSignals.Add(dbSignal);
                    await context.SaveChangesAsync();


                    var userSignalStrategy = context.UserSignalStrategies.FirstOrDefault(x => x.StrategyId == strategy.Id);
                    if (userSignalStrategy != null)
                    {
                        Models.UserTradingSignal dbSignal2 = new Models.UserTradingSignal();
                        dbSignal2.UserId = userSignalStrategy.UserId;
                        dbSignal2.TradingSignalId = dbSignal.Id;
                        context.UserTradingSignals.Add(dbSignal2);
                        await context.SaveChangesAsync();
                    }

                    logger.LogInformation($"Saved {symbol} signal to {dbSignal}");
                }
                catch (Exception e)
                {
                    logger.LogError($"Error : {e}");
                }
            }
        }

        // 1. Simple Moving Average Crossover
        public static SignalTypes MovingAverageCrossover(IEnumerable<decimal> prices, int shortPeriod, int longPeriod)
        {
            if (prices.Count() < longPeriod) return SignalTypes.Hold;

            var shortMA = prices.TakeLast(shortPeriod).Average();
            var longMA = prices.TakeLast(longPeriod).Average();

            if (shortMA > longMA) return SignalTypes.Buy;
            if (shortMA < longMA) return SignalTypes.Sell;
            return SignalTypes.Hold;
        }
    }
}