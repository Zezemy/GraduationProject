using Signalizer.Entities.Enums;

namespace Signalizer.Entities.Strategies.Options
{
    public class ExponentialMaCrossoverWithVolumeWorkerOptions
    {
        public TimeSpan WorkInterval { get; set; } = TimeSpan.FromMinutes(1);
        public ExponentialMaCrossoverWithVolumeStrategyOptions StrategyOptions { get; set; }
    }
    public class ExponentialMaCrossoverWithVolumeStrategyOptions : StrategyOptions
    {
        public int ShortPeriod { get; set; }
        public int LongPeriod { get; set; }
        public KLineIntervals KLineInterval { get; set; }
    }
}
