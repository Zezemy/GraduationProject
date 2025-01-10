using Binance.Net.Interfaces.Clients;
using Signalizer.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using Signalizer.Entities.Dtos;
using Signalizer.Entities.Interfaces;
using Signalizer.Managers;
using Signalizer.Entities.BackgroundServices;
using Signalizer.Context;
using Microsoft.EntityFrameworkCore;

namespace Signalizer.BackgroundServices
{
    internal sealed class StocksFeedUpdater(
        ActiveTickerManager activeTickerManager,
        IServiceScopeFactory serviceScopeFactory,
        IHubContext<StocksFeedHub, IPriceUpdateClientContract> hubContext,
        IOptions<UpdateOptions> options,
        ILogger<StocksFeedUpdater> logger,
        IBinanceRestClient restClient)
        : BackgroundService
    {
        private readonly Random _random = new();
        private readonly UpdateOptions _options = options.Value;


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var scope = serviceScopeFactory.CreateScope();
            using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var tradingPairs = context.SignalStrategies.Where(x => x.IsPredefined).Include(x => x.TradingPair).Select(x => x.TradingPair).ToList();

            while (!stoppingToken.IsCancellationRequested)
            {
                await UpdateStockPrices(tradingPairs);

                await SendTradingPairVolume(serviceScopeFactory, hubContext, logger, restClient);

                await Task.Delay(_options.UpdateInterval, stoppingToken);
            }
        }

        private async Task UpdateStockPrices(List<Models.TradingPair> tradingPairs)
        {
            try
            {
                var sendedTickerList = new List<string>();
                foreach (var pair in tradingPairs)
                {
                    var ticker = $"{pair.Base}{pair.Quote}";
                    sendedTickerList.Add(ticker);
                    await SendTradingPairPrice(serviceScopeFactory, hubContext, logger, restClient, tradingPairs, ticker);
                }

                var remainingTickerList = activeTickerManager.GetAllTickers().ToList().Except(sendedTickerList);
                foreach (var ticker in remainingTickerList)
                {
                    await SendTradingPairPrice(serviceScopeFactory, hubContext, logger, restClient, tradingPairs, ticker);
                }
            }
            catch (Exception e)
            {
                logger.LogError($"Error: {e}");
            }
        }

        private static async Task SendTradingPairPrice(IServiceScopeFactory serviceScopeFactory, IHubContext<StocksFeedHub, IPriceUpdateClientContract> hubContext, ILogger<StocksFeedUpdater> logger, IBinanceRestClient restClient, List<Models.TradingPair> tradingPairs, string ticker)
        {
            try
            {
                using var scope = serviceScopeFactory.CreateScope();
                using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                var tradingPairDb = context.TradingPairs.Where(x => x.Base + x.Quote == ticker).FirstOrDefault();
                var tradingPair = tradingPairDb == null ? new TradingPair { Base = ticker } : new TradingPair { Base = tradingPairDb.Base, Quote = tradingPairDb.Quote };

                var priceData = await restClient.SpotApi.ExchangeData.GetTradingDayTickerAsync(ticker);
                if (priceData != null)
                {
                    var update = new TradingDayTicker()
                    {
                        LastPrice = priceData.Data.LastPrice,
                        Symbol = ticker,
                        PriceChange = priceData.Data.PriceChange,
                        PriceChangePercent = priceData.Data.PriceChangePercent,
                        WeightedAveragePrice = priceData.Data.WeightedAveragePrice,
                        OpenPrice = priceData.Data.OpenPrice,
                        HighPrice = priceData.Data.HighPrice,
                        LowPrice = priceData.Data.LowPrice,
                        Volume = priceData.Data.Volume,
                        QuoteVolume = priceData.Data.QuoteVolume,
                        OpenTime = priceData.Data.OpenTime,
                        CloseTime = priceData.Data.CloseTime,
                        FirstTradeId = priceData.Data.FirstTradeId,
                        TotalTrades = priceData.Data.TotalTrades,
                        TradingPair = tradingPair
                    };

                    //await hubContext.Clients.All.ReceiveStockPriceUpdate(update);

                    await hubContext.Clients.Group(ticker).ReceiveStockPriceUpdate(update);

                    logger.LogInformation($"Updated {ticker} price to {priceData?.Data?.LastPrice}");
                }
            }
            catch (Exception e)
            {
                logger.LogError($"Error: {e}");
            }
        }

        private static async Task SendTradingPairVolume(IServiceScopeFactory serviceScopeFactory, IHubContext<StocksFeedHub, IPriceUpdateClientContract> hubContext, ILogger<StocksFeedUpdater> logger, IBinanceRestClient restClient)
        {
            try
            {
                using var scope = serviceScopeFactory.CreateScope();
                using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                var usdBases = new List<string>()
                {
                    "USDC", "FDUSD"
                };
                var symbols = context.TradingPairs.Where(x => x.Quote == "USDT" && !usdBases.Contains(x.Base)).ToDictionary(x => x.Base + x.Quote);
                var result = await restClient.SpotApi.ExchangeData.GetTickersAsync();
                var volumeRankings = result.Data.Where(x => symbols.Keys.Contains(x.Symbol)).OrderByDescending(x => x.QuoteVolume).Take(10);
                var gainers = result.Data.Where(x => symbols.Keys.Contains(x.Symbol)).OrderByDescending(x => x.PriceChangePercent).Take(10);
                var losers = result.Data.Where(x => symbols.Keys.Contains(x.Symbol)).OrderBy(x => x.PriceChangePercent).Take(10);

                await hubContext.Clients.All.ReceiveStockVolumeUpdate(volumeRankings);
                await hubContext.Clients.All.ReceiveStockGainersUpdate(gainers);
                await hubContext.Clients.All.ReceiveStockLosersUpdate(losers);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}