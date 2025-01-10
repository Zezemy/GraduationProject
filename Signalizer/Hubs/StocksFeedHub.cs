using Signalizer.Managers;
using Microsoft.AspNetCore.SignalR;
using Signalizer.Entities.Interfaces;

namespace Signalizer.Hubs
{
    internal sealed class StocksFeedHub(ActiveTickerManager activeTickerManager) : Hub<IPriceUpdateClientContract>
    {
        public async Task Subscribe(string ticker)
        {
            activeTickerManager.AddTicker(ticker);
            await Groups.AddToGroupAsync(Context.ConnectionId, ticker);
        }

        public async Task Unsubscribe(string ticker)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, ticker);
        }
    };
}