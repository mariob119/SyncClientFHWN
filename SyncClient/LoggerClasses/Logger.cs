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
        public static ConcurrentQueue<string> LastQueues;
        public static ConcurrentQueue<string> LastLogs;
        readonly static object _logger_lock = new object();
        readonly static object _log_lock = new object();
        public static void Init()
        {
            LogMessages = new ConcurrentQueue<string>();
            LastQueues = new ConcurrentQueue<string>();
            LastLogs = new ConcurrentQueue<string>();
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
                EnqueueLogMessage(Message);

                WriteMessagesToScreen();

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
        readonly static object _lock_screen_write = new object();
        public static void WriteMessagesToScreen()
        {
            lock (_lock_screen_write)
            {
                string PipeStringQueues = CreateQueueMessages(LastQueues);

                string PipeString = CreateLogMessages(LastLogs);

                using (NamedPipeServerStream namedPipeServer = new NamedPipeServerStream("LoggingPipe"))
                {
                    namedPipeServer.WaitForConnection();
                    byte[] bytes = Encoding.ASCII.GetBytes(PipeStringQueues + PipeString);
                    namedPipeServer.Write(bytes);
                }
            }
        }
        private static string CreateQueueMessages(ConcurrentQueue<string> Queues)
        {
            string PipeStringQueues = "\t\tQueue-States\n";
            IEnumerable<string> enumerableThing = LastQueues;
            foreach (string LastQueueState in enumerableThing.Reverse())
            {
                PipeStringQueues += "\n" + LastQueueState;
            }
            return PipeStringQueues;
        }
        private static string CreateLogMessages(ConcurrentQueue<string> LastLogMessages)
        {
            string PipeString = "\n\n\t\tLogs\n";

            foreach (string LogMessage in LastLogMessages)
            {
                PipeString += "\n" + LogMessage;
            }

            return PipeString;
        }
        public static void EnqueueQueueState(string Message)
        {
            if (LastQueues.Count() < SyncClient.Configuration.VisualisedQueues)
            {
                LastQueues.Enqueue(Message);
            }
            else
            {
                LastQueues.Enqueue(Message);
                LastQueues.TryDequeue(out string OutMessage);
            }
        }
        public static void EnqueueLogMessage(string Message)
        {
            if (LastLogs.Count() < SyncClient.Configuration.VisualisedLogs)
            {
                LastLogs.Enqueue(Message);
            }
            else
            {
                LastLogs.Enqueue(Message);
                LastLogs.TryDequeue(out string OutMessage);
            }
        }
        public static void LogStartMessage()
        {

        }
        private static string LogMessageFormated(string Message)
        {
            string MessageFormated = DateTime.Now.ToString("HH:mm:ss ON dd.MM.yyyy") + "\n";
            MessageFormated += Message;
            MessageFormated += "===============================================================";
            return MessageFormated;
        }
        public static void LogCopyFile(string SourceFileName, string TargetFileName)
        {
            string Message = $"Copied:\t\t{SourceFileName}\n";
            Message += $"To:\t\t{TargetFileName}\n";
            Log(LogMessageFormated(Message));
        }

        public static void LogDeleteFile(string TargetFileName)
        {
            string Message = $"Deleted:\t{TargetFileName}\n";
            Log(LogMessageFormated(Message));
        }

        public static void LogCreateDirectory(string TargetDirectoryPath)
        {
            string Message = $"Created:\t{TargetDirectoryPath}\n";
            Log(LogMessageFormated(Message));
        }
        public static void LogDeleteDirectory(string TargetDirectoryPath)
        {
            string Message = $"Deleted:\t{TargetDirectoryPath}\n";
            Log(LogMessageFormated(Message));
        }
        public static void LogFinishedBlockComparison(string SourceFilePath, string TargetFilePath)
        {
            string Message = $"Compared:\t{SourceFilePath}\n";
            Message += $"To:\t\t{TargetFilePath}\n";
            Log(LogMessageFormated(Message));
        }
    }
}
