using Signalizer.Entities.Enums;

namespace Signalizer.Entities.Strategies.Options
{
    public class StochasticOscillatorWorkerOptions
    {
        public TimeSpan WorkInterval { get; set; } = TimeSpan.FromMinutes(1);
        public StochasticOscillatorStrategyOptions StrategyOptions { get; set; }
    }
    public class StochasticOscillatorStrategyOptions : StrategyOptions
    {
        public int Period { get; set; }
        public int Overbought { get; set; }
        public int Oversold { get; set; }
        public KLineIntervals KLineInterval { get; set; }
    }
}
