using ProductivityTrack.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductivityTrack.Pages.Events
{
    public class OnAccountUpdated
    {
        public List<ProjectModel> Projects { get; set; }
        public OnAccountUpdated(List<ProjectModel> projects)
        {
            Projects = projects;
        }
    }

}
