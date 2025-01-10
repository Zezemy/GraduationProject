
namespace Signalizer.Entities.Dtos
{
    public class SignalStrategy
    {
        public long? Id { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? UpdateDate { get; set; }
        public string CreatedBy { get; set; }
        public int Interval { get; set; }
        public bool IsPredefined { get; set; }
        public int StrategyType { get; set; }
        public TradingPair TradingPair { get; set; }
        public string Properties { get; set; }
    }
}
