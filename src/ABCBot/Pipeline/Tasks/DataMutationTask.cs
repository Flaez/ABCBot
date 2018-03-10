using ABCBot.Services;
using System;
using System.Collections.Generic;
using System.Text;
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
            var url = context.MerchantDetails.Values["url"].Value;

            if (url.StartsWith("http://")) {
                var secureUrl = url.Replace("http://", "https://");

                if (await networkService.TestLiveliness(secureUrl)) {
                    context.MerchantDetails.UpsertValue("url").Value = secureUrl;
                }
            }

            return PipelineProcessingResult.Success();
        }
    }
}
