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
                Logger.WriteMessagesToScreen();

                FileInfo fileInfo = new FileInfo(FullPath);
                while (Functions.IsFileLocked(fileInfo)) { }
                File.Delete(FullPath);
                Logger.EnqueueQueueState(GetDoneMessage());
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
        public string GetDoneMessage()
        {
            string Message = "Done || Delete File " + FullPath;
            return Message;
        }
    }
}
