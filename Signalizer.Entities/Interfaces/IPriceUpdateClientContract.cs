using Signalizer.Entities.Dtos;

namespace Signalizer.Entities.Interfaces
{
    public interface IPriceUpdateClientContract
    {
        Task ReceiveStockPriceUpdate(TradingDayTicker update);
        Task ReceiveStockVolumeUpdate(object update);
        Task ReceiveStockGainersUpdate(object update);
        Task ReceiveStockLosersUpdate(object update);
    }
}