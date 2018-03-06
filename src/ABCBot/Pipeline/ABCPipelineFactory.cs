using ABCBot.Pipeline.Tasks;
using System;
using System.Collections.Generic;
using System.Text;

namespace ABCBot.Pipeline
{
    public static class ABCPipelineFactory
    {
        public static TaskPipeline BuildStandardPipeline(IPipelineContext context, IPipelineAnnouncer announcer) {
            return new TaskPipeline(context, announcer,
                                    new DataVerificationTask());
        }
    }
}
