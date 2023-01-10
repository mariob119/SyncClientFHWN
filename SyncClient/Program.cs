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

            //SyncJobs.MirrorAllDirectoriesOfSyncJobs(SyncJobs.SyncJobConfigurations);

            LoggingConsole.Run();
            MainMenu.Run();

            SyncJobs.SaveConfigurations();
            LoggingConsole.Stop();
        }
    }
}


