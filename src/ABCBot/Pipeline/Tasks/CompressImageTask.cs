using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ABCBot.Pipeline.Tasks
{
    public class CompressImageTask : IPipelineTask
    {
        private static readonly int TargetWidth = 32;
        private static readonly int TargetHeight = 32;

        public Task<PipelineProcessingResult> Process(IPipelineContext context) {
            var localImagePath = context.Data["ImageLocalPath"] as string;

            if (string.IsNullOrEmpty(localImagePath)) {
                return Task.FromResult(PipelineProcessingResult.Failure("Unable to find local copy of merchant image."));
            }

            var compressedImagePath = Path.GetTempFileName();

            using (var image = Image.Load(localImagePath)) {
                image.Mutate(x => x.Resize(TargetWidth, TargetHeight));

                var encoder = new PngEncoder();
                encoder.CompressionLevel = 9; // Max compression!

                using (var compressedImageStream = new FileStream(compressedImagePath, FileMode.Create)) {
                    image.SaveAsPng(compressedImageStream, encoder);
                }
            }

            context.Data.Add("CompressedImagePath", compressedImagePath);

            return Task.FromResult(PipelineProcessingResult.Success());
        }
    }
}
