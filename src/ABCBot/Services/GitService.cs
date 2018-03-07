using LibGit2Sharp;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABCBot.Services
{
    public class GitService : IGitService
    {
        public Task CloneRepository(string source, string destinationDirectory) {
            var cloneOptions = new CloneOptions()
            {
                Checkout = true,
                RecurseSubmodules = true
            };

            return Task.Run(() =>
            {
                Log.Debug("Starting to clone git repository from '{source}' into '{destination}'.", source, destinationDirectory);
                Repository.Clone(source, destinationDirectory, cloneOptions);
                Log.Debug("Completed cloning git repository from '{source}' into '{destination}'.", source, destinationDirectory);
            });
        }

        // https://github.com/libgit2/libgit2sharp/wiki/git-fetch
        public Task FetchChanges(string localRepositoryDirectory) {
            using (var repo = new Repository(localRepositoryDirectory)) {
                foreach (Remote remote in repo.Network.Remotes) {
                    IEnumerable<string> refSpecs = remote.FetchRefSpecs.Select(x => x.Specification);
                    Commands.Fetch(repo, remote.Name, refSpecs, null, "");
                }
            }

            return Task.CompletedTask;
        }

        // https://github.com/libgit2/libgit2sharp/wiki/git-checkout
        public Task Checkout(string localRepositoryDirectory, string branchName) {
            using (var repo = new Repository(localRepositoryDirectory)) {
                var branch = repo.Branches[branchName];

                if (branch == null) {
                    return Task.FromException(new Exception($"Branch '{branchName}' not found during checkout."));
                }

                Commands.Checkout(repo, branch);
            }

            return Task.CompletedTask;
        }

        public Task CreateBranch(string localRepositoryDirectory, string branchName) {
            using (var repository = new Repository(localRepositoryDirectory)) {
                repository.CreateBranch(branchName);
            }

            return Task.CompletedTask;
        }

        public Task StageChanges(string localRepositoryDirectory) {
            using (var repository = new Repository(localRepositoryDirectory)) {
                Commands.Stage(repository, "*");
            }

            return Task.CompletedTask;
        }

        public Task Commit(string localRepositoryDirectory, string message) {
            using (var repository = new Repository(localRepositoryDirectory)) {
                // TODO: Move bot commit signature information to configuration
                var signature = new Signature("AcceptBitcoin.Cash Bot", "acceptbitcoincash@example.com", DateTimeOffset.UtcNow);

                repository.Commit(message, signature, signature);
            }

            return Task.CompletedTask;
        }
    }
}
