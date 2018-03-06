using System;
using System.Threading.Tasks;

namespace ABCBot
{
    class Program
    {
        public static async Task Main(string[] args) {
            var startup = new Startup();
            startup.Configure();

            await startup.Run();
        }
    }
}
