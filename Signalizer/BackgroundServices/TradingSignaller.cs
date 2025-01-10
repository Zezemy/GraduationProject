using Signalizer.Hubs;
using Microsoft.AspNetCore.SignalR;
using Signalizer.Entities.Interfaces;
using Signalizer.Entities.Enums;
using Signalizer.Context;

namespace Signalizer.BackgroundServices
{
    internal sealed class TradingSignaller(
        IServiceScopeFactory serviceScopeFactory,
        IHubContext<TradingSignalSenderHub, ISignallerClientContract> hubContext,
        ILogger<TradingSignaller> logger)
        : BackgroundService
    {
        private readonly Random _random = new();

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await SendSignal();

                await Task.Delay(15000, stoppingToken);
            }
        }

        private async Task SendSignal()
        {
            using var scope = serviceScopeFactory.CreateScope();
            using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var signalRecords = context.TradingSignals.Where(x => x.DateTime >= DateTime.Today).OrderBy(x => x.Id);
            var latestSignals = signalRecords.GroupBy(x => x.Symbol + x.StrategyType + x.Interval).Select(g => g.OrderByDescending(l => l.Id).First());

            foreach (var signalRecord in latestSignals)
            {
                var signal = new Entities.Dtos.TradingSignal();
                signal.SignalType = (SignalTypes)Enum.Parse(typeof(SignalTypes), signalRecord.SignalType.ToString());
                signal.Symbol = signalRecord.Symbol;
                signal.DateTime = signalRecord.DateTime;
                signal.StrategyType = (StrategyTypes)Enum.Parse(typeof(StrategyTypes), signalRecord.StrategyType.ToString());
                signal.Interval = (KLineIntervals)Enum.Parse(typeof(KLineIntervals), signalRecord.Interval.ToString());

                await hubContext.Clients.Group(signal.Symbol + signal.StrategyType + signal.Interval).ReceiveSignalUpdate(signal);

                logger.LogInformation("Updated {ticker} signal to {signal}", signal.Symbol, signal);
            }
        }
    }
}