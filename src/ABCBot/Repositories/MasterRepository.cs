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
        public string MasterRepositoryDirectory { get; }

        IGitHubService gitHubService;
        IGitService gitService;

        public MasterRepository(IGitHubService gitHubService, IGitService gitService, string dataPath) {
            this.RepositoryDataDirectory = Path.Combine(dataPath, "repositories");
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

        public IRepositoryContext CreateContext() {
            return new RepositoryContext(this.MasterRepositoryDirectory);
        }
    }
}
