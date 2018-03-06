using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ABCBot.Pipeline
{
    public interface IPipelineTask
    {
        Task<PipelineProcessingResult> Process(IPipelineContext context);
    }
}
