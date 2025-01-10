using Signalizer.Entities.Dtos;

namespace Signalizer.Entities.Interfaces
{
    public interface ISignallerClientContract
    {
        Task ReceiveSignalUpdate(TradingSignal signal);
    }
}