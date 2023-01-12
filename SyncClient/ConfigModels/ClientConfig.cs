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
        public int ScanDiretoriesIntervalInMillis { get; set; }
        public bool ScanDirectoriesRepeatly { get; set; }
        public ClientConfig()
        {
            WriteToLogFile = true;
            LogFileSize = 10;
            LogFileName = "log";
            LogFilePath = "ApplicationDiretory";
            LogToDifferentPath = false;
            ScanDiretoriesIntervalInMillis = 500;
            ScanDirectoriesRepeatly = false;
        }
    }
}
