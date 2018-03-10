using ABCBot.Models;
using ABCBot.Pipeline;
using ABCBot.Pipeline.Tasks;
using ABCBot.Repositories;
using ABCBot.Schema;
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
        ISchemaItem schema = new MappingSchemaItem()
        {
            Mapping =
                {
                    { "websites", new SequenceSchemaItem()
                        {
                            Items =
                            {
                                new MappingSchemaItem()
                                {
                                    Name = "Website",
                                    Mapping =
                                    {
                                        { "name", new KeyValueSchemaItem() { Type = "str", Required = true, Unique = true } },
                                        { "url", new KeyValueSchemaItem() { Type = "str", Required = true, Unique = true } },
                                        { "img", new KeyValueSchemaItem() { Type = "str", Pattern = @"/\.png$/i" } },
                                        { "bch", new KeyValueSchemaItem() { Type = "bool", Required = true } },
                                        { "btc", new KeyValueSchemaItem() { Type = "bool" } },
                                        { "othercrypto", new KeyValueSchemaItem() { Type = "bool" } },
                                        { "facebook", new KeyValueSchemaItem() { Type = "str", Pattern = @"/(\w){1,50}$/" } },
                                        { "email_address", new KeyValueSchemaItem() { Type = "str", Pattern = @"/\A([\w+\-].?)+@[a-z\d\-]+(\.[a-z]+)*\.[a-z]+\z/i" } },
                                        { "doc", new KeyValueSchemaItem() { Type = "str" } },
                                        { "twitter", new KeyValueSchemaItem() { Type = "str" } }
                                    }
                                }
                            }
                        }
                    }
                }
        };

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
            context.SetupGet(x => x.Schema).Returns(schema);

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
                Values =
                {
                    { "name", new MerchantDetailsItem() { Value = "test" } },
                    { "category", new MerchantDetailsItem() { Value = "category" } },
                    { "img", new MerchantDetailsItem() { Value = "https://image.url" } },
                    { "url", new MerchantDetailsItem() { Value = "https://merchant.url" } },
                    { "bch", new MerchantDetailsItem() { Value = "yes" } },
                }
            };

            var repositoryContext = new Mock<IRepositoryContext>();
            repositoryContext.Setup(x => x.EnumerateCategories()).Returns(new string[] { merchantDetails.Values["category"].Value });

            var context = new Mock<IPipelineContext>();
            context.SetupGet(x => x.TaskIdentifier).Returns(taskIdentifier);
            context.SetupGet(x => x.MerchantDetails).Returns(merchantDetails);
            context.SetupGet(x => x.Data).Returns(new Dictionary<string, object>());
            context.SetupGet(x => x.RepositoryContext).Returns(repositoryContext.Object);
            context.SetupGet(x => x.Schema).Returns(schema);

            var task = BuildTask();

            var result = await task.Process(context.Object);

            Assert.True(result.IsSuccess);
            Assert.Null(result.Details);
        }

        [Fact]
        public async Task ItShouldUseTwitterProfileImageIfImageUrlNotSpecifiedAndTwitterHandleIsAvailable() {
            var taskIdentifier = 5;
            var twitterProfileImageUrl = "https://twitter.com/img";

            var merchantDetails = new MerchantDetails()
            {
                Values =
                {
                    { "twitter", new MerchantDetailsItem() { Value = "test" } }
                }
            };

            var repositoryContext = new Mock<IRepositoryContext>();
            repositoryContext.Setup(x => x.EnumerateCategories()).Returns(Array.Empty<string>());

            var context = new Mock<IPipelineContext>();
            context.SetupGet(x => x.TaskIdentifier).Returns(taskIdentifier);
            context.SetupGet(x => x.MerchantDetails).Returns(merchantDetails);
            context.SetupGet(x => x.Data).Returns(new Dictionary<string, object>());
            context.SetupGet(x => x.RepositoryContext).Returns(repositoryContext.Object);
            context.SetupGet(x => x.Schema).Returns(schema);

            var twitterService = new Mock<ITwitterService>();
            twitterService.Setup(x => x.GetProfileImageUrl(It.Is<string>(y => y == merchantDetails.Values["twitter"].Value))).ReturnsAsync(twitterProfileImageUrl);

            var task = BuildTask(twitterService: twitterService.Object);

            var result = await task.Process(context.Object);

            Assert.Equal(twitterProfileImageUrl, merchantDetails.Values["img"].Value);
        }

        [Fact]
        public async Task ItShouldUseTwitterProfileImageIfImageUrlEmptyAndTwitterHandleIsAvailable() {
            var taskIdentifier = 5;
            var twitterProfileImageUrl = "https://twitter.com/img";

            var merchantDetails = new MerchantDetails()
            {
                Values =
                {
                    { "twitter", new MerchantDetailsItem() { Value = "test" } },
                    { "img", new MerchantDetailsItem() { Value = "" } }
                }
            };

            var repositoryContext = new Mock<IRepositoryContext>();
            repositoryContext.Setup(x => x.EnumerateCategories()).Returns(Array.Empty<string>());

            var context = new Mock<IPipelineContext>();
            context.SetupGet(x => x.TaskIdentifier).Returns(taskIdentifier);
            context.SetupGet(x => x.MerchantDetails).Returns(merchantDetails);
            context.SetupGet(x => x.Data).Returns(new Dictionary<string, object>());
            context.SetupGet(x => x.RepositoryContext).Returns(repositoryContext.Object);
            context.SetupGet(x => x.Schema).Returns(schema);

            var twitterService = new Mock<ITwitterService>();
            twitterService.Setup(x => x.GetProfileImageUrl(It.Is<string>(y => y == merchantDetails.Values["twitter"].Value))).ReturnsAsync(twitterProfileImageUrl);

            var task = BuildTask(twitterService: twitterService.Object);

            var result = await task.Process(context.Object);

            Assert.Equal(twitterProfileImageUrl, merchantDetails.Values["img"].Value);
        }


        [Fact]
        public async Task ItShouldFailIfImageUrlEmptyAndTwitterHandleIsAvailableAndTwitterProfileUrlEmpty() {
            var taskIdentifier = 5;

            var merchantDetails = new MerchantDetails()
            {
                Values =
                {
                    { "img", new MerchantDetailsItem() { Value = "" } },
                    { "name", new MerchantDetailsItem() { Value = "test" } },
                    { "category", new MerchantDetailsItem() { Value = "test" } },
                    { "url", new MerchantDetailsItem() { Value = "http://test" } },
                    { "bch", new MerchantDetailsItem() { Value = "Yes" } },
                    { "twitter", new MerchantDetailsItem() { Value = "test" } },
                }
            };

            var repositoryContext = new Mock<IRepositoryContext>();
            repositoryContext.Setup(x => x.EnumerateCategories()).Returns(new string[] { "test" });

            var context = new Mock<IPipelineContext>();
            context.SetupGet(x => x.TaskIdentifier).Returns(taskIdentifier);
            context.SetupGet(x => x.MerchantDetails).Returns(merchantDetails);
            context.SetupGet(x => x.Data).Returns(new Dictionary<string, object>());
            context.SetupGet(x => x.RepositoryContext).Returns(repositoryContext.Object);
            context.SetupGet(x => x.Schema).Returns(schema);

            var twitterService = new Mock<ITwitterService>();
            twitterService.Setup(x => x.GetProfileImageUrl(It.Is<string>(y => y == merchantDetails.Values["twitter"].Value))).ReturnsAsync("");

            var task = BuildTask(twitterService: twitterService.Object);

            var result = await task.Process(context.Object);

            Assert.Equal("", merchantDetails.Values["img"].Value);
            Assert.False(result.IsSuccess);
        }
    }
}
