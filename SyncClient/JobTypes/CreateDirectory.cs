using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncClient.JobTypes
{
    internal class CreateDirectory : IJob
    {
        public string FullPath { get; set; }
        public bool IsDirectory { get; set; }
        public string SourcePath { get; set; }
        public string Targetpath { get; set; }
        public CreateDirectory(string SourceDirectoryPath, string SourcePath, string TargetPath)
        {
            this.FullPath = SourceDirectoryPath;
            this.IsDirectory = true;
            this.SourcePath = SourcePath;
            this.Targetpath = TargetPath;
        }

        public void DoJob()
        {
            string DirectoryName = FullPath.Replace(SourcePath, "");
            string TargetDirectory = Targetpath + DirectoryName;
            if (!Directory.Exists(TargetDirectory))
            {
                Directory.CreateDirectory(TargetDirectory);
                Logger.LogCreateDirectory(TargetDirectory);
            }
        }
    }
}
