using Microsoft.AspNetCore.Mvc;
using Signalizer.Entities.Dtos;
using Signalizer.Context;
using Signalizer.Entities.Enums;
using Signalizer.Entities;
using Microsoft.EntityFrameworkCore;
using Signalizer.Extensions;
using Binance.Net.Interfaces.Clients;
using Microsoft.AspNetCore.Authorization;

namespace Signalizer.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]

    public class QueryServiceController : ControllerBase
    {
        private ApplicationDbContext _context { get; }

        private ILogger<QueryServiceController> _logger;
        private IHttpContextAccessor _accessor;
        IBinanceRestClient _restClient;

        public QueryServiceController(ApplicationDbContext context, ILogger<QueryServiceController> logger, IHttpContextAccessor accessor, IBinanceRestClient restClient)
        {
            _context = context;
            _logger = logger;
            _accessor = accessor;
            _restClient = restClient;
        }

        [HttpGet(Name = "GetVolumeRankings")]
        public async Task<object> GetVolumeRankingsAsync()
        {
            try
            {
                var usdBases = new List<string>()
                {
                    "USDC", "FDUSD"
                };
                var symbols = _context.TradingPairs.Where(x => x.Quote == "USDT" && !usdBases.Contains(x.Base)).ToDictionary(x => x.Base + x.Quote);
                var result = await _restClient.SpotApi.ExchangeData.GetTickersAsync();
                var volumeRankings = result.Data.Where(x => symbols.Keys.Contains(x.Symbol)).OrderByDescending(x => x.QuoteVolume).Take(10);

                return volumeRankings;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet(Name = "GetGainersRankings")]
        public async Task<object> GetGainersRankingsAsync()
        {
            try
            {
                var usdBases = new List<string>()
                {
                    "USDC", "FDUSD"
                };
                var symbols = _context.TradingPairs.Where(x => x.Quote == "USDT" && !usdBases.Contains(x.Base)).ToDictionary(x => x.Base + x.Quote);
                var result = await _restClient.SpotApi.ExchangeData.GetTickersAsync();
                var gainers = result.Data.Where(x => symbols.Keys.Contains(x.Symbol)).OrderByDescending(x => x.PriceChangePercent).Take(10);

                return gainers;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet(Name = "GetLosersRankings")]
        public async Task<object> GetLosersRankingsAsync()
        {
            try
            {
                var usdBases = new List<string>()
                {
                    "USDC", "FDUSD"
                };
                var symbols = _context.TradingPairs.Where(x => x.Quote == "USDT" && !usdBases.Contains(x.Base)).ToDictionary(x => x.Base + x.Quote);
                var result = await _restClient.SpotApi.ExchangeData.GetTickersAsync();
                var losers = result.Data.Where(x => symbols.Keys.Contains(x.Symbol)).OrderBy(x => x.PriceChangePercent).Take(10);

                return losers;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet(Name = "GetSymbols")]
        public async Task<object> GetSymbolsAsync()
        {
            try
            {
                var result = await _restClient.SpotApi.ExchangeData.GetExchangeInfoAsync(returnPermissionSets: false);
                var symbols = result.Data.Symbols;

                //StringBuilder sb = new StringBuilder();
                //foreach (var symbol in symbols.Where(x => x.Status == SymbolStatus.Trading && x.QuoteAsset == "USDT"))
                //{
                //    sb.AppendLine($"new TradingPair()        {{Base = \"{symbol.BaseAsset}\",Quote = \"{symbol.QuoteAsset}\"}},");
                //}

                //var generated = sb.ToString();

                return symbols;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet(Name = "GetUserById")]
        public async Task<object> GetUserByIdAsync(string id)
        {
            try
            {
                return _context.Users.Where(x => x.Id == id).FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet(Name = "GetUsers")]
        public async Task<object> GetUsersAsync()
        {
            try
            {
                return _context.Users;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet(Name = "GetTradingPairs")]
        public async Task<object> GetTradingPairsAsync()
        {
            try
            {
                return _context.TradingPairs.Select(x => x.ConvertToTradingPair());
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Authorize]
        [HttpPost(Name = "SignalsList")]
        public async Task<object> SignalsListAsync()
        {
            try
            {
                return _context.TradingSignals;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost(Name = "ListSignals")]
        public async Task<ListSignalResponseMessage> ListSignals([FromBody] ListSignalRequest msg)
        {
            var ret = new ListSignalResponseMessage();
            try
            {
                var listSignals = _context.TradingSignals.Where(
                    x => x.DateTime <= msg.QueryEndDateTime
                      && x.DateTime >= msg.QueryStartDateTime
                      ).OrderByDescending(x => x.Id).ToList();

                var signals = new List<TradingSignal>();
                if (msg.StrategyType >= 0)
                    listSignals = listSignals.Where(x => x.StrategyType == msg.StrategyType).ToList();
                if (msg.SignalType >= 0)
                    listSignals = listSignals.Where(x => x.SignalType == msg.SignalType).ToList();
                if (msg.Interval >= 0)
                    listSignals = listSignals.Where(x => x.Interval == msg.Interval).ToList();
                if (!string.IsNullOrWhiteSpace(msg.Symbol))
                    listSignals = listSignals.Where(x => x.Symbol == msg.Symbol).ToList();

                foreach (var signal in listSignals)
                {
                    ret.TradingSignals.Add(new TradingSignal()
                    {
                        DateTime = signal.DateTime,
                        Interval = (KLineIntervals)Enum.Parse(typeof(KLineIntervals), signal.Interval.ToString()),
                        SignalType = (SignalTypes)Enum.Parse(typeof(SignalTypes), signal.SignalType.ToString()),
                        Symbol = signal.Symbol,
                        StrategyType = (StrategyTypes)Enum.Parse(typeof(StrategyTypes), signal.StrategyType.ToString()),
                    });
                }
                ret.ResponseCode = "0";
                ret.ResponseDescription = "Transaction is successful.";
                return ret;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return new ListSignalResponseMessage
                {
                    ResponseCode = "1",
                    ResponseDescription = "Transaction is failed."
                };
            }
        }

        [HttpPost(Name = "ListStrategies")]
        public async Task<ListStrategyResponseMessage> ListStrategies([FromBody] ListStrategyRequest msg)
        {
            var ret = new ListStrategyResponseMessage();
            try
            {
                var userId = _accessor.HttpContext.User.Claims.ToList()[0].Value;
                var listStrategies = _context.SignalStrategies.Include(x => x.TradingPair).ToList();

                if (msg.StrategyType >= 0)
                    listStrategies = listStrategies.Where(x => x.StrategyType == msg.StrategyType).ToList();
                if (msg.Interval >= 0)
                    listStrategies = listStrategies.Where(x => x.Interval == msg.Interval).ToList();
                if (!string.IsNullOrWhiteSpace(msg.Symbol))
                {
                    listStrategies = listStrategies.Where(x => x.TradingPair.Base + x.TradingPair.Quote == msg.Symbol).ToList();
                }
                if (!msg.IncludePredefined)
                {
                    listStrategies = listStrategies.Where(x => x.IsPredefined == msg.IncludePredefined && x.CreatedBy == userId).ToList();
                }

                foreach (var strategy in listStrategies)
                {
                    ret.SignalStrategies.Add(new SignalStrategy()
                    {
                        Id = strategy.Id,
                        CreateDate = strategy.CreateDate,
                        CreatedBy = strategy.CreatedBy,
                        UpdateDate = strategy.UpdateDate == null ? null : strategy.UpdateDate.Value,
                        Interval = strategy.Interval,
                        TradingPair = strategy.TradingPair.ConvertToTradingPair(),
                        StrategyType = strategy.StrategyType,
                        Properties = strategy.Properties,
                        IsPredefined = strategy.IsPredefined
                    });
                }
                ret.ResponseCode = "0";
                ret.ResponseDescription = "Transaction is successful.";
                return ret;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return new ListStrategyResponseMessage
                {
                    ResponseCode = "1",
                    ResponseDescription = "Transaction is failed."
                };
            }
        }

        [HttpGet(Name = "GetUserStrategiesWithPredefined")]
        public async Task<ListStrategyResponseMessage> GetUserStrategiesWithPredefined()
        {
            var ret = new ListStrategyResponseMessage();
            try
            {
                var userId = _accessor.HttpContext.User.Claims.ToList()[0].Value;
                var listStrategy = _context.SignalStrategies.Include(x => x.TradingPair).Where(x => x.IsPredefined == true && x.CreatedBy != userId).ToList();
                var listStrategy2 = _context.SignalStrategies.Include(x => x.TradingPair).Where(x => x.CreatedBy == userId && x.IsPredefined == false).ToList();
                var strategies = listStrategy.Union(listStrategy2);

                foreach (var strategy in strategies)
                {
                    ret.SignalStrategies.Add(new SignalStrategy()
                    {
                        Id = strategy.Id,
                        CreateDate = strategy.CreateDate,
                        CreatedBy = strategy.CreatedBy,
                        UpdateDate = strategy.UpdateDate == null ? null : strategy.UpdateDate.Value,
                        Interval = strategy.Interval,
                        TradingPair = strategy.TradingPair.ConvertToTradingPair(),
                        StrategyType = strategy.StrategyType,
                        Properties = strategy.Properties,
                        IsPredefined = strategy.IsPredefined
                    });
                }
                ret.ResponseCode = "0";
                ret.ResponseDescription = "Transaction is successful.";
                return ret;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return new ListStrategyResponseMessage
                {
                    ResponseCode = "1",
                    ResponseDescription = "Transaction is failed."
                };
            }
        }

        [HttpGet(Name = "GetTickersForReceivingSignals")]
        public async Task<object> GetTickersForReceivingSignalsAsync()
        {
            try
            {
                var userId = _accessor.HttpContext.User.Claims.ToList()[0].Value;
                return _context.UserSignalStrategies
                    .Include(x => x.Strategy)
                    .Where(x => x.UserId == userId)
                    .Include(x => x.Strategy.TradingPair)
                    .Select(x => x.Strategy.TradingPair.Base + x.Strategy.TradingPair.Quote);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return new BaseResponse
                {
                    ResponseCode = "1",
                    ResponseDescription = "Transaction is failed."
                };
            }
        }

        [HttpGet(Name = "GetLastSignalsForUser")]
        public async Task<object> GetLastSignalsForUserAsync()
        {
            try
            {
                var userId = _accessor.HttpContext.User.Claims.ToList()[0].Value;

                var predefinedStrategyRecords = _context.TradingSignals
                    .Include(x => x.Strategy)
                    .Where(x => x.DateTime >= DateTime.Today && x.Strategy.IsPredefined)
                    .GroupBy(x => x.Symbol + x.StrategyType + x.Interval)
                    .Select(g => g.OrderByDescending(l => l.Id).First().ConvertToTradingSignal()).ToList();

                //var y = _context.TradingSignals
                //    .Include(x => x.UserTradingSignals.Where(y => y.UserId == userId))
                //    .Where(x => x.DateTime >= DateTime.Today && x.UserTradingSignals.Any(y => y.UserId == userId))
                //    .GroupBy(x => x.Symbol + x.StrategyType + x.Interval)
                //    .Select(g => g.OrderByDescending(l => l.Id).First().ConvertToTradingSignal()).ToList();

                var latestSignals = _context.UserTradingSignals
                    .Include(x => x.TradingSignal)
                    .Where(x => x.TradingSignal.DateTime >= DateTime.Today && x.UserId == userId)
                    .Select(x => x.TradingSignal)
                    .GroupBy(x => x.Symbol + x.StrategyType + x.Interval)
                    .Select(g => g.OrderByDescending(l => l.Id).First().ConvertToTradingSignal()).ToList();

                var ret = latestSignals.Union(predefinedStrategyRecords).OrderByDescending(x => x.Id);
                return ret;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return new BaseResponse
                {
                    ResponseCode = "1",
                    ResponseDescription = "Transaction is failed."
                };
            }
        }

        [HttpGet(Name = "GetPredefinedStrategiesSymbols")]
        public async Task<object> GetPredefinedStrategiesSymbols()
        {
            try
            {
                var predefinedSymbols = _context.SignalStrategies.Include(x => x.TradingPair).Where(x => x.IsPredefined == true).Select(x => x.TradingPair.Base + x.TradingPair.Quote).ToList();
                return predefinedSymbols;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return new BaseResponse
                {
                    ResponseCode = "1",
                    ResponseDescription = "Transaction is failed."
                };
            }
        }
    }
}
