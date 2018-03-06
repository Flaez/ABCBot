using System;
using System.Collections.Generic;
using System.Text;

namespace ABCBot.Pipeline
{
    public class PipelineContext : IPipelineContext
    {
        public int TaskIdentifier { get; }
        public Dictionary<string, object> Data { get; }

        public PipelineContext(int taskIdentifier) {
            this.TaskIdentifier = taskIdentifier;
            this.Data = new Dictionary<string, object>();
        }
    }
}
