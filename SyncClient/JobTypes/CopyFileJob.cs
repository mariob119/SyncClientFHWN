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
        public CopyFileJob(string fullPath, string sourcePath, string targetPath)
        {
            FullPath = fullPath;
            IsDirectory = false;
            SourcePath = sourcePath;
            TargetPath = targetPath;
        }

        public void DoJob()
        {
            string RelativeFilePath = FullPath.Replace(SourcePath, "");
            string TargetFilePath = TargetPath + RelativeFilePath;
            File.Copy(FullPath, TargetFilePath);
            Logger.LogCopyFile(FullPath, TargetFilePath);
        }
    }
}
