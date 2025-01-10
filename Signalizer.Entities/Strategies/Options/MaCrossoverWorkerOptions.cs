using Signalizer.Entities.Enums;

namespace Signalizer.Entities.Strategies.Options
{
    public class MaCrossoverWorkerOptions
    {
        public TimeSpan WorkInterval { get; set; } = TimeSpan.FromMinutes(1);
        public MaCrossoverStrategyOptions StrategyOptions { get; set; }
    }

    public class MaCrossoverStrategyOptions : StrategyOptions 
    {
        public int ShortPeriod { get; set; }
        public int LongPeriod { get; set; }
        public KLineIntervals KLineInterval { get; set; }
    }
}
