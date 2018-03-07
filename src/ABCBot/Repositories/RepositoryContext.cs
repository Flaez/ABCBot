using ABCBot.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ABCBot.Repositories
{
    public class RepositoryContext : IRepositoryContext
    {
        private readonly string DataDirectory = "_data";

        public string RepositoryDirectory { get; }

        IGitService gitService;

        public RepositoryContext(string repositoryDirectory, IGitService gitService) {
            this.RepositoryDirectory = repositoryDirectory;
            this.gitService = gitService;
        }

        public IEnumerable<string> EnumerateCategories() {
            foreach (var file in Directory.EnumerateFiles(Path.Combine(RepositoryDirectory, DataDirectory), "*.yml", SearchOption.TopDirectoryOnly)) {
                yield return Path.GetFileNameWithoutExtension(file);
            }
        }

        public void Dispose() {
            //Directory.Delete(RepositoryDirectory, true);
        }

        public Task Checkout(string branchName) {
            return gitService.Checkout(RepositoryDirectory, branchName);
        }

        public Task CreateBranch(string branchName) {
            return gitService.CreateBranch(RepositoryDirectory, branchName);
        }

        public Task StageChanges() {
            return gitService.StageChanges(RepositoryDirectory);
        }

        public Task Commit(string message) {
            return gitService.Commit(RepositoryDirectory, message);
        }
    }
}
