using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ABCBot.Pipeline.Tasks
{
    public class DataVerificationTask : IPipelineTask
    {
        public Task<PipelineProcessingResult> Process(IPipelineContext context) {
            if (string.IsNullOrEmpty(context.MerchantDetails.Name)) {
                return Task.FromResult(PipelineProcessingResult.Failure("Merchant name is missing."));
            }
            if (string.IsNullOrEmpty(context.MerchantDetails.ImageUrl)) {
                return Task.FromResult(PipelineProcessingResult.Failure("No image url has been specified."));
            }
            if (string.IsNullOrEmpty(context.MerchantDetails.Url)) {
                return Task.FromResult(PipelineProcessingResult.Failure("No merchant url been specified."));
            }

            return Task.FromResult(PipelineProcessingResult.Success());
        }
    }
}
