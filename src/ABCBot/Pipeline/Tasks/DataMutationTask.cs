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
            if (context.MerchantDetails.Url.StartsWith("http://")) {
                var secureUrl = context.MerchantDetails.Url.Replace("http://", "https://");

                if (await networkService.TestLiveliness(secureUrl)) {
                    context.MerchantDetails.Url = secureUrl;
                }
            }

            return PipelineProcessingResult.Success();
        }
    }
}
