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
    }
}
