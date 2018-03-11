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

        [Theory]
        [InlineData("https://facebook.com/testing")]
        [InlineData("https://www.facebook.com/testing")]
        [InlineData("http://facebook.com/testing")]
        [InlineData("http://www.facebook.com/testing")]
        [InlineData("www.facebook.com/testing")]
        [InlineData("facebook.com/testing")]
        [InlineData("facebook.com/testing/")]
        [InlineData("testing")]
        public void ExtractFacebookHandleFromUrl(string facebookUrl) {
            var merchantDetails = new MerchantDetails()
            {
                Values =
                {
                    { "facebook", new MerchantDetailsItem() { Value = facebookUrl } }
                }
            };

            var task = new DataMutationTask(Mock.Of<INetworkService>());

            task.MutateFacebookLink(merchantDetails);

            Assert.Equal("testing", merchantDetails.Values["facebook"].Value);
        }

        [Theory]
        [InlineData("https://twitter.com/testing")]
        [InlineData("https://www.twitter.com/testing")]
        [InlineData("http://twitter.com/testing")]
        [InlineData("http://www.twitter.com/testing")]
        [InlineData("www.twitter.com/testing")]
        [InlineData("twitter.com/testing")]
        [InlineData("twitter.com/testing/")]
        [InlineData("@testing")]
        [InlineData("testing")]
        public void ExtractTwitterHandleFromUrl(string twitterUrl) {
            var merchantDetails = new MerchantDetails()
            {
                Values =
                {
                    { "twitter", new MerchantDetailsItem() { Value = twitterUrl } }
                }
            };

            var task = new DataMutationTask(Mock.Of<INetworkService>());

            task.MutateTwitterLink(merchantDetails);

            Assert.Equal("testing", merchantDetails.Values["twitter"].Value);
        }
    }
}
