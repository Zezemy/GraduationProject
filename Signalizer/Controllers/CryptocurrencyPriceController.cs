﻿using Microsoft.AspNetCore.Mvc;
using Binance.Net.Interfaces.Clients;
using Signalizer.Entities.Dtos;

namespace Signalizer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class CryptocurrencyPriceController : ControllerBase
    {
        private IBinanceRestClient _restClient { get; }
        public CryptocurrencyPriceController(IBinanceRestClient restClient)
        {
            _restClient = restClient;
        }


        [HttpGet(Name = "GetStockPrice")]
        public async Task<object> GetAsync(string? ticker)
        {
            try
            {
                ticker = ticker ?? "BTCUSDT";
                var price = await _restClient.SpotApi.ExchangeData.GetTradingDayTickerAsync(ticker);
                var jsonStr = Newtonsoft.Json.JsonConvert.SerializeObject(price.Data);
                return Newtonsoft.Json.JsonConvert.DeserializeObject<TradingDayTicker>(jsonStr);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}