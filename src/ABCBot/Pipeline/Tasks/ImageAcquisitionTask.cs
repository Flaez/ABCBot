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

            await networkService.DownloadFile(context.MerchantDetails.ImageUrl, imageLocalPath);

            context.Data.Add("ImageLocalPath", imageLocalPath);

            return PipelineProcessingResult.Success();
        }
    }
}
