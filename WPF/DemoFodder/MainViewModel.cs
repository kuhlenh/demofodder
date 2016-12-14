using Octokit;
using Octokit.Internal;
using Octokit.Reactive;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReactiveUI;
using System.Reactive.Linq;
using static DemoFodder.GitHubStuff;
using System.Windows.Input;

namespace DemoFodder
{
    public class MainViewModel : ReactiveObject, IContributionViewModel, INotifyPropertyChanged
    {
        public IReactiveDerivedList<Contribution> Contributions { get; }
        public event PropertyChangedEventHandler PropertyChanged;
        public IReactiveDerivedList<Contribution> displayContributions;


        public IReactiveDerivedList<Contribution> DisplayContributions
        {
            get { return displayContributions; }
            set
            {
                displayContributions = value;
                RaisePropertyChangedEvent("DisplayContributions");
            }
        }

        public MainViewModel()
        {
            Contributions = GitHubStuff.GetContributions("dotnet", "roslyn").CreateCollection();
            DisplayContributions = Contributions;
        }

        public ICommand FilterByCommunity
        {
            get { return new RelayCommand(GetCommunityContributions); }  
        }

        public ICommand AllContributions
        {
            get { return new RelayCommand(GetAllContributions); }
        }

        public ICommand ShowPullRequests
        {
            get { return new RelayCommand(GetOnlyPullRequests); }
        }

        private void GetOnlyPullRequests()
        {
            DisplayContributions = DisplayContributions.Where(x => x.Type == "PullRequest").CreateDerivedCollection(x => x);
        }

        private void GetAllContributions()
        {
            DisplayContributions = Contributions;
        }

        private void GetCommunityContributions()
        {
            DisplayContributions = DisplayContributions.Where(x=>x.IsInDotNetOrg==false).CreateDerivedCollection(x=>x);
        }

        protected void RaisePropertyChangedEvent(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    public interface IContributionViewModel
    {
        IReactiveDerivedList<Contribution> Contributions { get; }
        IReactiveDerivedList<Contribution> DisplayContributions { get; set; }
    }


    class RelayCommand : ICommand
    {
        Action _action;
        public event EventHandler CanExecuteChanged;

        public RelayCommand(Action action)
        {
            _action = action;
        }

        public bool CanExecute(object parameter) => true; 
        public void Execute(object parameter) => _action();
    }



}
