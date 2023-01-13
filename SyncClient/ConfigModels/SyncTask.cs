using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncClient.ConfigModels
{
    internal class SyncTask
    {
        public string SourceDirectory { get; set; }
        public List<string> TargetDirectories { get; set; }
        public bool IncludeSubdiretories { set; get; }
        public List<string> ExcludedDiretories { set; get; }
        public SyncTask()
        {
            TargetDirectories = new List<string>();
            IncludeSubdiretories = false;
            ExcludedDiretories = new List<string>();
        }

        public void SetSourceDirectory(string RootFolder)
        {
            SourceDirectory = RootFolder;
        }
        public string GetSourceDirectory() { return SourceDirectory; }
        public void AddTargetDirectory(string TargetDirectory)
        {
            TargetDirectories.Add(TargetDirectory);
        }
        public void SetIndludeSubdirectoriesAttribute(bool Input)
        {
            IncludeSubdiretories = Input;
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
