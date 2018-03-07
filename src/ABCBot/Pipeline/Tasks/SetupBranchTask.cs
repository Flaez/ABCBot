using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ABCBot.Pipeline.Tasks
{
    public class SetupBranchTask : IPipelineTask
    {
        public async Task<PipelineProcessingResult> Process(IPipelineContext context) {
            var branchName = $"issue-{context.TaskIdentifier}";

            await context.RepositoryContext.CreateBranch(branchName);
            await context.RepositoryContext.Checkout(branchName);

            return PipelineProcessingResult.Success();
        }
    }
}
