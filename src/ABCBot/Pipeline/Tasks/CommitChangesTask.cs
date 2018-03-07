using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ABCBot.Pipeline.Tasks
{
    public class CommitChangesTask : IPipelineTask
    {
        public async Task<PipelineProcessingResult> Process(IPipelineContext context) {
            var branchName = $"issue-{context.TaskIdentifier}";

            await context.RepositoryContext.StageChanges();
            await context.RepositoryContext.Commit($"closes #{context.TaskIdentifier}");
            await context.RepositoryContext.Push(branchName);

            return PipelineProcessingResult.Success();
        }
    }
}
