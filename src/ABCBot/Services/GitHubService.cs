using Microsoft.Extensions.Configuration;
using Octokit;
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

        public GitHubService(IConfigurationSection githubConfigurationSection) {
            client = new GitHubClient(new ProductHeaderValue("acceptbitcoincash-bot"));

            if (!string.IsNullOrEmpty(githubConfigurationSection["Token"])) {
                client.Credentials = new Credentials(githubConfigurationSection["Token"]);
            }

            RepositoryOwner = githubConfigurationSection.GetSection("Repository")["Owner"];
            RepositoryName = githubConfigurationSection.GetSection("Repository")["Name"];
        }

        public Task<Issue> GetIssue(int id) {
            return client.Issue.Get(RepositoryOwner, RepositoryName, id);
        }

        public Task<IssueComment> CreateComment(int issueId, string commentBody) {
            return client.Issue.Comment.Create(RepositoryOwner, RepositoryName, issueId, commentBody);
        }
    }
}
