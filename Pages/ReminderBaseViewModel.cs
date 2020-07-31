using ProductivityTrack.Models;
using ProductivityTrack.Pages.Events;
using Stylet;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace ProductivityTrack.Pages
{
    public class ReminderBaseViewModel : Screen, IHandle<OnLoginAccepted>
    {
        private IEventAggregator _eventAggregator { get; }

        public string InputProject { get; set; }

        public string InputDescription { get; set; }

        public string InputReminder { get; set; }

        public ProjectModel CurrentProject { get; set; }

        private ObservableCollection<ProjectModel> _projects;
        public ObservableCollection<ProjectModel> Projects
        {
            get => _projects;
            set {
                SetAndNotify(ref _projects, value);
            }
        }

        public ReminderBaseViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            _eventAggregator.Subscribe(this);
        }

        public void OnClick_Logout()
        {
            _eventAggregator.Publish(new OnLogout());
        }
        
        public void OnClick_AddReminder()
        {
            if (CurrentProject is null)
                return;

            var reminder = new RemindersModel { Name = InputReminder, Description = InputDescription };
            CurrentProject.Reminders.Add(reminder);
            _eventAggregator.Publish(new OnAccountUpdated(new List<ProjectModel>(Projects)));

        }

        public void OnClick_AddProject()
        {
            if (string.IsNullOrEmpty(InputProject))
                return;

            var project = new ProjectModel { Name = InputProject };
            Projects.Add(project);
            _eventAggregator.Publish(new OnAccountUpdated(new List<ProjectModel>(Projects)));
        }

        public void OnClick_Metrics()
        {
            _eventAggregator.Publish(new OnMetricsActivate());
        }

        public void Handle(OnLoginAccepted message)
        {
            Projects = new ObservableCollection<ProjectModel>(message.AccountInfo.Projects);
        }
    }
}