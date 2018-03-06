using ABCBot.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ABCBot.Pipeline.GitHub
{
    public class GitHubPipelineAnnouncer : IPipelineAnnouncer
    {
        IGitHubService gitHubService;

        public GitHubPipelineAnnouncer(IGitHubService gitHubService) {
            this.gitHubService = gitHubService;
        }

        public Task Announce(IPipelineContext context, string message) {
            return gitHubService.CreateComment(context.TaskIdentifier, message);
        }
    }
}
