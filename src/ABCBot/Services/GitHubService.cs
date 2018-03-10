using Microsoft.Extensions.Configuration;
using Octokit;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ABCBot.Services
{
    public class GitHubService : IGitHubService
    {
        GitHubClient client;

        public string RepositoryOwner { get; }
        public string RepositoryName { get; }

        public string BotRepositoryOwner { get; }
        public string BotRepositoryName { get; }

        public string WebhookSecret { get; }

        public Credentials Credentials {
            get { return client.Credentials; }
        }

        public GitHubService(IConfigurationSection githubConfigurationSection) {
            client = new GitHubClient(new ProductHeaderValue("acceptbitcoincash-bot"));

            if (!string.IsNullOrEmpty(githubConfigurationSection["Token"])) {
                client.Credentials = new Credentials(githubConfigurationSection["Token"]);
                Log.Information("Github token set.");
            }

            RepositoryOwner = githubConfigurationSection.GetSection("Repository")["Owner"];
            RepositoryName = githubConfigurationSection.GetSection("Repository")["Name"];

            BotRepositoryOwner = githubConfigurationSection.GetSection("BotRepository")["Owner"];
            BotRepositoryName = githubConfigurationSection.GetSection("BotRepository")["Name"];

            WebhookSecret = githubConfigurationSection["WebhookSecret"];

            Log.Information("Using {owner}/{name} as the master repository.", RepositoryOwner, RepositoryName);
            Log.Information("Using {owner}/{name} as the bot repository", BotRepositoryOwner, BotRepositoryName);
        }

        public Task<Issue> GetIssue(RepositoryTarget repositoryTarget, int id) {
            return client.Issue.Get(GetRepositoryOwnerForTarget(repositoryTarget), GetRepositoryNameForTarget(repositoryTarget), id);
        }

        public Task<IssueComment> CreateComment(RepositoryTarget repositoryTarget, int issueId, string commentBody) {
            return client.Issue.Comment.Create(GetRepositoryOwnerForTarget(repositoryTarget), GetRepositoryNameForTarget(repositoryTarget), issueId, commentBody);
        }

        public Task<IReadOnlyList<IssueComment>> GetIssueComments(RepositoryTarget repositoryTarget, int issueId) {
            return client.Issue.Comment.GetAllForIssue(GetRepositoryOwnerForTarget(repositoryTarget), GetRepositoryNameForTarget(repositoryTarget), issueId);
        }

        public Task<Repository> GetRepository(RepositoryTarget repositoryTarget) {
            return client.Repository.Get(GetRepositoryOwnerForTarget(repositoryTarget), GetRepositoryNameForTarget(repositoryTarget));
        }

        public Task<bool> IsCollaborator(RepositoryTarget repositoryTarget, string user) {
            return client.Repository.Collaborator.IsCollaborator(GetRepositoryOwnerForTarget(repositoryTarget), GetRepositoryNameForTarget(repositoryTarget), user);
        }

        public Task CreatePullRequest(string title, string sourceBranchName, string targetBranchName, string body = "") {
            var pullRequest = new NewPullRequest(title, $"refs/heads/{sourceBranchName}", $"refs/heads/{targetBranchName}");

            if (!string.IsNullOrEmpty(body)) {
                pullRequest.Body = body;
            }

            return client.Repository.PullRequest.Create(GetRepositoryOwnerForTarget(RepositoryTarget.Upstream), GetRepositoryNameForTarget(RepositoryTarget.Upstream), pullRequest);
        }

        private string GetRepositoryOwnerForTarget(RepositoryTarget target) {
            switch (target) {
                case RepositoryTarget.Upstream: {
                        return RepositoryOwner;
                    }
                case RepositoryTarget.Bot: {
                        return BotRepositoryOwner;
                    }
            }

            throw new InvalidOperationException();
        }

        private string GetRepositoryNameForTarget(RepositoryTarget target) {
            switch (target) {
                case RepositoryTarget.Upstream: {
                        return RepositoryName;
                    }
                case RepositoryTarget.Bot: {
                        return BotRepositoryName;
                    }
            }

            throw new InvalidOperationException();
        }
    }
}
