using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncClient
{
    public static class LoggingConsole
    {
        private static Process LoggingConsoleWindow = new Process();
        public static void Run()
        {
            string CurrentDirectory = Directory.GetCurrentDirectory();
            string GetCommonSourceDirectory = CurrentDirectory.Replace("SyncClient\\bin\\Debug\\net6.0", "");

            string ProcessPath = GetCommonSourceDirectory + "Logging\\bin\\Debug\\net6.0\\Logging.exe";

            LoggingConsoleWindow.StartInfo.UseShellExecute = true;
            LoggingConsoleWindow.StartInfo.RedirectStandardOutput = false;
            LoggingConsoleWindow.StartInfo.FileName = ProcessPath;
            LoggingConsoleWindow.Start();
        }

        public static void Stop()
        {
            LoggingConsoleWindow.Kill();
        }
    }
}

