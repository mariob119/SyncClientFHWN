using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.IO;

namespace SyncClient
{
    internal class Program
    {
        static void Main(string[] args)
        {
            SyncJobs.Init();
            SyncJobs.LoadConfigurations();
            SyncJobs.HealthCheck();
            SyncJobs.StartSyncJobs();

            SyncJobs.ScanDirectories();
            //SyncJobs.test("C:\\1");
            LoggingConsole.Run();
            MainMenu.Run();

            SyncJobs.SaveConfigurations();
            LoggingConsole.Stop();
        }
    }
}


