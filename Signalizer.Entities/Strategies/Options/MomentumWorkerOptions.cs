using Signalizer.Entities.Enums;

namespace Signalizer.Entities.Strategies.Options
{
    public class MomentumWorkerOptions
    {
        public TimeSpan WorkInterval { get; set; } = TimeSpan.FromMinutes(1);
        public MomentumStrategyOptions StrategyOptions { get; set; }
    }
    public class MomentumStrategyOptions : StrategyOptions
    {
        public int Period { get; set; }
        public KLineIntervals KLineInterval { get; set; }
    }
}
