using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ABCBot.Services
{
    public class NetworkService : INetworkService
    {
        WebClient webClient;

        public NetworkService() {
            this.webClient = new WebClient();
        }

        public Task DownloadFile(string url, string filePath) {
            return webClient.DownloadFileTaskAsync(url, filePath);
        }
    }
}
