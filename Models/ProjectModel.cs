using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ProductivityTrack.Models
{
    public class ProjectModel
    {
        public string Name { get; set; }

        public ObservableCollection<RemindersModel> Reminders { get; set; } = new ObservableCollection<RemindersModel>();
    }
}
