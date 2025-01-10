using Signalizer.Entities.Dtos;
using Signalizer.Entities.Models;

namespace Signalizer.Entities
{
    public class UpdateStrategyRequestMessage
    {
        public SignalStrategy SignalStrategy { get; set; }
    }
    public class UpdateStrategyResponseMessage: BaseResponse
    {
    }
}