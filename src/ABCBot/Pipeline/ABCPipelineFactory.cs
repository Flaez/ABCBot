using ABCBot.Pipeline.Tasks;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using ABCBot.Services;

namespace ABCBot.Pipeline
{
    public static class ABCPipelineFactory
    {
        public static TaskPipeline BuildStandardPipeline(IPipelineContext context, IServiceProvider services) {
            var announcer = services.GetService<IPipelineAnnouncer>();
            var twitterService = services.GetService<ITwitterService>();
            var diskService = services.GetService<IDiskService>();
            var networkService = services.GetService<INetworkService>();
            var githubService = services.GetService<IGitHubService>();

            return new TaskPipeline(context, announcer,
                                    new TriggerCheckingTask(),
                                    new DataVerificationTask(twitterService),
                                    new DataMutationTask(networkService),
                                    new SetupBranchTask(),
                                    new ImageAcquisitionTask(diskService, networkService),
                                    new CompressImageTask(),
                                    new ImagePlacementTask(),
                                    new CategoryYmlAmendmentTask(),
                                    new CleanupTask(),
                                    new CommitChangesTask(),
                                    new CreatePullRequestTask(githubService)
                                    );
        }
    }
}
