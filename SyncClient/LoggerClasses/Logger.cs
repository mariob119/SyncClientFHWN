using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncClient
{
    internal static class Logger
    {
        public static ConcurrentQueue<string> LogMessages;
        readonly static object _logger_lock = new object();
        readonly static object _log_lock = new object();
        public static void Init()
        {
            LogMessages = new ConcurrentQueue<string>();
        }
        public static void Log(string Message)
        {
            LogMessages.Enqueue(Message);
            if (Monitor.TryEnter(_log_lock))
            {
                Monitor.Exit(_log_lock);
                ThreadPool.QueueUserWorkItem(DequeueLog);
            }
        }
        private static void DequeueLog(object state)
        {
            lock (_log_lock)
            {
                while (!LogMessages.IsEmpty)
                {
                    LogMessages.TryDequeue(out string LogMessage);
                    ExecuteLog(LogMessage);
                }
            }
        }
        private static void ExecuteLog(string Message)
        {
            lock (_logger_lock)
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
