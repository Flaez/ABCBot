using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ABCBot.Services
{
    public interface IGitService
    {
        Task CloneRepository(string source, string destinationDirectory);
        Task FetchChanges(string localRepositoryDirectory);
        Task Checkout(string localRepositoryDirectory, string branchName);
        Task CreateBranch(string localRepositoryDirectory, string branchName);
        Task StageChanges(string localRepositoryDirectory);
        Task Commit(string localRepositoryDirectory, string message);
        Task CreateRemote(string localRepositoryDirectory, string remoteName, string remoteUrl);
        Task Push(string localRepositoryDirectory, string remoteName, string branch, Credentials remoteCredentials);
    }
}
