using ABCBot.Pipeline.Tasks;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using ABCBot.Services;

namespace ABCBot.Pipeline
{
    public static class ABCPipelineFactory
    {
        public static TaskPipeline BuildStandardPipeline(IPipelineContext context, IServiceProvider services) {
            var announcer = services.GetService<IPipelineAnnouncer>();
            var twitterService = services.GetService<ITwitterService>();

            return new TaskPipeline(context, announcer,
                                    new DataVerificationTask(twitterService),
                                    new ImageAcquisitionTask(),
                                    new CompressImageTask(),
                                    new ImagePlacementTask(),
                                    new CleanupTask());
        }
    }
}
