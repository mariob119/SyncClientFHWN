using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncClient
{
    internal class JobInstruction
    {
        public string Instruction { get; }
        public string SourceDirectory { get; }
        public string TargetDirectory { get; }
        public string FileName { get; }
        public string SyncJobSourceDirectory { get; }
        public bool IsDirectory { get; }
        public JobInstruction(string TargetDirectory, string FileName)
        {
            this.Instruction = "DELETEFILE";
            this.FileName = FileName;
            this.TargetDirectory = TargetDirectory;
        }
        public JobInstruction(string SourceDirectory, string TargetDirectory, string Filename)
        {
            this.Instruction = "COPYFILE";
            this.SourceDirectory = SourceDirectory;
            this.TargetDirectory = TargetDirectory;
            this.FileName = Filename;
        }
        public JobInstruction(string SyncJobSourceDirectory, string SourceDirectory, string TargetDirectory, bool IsDirectory)
        {
            this.Instruction = "COPYDIRECTORY";
            this.IsDirectory = IsDirectory;
            this.SourceDirectory = SourceDirectory;
            this.TargetDirectory = TargetDirectory;
            this.SyncJobSourceDirectory = SyncJobSourceDirectory;
        }
    }
}
