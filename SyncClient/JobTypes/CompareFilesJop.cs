using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncClient.JobTypes
{
    internal class CompareFilesJop : IJob
    {
        public string FullPath { get; set; }
        public bool IsDirectory { get; set; }
        public string SourcePath { get; set; }
        public string TargetPath { get; set; }
        public CompareFilesJop(string FullPath, string SourcePath, string TargetPath)
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

            FileInfo fileInfo = new FileInfo(FullPath);

            DoBlockSync(FullPath, TargetFilePath);

            Logger.EnqueueQueueState(GetDoneMessage());
            Logger.LogFinishedBlockComparison(FullPath, TargetFilePath);
        }
        public string GetQueuedMessage()
        {
            string Message = "Queued || Compare " + Functions.ShortenFileName(Path.GetFileName(FullPath));
            Message += " from " + Functions.ShortenPath(SourcePath) + " to " + Functions.ShortenPath(TargetPath);
            return Message;
        }
        public string GetProcessingMessage()
        {
            string Message = "Processing || Compare " + Functions.ShortenFileName(Path.GetFileName(FullPath));
            Message += " from " + Functions.ShortenPath(SourcePath) + " to " + Functions.ShortenPath(TargetPath);
            return Message;
        }
        public string GetDoneMessage()
        {
            string Message = "Done || Compare " + Functions.ShortenFileName(Path.GetFileName(FullPath));
            Message += " from " + Functions.ShortenPath(SourcePath) + " to " + Functions.ShortenPath(TargetPath);
            return Message;
        }
        public void DoBlockSync(string SourcePath, string TargetPath)
        {
            Dictionary<long, byte[]> keyValuePairs = new Dictionary<long, byte[]>();
            using (FileStream fs1r = File.OpenRead(SourcePath))
            using (FileStream fs2r = File.OpenRead(TargetPath))
            {
                Logger.EnqueueQueueState(GetProcessingMessage());
                Logger.WriteMessagesToScreen();
                long BlockSize;
                if (SyncClient.Configuration.BlockSyncBlockSize > fs1r.Length)
                {
                    BlockSize = fs1r.Length;
                }
                else
                {
                    BlockSize = SyncClient.Configuration.BlockSyncBlockSize;
                }

                byte[] ByteArrayFile1 = new byte[BlockSize];
                byte[] ByteArrayFile2 = new byte[BlockSize];

                long i = 0;

                while (i < fs1r.Length)
                {
                    fs1r.Position = i;
                    fs2r.Position = i;

                    fs1r.Read(ByteArrayFile1);
                    fs2r.Read(ByteArrayFile2);

                    if (ByteArrayFile1 != ByteArrayFile2)
                    {
                        keyValuePairs.Add(i, ByteArrayFile1);
                    }

                    i += SyncClient.Configuration.BlockSyncBlockSize;
                }

                int file1byte;
                int file2byte;
                if (fs1r.Position < fs1r.Length)
                {
                    do
                    {
                        // Read one byte from each file.
                        long Position = fs1r.Position;
                        file1byte = fs1r.ReadByte();
                        file2byte = fs2r.ReadByte();

                        if (file1byte != file2byte)
                        {
                            keyValuePairs.Add(Position, BitConverter.GetBytes(file1byte));
                        }
                        file2byte = file1byte;
                    }

                    while ((file1byte == file2byte) && (file1byte != -1));
                }
                fs1r.Dispose();
                fs2r.Dispose();
            }
            using (FileStream fs2w = File.OpenWrite(TargetPath))
            {
                foreach (var kv in keyValuePairs)
                {
                    fs2w.Position = kv.Key;
                    fs2w.Write(kv.Value);
                }
                fs2w.Dispose();
            }
        }
    }
}
