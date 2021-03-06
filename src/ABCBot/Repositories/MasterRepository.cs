﻿using ABCBot.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ABCBot.Repositories
{
    public class MasterRepository
    {
        public string RepositoryDataDirectory { get; }
        public string TaskRepositoriesDataDirectory { get; }
        public string MasterRepositoryDirectory { get; }

        IGitHubService gitHubService;
        IGitService gitService;
        IDiskService diskService;

        public MasterRepository(IGitHubService gitHubService, IGitService gitService, IDiskService diskService, string dataPath) {
            this.RepositoryDataDirectory = Path.Combine(dataPath, "repositories");
            this.TaskRepositoriesDataDirectory = Path.Combine(RepositoryDataDirectory, "tasks");
            this.MasterRepositoryDirectory = Path.Combine(this.RepositoryDataDirectory, "master");

            this.gitHubService = gitHubService;
            this.gitService = gitService;
            this.diskService = diskService;
        }

        public async Task Initialize() {
            var masterRepository = await gitHubService.GetRepository(RepositoryTarget.Bot);

            if (!Directory.Exists(MasterRepositoryDirectory)) {
                await gitService.CloneRepository(masterRepository.CloneUrl, MasterRepositoryDirectory);
            } else {
                await gitService.FetchChanges(MasterRepositoryDirectory);
                await gitService.Checkout(MasterRepositoryDirectory, "master");
            }
        }

        public async Task<IRepositoryContext> CreateContext(int identifier) {
            var botRepository = await gitHubService.GetRepository(RepositoryTarget.Bot);

            var taskDirectory = Path.Combine(TaskRepositoriesDataDirectory, $"task-{identifier}");

            // Ensure we get a clean start each time
            diskService.DeleteDirectory(taskDirectory);

            await gitService.CloneRepository(MasterRepositoryDirectory, taskDirectory);

            var gitCredentials = new LibGit2Sharp.UsernamePasswordCredentials();

            switch (gitHubService.Credentials.AuthenticationType) {
                case Octokit.AuthenticationType.Oauth: {
                        gitCredentials.Username = gitHubService.Credentials.Password;
                        gitCredentials.Password = "";
                    }
                    break;
                case Octokit.AuthenticationType.Basic: {
                        gitCredentials.Username = gitHubService.Credentials.Login;
                        gitCredentials.Password = gitHubService.Credentials.Password;
                    }
                    break;
            }

            await gitService.CreateRemote(taskDirectory, "bot", botRepository.HtmlUrl);

            return new RepositoryContext(taskDirectory, botRepository.Url, gitCredentials, gitService, diskService);
        }
    }
}
