using Signalizer.Entities.Dtos;

namespace Signalizer.Client.State_Management
{
    public class StrategyManagementState
    {
        private SignalStrategy strategy;

        public SignalStrategy Strategy
        {
            get => strategy;
            set
            {
                strategy = value;
                NotifyStateChanged();
            }
        }

        public event Action? OnChange;

        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}
