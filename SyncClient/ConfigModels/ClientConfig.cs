using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncClient.ConfigModels
{
    internal class ClientConfig
    {
        public bool WriteToLogFile { get; set; }
        public double LogFileSize { get; set; }
        public string LogFileName { get; set; }
        public string LogFilePath { get; set; }
        public bool LogToDifferentPath { get; set; }
        public bool ParallelSync { get; set; }
        public int VisualisedQueues { get; set; }
        public int VisualisedLogs { get; set; }
        public long BlockSyncFileSize { get; set; }
        public long BlockSyncBlockSize { get; set; }
        public ClientConfig()
        {
            WriteToLogFile = true;
            LogFileSize = 10;
            LogFileName = "log";
            LogFilePath = "ApplicationDiretory";
            LogToDifferentPath = false;
            ParallelSync = true;
            VisualisedQueues = 10;
            VisualisedLogs = 5;
            BlockSyncFileSize = 5;
            BlockSyncBlockSize = 10;
        }
    }
}
