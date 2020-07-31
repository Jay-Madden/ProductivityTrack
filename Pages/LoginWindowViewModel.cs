using Newtonsoft.Json;
using ProductivityTrack.Models;
using ProductivityTrack.Pages.Events;
using Stylet;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ProductivityTrack.Pages
{
    public class LoginWindowViewModel : Screen, IHandle<OnAccountUpdated>
    {
        private readonly IEventAggregator _eventAggregator;

        private List<AccountInfoModel> _accounts = new List<AccountInfoModel>();

        private AccountInfoModel _currentAccount;

        public string InputUsername { get; set; }

        public string InputPassword { get; set; }

        private string _accountPath = @"AccountInfo.json";

        public LoginWindowViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            _eventAggregator.Subscribe(this);

            if (File.Exists(_accountPath))
            {
                using (StreamReader sr = File.OpenText(_accountPath))
                {
                    _accounts = JsonConvert.DeserializeObject<List<AccountInfoModel>>(sr.ReadToEnd());
                }
            }
        }

        public void OnClick_Login()
        {
            if (string.IsNullOrEmpty(InputUsername) || string.IsNullOrEmpty(InputPassword))
                return;

            var account = _accounts
                .Where(x => x.Username == InputUsername && x.Password == InputPassword)
                .FirstOrDefault();

            if (!(account is null))
            {
                _eventAggregator.PublishOnUIThread(new OnLoginAccepted { AccountInfo = account });
                _currentAccount = account;
            }
            else
            {
                var newAccount = new AccountInfoModel
                (
                    InputUsername,
                    InputPassword
                );

                _accounts.Add(newAccount);

                File.WriteAllText(_accountPath, JsonConvert.SerializeObject(_accounts, Formatting.Indented));

                _eventAggregator.PublishOnUIThread(new OnLoginAccepted { AccountInfo = newAccount });
                _currentAccount = newAccount;
            }

            InputUsername = InputPassword = string.Empty;
        }

        public void Handle(OnAccountUpdated message)
        {
            _currentAccount.Projects = message.Projects;
            File.WriteAllText(_accountPath, JsonConvert.SerializeObject(_accounts, Formatting.Indented));
        }
    }
}