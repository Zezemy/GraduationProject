using TradingPairDb = Signalizer.Models.TradingPair;
using TradingPair = Signalizer.Entities.Dtos.TradingPair;

using TradingSignalDb = Signalizer.Models.TradingSignal;
using TradingSignal = Signalizer.Entities.Dtos.TradingSignal;
using Signalizer.Entities.Enums;

namespace Signalizer.Extensions
{
    public static class Extensions
    {
        public static TradingPair ConvertToTradingPair(this TradingPairDb tradingPair)
        {
            return new TradingPair()
            {
                Id = tradingPair.Id,
                Base = tradingPair.Base,
                Quote = tradingPair.Quote
            };
        }

        public static TradingSignal ConvertToTradingSignal(this TradingSignalDb tradingSignalInDb)
        {
            return new TradingSignal()
            {
                Id = tradingSignalInDb.Id,
                DateTime = tradingSignalInDb.DateTime,
                Interval = (KLineIntervals)Enum.Parse(typeof(KLineIntervals),tradingSignalInDb.Interval.ToString()),
                SignalType = (SignalTypes)Enum.Parse(typeof(SignalTypes), tradingSignalInDb.SignalType.ToString()),
                StrategyType = (StrategyTypes)Enum.Parse(typeof(StrategyTypes), tradingSignalInDb.StrategyType.ToString()),
                Symbol = tradingSignalInDb.Symbol
            };
        }
    }
}
