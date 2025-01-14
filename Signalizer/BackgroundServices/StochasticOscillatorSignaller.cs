using Binance.Net.Interfaces.Clients;
using Signalizer.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using Signalizer.Entities.Interfaces;
using Signalizer.Entities.Strategies.Options;
using Signalizer.Extensions;
using Signalizer.Entities.Enums;
using Signalizer.Context;
using Microsoft.EntityFrameworkCore;

namespace Signalizer.BackgroundServices
{
    internal sealed class StochasticOscillatorSignaller(
        IServiceScopeFactory serviceScopeFactory,
        IOptions<StochasticOscillatorWorkerOptions> options,
        ILogger<StochasticOscillatorSignaller> logger,
        IBinanceRestClient restClient)
        : BackgroundService
    {
        private readonly Random _random = new();
        private readonly StochasticOscillatorWorkerOptions _options = options.Value;

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
            var strategies = context.SignalStrategies.Where(x => x.StrategyType == (int)StrategyTypes.StochasticOscillator).Include(b => b.TradingPair).ToList();

            foreach (var strategy in strategies)
            {
                try
                {
                    var props = Newtonsoft.Json.JsonConvert.DeserializeObject<StochasticOscillatorStrategyOptions>(strategy.Properties);
                    var symbol = strategy.TradingPair.Base + strategy.TradingPair.Quote;
                    var kLineInterval = (Binance.Net.Enums.KlineInterval)Enum.Parse(typeof(Binance.Net.Enums.KlineInterval), props.KLineInterval.ToString());
                    var kLines = await restClient.SpotApi.ExchangeData.GetKlinesAsync(symbol, kLineInterval, limit: props.Period);
                    var closePricesLongList = kLines.Data.TakeLast(props.Period).Select(x => x.ConvertToKLine());
                    var latestCloseTime = kLines.Data.TakeLast(1).Select(x => x.CloseTime.ToLocalTime()).FirstOrDefault();
                    //DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(latestCloseTime);
                    //DateTime latestUtcCloseTime = dateTimeOffset.UtcDateTime;

                    Models.TradingSignal dbSignal = new Models.TradingSignal();
                    dbSignal.SignalType = (int)StochasticSignal(closePricesLongList, props.Period, props.Overbought, props.Oversold);
                    dbSignal.Symbol = symbol;
                    dbSignal.DateTime = latestCloseTime;
                    dbSignal.StrategyId = strategy.Id;
                    dbSignal.StrategyType = (int)StrategyTypes.StochasticOscillator;
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

        // 5. Stochastic Oscillator
        public static SignalTypes StochasticSignal(IEnumerable<IKLine> prices, int period = 14, decimal overbought = 80, decimal oversold = 20)
        {
            if (prices.Count() < period) return SignalTypes.Hold;

            var recentPrices = prices.TakeLast(period).ToList();
            var highestHigh = recentPrices.Max(p => p.HighPrice);
            var lowestLow = recentPrices.Min(p => p.LowPrice);

            if (highestHigh - lowestLow == 0) return SignalTypes.Hold;

            var k = ((prices.Last().ClosePrice - lowestLow) / (highestHigh - lowestLow)) * 100;

            if (k < oversold) return SignalTypes.Buy;
            if (k > overbought) return SignalTypes.Sell;
            return SignalTypes.Hold;
        }
    }
}