using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace SyncClient
{
    internal class Program
    {
        static void Main(string[] args)
        {
            SyncJobs.InitSyncJobs();
            SyncJobs.LoadConfigurations();
            SyncJobs.HealthCheck();

            Console.SetCursorPosition(0, 10);

            MainMenu.Run();
            SyncJobs.SaveConfigurations();
        }
    }
}


