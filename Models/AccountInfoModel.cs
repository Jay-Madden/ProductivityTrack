using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;

namespace ProductivityTrack.Models
{
    public class AccountInfoModel
    {
        public string Username { get; set; }

        public string Password { get; set; }

        public List<ProjectModel> Projects { get; set; }

        public AccountInfoModel(string user, string pass, List<ProjectModel> proj= null)
        {
            Username = user;
            Password = pass;
            Projects = proj ?? new List<ProjectModel>();
        }
    }
}
