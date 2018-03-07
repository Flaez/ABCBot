using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ABCBot.Repositories
{
    public class RepositoryContext : IRepositoryContext
    {
        private readonly string DataDirectory = "_data";

        public string RepositoryDirectory { get; }

        public RepositoryContext(string repositoryDirectory) {
            this.RepositoryDirectory = repositoryDirectory;
        }

        public IEnumerable<string> EnumerateCategories() {
            foreach (var file in Directory.EnumerateFiles(Path.Combine(RepositoryDirectory, DataDirectory), "*.yml", SearchOption.TopDirectoryOnly)) {
                yield return Path.GetFileNameWithoutExtension(file);
            }
        }

        public void Dispose() {
            //Directory.Delete(RepositoryDirectory, true);
        }
    }
}
