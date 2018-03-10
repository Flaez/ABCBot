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
    public class ImageAcquisitionTaskTests
    {
        [Fact]
        public async Task ItShouldDownloadTheCorrectImageFileAndStoreThePath() {
            var imageUrl = "https://google.com/img";
            var tempFilePath = "/path/to/temp/file";
            var taskIdentifier = 5;

            var merchantDetails = new MerchantDetails()
            {
                Values =
                {
                    { "img", new MerchantDetailsItem() {Value = imageUrl} }
                }
            };

            var dataCollection = new Dictionary<string, object>();

            var diskService = new Mock<IDiskService>();
            diskService.Setup(x => x.GetTempFilePath()).Returns(tempFilePath);

            var networkService = new Mock<INetworkService>();

            var pipelineContext = new Mock<IPipelineContext>();
            pipelineContext.SetupGet(x => x.TaskIdentifier).Returns(taskIdentifier);
            pipelineContext.SetupGet(x => x.MerchantDetails).Returns(merchantDetails);
            pipelineContext.SetupGet(x => x.Data).Returns(dataCollection);

            var task = new ImageAcquisitionTask(diskService.Object, networkService.Object);

            await task.Process(pipelineContext.Object);

            Assert.True(dataCollection.ContainsKey("ImageLocalPath"));
            Assert.Equal(tempFilePath, dataCollection["ImageLocalPath"]);

            networkService.Verify(x => x.DownloadFile(It.Is<string>(y => y == imageUrl), It.Is<string>(y => y == tempFilePath)));
        }
    }
}
