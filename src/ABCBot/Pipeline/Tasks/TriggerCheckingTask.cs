using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ABCBot.Pipeline.Tasks
{
    public class TriggerCheckingTask : IPipelineTask
    {
        public Task<PipelineProcessingResult> Process(IPipelineContext context) { 
            if (context.MerchantDetails.ShouldStopExecuting) {
                return Task.FromResult(PipelineProcessingResult.SoftExit());
            }

            return Task.FromResult(PipelineProcessingResult.Success());
        }
    }
}
