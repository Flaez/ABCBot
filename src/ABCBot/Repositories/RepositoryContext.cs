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

        public RepositoryContext(string repositoryDirectory, string remoteRepositoryUrl, Credentials credentials, IGitService gitService) {
            this.RepositoryDirectory = repositoryDirectory;
            this.RemoteRepositoryUrl = remoteRepositoryUrl;
            this.credentials = credentials;
            this.gitService = gitService;
        }

        public IEnumerable<string> EnumerateCategories() {
            foreach (var file in Directory.EnumerateFiles(Path.Combine(RepositoryDirectory, DataDirectory), "*.yml", SearchOption.TopDirectoryOnly)) {
                yield return Path.GetFileNameWithoutExtension(file);
            }
        }

        public void Dispose() {
            // An exception is thrown while trying to delete files created by git
            // Resetting attributes seems to work around this
            ResetAttributes(new DirectoryInfo(RepositoryDirectory));
            Directory.Delete(RepositoryDirectory, true);
        }

        // Source: https://stackoverflow.com/a/30673648
        private void ResetAttributes(DirectoryInfo directory) {
            foreach (var subDirectory in directory.GetDirectories()) {
                ResetAttributes(subDirectory);
                subDirectory.Attributes = FileAttributes.Normal;
            }
            foreach (var file in directory.GetFiles()) {
                file.Attributes = FileAttributes.Normal;
            }
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
