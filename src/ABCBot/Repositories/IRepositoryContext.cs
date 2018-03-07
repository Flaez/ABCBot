using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ABCBot.Repositories
{
    public interface IRepositoryContext : IDisposable
    {
        string RepositoryDirectory { get; }

        IEnumerable<string> EnumerateCategories();

        Task Checkout(string branchName);
        Task CreateBranch(string branchName);
        Task StageChanges();
        Task Commit(string message);
    }
}
