using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.IO;

namespace SyncClient
{
    internal class Program
    {
        static void Main(string[] args)
        {
            SyncJobs.InitSyncJobs();
            SyncJobs.LoadConfigurations();
            SyncJobs.HealthCheck();
            LoggingConsole.Start();
            MainMenu.Run();
            SyncJobs.SaveConfigurations();
            LoggingConsole.Stop();
        }
    }
}


