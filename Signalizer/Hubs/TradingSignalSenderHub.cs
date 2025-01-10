using Signalizer.Managers;
using Microsoft.AspNetCore.SignalR;
using Signalizer.Entities.Interfaces;
using Signalizer.Entities.Enums;

namespace Signalizer.Hubs
{
    internal sealed class TradingSignalSenderHub() : Hub<ISignallerClientContract>
    {
        public async Task Subscribe(string ticker, StrategyTypes strategyType, KLineIntervals interval)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, ticker + strategyType + interval);
        }

        public async Task Unsubscribe(string ticker, StrategyTypes strategyType, KLineIntervals interval)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, ticker + strategyType + interval);
        }
    };
}