using Signalizer.Entities.Enums;

namespace Signalizer.Entities.Strategies.Options
{
    public class MacdWorkerOptions
    {
        public TimeSpan WorkInterval { get; set; } = TimeSpan.FromMinutes(1);
        public MacdStrategyOptions StrategyOptions { get; set; }
    }
    public class MacdStrategyOptions : StrategyOptions
    {
        public int LongPeriod { get; set; }
        public int ShortPeriod { get; set; }
        public int Period { get; set; }
        public KLineIntervals KLineInterval { get; set; }
    }
}
