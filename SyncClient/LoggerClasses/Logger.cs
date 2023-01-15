using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#pragma warning disable CS8602

namespace SyncClient
{
    internal static class Logger
    {
        private static ConcurrentQueue<string>? LogMessages;
        private static ConcurrentQueue<string>? LastQueues;
        private static ConcurrentQueue<string>? LastLogs;
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
                    LogMessages.TryDequeue(out string? LogMessage);
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
                    string LogFileDirectoryPath = String.Empty;
                    string LogFilePath = String.Empty;
                    if (Directory.Exists(SyncClient.Configuration.LogFilePath) && SyncClient.Configuration.LogToDifferentPath)
                    {
                        LogFileDirectoryPath = SyncClient.Configuration.LogFilePath;
                        LogFilePath = LogFileDirectoryPath + "\\" + SyncClient.Configuration.LogFileName + ".txt";
                    }
                    else
                    {
                        LogFilePath = SyncClient.Configuration.LogFileName + ".txt";
                    }
                    if (!File.Exists(LogFilePath))
                    {
                        File.Create(LogFilePath).Close();
                    }
                    double SizeInBytes = new FileInfo(LogFilePath).Length;
                    double FileSizeInMB = SizeInBytes / 1000000;
                    if (FileSizeInMB > SyncClient.Configuration.LogFileSize)
                    {
                        if (File.Exists(LogFilePath + ".bak"))
                        {
                            File.Delete(LogFilePath + ".bak");
                        }
                        File.Copy(LogFilePath, LogFilePath + ".bak");
                        File.Delete(LogFilePath);
                        File.WriteAllText(LogFilePath, "");
                    }
                    using (StreamWriter sw = File.AppendText(LogFilePath))
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
                ConcurrentQueue<string>? lastLogs = LastLogs;

                string PipeStringQueues = CreateQueueMessages(LastQueues);

                string PipeString = CreateLogMessages(lastLogs);

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
            IEnumerable<string> enumerableThing = Queues;
            StringBuilder PipeStringQueues = new StringBuilder();
            PipeStringQueues.Append("\t\tQueue-States\n");
            foreach (string LastQueueState in enumerableThing.Reverse())
            {
                PipeStringQueues.Append($"\n{LastQueueState}");
            }
            return PipeStringQueues.ToString();
        }
        private static string CreateLogMessages(ConcurrentQueue<string> Messages)
        {
            IEnumerable<string> enumerableThing = Messages;
            StringBuilder PipeString = new StringBuilder();
            PipeString.Append("\n\n\t\tLogs\n");

            foreach (string LogMessage in enumerableThing.Reverse())
            {
                PipeString.Append($"\n{LogMessage}");
            }

            return PipeString.ToString();
        }
        public static void EnqueueQueueState(string Message)
        {
            if (LastQueues.Count < SyncClient.Configuration.VisualisedQueues)
            {
                LastQueues.Enqueue(Message);
            }
            else
            {
                LastQueues.Enqueue(Message);
                LastQueues.TryDequeue(out string? OutMessage);
            }
        }
        public static void EnqueueLogMessage(string Message)
        {
            if (LastLogs.Count < SyncClient.Configuration.VisualisedLogs)
            {
                LastLogs.Enqueue(Message);
            }
            else
            {
                LastLogs.Enqueue(Message);
                LastLogs.TryDequeue(out string? OutMessage);
            }
        }
        public static void LogStartMessage()
        {
            string Message = $"\n\n+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++\n\n";
            Message += "Started\n";
            Message += DateTime.Now.ToString("HH:mm:ss ON dd.MM.yyyy");
            Message += "\n\n+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++\n\n";
            Log(Message);
        }
        public static void LogStopMessage()
        {
            string Message = $"\n\n+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++\n\n";
            Message += "Stopped\n";
            Message += DateTime.Now.ToString("HH:mm:ss ON dd.MM.yyyy");
            Message += "\n\n+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++\n\n";
            Log(Message);
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
