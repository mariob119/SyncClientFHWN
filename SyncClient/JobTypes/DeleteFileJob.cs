using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncClient.JobTypes
{
    internal class DeleteFileJob : IJob
    {
        public string FullPath { get; set; }
        public bool IsDirectory { get; set; }
        public string TargetPath { get; set; }
        public DeleteFileJob(string FullPath)
        {
            this.FullPath = FullPath;
        }
        public void DoJob()
        {
            if (File.Exists(FullPath))
            {
                Logger.EnqueueQueueState(GetProcessingMessage());
                FileInfo fileInfo = new FileInfo(FullPath);
                File.Delete(FullPath);
                Logger.LogDeleteFile(FullPath);
            }
        }
        public string GetQueuedMessage()
        {
            string Message = "Queued || Delete File " + FullPath;
            return Message;
        }
        public string GetProcessingMessage()
        {
            string Message = "Processing || Delete File " + FullPath;
            return Message;
        }
    }
}
