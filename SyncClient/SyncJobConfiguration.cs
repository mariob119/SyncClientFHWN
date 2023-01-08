using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncClient
{
    internal class SyncJobConfiguration
    {
        public string SourceDiretory { get; set; }
        public List<string> TargetDirectories { get; set; }
        public bool IncludeSubdiretories { set; get; }
        public List<string> ExcludedDiretories { set; get; }
        public SyncJobConfiguration()
        {
            TargetDirectories = new List<string>();
            IncludeSubdiretories = false;
            ExcludedDiretories = new List<string>();
        }

        public void SetSourceDirectory(string RootFolder)
        {
            this.SourceDiretory = RootFolder;
        }
        public string GetSourceDirectory() { return this.SourceDiretory; }
        public void AddTargetDirectory(string TargetDirectory)
        {
            TargetDirectories.Add(TargetDirectory);
        }
        public void SetIndludeSubdirectoriesAttribute(bool Input)
        {
            this.IncludeSubdiretories = Input;
        }
        public string GetSyncJustRootDirectoryAttribute()
        {
            if (IncludeSubdiretories) { return "true"; }
            else { return "false"; }
        }
        public void AddExcludedDirectory(string ExcludedDirectory)
        {
            ExcludedDiretories.Add(ExcludedDirectory);
        }
    }
}
