using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ABCBot.Pipeline
{
    public class TaskPipeline
    {
        public IPipelineContext Context { get; }
        public IPipelineAnnouncer Announcer { get; }
        public IReadOnlyList<IPipelineTask> Tasks { get; }

        public TaskPipeline(IPipelineContext context, IPipelineAnnouncer announcer, params IPipelineTask[] tasks) {
            this.Context = context;
            this.Announcer = announcer;
            this.Tasks = tasks;
        }

        public async Task<bool> Process() {
            foreach (var task in Tasks) {
                var taskResult = await task.Process(Context);

                if (!taskResult.IsSuccess) {
                    if (!taskResult.IsSoftExit) {
                        await Announcer.Announce(Context, taskResult.Details);
                    }
                    return false;
                }
            }

            return true;
        }
    }
}
