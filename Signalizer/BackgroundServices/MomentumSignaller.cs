using Binance.Net.Interfaces.Clients;
using Signalizer.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using Signalizer.Entities.Interfaces;
using Signalizer.Managers;
using Signalizer.Entities.Strategies.Options;
using Signalizer.Entities.Enums;
using Signalizer.Context;
using Microsoft.EntityFrameworkCore;

namespace Signalizer.BackgroundServices
{
    internal sealed class MomentumSignaller(
        IServiceScopeFactory serviceScopeFactory,
        IOptions<MomentumWorkerOptions> options,
        ILogger<MomentumSignaller> logger,
        IBinanceRestClient restClient)
        : BackgroundService
    {
        private readonly Random _random = new();
        private readonly MomentumWorkerOptions _options = options.Value;

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
            var strategies = context.SignalStrategies.Where(x => x.StrategyType == (int)StrategyTypes.Momentum).Include(b => b.TradingPair).ToList();

            foreach (var strategy in strategies)
            {
                try
                {
                    var props = Newtonsoft.Json.JsonConvert.DeserializeObject<MomentumStrategyOptions>(strategy.Properties);
                    var ticker = strategy.TradingPair.Base+strategy.TradingPair.Quote;
                    var kLineInterval = (Binance.Net.Enums.KlineInterval)Enum.Parse(typeof(Binance.Net.Enums.KlineInterval), props.KLineInterval.ToString());
                    var kLines = await restClient.SpotApi.ExchangeData.GetKlinesAsync(ticker, kLineInterval, limit: props.Period+1);
                    var closePricesLongList = kLines.Data.TakeLast(props.Period+1).Select(x => x.ClosePrice);
                    var latestCloseTime = kLines.Data.TakeLast(1).Select(x => x.CloseTime.ToLocalTime()).FirstOrDefault();
                    //DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(latestCloseTime);
                    //DateTime latestUtcCloseTime = dateTimeOffset.UtcDateTime;

                    Models.TradingSignal dbSignal = new Models.TradingSignal();
                    dbSignal.SignalType = (int)MomentumSignal(closePricesLongList.ToList(), props.Period);
                    dbSignal.Symbol = ticker;
                    dbSignal.DateTime = latestCloseTime;
                    dbSignal.StrategyId = strategy.Id;
                    dbSignal.StrategyType = (int)StrategyTypes.Momentum;
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

                    logger.LogInformation($"Saved {ticker} signal to {dbSignal}");
                }
                catch (Exception e)
                {
                    logger.LogError($"Error : {e}");
                }
            }
        }

        // 9. Momentum Strategy
        public static SignalTypes MomentumSignal(List<decimal> prices, int period)
        {
            if (prices.Count() < period) return SignalTypes.Hold;

            var momentum = prices.Last() - prices[prices.Count() - period];
            var previousMomentum = prices[prices.Count() - 2] - prices[prices.Count() - period - 1];

            if (momentum > previousMomentum && momentum > 0) return SignalTypes.Buy;
            if (momentum < previousMomentum && momentum < 0) return SignalTypes.Sell;
            return SignalTypes.Hold;
        }
    }
}