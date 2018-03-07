using ABCBot.Services;
using Microsoft.Extensions.Configuration;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace ABCBot.Tests.Services
{
    public class GitHubServiceTests
    {
        [Fact]
        public void ItShouldReadConfigurationValues() {
            var repositoryOwner = "acceptbitcoincash-owner";
            var repositoryName = "acceptbitcoincash-name";

            var botRepositoryOwner = "bot-acceptbitcoincash-owner";
            var botRepositoryName = "bot-acceptbitcoincash-name";

            var repositorySection = new Mock<IConfigurationSection>();
            repositorySection.Setup(x => x["Owner"]).Returns(repositoryOwner);
            repositorySection.Setup(x => x["Name"]).Returns(repositoryName);

            var botRepositorySection = new Mock<IConfigurationSection>();
            botRepositorySection.Setup(x => x["Owner"]).Returns(botRepositoryOwner);
            botRepositorySection.Setup(x => x["Name"]).Returns(botRepositoryName);

            var configurationSection = new Mock<IConfigurationSection>();
            configurationSection.Setup(x => x.GetSection(It.Is<string>(y => y == "Repository"))).Returns(repositorySection.Object);
            configurationSection.Setup(x => x.GetSection(It.Is<string>(y => y == "BotRepository"))).Returns(botRepositorySection.Object);

            var githubService = new GitHubService(configurationSection.Object);

            Assert.Equal(repositoryOwner, githubService.RepositoryOwner);
            Assert.Equal(repositoryName, githubService.RepositoryName);

            Assert.Equal(botRepositoryOwner, githubService.BotRepositoryOwner);
            Assert.Equal(botRepositoryName, githubService.BotRepositoryName);
        }
    }
}
