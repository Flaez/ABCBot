using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ABCBot.Pipeline
{
    public class PipelineAnnouncerGroup : IPipelineAnnouncer
    {
        public IReadOnlyList<IPipelineAnnouncer> Announcers { get; }

        public PipelineAnnouncerGroup(params IPipelineAnnouncer[] announcers) {
            this.Announcers = announcers;
        }

        public async Task Announce(IPipelineContext context, string message) {
            foreach (var announcer in Announcers) {
                await announcer.Announce(context, message);
            }
        }
    }
}
