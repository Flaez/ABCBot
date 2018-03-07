using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ABCBot.Pipeline.Tasks
{
    public class CleanupTask : IPipelineTask
    {
        public Task<PipelineProcessingResult> Process(IPipelineContext context) {
            var localImagePath = context.Data["ImageLocalPath"] as string;
            if (!string.IsNullOrEmpty(localImagePath)) {
                File.Delete(localImagePath);
            }

            var compressedImagePath = context.Data["CompressedImagePath"] as string;
            if (!string.IsNullOrEmpty(compressedImagePath)) {
                File.Delete(compressedImagePath);
            }

            return Task.FromResult(PipelineProcessingResult.Success());
        }
    }
}
