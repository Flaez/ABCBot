using ABCBot.Interop;
using ABCBot.Pipeline;
using ABCBot.Pipeline.GitHub;
using ABCBot.Repositories;
using ABCBot.Schema;
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
            var dataDirectory = Configuration["DataDirectory"];

            services.AddSingleton<MasterRepository>(provider => new MasterRepository(provider.GetService<IGitHubService>(), provider.GetService<IGitService>(), provider.GetService<IDiskService>(), dataDirectory));

            services.AddSingleton<IConfiguration>(Configuration);

            services.AddSingleton<IGitHubService>(provider => new GitHubService(Configuration.GetSection("Github")));
            services.AddSingleton<IGitService, GitService>();
            services.AddScoped<IMerchantDetailsLoader, GithubIssueMerchantDetailsLoader>();
            services.AddScoped<ITwitterService>(provider => new TwitterService(Configuration.GetSection("Twitter")));
            services.AddScoped<INetworkService, NetworkService>();
            services.AddScoped<IDiskService, DiskService>();
            services.AddScoped<ISchemaLoader, SchemaLoader>();

            services.AddScoped<IPipelineAnnouncer, PipelineAnnouncerGroup>(provider =>
            {
                return new PipelineAnnouncerGroup(new SerilogPipelineAnnouncer(), new GitHubPipelineAnnouncer(provider.GetService<IGitHubService>()));
            });

            services.AddScoped<IPipelineRunnerService, PipelineRunnerService>();

            services.AddMvc();

            this.Services = services.BuildServiceProvider();
        }
    }
}
