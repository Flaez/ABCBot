using ABCBot.Pipeline;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace ABCBot.Tests.Pipeline
{
    public class TaskPipelineTests
    {
        [Fact]
        public async Task ItShouldProcessAllTasksSuccessfully() {
            var taskA = new Mock<IPipelineTask>();
            taskA.Setup(x => x.Process(It.IsAny<IPipelineContext>())).ReturnsAsync(PipelineProcessingResult.Success());

            var taskB = new Mock<IPipelineTask>();
            taskB.Setup(x => x.Process(It.IsAny<IPipelineContext>())).ReturnsAsync(PipelineProcessingResult.Success());

            var pipelineContext = Mock.Of<IPipelineContext>();
            var pipelineAnnouncer = new Mock<IPipelineAnnouncer>();

            var pipeline = new TaskPipeline(pipelineContext, pipelineAnnouncer.Object, taskA.Object, taskB.Object);

            var result = await pipeline.Process();

            Assert.True(result);

            // Ensure both tasks have been run
            taskA.Verify(x => x.Process(It.IsAny<IPipelineContext>()));
            taskB.Verify(x => x.Process(It.IsAny<IPipelineContext>()));

            // Ensure no announcements are made for succeeding tasks
            pipelineAnnouncer.Verify(x => x.Announce(It.IsAny<IPipelineContext>(), It.IsAny<string>()), Times.Never());
        }

        [Fact]
        public async Task ItShouldProcessAFailingTaskAndAnnounce() {
            var failingMessage = "Something went wrong!";

            var taskA = new Mock<IPipelineTask>();
            taskA.Setup(x => x.Process(It.IsAny<IPipelineContext>())).ReturnsAsync(PipelineProcessingResult.Failure(failingMessage));

            var pipelineContext = Mock.Of<IPipelineContext>();
            var pipelineAnnouncer = new Mock<IPipelineAnnouncer>();

            var pipeline = new TaskPipeline(pipelineContext, pipelineAnnouncer.Object, taskA.Object);

            var result = await pipeline.Process();

            Assert.False(result);

            // Ensure the task has been run
            taskA.Verify(x => x.Process(It.IsAny<IPipelineContext>()));

            // Ensure that an announcement is made upon failure
            pipelineAnnouncer.Verify(x => x.Announce(It.IsAny<IPipelineContext>(), It.Is<string>(y => y == failingMessage)), Times.Once());
        }
    }
}
