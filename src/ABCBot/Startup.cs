using ABCBot.Repositories;
using ABCBot.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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

            this.Services = serviceCollection.BuildServiceProvider();

            return Task.CompletedTask;
        }
    }
}
