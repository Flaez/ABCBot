using ABCBot.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ABCBot.Pipeline.Tasks
{
    public class ImageAcquisitionTask : IPipelineTask
    {
        IDiskService diskService;
        INetworkService networkService;

        public ImageAcquisitionTask(IDiskService diskService, INetworkService networkService) {
            this.diskService = diskService;
            this.networkService = networkService;
        }

        public async Task<PipelineProcessingResult> Process(IPipelineContext context) {
            var imageLocalPath = diskService.GetTempFilePath();

            try {
                await networkService.DownloadFile(context.MerchantDetails.Values["img"].Value, imageLocalPath);
            } catch (WebException) {
                return PipelineProcessingResult.Failure($"Unable to download image at `{context.MerchantDetails.Values["img"].Value}`.");
            }

            context.Data.Add("ImageLocalPath", imageLocalPath);

            return PipelineProcessingResult.Success();
        }
    }
}
