using ABCBot.Models;
using ABCBot.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ABCBot.Pipeline.Tasks
{
    public class DataMutationTask : IPipelineTask
    {
        INetworkService networkService;

        public DataMutationTask(INetworkService networkService) {
            this.networkService = networkService;
        }

        public async Task<PipelineProcessingResult> Process(IPipelineContext context) {
            await MutateToSecureUrl(context.MerchantDetails);

            MutateFacebookLink(context.MerchantDetails);
            MutateTwitterLink(context.MerchantDetails);

            return PipelineProcessingResult.Success();
        }

        public async Task MutateToSecureUrl(MerchantDetails merchantDetails) {
            var url = merchantDetails.Values["url"].Value;

            if (url.StartsWith("http://")) {
                var secureUrl = url.Replace("http://", "https://");

                if (await networkService.TestLiveliness(secureUrl)) {
                    merchantDetails.UpsertValue("url").Value = secureUrl;
                }
            }
        }

        public void MutateFacebookLink(MerchantDetails merchantDetails) {
            if (merchantDetails.Values.TryGetValue("facebook", out var item)) {
                merchantDetails.UpsertValue("facebook").Value = Regex.Replace(item.Value, @"(https?:\/\/)?(www.)?facebook.com\/?", "").Trim('/');
            }
        }

        public void MutateTwitterLink(MerchantDetails merchantDetails) {
            if (merchantDetails.Values.TryGetValue("twitter", out var item)) {
                merchantDetails.UpsertValue("twitter").Value = Regex.Replace(item.Value, @"((https?:\/\/)?(www.)?twitter.com\/?)|(@)", "").Trim('/');
            }
        }
    }
}
