using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncClient
{
    public interface IJob
    {
        public string FullPath { get; set; }
        public bool IsDirectory { get; set; }
        public void DoJob();
    }
}
