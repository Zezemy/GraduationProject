using Signalizer.Entities.Dtos;

namespace Signalizer.Entities
{
    public class ListStrategyRequest
    {
        public string Symbol { get;  set; }
        public int SignalType { get;  set; }
        public int Interval { get;  set; }
        public int StrategyType { get;  set; }
        public DateTime QueryStartDateTime { get;  set; }
        public DateTime QueryEndDateTime { get;  set; }
        public bool IncludePredefined { get; set; } = true;
    }
    public class ListStrategyResponseMessage : BaseResponse
    {
        public List<SignalStrategy> SignalStrategies { get; set; } = new List<SignalStrategy>();
    }
}