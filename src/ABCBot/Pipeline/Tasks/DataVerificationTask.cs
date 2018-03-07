using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ABCBot.Pipeline.Tasks
{
    public class DataVerificationTask : IPipelineTask
    {
        public Task<PipelineProcessingResult> Process(IPipelineContext context) {
            var missingFields = new List<string>();

            if (string.IsNullOrEmpty(context.MerchantDetails.Name)) {
                missingFields.Add("name");
            }
            if (string.IsNullOrEmpty(context.MerchantDetails.ImageUrl)) {
                missingFields.Add("img");
            }
            if (string.IsNullOrEmpty(context.MerchantDetails.Url)) {
                missingFields.Add("url");
            }

            if (missingFields.Count == 0) {
                return Task.FromResult(PipelineProcessingResult.Success());
            }else {
                var messageBuilder = new StringBuilder();
                messageBuilder.AppendLine("The following fields are missing:");
                foreach (var field in missingFields) {
                    messageBuilder.Append("- ");
                    messageBuilder.AppendLine(field);
                }

                return Task.FromResult(PipelineProcessingResult.Failure(messageBuilder.ToString()));
            }

        }
    }
}
