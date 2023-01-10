using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncClient
{
    internal class Config
    {
        public bool WriteToLogFile { get; set; }
        public double LogFileSize { get; set; }
        public string LogFileName { get; set; }
        public string LogFilePath { get; set; }
        public bool LogToDifferentPath { get; set; }
        public int ScanDiretoriesIntervalInMillis { get; set; }
        public bool ScanDirectoriesRepeatly { get; set; }
        public Config()
        {
            this.WriteToLogFile = true;
            this.LogFileSize = 10;
            this.LogFileName = "log";
            this.LogFilePath = "ApplicationDiretory";
            this.LogToDifferentPath = false;
            this.ScanDiretoriesIntervalInMillis = 500;
            this.ScanDirectoriesRepeatly = false;
        }
    }
}
