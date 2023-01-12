using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncClient
{
    internal static class Logger
    {
        readonly static object _lock = new object();
        public static void Log(string Message)
        {
            lock (_lock)
            {
                using (NamedPipeServerStream namedPipeServer = new NamedPipeServerStream("LoggingPipe"))
                {
                    namedPipeServer.WaitForConnection();
                    byte[] bytes = Encoding.ASCII.GetBytes(Message);
                    namedPipeServer.Write(bytes);
                }
                if (SyncClient.Configuration.WriteToLogFile)
                {
                    if (!File.Exists("log.txt"))
                    {
                        File.Create("log.txt");
                    }
                    double SizeInBytes = new FileInfo("log.txt").Length;
                    double FileSizeInMB = SizeInBytes / 1000000;
                    if (FileSizeInMB > SyncClient.Configuration.LogFileSize)
                    {
                        if (File.Exists("log.txt.bak"))
                        {
                            File.Delete("log.txt.bak");
                        }
                        File.Copy("log.txt", "log.txt.bak");
                        File.Delete("log.txt");
                        File.WriteAllText("log.txt", "");
                    }
                    using (StreamWriter sw = File.AppendText("log.txt"))
                    {
                        sw.WriteLine(Message);
                    }
                }
            }
        }
    }
}
