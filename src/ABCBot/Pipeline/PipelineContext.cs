using ABCBot.Models;
using ABCBot.Repositories;
using ABCBot.Schema;
using System;
using System.Collections.Generic;
using System.Text;

namespace ABCBot.Pipeline
{
    public class PipelineContext : IPipelineContext
    {
        public int TaskIdentifier { get; }
        public MerchantDetails MerchantDetails { get; }
        public IRepositoryContext RepositoryContext { get; }
        public ISchemaItem Schema { get; }
        public Dictionary<string, object> Data { get; }

        public PipelineContext(int taskIdentifier, MerchantDetails merchantDetails, IRepositoryContext repositoryContext, ISchemaItem schema) {
            this.TaskIdentifier = taskIdentifier;
            this.MerchantDetails = merchantDetails;
            this.RepositoryContext = repositoryContext;
            this.Schema = schema;
            this.Data = new Dictionary<string, object>();
        }
    }
}
