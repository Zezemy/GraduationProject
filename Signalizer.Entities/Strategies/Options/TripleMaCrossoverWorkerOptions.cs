using Signalizer.Entities.Enums;

namespace Signalizer.Entities.Strategies.Options
{
    public class TripleMaCrossoverWorkerOptions
    {
        public TimeSpan WorkInterval { get; set; } = TimeSpan.FromMinutes(1);
        public TripleMaCrossoverStrategyOptions StrategyOptions { get; set; }
    }
    public class TripleMaCrossoverStrategyOptions : StrategyOptions
    {
        public int ShortPeriod { get; set; }
        public int MediumPeriod { get; set; }
        public int LongPeriod { get; set; }
        public KLineIntervals KLineInterval { get; set; }
    }
}
