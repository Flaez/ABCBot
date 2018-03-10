using ABCBot.Interop;
using ABCBot.Pipeline;
using ABCBot.Repositories;
using ABCBot.Schema;
using Optional.Unsafe;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ABCBot.Services
{
    public class PipelineRunnerService : IPipelineRunnerService
    {
        MasterRepository masterRepository;
        IMerchantDetailsLoader merchantDetailsLoader;
        IServiceProvider serviceProvider;
        ISchemaLoader schemaLoader;

        public PipelineRunnerService(MasterRepository masterRepository, IMerchantDetailsLoader merchantDetailsLoader, IServiceProvider serviceProvider, ISchemaLoader schemaLoader) {
            this.masterRepository = masterRepository;
            this.merchantDetailsLoader = merchantDetailsLoader;
            this.serviceProvider = serviceProvider;
            this.schemaLoader = schemaLoader;
        }

        public async Task ProcessIssue(int id) {
            await masterRepository.Initialize();

            using (var repositoryContext = await masterRepository.CreateContext(id)) {
                var schemaFilePath = Path.Combine(repositoryContext.RepositoryDirectory, "websites_schema.yml");

                ISchemaItem schema;
                using (var schemaFileStream = new FileStream(schemaFilePath, FileMode.Open)) {
                    using (var streamReader = new StreamReader(schemaFileStream)) {
                        schema = schemaLoader.LoadSchema(streamReader);
                    }
                }

                var merchantDetailsOption = await merchantDetailsLoader.ExtractDetails(schema, id);

                var merchantDetails = merchantDetailsOption.ValueOrFailure();

                var pipelineContext = new PipelineContext(id, merchantDetails, repositoryContext, schema);

                var pipeline = ABCPipelineFactory.BuildStandardPipeline(pipelineContext, serviceProvider);

                Log.Information("Starting pipeline for id: {id}", id);

                var result = await pipeline.Process();

                Log.Information("Pipeline completed: {result}", result);
            }
        }
    }
}
