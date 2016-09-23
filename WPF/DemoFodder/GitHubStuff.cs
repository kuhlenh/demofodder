using Octokit;
using Octokit.Internal;
using Octokit.Reactive;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reactive.Linq;
using CustomExtensions;
using System.Threading;
using System.Diagnostics;

namespace DemoFodder
{
    public abstract class Contribution
    {
        public abstract string Title { get; }
        public abstract DateTimeOffset DateCreated { get; }
        public abstract User User { get; }
        public abstract int Number { get; }
        public abstract Milestone Milestone { get; }
        public abstract string Type { get; }
        public abstract bool IsInDotNetOrg { get; set; }

    }

    public class PullRequestContribution : Contribution
    {
        public PullRequest PullRequest { get; }
        public override string Title => PullRequest.Title;
        public override DateTimeOffset DateCreated => PullRequest.CreatedAt;
        public override User User => PullRequest.User;
        public override int Number => PullRequest.Number;
        public override Milestone Milestone => PullRequest.Milestone;
        public override string Type => "PullRequest";
        public override Boolean IsInDotNetOrg { get; set; }

        public PullRequestContribution(PullRequest pullRequest)
        {
            PullRequest = pullRequest;
        }
    }

    public class IssueContribution : Contribution
    {
        public Issue Issue { get; }
        public override string Title => Issue.Title;
        public override DateTimeOffset DateCreated => Issue.CreatedAt;
        public override User User => Issue.User;
        public override int Number => Issue.Number;
        public override Milestone Milestone => Issue.Milestone;
        public override string Type => "Issue";
        public override Boolean IsInDotNetOrg { get; set; }

        public IssueContribution(Issue issue)
        {
            Issue = issue;
        }
    }

    public class GitHubStuff
    {
        private const String kaseyToken = "79dd73a8b850d4a28e7af6e9a6d3eb823b1afbf4";
        private const String jonToken = "7fb6b9b09c7b1020486014fc5eec517619c0e232";
        //private const String koolToken = "eebf82cc3b8d39aa8c2cbfd5d7cf4957c7a1ab43";

        static InMemoryCredentialStore credentials = new InMemoryCredentialStore(new Credentials(kaseyToken));
        static ObservableGitHubClient client = new ObservableGitHubClient(new ProductHeaderValue("DemoFodder.Setup"), credentials);

        public static IObservable<PullRequestContribution> GetPullRequests(string owner, string name)
        {
            var filter = new PullRequestRequest() { State = ItemStateFilter.Closed };
            var query = client.PullRequest.GetAllForRepository(owner, name, filter, new ApiOptions { PageCount = 2 })
                .SelectMany(pullrequest =>
                {
                    var isInDotnetOrg = client.Organization.Member.CheckMember("dotnet", pullrequest.User.Login);
                    return isInDotnetOrg.Zip(new[] { new PullRequestContribution(pullrequest) });
                });
            //.Where(x => x.Item1 == false).Select(x => x.Item2)
            //.Catch((Exception ex) => 
            //{
            //    LogException(ex);
            //    return Observable.Create((Func<IObserver<PullRequestContribution>>) EmptyObservable);
            //});
            //.Catch((Exception ex) =>
            // {
            //     LogException(ex);
            //     return Observable.Create(subscribeAsync: (Func<IObserver<PullRequest>, CancellationToken, Task<Action>>)EmptyObservable);
            // });

            return query.Select(tuple => { tuple.Item2.IsInDotNetOrg = tuple.Item1; return tuple.Item2; });
        }


        public static IObservable<IssueContribution> GetIssues(string owner, string name)
        {
            var filter = new RepositoryIssueRequest()
            {
                Filter = IssueFilter.All,
                State = ItemStateFilter.Closed,
                Since = DateTimeOffset.Now.Subtract(TimeSpan.FromDays(1))
            };
            var query = client.Issue.GetAllForRepository(owner, name, filter)
                .SelectMany(issue =>
                {
                    var isInDotnetOrg = client.Organization.Member.CheckMember("dotnet", issue.User.Login);
                    return isInDotnetOrg.Zip(new[] { new IssueContribution(issue) });
                });
                //.Catch((Exception ex) => {
                //     LogException(ex);
                //     return Observable.Create(subscribeAsync: (Func<IObserver<Issue>, CancellationToken, Task<Action>>)EmptyObservable);
                // });
            return query.Select(tuple => { tuple.Item2.IsInDotNetOrg = tuple.Item1; return tuple.Item2; });
        }

        public static IObservable<Contribution> GetContributions(string owner, string name)
        {
            var issues = (IObservable<Contribution>) GetIssues(owner, name);
            var pullRequests = (IObservable<Contribution>) GetPullRequests(owner, name);
            return issues.Concat(pullRequests);
        }

        #region Error Testing stuff
        private static void LogException(Exception ex)
        {
            Debug.Print(ex.Message);
        }

        private static Task<Action> EmptyObservable(IObserver<PullRequestContribution> arg1, CancellationToken arg2)
        {
            Action a = new Action(() => { });
            return Task.FromResult(a);
        }

        private static Task<Action> EmptyObservable(IObserver<PullRequest> arg1, CancellationToken arg2)
        {
            Action a = new Action(() => { });
            return Task.FromResult(a);
        }
        private static Task<Action> EmptyObservable(IObserver<Issue> arg1, CancellationToken arg2)
        {
            Action a = new Action(() => { });
            return Task.FromResult(a);
        }
        #endregion

    }
}

namespace CustomExtensions
{
    public static class ZipExtension
    {
        public static IObservable<(T1, T2)> Zip<T1, T2>(this IObservable<T1> first, IObservable<T2> second)
        {
            return first.Zip(second, (x, y) => (x, y));
        }

        public static IObservable<(T1, T2)> Zip<T1, T2>(this IObservable<T1> first, T2[] second)
        {
            return first.Zip(second, (x, y) => (x, y));
        }
    }

  
}
