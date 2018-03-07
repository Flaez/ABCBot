using Serilog;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ABCBot.Pipeline
{
    public class SerilogPipelineAnnouncer : IPipelineAnnouncer
    {
        public Task Announce(IPipelineContext context, string message) {
            Log.Information("[Id {id}] {message}", context.TaskIdentifier, message);

            return Task.CompletedTask;
        }
    }
}
