using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.IO;

namespace SyncClient
{
    internal class Program
    {
        static void Main(string[] args)
        {
            SyncTasks Jobs = new SyncTasks();

            LoggingConsole.Run();

            SyncTasks.LoadConfigurations();


            //SyncTasks.HealthCheck();
            //SyncTasks.SynchronizeDirectories();

            MainMenu.Run();

            SyncTasks.SaveConfigurations();
            LoggingConsole.Stop();
        }
    }
}


