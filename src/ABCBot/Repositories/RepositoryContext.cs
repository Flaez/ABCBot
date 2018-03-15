using ABCBot.Services;
using LibGit2Sharp;
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
        public string RemoteRepositoryUrl { get; }

        Credentials credentials;

        IGitService gitService;
        IDiskService diskService;

        public RepositoryContext(string repositoryDirectory, string remoteRepositoryUrl, Credentials credentials, IGitService gitService, IDiskService diskService) {
            this.RepositoryDirectory = repositoryDirectory;
            this.RemoteRepositoryUrl = remoteRepositoryUrl;
            this.credentials = credentials;
            this.gitService = gitService;
            this.diskService = diskService;
        }

        public IEnumerable<string> EnumerateCategories() {
            foreach (var file in Directory.EnumerateFiles(Path.Combine(RepositoryDirectory, DataDirectory), "*.yml", SearchOption.TopDirectoryOnly)) {
                var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file);

                if (!fileNameWithoutExtension.EndsWith("sections")) {
                    yield return fileNameWithoutExtension;
                }
            }
        }

        public void Dispose() {
            diskService.DeleteDirectory(RepositoryDirectory);
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

        public Task Push(string branchName) {
            return gitService.Push(RepositoryDirectory, "bot", branchName, credentials);
        }
    }
}
