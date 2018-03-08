using ABCBot.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace ABCBot.Tests.Services
{
    public class NetworkServiceTests
    {
        [Fact]
        public async Task TestLivelinessOfHttpsSiteWithInvalidCertificateAndFail() {
            var url = "https://www.stealmylogin.com/";

            var networkService = new NetworkService();

            var result = await networkService.TestLiveliness(url);

            Assert.False(result);
        }

        [Fact]
        public async Task TestLivelinessOfHttpsSiteWithValidCertificateAndSucceed() {
            var url = "https://www.google.com/";

            var networkService = new NetworkService();

            var result = await networkService.TestLiveliness(url);

            Assert.True(result);
        }

        [Fact]
        public async Task TestLivelinessOfHttpSiteAndSucceed() {
            var url = "http://www.google.com/";

            var networkService = new NetworkService();

            var result = await networkService.TestLiveliness(url);

            Assert.True(result);
        }
    }
}
