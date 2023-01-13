using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncClient.JobTypes
{
    internal class CopyFileJob : IJob
    {
        public string FullPath { get; set; }
        public bool IsDirectory { get; set; }
        public string SourcePath { get; set; }
        public string TargetPath { get; set; }
        public CopyFileJob(string FullPath, string SourcePath, string TargetPath)
        {
            this.FullPath = FullPath;
            IsDirectory = false;
            this.SourcePath = SourcePath;
            this.TargetPath = TargetPath;
        }

        public void DoJob()
        {
            string RelativeFilePath = FullPath.Replace(SourcePath, "");
            string TargetFilePath = TargetPath + RelativeFilePath;
            string ParentDirectoryPath = Path.GetDirectoryName(TargetFilePath);
            if (!File.Exists(TargetFilePath) && Directory.Exists(ParentDirectoryPath))
            {
                FileInfo fileInfo = new FileInfo(FullPath);
                while (Functions.IsFileLocked(fileInfo)) { }
                File.Copy(FullPath, TargetFilePath);
                Logger.LogCopyFile(FullPath, TargetFilePath);
            }
        }
    }
}
