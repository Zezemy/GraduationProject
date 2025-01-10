using Signalizer.Entities.Enums;

namespace Signalizer.Entities.Dtos
{
    public class TradingSignal
    {
        public long Id { get; set; }
        public string Symbol { get; set; }
        public SignalTypes SignalType { get; set; }
        public DateTime DateTime { get; set; }
        public StrategyTypes StrategyType { get; set; }
        public KLineIntervals Interval { get; set; }
    }
}