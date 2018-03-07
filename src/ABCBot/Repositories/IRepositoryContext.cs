using System;
using System.Collections.Generic;
using System.Text;

namespace ABCBot.Repositories
{
    public interface IRepositoryContext : IDisposable
    {
        string RepositoryDirectory { get; }

        IEnumerable<string> EnumerateCategories();
    }
}
