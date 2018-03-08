using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ABCBot.Services
{
    public interface INetworkService
    {
        Task DownloadFile(string url, string filePath);
    }
}
