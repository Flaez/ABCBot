using ABCBot.Services;
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

        public MasterRepository(IGitHubService gitHubService, IGitService gitService, string dataPath) {
            this.RepositoryDataDirectory = Path.Combine(dataPath, "repositories");
            this.TaskRepositoriesDataDirectory = Path.Combine(RepositoryDataDirectory, "tasks");
            this.MasterRepositoryDirectory = Path.Combine(this.RepositoryDataDirectory, "master");

            this.gitHubService = gitHubService;
            this.gitService = gitService;
        }

        public async Task Initialize() {
            var masterRepository = await gitHubService.GetRepository();

            if (!Directory.Exists(MasterRepositoryDirectory)) {
                await gitService.CloneRepository(masterRepository.CloneUrl, MasterRepositoryDirectory);
            } else {
                await gitService.FetchChanges(MasterRepositoryDirectory);
                await gitService.Checkout(MasterRepositoryDirectory, "master");
            }
        }

        public async Task<IRepositoryContext> CreateContext(int identifier) {
            var taskDirectory = Path.Combine(TaskRepositoriesDataDirectory, $"task-{identifier}");

            // Ensure we get a clean start each time
            if (Directory.Exists(taskDirectory)) {
                Directory.Delete(taskDirectory, true);
            }

            await gitService.CloneRepository(MasterRepositoryDirectory, taskDirectory);

            return new RepositoryContext(taskDirectory, gitService);
        }
    }
}
