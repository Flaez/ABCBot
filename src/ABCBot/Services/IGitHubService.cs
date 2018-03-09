using Octokit;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ABCBot.Services
{
    public interface IGitHubService
    {
        Credentials Credentials { get; }

        string WebhookSecret { get; }

        Task<Issue> GetIssue(RepositoryTarget repositoryTarget, int id);
        Task<IssueComment> CreateComment(RepositoryTarget repositoryTarget, int issueId, string commentBody);
        Task<IReadOnlyList<IssueComment>> GetIssueComments(RepositoryTarget repositoryTarget, int issueId);
        Task<Repository> GetRepository(RepositoryTarget repositoryTarget);
        Task CreatePullRequest(string title, string sourceBranchName, string targetBranchName, string body = "");
        Task<bool> IsCollaborator(RepositoryTarget repositoryTarget, string user);
    }
}
