using ABCBot.Models;
using ABCBot.Pipeline;
using ABCBot.Pipeline.Tasks;
using ABCBot.Repositories;
using ABCBot.Services;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace ABCBot.Tests.Pipeline.Tasks
{
    public class DataVerificationTaskTests
    {
        private DataVerificationTask BuildTask(ITwitterService twitterService = null) {
            if (twitterService == null) {
                twitterService = Mock.Of<ITwitterService>();
            }

            return new DataVerificationTask(twitterService);
        }

        [Fact]
        public async Task ItShouldFailIfAllRequiredFieldsAreMissing() {
            var taskIdentifier = 5;

            var merchantDetails = new MerchantDetails();

            var repositoryContext = new Mock<IRepositoryContext>();
            repositoryContext.Setup(x => x.EnumerateCategories()).Returns(Array.Empty<string>());

            var context = new Mock<IPipelineContext>();
            context.SetupGet(x => x.TaskIdentifier).Returns(taskIdentifier);
            context.SetupGet(x => x.MerchantDetails).Returns(merchantDetails);
            context.SetupGet(x => x.Data).Returns(new Dictionary<string, object>());
            context.SetupGet(x => x.RepositoryContext).Returns(repositoryContext.Object);

            var task = BuildTask();

            var result = await task.Process(context.Object);

            Assert.False(result.IsSuccess);
            Assert.NotNull(result.Details);
        }

        [Fact]
        public async Task ItShouldSucceedIfAllRequiredFieldsArePresent() {
            var taskIdentifier = 5;

            var merchantDetails = new MerchantDetails()
            {
                Name = "test",
                Category = "category",
                ImageUrl = "https://image.url",
                Url = "https://merchant.url"
            };

            var repositoryContext = new Mock<IRepositoryContext>();
            repositoryContext.Setup(x => x.EnumerateCategories()).Returns(new string[] { merchantDetails.Category });

            var context = new Mock<IPipelineContext>();
            context.SetupGet(x => x.TaskIdentifier).Returns(taskIdentifier);
            context.SetupGet(x => x.MerchantDetails).Returns(merchantDetails);
            context.SetupGet(x => x.Data).Returns(new Dictionary<string, object>());
            context.SetupGet(x => x.RepositoryContext).Returns(repositoryContext.Object);

            var task = BuildTask();

            var result = await task.Process(context.Object);

            Assert.True(result.IsSuccess);
            Assert.Null(result.Details);
        }

        [Fact]
        public async Task ItShouldUseTwitterProfileImageIfImageUrlNotSpecifiedAndTwitterHandleIsAvailable() {
            var taskIdentifier = 5;
            var twitterProfileImageUrl = "https://twitter.com/img";

            var merchantDetails = new MerchantDetails();
            merchantDetails.TwitterHandle = "test";

            var repositoryContext = new Mock<IRepositoryContext>();
            repositoryContext.Setup(x => x.EnumerateCategories()).Returns(Array.Empty<string>());

            var context = new Mock<IPipelineContext>();
            context.SetupGet(x => x.TaskIdentifier).Returns(taskIdentifier);
            context.SetupGet(x => x.MerchantDetails).Returns(merchantDetails);
            context.SetupGet(x => x.Data).Returns(new Dictionary<string, object>());
            context.SetupGet(x => x.RepositoryContext).Returns(repositoryContext.Object);

            var twitterService = new Mock<ITwitterService>();
            twitterService.Setup(x => x.GetProfileImageUrl(It.Is<string>(y => y == merchantDetails.TwitterHandle))).ReturnsAsync(twitterProfileImageUrl);

            var task = BuildTask(twitterService: twitterService.Object);

            var result = await task.Process(context.Object);

            Assert.Equal(twitterProfileImageUrl, merchantDetails.ImageUrl);
        }
    }
}
