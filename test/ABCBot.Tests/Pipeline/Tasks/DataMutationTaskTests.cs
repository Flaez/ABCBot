using ABCBot.Models;
using ABCBot.Pipeline;
using ABCBot.Pipeline.Tasks;
using ABCBot.Services;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace ABCBot.Tests.Pipeline.Tasks
{
    public class DataMutationTaskTests
    {
        [Fact]
        public async Task ItShouldBeGivenAnHttpUrlAndTestTheHttpsVersionAndSucceedAndAlterTheMerchantUrl() {
            var httpsUrlVariant = "https://google.com";

            var merchantDetails = new MerchantDetails()
            {
                Values =
                {
                    { "url", new MerchantDetailsItem() { Value = "http://google.com" } }
                }
            };

            var networkService = new Mock<INetworkService>();
            networkService.Setup(x => x.TestLiveliness(It.Is<string>(y => y == httpsUrlVariant))).ReturnsAsync(true);

            var context = new Mock<IPipelineContext>();
            context.SetupGet(x => x.MerchantDetails).Returns(merchantDetails);

            var task = new DataMutationTask(networkService.Object);

            var result = await task.Process(context.Object);

            Assert.Equal(httpsUrlVariant, merchantDetails.Values["url"].Value);
        }

        [Fact]
        public async Task ItShouldBeGivenAnHttpUrlAndTestTheHttpsVersionAndFail() {
            var httpsUrlVariant = "https://google.com";
            var httpUrlVariant = "http://google.com";

            var merchantDetails = new MerchantDetails()
            {
                Values =
                {
                    { "url", new MerchantDetailsItem() { Value = httpUrlVariant } }
                }
            };

            var networkService = new Mock<INetworkService>();
            networkService.Setup(x => x.TestLiveliness(It.Is<string>(y => y == httpsUrlVariant))).ReturnsAsync(false);

            var context = new Mock<IPipelineContext>();
            context.SetupGet(x => x.MerchantDetails).Returns(merchantDetails);

            var task = new DataMutationTask(networkService.Object);

            var result = await task.Process(context.Object);

            Assert.Equal(httpUrlVariant, merchantDetails.Values["url"].Value);
        }

        [Fact]
        public async Task ItShouldBeGivenAnHttpsUrlAndTestTheHttpsVersionAndDoNothing() {
            var httpsUrlVariant = "https://google.com";

            var merchantDetails = new MerchantDetails()
            {
                Values =
                {
                    { "url", new MerchantDetailsItem() { Value = httpsUrlVariant } }
                }
            };

            var networkService = new Mock<INetworkService>();
            networkService.Setup(x => x.TestLiveliness(It.Is<string>(y => y == httpsUrlVariant))).ReturnsAsync(true);

            var context = new Mock<IPipelineContext>();
            context.SetupGet(x => x.MerchantDetails).Returns(merchantDetails);

            var task = new DataMutationTask(networkService.Object);

            var result = await task.Process(context.Object);

            Assert.Equal(httpsUrlVariant, merchantDetails.Values["url"].Value);
        }
    }
}
