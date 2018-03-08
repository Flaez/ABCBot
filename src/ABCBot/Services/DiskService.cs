using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ABCBot.Services
{
    public class DiskService : IDiskService
    {
        public string GetTempFilePath() {
            return Path.GetTempFileName();
        }

        public void DeleteDirectory(string directoryPath) {
            if (Directory.Exists(directoryPath)) {
                // An exception is thrown while trying to delete files created by git
                // Resetting attributes seems to work around this
                ResetAttributes(new DirectoryInfo(directoryPath));
                Directory.Delete(directoryPath, true);
            }
        }

        // Source: https://stackoverflow.com/a/30673648
        private void ResetAttributes(DirectoryInfo directory) {
            foreach (var subDirectory in directory.GetDirectories()) {
                ResetAttributes(subDirectory);
                subDirectory.Attributes = FileAttributes.Normal;
            }
            foreach (var file in directory.GetFiles()) {
                file.Attributes = FileAttributes.Normal;
            }
        }
    }
}
