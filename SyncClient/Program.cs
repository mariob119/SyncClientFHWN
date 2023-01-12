using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.IO;

namespace SyncClient
{
    internal class Program
    {
        static void Main(string[] args)
        {
            SyncClient Jobs = new SyncClient();

            LoggingConsole.Run();

            SyncClient.LoadConfigurations();


            //SyncClient.HealthCheck();
            //SyncClient.SynchronizeDirectories();

            MainMenu.Run();

            SyncClient.SaveConfigurations();
            LoggingConsole.Stop();
        }
    }
}


