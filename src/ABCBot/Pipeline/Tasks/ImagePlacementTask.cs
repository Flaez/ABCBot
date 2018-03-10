using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ABCBot.Pipeline.Tasks
{
    public class ImagePlacementTask : IPipelineTask
    {
        public Task<PipelineProcessingResult> Process(IPipelineContext context) {
            var compressedImagePath = context.Data["CompressedImagePath"] as string;

            if (string.IsNullOrEmpty(compressedImagePath)) {
                return Task.FromResult(PipelineProcessingResult.Failure("Unable to find local copy of compressed merchant image."));
            }

            var targetImageName = context.MerchantDetails.Values["name"].Value.Sanitize();
            var targetImagePath = Path.Combine(context.RepositoryContext.RepositoryDirectory, "img", context.MerchantDetails.Values["category"].Value, $"{targetImageName}.png").ToLower();

            File.Copy(compressedImagePath, targetImagePath, true);

            context.MerchantDetails.PlacedImageName = $"{targetImageName}.png";

            return Task.FromResult(PipelineProcessingResult.Success());
        }
    }
}
