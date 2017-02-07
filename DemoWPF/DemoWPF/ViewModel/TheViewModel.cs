using DemoWPF.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using Octokit;

namespace DemoWPF.ViewModel
{
    class TheViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChangedEvent(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    //ICommand Stuff
    class DelegateCommand : ICommand
    {
        Action _action;

        public DelegateCommand(Action action)
        {
            _action = action;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            _action();
        }
    }

    class Presenter : TheViewModel
    {
        GitHubQueryUtil _util = new GitHubQueryUtil(new GitHubConnection());
        List<Contribution> _someContributions;
        ObservableCollection<string> _history = new ObservableCollection<string>();
        ObservableCollection<string> _contributions = new ObservableCollection<string>();

        public IEnumerable<string> ContributorList
        {
            get { return _someContributions?.Select(s => s.PrintContribution()); }
        }

        public ICommand ConvertTextCommand
        {
            get { return new DelegateCommand(GetOSS); }
        }

        private void GetOSS()
        {
            PrintContributors(_util);
        }

        private async void PrintContributors(GitHubQueryUtil util)
        {
            var b = await Initialize();
            if (b)
            {
                var microsoftEmployees = await util.GetMicrosoftLogins();
                _someContributions = await util.GetCommunityContributions(_someContributions, microsoftEmployees);
            }
        }

        public List<Contribution> SomeContribution
        {
            get { return _someContributions; }
            set
            {
                _someContributions = value;
                RaisePropertyChangedEvent("SomeContribution");
            }
        }

        public async Task<bool> Initialize()
        {
            var roslyn = await _util.GetRepo("dotnet/roslyn");
            var c = await _util.GetAllContributionsAsync(roslyn);
            _someContributions = c;
            return true;
        }
        
    }
}
