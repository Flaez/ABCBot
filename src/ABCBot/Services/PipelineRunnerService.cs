using ABCBot.Interop;
using ABCBot.Pipeline;
using ABCBot.Repositories;
using Optional.Unsafe;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ABCBot.Services
{
    public class PipelineRunnerService : IPipelineRunnerService
    {
        MasterRepository masterRepository;
        IMerchantDetailsLoader merchantDetailsLoader;
        IServiceProvider serviceProvider;

        public PipelineRunnerService(MasterRepository masterRepository, IMerchantDetailsLoader merchantDetailsLoader, IServiceProvider serviceProvider) {
            this.masterRepository = masterRepository;
            this.merchantDetailsLoader = merchantDetailsLoader;
            this.serviceProvider = serviceProvider;
        }

        public async Task ProcessIssue(int id) {
            await masterRepository.Initialize();

            using (var repositoryContext = await masterRepository.CreateContext(id)) {
                var merchantDetailsOption = await merchantDetailsLoader.ExtractDetails(id);

                var merchantDetails = merchantDetailsOption.ValueOrFailure();

                var pipelineContext = new PipelineContext(id, merchantDetails, repositoryContext);

                var pipeline = ABCPipelineFactory.BuildStandardPipeline(pipelineContext, serviceProvider);

                Log.Information("Starting pipeline for id: {id}", id);

                var result = await pipeline.Process();

                Log.Information("Pipeline completed: {result}", result);
            }
        }
    }
}
