using Microsoft.Extensions.Configuration;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ABCBot
{
    public class Startup
    {
        public IConfiguration Configuration { get; private set; }

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
            Log.Information("Bot is now running.");

            // This is a dumb waiting mechanism to prevent the bot from closing early
            // TODO: Switch to a proper event-driven loop later.
            await Task.Delay(-1);

            Log.Information("Bot is now shutting down.");
        }
    }
}
