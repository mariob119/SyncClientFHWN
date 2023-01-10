using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.IO;

namespace SyncClient
{
    internal class Program
    {
        static void Main(string[] args)
        {
            SyncJobs Jobs = new SyncJobs();

            LoggingConsole.Run();
            Jobs.Init();
            SyncJobs.LoadConfigurations();
            Jobs.GetLogicalDrives(SyncJobs.SyncJobConfigurations);
            Jobs.RegenerateLogicalDriveQueues();
            Jobs.RegenerateLogicalDriveQueues();

            //SyncJobs.HealthCheck();
            //SyncJobs.SynchronizeDirectories();

            MainMenu.Run();

            SyncJobs.SaveConfigurations();
            LoggingConsole.Stop();
        }
    }
}


