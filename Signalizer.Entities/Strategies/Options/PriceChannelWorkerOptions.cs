using Signalizer.Entities.Enums;

namespace Signalizer.Entities.Strategies.Options
{
    public class PriceChannelWorkerOptions
    {
        public TimeSpan WorkInterval { get; set; } = TimeSpan.FromMinutes(1);
        public PriceChannelStrategyOptions StrategyOptions { get; set; }
    }
    public class PriceChannelStrategyOptions : StrategyOptions
    {
        public int Period { get; set; }
        public KLineIntervals KLineInterval { get; set; }
    }
}
