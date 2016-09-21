using Octokit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoWPF.Model
{
    public abstract class Contribution
    {
        public abstract string Title { get; }
        public abstract DateTimeOffset DateCreated { get; }
        public abstract User User { get; }
        public abstract int Number { get; }
        public abstract Milestone Milestone { get; }

        public string PrintContribution()
        {
            return $"{Title} by {User.Login} in {Milestone.Title}";
        }
    }

    public class PullRequestContribution : Contribution
    {
        public PullRequest PullRequest { get; }
        public override string Title => PullRequest.Title;
        public override DateTimeOffset DateCreated => PullRequest.CreatedAt;
        public override User User => PullRequest.User;
        public override int Number => PullRequest.Number;
        public override Milestone Milestone => PullRequest.Milestone;

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

        public IssueContribution(Issue issue)
        {
            Issue = issue;
        }
    }


    public class GitHubConnection
    {
        private string Token = "3818b2b6df9c6873d34a26693b87b05e8dfe7c40";
        public GitHubClient Client { get; set; }

        public GitHubConnection()
        {
            Client = new GitHubClient(new ProductHeaderValue("demo"));
            Client.Credentials = new Credentials(Token);
        }
    }

    public class GitHub
    {

    }

    public class GitHubQueryUtil
    {
        private readonly GitHubConnection _gitHubConnection;

        public GitHubQueryUtil(GitHubConnection gitHubConnection)
        {
            _gitHubConnection = gitHubConnection;
        }

        public async Task<List<Contribution>> GetClosedIssuesAsync(Repository repo)
        {
            var filter = new RepositoryIssueRequest()
            {
                Filter = IssueFilter.All,
                State = ItemStateFilter.Closed,
                Since = DateTimeOffset.Now.Subtract(TimeSpan.FromDays(30))
            };
            var issues = await _gitHubConnection.Client.Issue.GetAllForRepository(repo.Owner.Login, repo.Name, filter);
            return issues.Select(x=> new IssueContribution(x)).ToList<Contribution>();
        }

        public async Task<List<Contribution>> GetClosePullRequestsAsync(Repository repo)
        {
            var filter = new PullRequestRequest() { State = ItemStateFilter.Closed };
            var pullRequests = await _gitHubConnection.Client.Repository.PullRequest.GetAllForRepository(repo.Owner.Login, repo.Name, filter);
            return pullRequests.Select(x=> new PullRequestContribution(x)).ToList<Contribution>();
        }

        public async Task<List<Contribution>> GetAllContributionsAsync(Repository repo)
        {
            var pullRequests = await GetClosePullRequestsAsync(repo);
            var issues = await GetClosedIssuesAsync(repo);
            return issues.Concat(pullRequests).ToList();
        }

        public async Task<Repository> GetRepo(string repoName)
        {
            var allDotNetRepos = await _gitHubConnection.Client.Repository.GetAllForOrg("dotnet");
            return allDotNetRepos.Where(x => x.FullName == repoName).First();
        }

        public async Task<IEnumerable<string>> GetMicrosoftLogins()
        {
            var microsoft = await _gitHubConnection.Client.Organization.Member.GetAll("microsoft");
            var microsoftEnumerable = microsoft.Select(x => x.Login);
            return microsoftEnumerable;
        }

        public async Task<List<Contribution>> GetCommunityContributions(List<Contribution> contributions, IEnumerable<string> msft)
        {
            List<Contribution> community = new List<Contribution>();
            foreach (var c in contributions)
            {
                switch (c)
                {
                    case PullRequestContribution p when p.PullRequest.MergedAt.HasValue && msft.Where(x => p.User.Login != x).Any():
                        community.Add(p);
                        break;
                    case IssueContribution i when msft.Where(x => i.User.Login != x).Any():
                        community.Add(i);
                        break;
                    default:
                        break;
                }
            }

            return community;
        }
    }



}
