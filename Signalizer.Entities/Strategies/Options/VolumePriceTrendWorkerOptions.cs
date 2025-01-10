using Signalizer.Entities.Enums;

namespace Signalizer.Entities.Strategies.Options
{
    public class VolumePriceTrendWorkerOptions
    {
        public TimeSpan WorkInterval { get; set; } = TimeSpan.FromMinutes(1);
        public VolumePriceTrendStrategyOptions StrategyOptions { get; set; }
    }
    public class VolumePriceTrendStrategyOptions : StrategyOptions
    {
        public int Period { get; set; }
        public KLineIntervals KLineInterval { get; set; }
    }
}
