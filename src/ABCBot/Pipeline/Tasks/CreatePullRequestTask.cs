using ABCBot.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ABCBot.Pipeline.Tasks
{
    public class CreatePullRequestTask : IPipelineTask
    {
        IGitHubService gitHubService;

        public CreatePullRequestTask(IGitHubService gitHubService) {
            this.gitHubService = gitHubService;
        }

        public async Task<PipelineProcessingResult> Process(IPipelineContext context) {
            var branchName = $"issue-{context.TaskIdentifier}";

            var title = $"[Bot] Closes #{context.TaskIdentifier}";
            var body = "*This pull request was generated automatically.*";

            await gitHubService.CreatePullRequest(title, branchName, "master", body);

            return PipelineProcessingResult.Success();
        }
    }
}
