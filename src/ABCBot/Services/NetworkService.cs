using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
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

        public Task<bool> TestLiveliness(string url) {
            var alive = false;

            var request = (HttpWebRequest)WebRequest.Create(url);
            try {
                // Check if it succeeds in getting a response. Status codes >= 400 throw a web exception, so no need to check the actual code
                request.GetResponse();
                alive = true;
            } catch (WebException) {
                alive = false;
            }

            return Task.FromResult(alive);
        }
    }
}
