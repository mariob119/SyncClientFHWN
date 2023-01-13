using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncClient.JobTypes
{
    internal class DeleteDirectory : IJob
    {
        public string FullPath { get; set; }
        public bool IsDirectory { get; set; }
        public DeleteDirectory(string TargetDirectoryPath)
        {
            this.FullPath = TargetDirectoryPath;
            this.IsDirectory = true;
        }

        public void DoJob()
        {
            if (Directory.Exists(FullPath))
            {
                Directory.Delete(FullPath, true);
                Logger.LogDeleteDirectory(FullPath);
            }
        }
    }
}
