using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncClient
{
    internal class Configs
    {
        public bool WriteToLogFile { get; set; }
        public double LogFileSize { get; set; }
        public Configs()
        {
            this.WriteToLogFile = true;
            this.LogFileSize = 10;
        }
    }
}
