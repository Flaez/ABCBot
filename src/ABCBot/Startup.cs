using ABCBot.Interop;
using ABCBot.Pipeline;
using ABCBot.Repositories;
using ABCBot.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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

        public void ConfigureKestrel(IApplicationBuilder app) {
            app.UseMvc();
        }

        public void Configure() {
            Configuration = ConfigurationBinder.CreateConfiguration();

            ConfigureGlobalLogger();
        }

        private void ConfigureGlobalLogger() {
            Log.Logger = new LoggerConfiguration()
                             .WriteTo.Console()
                             .CreateLogger();
        }

        public void Initialize() {
            Log.Information("Bot is starting.");

            Log.Information("Starting to configure services.");
            ConfigureServices(new ServiceCollection());
            Log.Information("Service configuration complete.");
        }

        public void ConfigureServices(IServiceCollection services) {
            services.AddSingleton<IGitHubService>(provider => new GitHubService(Configuration.GetSection("Github")));
            services.AddSingleton<IGitService, GitService>();
            services.AddScoped<IMerchantDetailsLoader, GithubIssueMerchantDetailsLoader>();
            services.AddScoped<ITwitterService>(provider => new TwitterService(Configuration.GetSection("Twitter")));
            services.AddScoped<INetworkService, NetworkService>();
            services.AddScoped<IDiskService, DiskService>();

            services.AddScoped<IPipelineAnnouncer, SerilogPipelineAnnouncer>();

            services.AddMvc();

            this.Services = services.BuildServiceProvider();
        }

        private async Task ProcessIssue(int issueId) {
            var dataDirectory = Configuration["DataDirectory"];

            using (var scope = Services.CreateScope()) {
                var services = scope.ServiceProvider;

                var masterRepository = new MasterRepository(services.GetService<IGitHubService>(), services.GetService<IGitService>(), services.GetService<IDiskService>(), dataDirectory);

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
