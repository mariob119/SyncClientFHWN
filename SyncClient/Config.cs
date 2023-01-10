using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncClient
{
    internal class Config
    {
        public bool WriteToLogFile { get; private set; }
        public double LogFileSize { get; private set; }
        public string LogFileName { get; private set; }
        public Config()
        {
            this.WriteToLogFile = true;
            this.LogFileSize = 10;
            this.LogFileName = "log";
        }
    }
}
