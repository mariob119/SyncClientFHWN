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
        private static Process p = new Process();
        public static void Run()
        {
            string now = Directory.GetCurrentDirectory();
            string new1 = now.Replace("SyncClient\\bin\\Debug\\net6.0", "");

            string ProcessPath = new1 + "Logging\\bin\\Debug\\net6.0\\Logging.exe";

            // Redirect the output stream of the child process.
            p.StartInfo.UseShellExecute = true;
            p.StartInfo.RedirectStandardOutput = false;
            //p.StartInfo.FileName = "C:\\Users\\mario\\source\\repos\\SyncClientFHWN\\Logging\\bin\\Debug\\net6.0\\Logging.exe";
            p.StartInfo.FileName = ProcessPath;
            p.Start();
        }

        public static void Stop()
        {
            p.Kill();
        }
    }
}

