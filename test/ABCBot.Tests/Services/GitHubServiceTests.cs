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

            var repositorySection = new Mock<IConfigurationSection>();
            repositorySection.Setup(x => x["Owner"]).Returns(repositoryOwner);
            repositorySection.Setup(x => x["Name"]).Returns(repositoryName);

            var configurationSection = new Mock<IConfigurationSection>();
            configurationSection.Setup(x => x.GetSection(It.Is<string>(y => y == "Repository"))).Returns(repositorySection.Object);

            var githubService = new GitHubService(configurationSection.Object);

            Assert.Equal(repositoryOwner, githubService.RepositoryOwner);
            Assert.Equal(repositoryName, githubService.RepositoryName);
        }
    }
}
