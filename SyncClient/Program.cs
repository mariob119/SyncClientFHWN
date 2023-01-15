using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.IO;

namespace SyncClient
{
    internal class Program
    {
        static void Main(string[] args)
        {
            SyncClient Client = new SyncClient();

            LoggingConsole.Run();
            Client.Init();
            Logger.LogStartMessage();

            MainMenu.Run();

            Logger.LogStopMessage();
            SyncClient.SaveEverything();
            LoggingConsole.Stop();

        }
    }
}


