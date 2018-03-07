using ABCBot.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABCBot.Pipeline.Tasks
{
    public class DataVerificationTask : IPipelineTask
    {
        ITwitterService twitterService;

        public DataVerificationTask(ITwitterService twitterService) {
            this.twitterService = twitterService;
        }

        public async Task<PipelineProcessingResult> Process(IPipelineContext context) {
            var missingFields = new List<string>();

            if (string.IsNullOrEmpty(context.MerchantDetails.Name)) {
                missingFields.Add("name");
            }
            if (string.IsNullOrEmpty(context.MerchantDetails.ImageUrl)) {
                bool discoveredImage = false;

                // Try to find an image for the merchant using the twitter account
                if (!string.IsNullOrEmpty(context.MerchantDetails.TwitterHandle)) {
                    var twitterProfileImageUrl = await twitterService.GetProfileImageUrl(context.MerchantDetails.TwitterHandle);

                    if (!string.IsNullOrEmpty(twitterProfileImageUrl)) {
                        context.MerchantDetails.ImageUrl = twitterProfileImageUrl;
                        discoveredImage = true;
                    }
                }

                if (!discoveredImage) {
                    missingFields.Add("img");
                }
            }
            if (string.IsNullOrEmpty(context.MerchantDetails.Url)) {
                missingFields.Add("url");
            }

            // Check categories
            if (!context.RepositoryContext.EnumerateCategories().Contains(context.MerchantDetails.Category, StringComparer.OrdinalIgnoreCase)) {
                missingFields.Add("category");
            }

            if (missingFields.Count == 0) {
                return PipelineProcessingResult.Success();
            } else {
                var messageBuilder = new StringBuilder();
                messageBuilder.AppendLine("The following fields are missing:");
                foreach (var field in missingFields) {
                    messageBuilder.Append("- ");
                    messageBuilder.AppendLine(field);
                }

                return PipelineProcessingResult.Failure(messageBuilder.ToString());
            }

        }
    }
}
