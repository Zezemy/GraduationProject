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
    internal sealed class ExponentialMaCrossoverWithVolumeSignaller(
        IServiceScopeFactory serviceScopeFactory,
		IOptions<ExponentialMaCrossoverWithVolumeWorkerOptions> options,
		ILogger<ExponentialMaCrossoverWithVolumeSignaller> logger,
		IBinanceRestClient restClient)
		: BackgroundService
	{
		private readonly Random _random = new();
		private readonly ExponentialMaCrossoverWithVolumeWorkerOptions _options = options.Value;

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			while (!stoppingToken.IsCancellationRequested)
			{
				await SendSignal();

				await Task.Delay(_options.WorkInterval, stoppingToken);
			}
		}

		private async Task SendSignal()
		{
            using var scope = serviceScopeFactory.CreateScope();
            using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var strategies = context.SignalStrategies.Where(x => x.StrategyType == (int)StrategyTypes.ExponentialMaCrossoverWithVolume).Include(b => b.TradingPair).ToList();

            foreach (var strategy in strategies)
			{
                try
                {
                    var props = Newtonsoft.Json.JsonConvert.DeserializeObject<ExponentialMaCrossoverWithVolumeStrategyOptions>(strategy.Properties);
                    var ticker = strategy.TradingPair.Base + strategy.TradingPair.Quote;
                    var kLineInterval = (Binance.Net.Enums.KlineInterval)Enum.Parse(typeof(Binance.Net.Enums.KlineInterval), props.KLineInterval.ToString());
                    var kLines = await restClient.SpotApi.ExchangeData.GetKlinesAsync(ticker, kLineInterval, limit: props.LongPeriod);
                    var closePricesLongList = kLines.Data.TakeLast(props.LongPeriod).Select(x => x.ConvertToKLine());
                    var latestCloseTime = kLines.Data.TakeLast(1).Select(x => x.CloseTime.ToLocalTime()).FirstOrDefault();
                    //DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(latestCloseTime);
                    //DateTime latestUtcCloseTime = dateTimeOffset.UtcDateTime;

                    Models.TradingSignal dbSignal = new Models.TradingSignal();
                    dbSignal.SignalType = (int)EMAVolumeSignal(closePricesLongList, props.ShortPeriod, props.LongPeriod);
                    dbSignal.Symbol = ticker;
                    dbSignal.DateTime = latestCloseTime;
                    dbSignal.StrategyId = strategy.Id;
                    dbSignal.StrategyType = (int)StrategyTypes.ExponentialMaCrossoverWithVolume;
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

        // 10. Exponential Moving Average Crossover with Volume
        public static SignalTypes EMAVolumeSignal(IEnumerable<IKLine> prices, int shortPeriod, int longPeriod)
        {
            if (prices.Count() < longPeriod) return SignalTypes.Hold;

            var closePrices = prices.Select(p => p.ClosePrice).ToList();
            var volumes = prices.Select(p => p.Volume).ToList();

            var shortEMA = CalculateEMA(closePrices, shortPeriod);
            var longEMA = CalculateEMA(closePrices, longPeriod);
            var averageVolume = volumes.TakeLast(shortPeriod).Average();
            var currentVolume = volumes.Last();

            if (shortEMA > longEMA && currentVolume > averageVolume) return SignalTypes.Buy;
            if (shortEMA < longEMA && currentVolume > averageVolume) return SignalTypes.Sell;
            return SignalTypes.Hold;
        }

        private static decimal CalculateEMA(List<decimal> prices, int period)
        {
            var multiplier = 2.0m / (period + 1);
            var ema = prices.Take(period).Average();

            foreach (var price in prices.Skip(period))
            {
                ema = (price - ema) * multiplier + ema;
            }

            return ema;
        }
    }
}