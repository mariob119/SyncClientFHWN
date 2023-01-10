using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.IO;

namespace SyncClient
{
    internal class Program
    {
        static void Main(string[] args)
        {
            LoggingConsole.Run();

            SyncJobs.Init();
            SyncJobs.LoadConfigurations();
            SyncJobs.HealthCheck();
            SyncJobs.SynchronizeDirectories();

            MainMenu.Run();

            SyncJobs.SaveConfigurations();
            LoggingConsole.Stop();
        }
    }
}


