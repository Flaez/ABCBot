using ABCBot.Interop;
using ABCBot.Pipeline;
using ABCBot.Repositories;
using ABCBot.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Optional;
using Optional.Unsafe;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ABCBot
{
    public class Startup
    {
        public IConfiguration Configuration { get; private set; }
        public IServiceProvider Services { get; private set; }

        public void Configure() {
            Configuration = ConfigurationBinder.CreateConfiguration();

            ConfigureGlobalLogger();
        }

        private void ConfigureGlobalLogger() {
            Log.Logger = new LoggerConfiguration()
                             .WriteTo.Console()
                             .CreateLogger();
        }

        public async Task Run() {
            Log.Information("Bot is starting.");

            Log.Information("Starting to configure services.");
            await ConfigureServices();
            Log.Information("Service configuration complete.");

            Log.Information("Waiting for commands.");

            // This is a dumb waiting mechanism to prevent the bot from closing early
            // TODO: Switch to a proper event-driven loop later.
            await Task.Delay(-1);

            Log.Information("Bot is now shutting down.");
        }

        public Task ConfigureServices() {
            var serviceCollection = new ServiceCollection();

            serviceCollection.AddSingleton<IGitHubService>(provider => new GitHubService(Configuration.GetSection("Github")));
            serviceCollection.AddSingleton<IGitService, GitService>();
            serviceCollection.AddScoped<IMerchantDetailsLoader, GithubIssueMerchantDetailsLoader>();
            serviceCollection.AddScoped<ITwitterService>(provider => new TwitterService(Configuration.GetSection("Twitter")));

            serviceCollection.AddScoped<IPipelineAnnouncer, SerilogPipelineAnnouncer>();

            this.Services = serviceCollection.BuildServiceProvider();

            return Task.CompletedTask;
        }

        private async Task ProcessIssue(int issueId) {
            var dataDirectory = Configuration["DataDirectory"];

            using (var scope = Services.CreateScope()) {
                var services = scope.ServiceProvider;

                var masterRepository = new MasterRepository(services.GetService<IGitHubService>(), services.GetService<IGitService>(), dataDirectory);

                await masterRepository.Initialize();

                using (var repositoryContext = await masterRepository.CreateContext(issueId)) {
                    var merchantDetailsLoader = services.GetService<IMerchantDetailsLoader>();
                    var merchantDetailsOption = await merchantDetailsLoader.ExtractDetails(issueId);

                    var merchantDetails = merchantDetailsOption.ValueOrFailure();

                    var pipelineContext = new PipelineContext(issueId, merchantDetails, repositoryContext);

                    var pipeline = ABCPipelineFactory.BuildStandardPipeline(pipelineContext, services);

                    Log.Information("Starting pipeline for id: {id}", issueId);

                    var result = await pipeline.Process();

                    Log.Information("Pipeline completed: {result}", result);
                }
            }
        }

    }
}
