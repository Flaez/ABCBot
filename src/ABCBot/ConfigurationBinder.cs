using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ABCBot
{
    public static class ConfigurationBinder
    {
        public static IConfiguration CreateConfiguration() {
            var builder = new ConfigurationBuilder()
                              .SetBasePath(Directory.GetCurrentDirectory())
                              .AddJsonFile("appsettings.json", optional: true)
#if DEBUG
                              .AddJsonFile("appsettings.development.json", optional: true)
#endif
                              .AddEnvironmentVariables();

            return builder.Build();
        }
    }
}
