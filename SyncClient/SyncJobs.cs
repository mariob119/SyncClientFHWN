using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
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

        public static void Init()
        {
            SyncJobConfigurations = new List<SyncJobConfiguration>();
            JobInstructions = new ConcurrentQueue<JobInstruction>();
            FileSystemWatchers = new List<FileSystemWatcher>();
            Configurations = new Configs();
            //ScanDirectoriesTimer = new Timer(new TimerCallback(ScanDirectories), null, 1000, 1000);
        }
        public static void LoadConfigurations()
        {
            if (File.Exists("syncjobs.json"))
            {
                string JsonSettings = File.ReadAllText("syncjobs.json");
                SyncJobConfigurations = JsonSerializer.Deserialize<List<SyncJobConfiguration>>(JsonSettings)!;
            }
            if (File.Exists("configs.json"))
            {
                string JsonConfigurations = File.ReadAllText("configs.json");
                Configurations = JsonSerializer.Deserialize<Configs>(JsonConfigurations)!;
            }
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
                string SourceFilePath = SourceDirectory + "\\" + RelativeFileName;
                string TargetFilePath = TargetDirectory + "\\" + RelativeFileName;
                try
                {
                    File.Copy(SourceFilePath, TargetFilePath, true);
                    Logger.log($"Copied {SourceFilePath} to {TargetFilePath}");
                }
                catch
                {
                    CreateCopyFileQueue(SourceDirectory, TargetDirectory, FileName);
                    Logger.log($"File {SourceFilePath} is being used by another Process!");
                }
            }
        }
        public static object _lock_deletefile = new object();
        public static void DeleteFile(string TargetDirectory, string FileName)
        {
            lock (_lock_deletefile)
            {
                try
                {
                    Logger.log($"Deletet {TargetDirectory}{FileName}");
                    File.Delete(TargetDirectory + FileName);
                }
                catch
                {
                    Logger.log($"File {FileName} is being used by another Process!");
                }
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
            List<string> RemoveTargetDirectories = new List<string>();
            foreach (SyncJobConfiguration Config in SyncJobConfigurations)
            {
                if (!Directory.Exists(Config.GetSourceDirectory()))
                {
                    RemoveSyncJob.Add(Config);
                }
                foreach (string TargetDirectory in Config.TargetDirectories)
                {
                    if (!Directory.Exists(TargetDirectory))
                    {
                        RemoveTargetDirectories.Add(TargetDirectory);
                    }
                }
                foreach (string TargetDirectoryToRemove in RemoveTargetDirectories)
                {
                    Config.TargetDirectories.Remove(TargetDirectoryToRemove);
                }
                if (Config.TargetDirectories.Count == 0)
                {
                    RemoveSyncJob.Add(Config);
                }
            }
            foreach (SyncJobConfiguration Config in RemoveSyncJob)
            {
                SyncJobConfigurations.Remove(Config);
            }
            SyncJobs.SaveConfigurations();
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
        public static void StartSyncJobs()
        {
            foreach (SyncJobConfiguration syncJobConfiguration in SyncJobConfigurations)
            {
                FileSystemWatchers.Add(MyWatcherFactory(syncJobConfiguration));
            }
        }
        public static void RefreshSyncJobs()
        {
            FileSystemWatchers.Clear();
            HealthCheck();
            StartSyncJobs();
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
                                    CreateCopyFolderQueue(syncJobConfiguration.SourceDiretory, TargetDirectory, e.FullPath);
                                }
                                else
                                {
                                    CreateCopyFileQueue(syncJobConfiguration.SourceDiretory, TargetDirectory, e.FullPath);
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
                if (Path.GetDirectoryName(e.FullPath).Contains(syncJobConfiguration.SourceDiretory))
                {
                    foreach (string TargetDirectory in syncJobConfiguration.TargetDirectories)
                    {
                        Console.WriteLine(e.FullPath);
                        string RelativeFileName = e.FullPath.Replace(syncJobConfiguration.SourceDiretory, "");
                        JobInstruction jobInstruction = new JobInstruction(TargetDirectory, RelativeFileName);
                        if (!JobInstructions.Contains<JobInstruction>(jobInstruction))
                        {
                            Console.WriteLine(e.FullPath);
                            JobInstructions.Enqueue(jobInstruction);
                        }
                    }
                }
            }
            ThreadPool.QueueUserWorkItem(SyncData);
        }
        private static void CreateCopyFileQueue(string SyncJobSourceDirectory, string SyncJobTargetDirectory, string FilePath)
        {
            string RelativeFileName = FilePath.Replace(SyncJobSourceDirectory, "");
            string NewFilePath = Path.Combine(SyncJobTargetDirectory, RelativeFileName);
            if (!File.Exists(NewFilePath))
            {
                JobInstruction jobInstruction = new JobInstruction(SyncJobSourceDirectory, SyncJobTargetDirectory, FilePath);
                if (!JobInstructions.Contains<JobInstruction>(jobInstruction))
                {
                    JobInstructions.Enqueue(jobInstruction);
                    //Console.WriteLine("File");
                }
            }
        }
        private static void CreateCopyFolderQueue(string SyncJobSourceDirectory, string SyncJobTargetDirectory, string DirectoryPath)
        {
            JobInstruction jobInstruction = new JobInstruction(SyncJobSourceDirectory, DirectoryPath, SyncJobTargetDirectory, true);
            if (!JobInstructions.Contains<JobInstruction>(jobInstruction))
            {
                JobInstructions.Enqueue(jobInstruction);
                //Console.WriteLine("Directory");
            }
        }
        public static void ScanDirectories()
        {
            foreach (SyncJobConfiguration syncJobConfiguration in SyncJobConfigurations)
            {
                foreach (string TargetDirectory in syncJobConfiguration.TargetDirectories)
                {
                    CompareTwoDiretories(syncJobConfiguration.SourceDiretory, TargetDirectory);
                }

                string[] dirs = Directory.GetDirectories(syncJobConfiguration.SourceDiretory, "*", SearchOption.AllDirectories);
                if (syncJobConfiguration.IncludeSubdiretories)
                {
                    foreach (string TargetDirectory in syncJobConfiguration.TargetDirectories)
                    {
                        foreach (string dir in dirs)
                        {
                            foreach (string ExcludedDirectory in syncJobConfiguration.ExcludedDiretories)
                            {
                                if (!dir.Contains(ExcludedDirectory + "\\"))
                                {
                                    CreateCopyFolderQueue(syncJobConfiguration.SourceDiretory, TargetDirectory, dir);
                                }
                            }
                        }
                    }
                }
            }
            ThreadPool.QueueUserWorkItem(SyncData);
        }
        public static void test(string Path1)
        {
            string[] dirs = Directory.GetDirectories(Path1, "*", SearchOption.AllDirectories);

            foreach (string dir in dirs)
            {
                CreateCopyFolderQueue(Path1, "C:\\2", dir);
                Console.WriteLine(dir);
            }
            CompareTwoDiretories("C:\\1", "C:\\2");
            ThreadPool.QueueUserWorkItem(SyncData);
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
                if (true || !TargetList.Any())
                {
                    JobInstruction jobInstruction = new JobInstruction(SourcePath, TargetPath, SourceFile.Name);
                    if (!JobInstructions.Contains<JobInstruction>(jobInstruction))
                    {
                        JobInstructions.Enqueue(jobInstruction);
                    }
                }
            }
            ThreadPool.QueueUserWorkItem(SyncData);
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
