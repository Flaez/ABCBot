using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ABCBot.Pipeline
{
    public interface IPipelineAnnouncer
    {
        Task Announce(IPipelineContext context, string message);
    }
}
