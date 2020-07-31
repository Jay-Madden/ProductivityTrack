using ProductivityTrack.Pages.Events;
using Stylet;

namespace ProductivityTrack.Pages
{
    public class ShellViewModel : Conductor<IScreen>, 
        IHandle<OnLoginAccepted>,
        IHandle<OnLogout>,
        IHandle<OnMetricsActivate>,
        IHandle<ExitMetrics>
    {
        private LoginWindowViewModel _loginWindowViewModel { get; }

        private ReminderBaseViewModel _reminderBaseViewModel { get; }

        private MetricsViewModel _metricsViewModel { get; }

        private IEventAggregator _eventAggregator;

        public ShellViewModel(IEventAggregator eventAggregator,
            LoginWindowViewModel loginWindowViewModel,
            ReminderBaseViewModel reminderBaseViewModel,
            MetricsViewModel metricsViewModel
            )
        {
            _eventAggregator = eventAggregator;
            _eventAggregator.Subscribe(this);

            _loginWindowViewModel = loginWindowViewModel;
            _reminderBaseViewModel = reminderBaseViewModel;
            _metricsViewModel = metricsViewModel;

            ActiveItem = _loginWindowViewModel;
        }

        public void Handle(OnLoginAccepted message)
        {
            ActiveItem = _reminderBaseViewModel;
        }

        public void Handle(OnLogout message)
        {
            ActiveItem = _loginWindowViewModel;
        }

        public void Handle(OnMetricsActivate message)
        {
            ActiveItem = _metricsViewModel;
        }

        public void Handle(ExitMetrics message)
        {
            ActiveItem = _reminderBaseViewModel;
        }
    }
}