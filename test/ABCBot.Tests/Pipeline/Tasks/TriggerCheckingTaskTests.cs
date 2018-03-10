using ABCBot.Models;
using ABCBot.Pipeline;
using ABCBot.Pipeline.Tasks;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace ABCBot.Tests.Pipeline.Tasks
{
    public class TriggerCheckingTaskTests
    {
        [Fact]
        public async Task ItShouldSoftExitIfMerchantDetailsShouldStopExecuting() {
            var merchantDetails = new MerchantDetails()
            {
                ShouldStopExecuting = true
            };

            var pipelineContext = new Mock<IPipelineContext>();
            pipelineContext.SetupGet(x => x.MerchantDetails).Returns(merchantDetails);

            var task = new TriggerCheckingTask();

            var result = await task.Process(pipelineContext.Object);

            Assert.True(result.IsSoftExit);
            Assert.False(result.IsSuccess);
        }

        [Fact]
        public async Task ItShouldSucceedIfMerchantDetailsNotShouldStopExecuting() {
            var merchantDetails = new MerchantDetails()
            {
                ShouldStopExecuting = false
            };

            var pipelineContext = new Mock<IPipelineContext>();
            pipelineContext.SetupGet(x => x.MerchantDetails).Returns(merchantDetails);

            var task = new TriggerCheckingTask();

            var result = await task.Process(pipelineContext.Object);

            Assert.False(result.IsSoftExit);
            Assert.True(result.IsSuccess);
        }
    }
}
