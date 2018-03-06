using ABCBot.Pipeline;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace ABCBot.Tests.Pipeline
{
    public class PipelineProcessingResultTests
    {
        [Fact]
        public void ItShouldCreateASuccessfulProcessingResult() {
            var result = PipelineProcessingResult.Success();

            Assert.True(result.IsSuccess);
            Assert.Null(result.Details);
        }

        [Fact]
        public void ItShouldCreateAFailingProcessingResult() {
            var details = "There was an error";

            var result = PipelineProcessingResult.Failure(details);

            Assert.False(result.IsSuccess);
            Assert.Equal(details, result.Details);
        }
    }
}
