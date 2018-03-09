using Microsoft.AspNetCore.Hosting;
using System;
using System.Threading.Tasks;

namespace ABCBot
{
    class Program
    {
        public static async Task Main(string[] args) {
            var startup = new Startup();
            startup.Configure();

            startup.Initialize();

            await InitializeKestrel(startup);
        }

        private static Task InitializeKestrel(Startup startup) {
            var host = new WebHostBuilder()
                           .UseKestrel()
                           .UseStartup<Startup>()
                           .Configure(startup.ConfigureKestrel)
                           .ConfigureServices(startup.ConfigureServices)
                           .UseUrls("http://*:3000")
                           .Build();

            return host.RunAsync();
        }
    }
}
