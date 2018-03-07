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
        public async Task<PipelineProcessingResult> Process(IPipelineContext context) {
            var imageLocalPath = Path.GetTempFileName();

            using (var webClient = new WebClient()) {
                await webClient.DownloadFileTaskAsync(context.MerchantDetails.ImageUrl, imageLocalPath);
            }

            context.Data.Add("ImageLocalPath", imageLocalPath);

            return PipelineProcessingResult.Success();
        }
    }
}
