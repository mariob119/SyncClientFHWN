using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SyncClient
{
    static class SyncJobs
    {
        public static List<SyncJobConfiguration>? SyncJobConfigurations { get; set; }
        private static ConcurrentQueue<JobInstruction>? JobInstructions;
        private static List<FileSystemWatcher>? FileSystemWatchers;
        private static Timer? ScanDirectoriesTimer;
        private static Configs Configurations;

        public static void InitSyncJobs()
        {
            SyncJobConfigurations = new List<SyncJobConfiguration>();
            JobInstructions = new ConcurrentQueue<JobInstruction>();
            FileSystemWatchers = new List<FileSystemWatcher>();
            Configurations = new Configs();
            //ScanDirectoriesTimer = new Timer(new TimerCallback(ScanDirectories), null, 1000, 1000);
        }
        public static void LoadConfigurations()
        {
            string JsonSettings = File.ReadAllText("syncjobs.json");
            SyncJobConfigurations = JsonSerializer.Deserialize<List<SyncJobConfiguration>>(JsonSettings)!;
            ScanDataAndGenerateJobs();
        }
        public static void SyncData(object state)
        {
            while (!JobInstructions.IsEmpty)
            {
                JobInstructions.TryDequeue(out JobInstruction? instruction);
                if (instruction != null)
                {
                    DoSyncInstruction(instruction);
                }
            }
        }
        public static void DoSyncInstruction(JobInstruction jobInstruction)
        {
            switch (jobInstruction.Instruction)
            {
                case "COPYFILE":
                    {
                        CopyFile(jobInstruction.SourceDirectory, jobInstruction.TargetDirectory, jobInstruction.FileName);
                        break;
                    }
                case "DELETEFILE":
                    {
                        DeleteFile(jobInstruction.TargetDirectory, jobInstruction.FileName);
                        break;
                    }
                case "COPYDIRECTORY":
                    {
                        CopyDirectory(jobInstruction.SyncJobSourceDirectory, jobInstruction.SourceDirectory, jobInstruction.TargetDirectory);
                        break;
                    }
            }
        }
        readonly static object _lock_copyfile = new object();
        public static void CopyFile(string SourceDirectory, string TargetDirectory, string FileName)
        {
            lock (_lock_copyfile)
            {
                string RelativeFileName = FileName.Replace(SourceDirectory, "");
                string SourceFilePath = SourceDirectory + RelativeFileName;
                string TargetFilePath = TargetDirectory + RelativeFileName;
                try
                {
                    File.Copy(SourceFilePath, TargetFilePath, true);
                }
                catch { Logger.log($"File {SourceFilePath} is being used by another Process!"); }
                Logger.log($"Copied {SourceFilePath} to {TargetFilePath}");
            }
        }
        public static object _lock_deletefile = new object();
        public static void DeleteFile(string TargetDirectory, string FileName)
        {
            lock (_lock_deletefile)
            {
                try
                {
                    File.Delete(Path.Combine(TargetDirectory, FileName));
                }
                catch { Logger.log($"File {FileName} is being used by another Process!"); }
            }
        }
        public static object _lock_copydirectory = new object();
        public static void CopyDirectory(string SyncJobSourceDirectory, string SourceDirectory, string TargetDirectory)
        {
            lock (_lock_copydirectory)
            {
                string DirectoryName = SourceDirectory.Replace(SyncJobSourceDirectory, "");
                Directory.CreateDirectory(TargetDirectory + DirectoryName);
            }
        }
        public static void SaveConfigurations()
        {
            string JsonSettings = JsonSerializer.Serialize(SyncJobConfigurations);
            File.WriteAllText("syncjobs.json", JsonSettings);
            string Configs = JsonSerializer.Serialize(Configurations);
            File.WriteAllText("configs.json", Configs);
        }
        public static void HealthCheck()
        {
            List<SyncJobConfiguration> RemoveSyncJob = new List<SyncJobConfiguration>();
            foreach (SyncJobConfiguration config in SyncJobConfigurations)
            {
                if (!Directory.Exists(config.GetSourceDirectory()))
                {
                    RemoveSyncJob.Add(config);
                }
            }
            foreach (SyncJobConfiguration config in RemoveSyncJob)
            {
                SyncJobConfigurations.Remove(config);
            }
        }
        public static object _lock_threadnumber = new object();
        public static void AddConfiguration(SyncJobConfiguration syncJobConfiguration)
        {
            SyncJobConfigurations.Add(syncJobConfiguration);
        }
        public static void ShowSyncDiretories()
        {
            if (SyncJobConfigurations.Count > 0)
            {
                foreach (SyncJobConfiguration syncJobConfiguration in SyncJobConfigurations)
                {
                    int SyncJobNumber = SyncJobConfigurations.IndexOf(syncJobConfiguration) + 1;
                    Console.WriteLine($"Sync Job Number {SyncJobNumber} || Root Directory:\t{syncJobConfiguration.GetSourceDirectory()}");
                }
            }
            else { Console.WriteLine("There are no Sync Jobs configured at thist time!"); }
        }
        public static int GetAmountOfSyncJobs()
        {
            return SyncJobConfigurations.Count;
        }
        public static void ShowDirectoryDetails(int FolderIndex)
        {
            if (FolderIndex >= 0 && FolderIndex < SyncJobConfigurations.Count)
            {
                Console.WriteLine($"Root Directory: {SyncJobConfigurations[FolderIndex].GetSourceDirectory()}\n");

                Console.WriteLine("Target Folders:");
                int i = 1;
                foreach (String TargetFolder in SyncJobConfigurations[FolderIndex].TargetDirectories)
                {
                    Console.WriteLine($"{i})\t{TargetFolder}");
                    i++;
                }

                Console.WriteLine($"\nSync just root directory: {SyncJobConfigurations[FolderIndex].GetSyncJustRootDirectoryAttribute()}");

                if (SyncJobConfigurations[FolderIndex].ExcludedDiretories.Count > 0)
                {
                    Console.WriteLine("\nExcluded Folders:");
                    i = 1;
                    foreach (String ExcludedFolder in SyncJobConfigurations[FolderIndex].ExcludedDiretories)
                    {
                        Console.WriteLine($"{i})\t{ExcludedFolder}");
                        i++;
                    }
                }
            }
            else { Console.WriteLine("\nFolder with this index does not exist!"); }
        }
        public static void ScanDataAndGenerateJobs()
        {
            foreach (SyncJobConfiguration syncJobConfiguration in SyncJobConfigurations)
            {
                FileSystemWatchers.Add(MyWatcherFactory(syncJobConfiguration));
            }
        }
        public static void ReloadFileSystemWatchers()
        {
            FileSystemWatchers.Clear();
            foreach (SyncJobConfiguration syncJobConfiguration in SyncJobConfigurations)
            {
                FileSystemWatchers.Add(MyWatcherFactory(syncJobConfiguration));
            }
        }
        public static FileSystemWatcher MyWatcherFactory(SyncJobConfiguration syncJobConfiguration)
        {
            FileSystemWatcher watcher = new FileSystemWatcher(syncJobConfiguration.SourceDiretory);
            watcher.NotifyFilter = NotifyFilters.Attributes
                                         | NotifyFilters.CreationTime
                                         | NotifyFilters.DirectoryName
                                         | NotifyFilters.FileName
                                         | NotifyFilters.LastAccess
                                         | NotifyFilters.LastWrite
                                         | NotifyFilters.Security
                                         | NotifyFilters.Size;
            //watcher.Changed += OnSourceChange;
            watcher.Created += OnSourceChange;
            watcher.Renamed += OnSourceChange;
            watcher.Deleted += OnSourceDeleted;
            watcher.IncludeSubdirectories = syncJobConfiguration.IncludeSubdiretories;
            watcher.EnableRaisingEvents = true;
            return watcher;
        }
        public static void OnSourceChange(object sender, FileSystemEventArgs e)
        {
            foreach (SyncJobConfiguration syncJobConfiguration in SyncJobConfigurations)
            {
                //if (syncJobConfiguration.RootFolder == Path.GetDirectoryName(e.FullPath))
                if (Path.GetDirectoryName(e.FullPath).Contains(syncJobConfiguration.SourceDiretory))
                {
                    foreach (string TargetDirectory in syncJobConfiguration.TargetDirectories)
                    {
                        foreach (string ExcludedDiretory in syncJobConfiguration.ExcludedDiretories)
                        {
                            if (!e.FullPath.Contains(ExcludedDiretory + "\\"))
                            {
                                FileAttributes attr = File.GetAttributes(e.FullPath);
                                if (attr.HasFlag(FileAttributes.Directory))
                                {
                                    JobInstruction jobInstruction = new JobInstruction(syncJobConfiguration.SourceDiretory, e.FullPath, TargetDirectory, true);
                                    if (!JobInstructions.Contains<JobInstruction>(jobInstruction))
                                    {
                                        JobInstructions.Enqueue(jobInstruction);
                                        //Console.WriteLine("Directory");
                                    }
                                }
                                else
                                {
                                    string RelativeFileName = e.FullPath.Replace(syncJobConfiguration.SourceDiretory, "");
                                    string NewFilePath = Path.Combine(TargetDirectory, RelativeFileName);
                                    if (!File.Exists(NewFilePath))
                                    {
                                        JobInstruction jobInstruction = new JobInstruction(syncJobConfiguration.SourceDiretory, TargetDirectory, e.FullPath);
                                        if (!JobInstructions.Contains<JobInstruction>(jobInstruction))
                                        {
                                            JobInstructions.Enqueue(jobInstruction);
                                            //Console.WriteLine("File");
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            ThreadPool.QueueUserWorkItem(SyncData);
        }
        public static void OnSourceDeleted(object sender, FileSystemEventArgs e)
        {
            foreach (SyncJobConfiguration syncJobConfiguration in SyncJobConfigurations)
            {
                if (syncJobConfiguration.SourceDiretory == Path.GetDirectoryName(e.FullPath))
                {
                    foreach (string TargetDirectory in syncJobConfiguration.TargetDirectories)
                    {
                        JobInstruction jobInstruction = new JobInstruction(TargetDirectory, Path.GetFileName(e.FullPath));
                        if (!JobInstructions.Contains<JobInstruction>(jobInstruction))
                        {
                            JobInstructions.Enqueue(jobInstruction);
                        }
                    }
                }
            }
            ThreadPool.QueueUserWorkItem(SyncData);
        }
        public static void ScanDirectories(object state)
        {
            foreach (SyncJobConfiguration syncJobConfiguration in SyncJobConfigurations)
            {
                foreach (string TargetDirectory in syncJobConfiguration.TargetDirectories)
                {
                    CompareTwoDiretories(syncJobConfiguration.SourceDiretory, TargetDirectory);
                }
            }
            ThreadPool.QueueUserWorkItem(SyncData);
        }
        public static void test(string Path)
        {
            string[] dirs = Directory.GetDirectories(Path, "*", SearchOption.AllDirectories);

            foreach (string dir in dirs)
            {
                //Console.WriteLine(dir);
            }
        }
        public static void CompareTwoDiretories(string SourcePath, string TargetPath)
        {
            DirectoryInfo dir1 = new DirectoryInfo(SourcePath);
            DirectoryInfo dir2 = new DirectoryInfo(TargetPath);

            IEnumerable<FileInfo> SourceList = dir1.GetFiles("*.*",
            SearchOption.TopDirectoryOnly);

            IEnumerable<FileInfo> TargetList = dir2.GetFiles("*.*",
            SearchOption.TopDirectoryOnly);

            bool Remove;
            foreach (FileInfo TargetFile in TargetList)
            {
                Remove = false;
                foreach (FileInfo SourceFile in SourceList)
                {
                    if (TargetFile.Name == SourceFile.Name)
                    {
                        Remove = false; break;
                    }
                    else
                    {
                        Remove = true;
                    }
                }
                if (Remove || SourceList.Count() == 0)
                {
                    JobInstruction jobInstruction = new JobInstruction(TargetPath, TargetFile.Name);
                    if (!JobInstructions.Contains<JobInstruction>(jobInstruction))
                    {
                        JobInstructions.Enqueue(jobInstruction);
                    }
                }
            }
            bool Add;
            foreach (FileInfo SourceFile in SourceList)
            {
                Add = false;
                foreach (FileInfo TargetFile in TargetList)
                {
                    if (SourceFile.Name == TargetFile.Name)
                    {
                        if (CompareTwoFiles(SourceFile.FullName, TargetFile.FullName))
                        {
                            Add = false; break;
                        }
                    }
                    else
                    {
                        Add = true;
                    }
                }
                if (Add || !TargetList.Any())
                {
                    JobInstruction jobInstruction = new JobInstruction(SourcePath, TargetPath, SourceFile.Name);
                    if (!JobInstructions.Contains<JobInstruction>(jobInstruction))
                    {
                        JobInstructions.Enqueue(jobInstruction);
                    }
                }
            }
        }
        public static bool CompareTwoFiles(string SourcePath, string TargetPath)
        {
            if (File.GetAttributes(SourcePath) == File.GetAttributes(TargetPath))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
