using ABCBot.Models;
using ABCBot.Schema;
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

            var baseSchemaItem = (((context.Schema as MappingSchemaItem).Mapping["websites"] as SequenceSchemaItem).Items[0] as MappingSchemaItem);

            foreach (var kvp in baseSchemaItem.Mapping) {
                switch (kvp.Value) {
                    case KeyValueSchemaItem keyValueItem:
                        if (!context.MerchantDetails.Values.ContainsKey(kvp.Key) || string.IsNullOrEmpty(context.MerchantDetails.Values[kvp.Key].Value)) {
                            await TryResolveMissingKey(context.MerchantDetails, kvp.Key);
                        }

                        if (keyValueItem.Required) {
                            if (!context.MerchantDetails.Values.ContainsKey(kvp.Key) || string.IsNullOrEmpty(context.MerchantDetails.Values[kvp.Key].Value)) {
                                missingFields.Add(kvp.Key);
                            }
                        }
                        break;
                }
            }

            // Check categories
            if (!context.MerchantDetails.Values.ContainsKey("category") || !context.RepositoryContext.EnumerateCategories().Contains(context.MerchantDetails.Values["category"].Value, StringComparer.OrdinalIgnoreCase)) {
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

        private async Task<bool> TryResolveMissingKey(MerchantDetails merchantDetails, string key) {
            switch (key) {
                case "img": {
                        if (merchantDetails.Values.ContainsKey("twitter") && !string.IsNullOrEmpty(merchantDetails.Values["twitter"].Value)) {
                            // Try to find an image for the merchant using the twitter account
                            var twitterProfileImageUrl = await twitterService.GetProfileImageUrl(merchantDetails.Values["twitter"].Value);

                            if (!string.IsNullOrEmpty(twitterProfileImageUrl)) {
                                merchantDetails.UpsertValue("img").Value = twitterProfileImageUrl;
                                return true;
                            }
                        }

                        return false;
                    }
            }

            return false;
        }
    }
}
