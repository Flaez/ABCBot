using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ABCBot.Services
{
    public interface IGitService
    {
        Task CloneRepository(string source, string destinationDirectory);
        Task FetchChanges(string localRepositoryDirectory);
        Task Checkout(string localRepositoryDirectory, string branchName);
    }
}
