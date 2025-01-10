using Signalizer.Entities.Dtos;

namespace Signalizer.Entities
{
    public class CreateStrategyRequestMessage
    {
        public SignalStrategy SignalStrategy { get;  set; }
    }
    public class CreateStrategyResponseMessage: BaseResponse
    {
    }
}