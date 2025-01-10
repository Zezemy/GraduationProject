using Signalizer.Entities.Enums;

namespace Signalizer.Entities.Strategies.Options
{
    public class BollingerBandsWorkerOptions
    {
        public TimeSpan WorkInterval { get; set; } = TimeSpan.FromMinutes(1);
        public BollingerBandsStrategyOptions StrategyOptions { get; set; }
    }

    public class BollingerBandsStrategyOptions : StrategyOptions
    {
        public int Period { get; set; }
        public int StandardDeviations { get; set; }
        public KLineIntervals KLineInterval { get; set; }
    }
}
