using ABCBot.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ABCBot.Pipeline
{
    public class PipelineContext : IPipelineContext
    {
        public int TaskIdentifier { get; }
        public MerchantDetails MerchantDetails { get; }
        public Dictionary<string, object> Data { get; }

        public PipelineContext(int taskIdentifier, MerchantDetails merchantDetails) {
            this.TaskIdentifier = taskIdentifier;
            this.MerchantDetails = merchantDetails;
            this.Data = new Dictionary<string, object>();
        }
    }
}
