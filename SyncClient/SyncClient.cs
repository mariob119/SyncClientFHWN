using SyncClient.ConfigModels;
using SyncClient.JobTypes;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

#pragma warning disable CS8602

namespace SyncClient
{
    class SyncClient
    {
        public static List<SyncTask>? Tasks { get; private set; }
        public static List<JobQueue>? Jobs { get; set; }
        public static ClientConfig? Configuration { get; set; }
        private static List<FileSystemWatcher>? FileSystemWatchers { get; set; }

        public SyncClient()
        {
            Tasks = new List<SyncTask>();
            Configuration = new ClientConfig();
            Jobs = new List<JobQueue>();
            FileSystemWatchers = new List<FileSystemWatcher>();

            Jobs.Add(new JobQueue("NoParallelSync"));
        }

        // Configurations

        public void Init()
        {
            Logger.Init();
            LoadSyncClientConfig();
            LoadTasks();
            HealthCheck();
            SaveEverything();
            RefreshLogicalDriveQueues();
            SynchronizeDirectories();
            RefreshFileSystemWatchers();
        }
        public static void SaveEverything()
        {
            SaveTasks();
            SaveSyncClientConfig();
        }
        private void LoadTasks()
        {
            if (File.Exists("syncjobs.json"))
            {
                string JsonSettings = File.ReadAllText("syncjobs.json");
                Tasks = JsonSerializer.Deserialize<List<SyncTask>>(JsonSettings)!;
            }
        }
        private void LoadSyncClientConfig()
        {
            if (File.Exists("configs.json"))
            {
                string JsonConfigurations = File.ReadAllText("configs.json");
                Configuration = JsonSerializer.Deserialize<ClientConfig>(JsonConfigurations)!;
            }
            if (!Directory.Exists(Configuration.LogFilePath))
            {
                Configuration.LogFilePath = "ApplicationDiretory";
            }
            if (Configuration.LogFilePath == "ApplicationDiretory")
            {
                Configuration.LogFilePath = Directory.GetCurrentDirectory();
            }
        }
        private static void SaveTasks()
        {
            if (!File.Exists("syncjobs.json"))
            {
                File.Create("syncjobs.json").Close();
            }

            FileInfo fileInfo = new FileInfo("syncjobs.json");
            if (!Functions.IsFileLocked(fileInfo))
            {
                string JsonSettings = JsonSerializer.Serialize(Tasks);
                File.WriteAllText("syncjobs.json", JsonSettings);
            }
        }
        private static void SaveSyncClientConfig()
        {
            if (!Configuration.LogToDifferentPath)
            {
                Configuration.LogFilePath = "ApplicationDiretory";
            }

            string Configs = JsonSerializer.Serialize(Configuration);
            File.WriteAllText("configs.json", Configs);
        }
        public static void HealthCheck()
        {
            List<SyncTask> RemoveSyncJob = new List<SyncTask>();
            List<string> RemoveTargetDirectories = new List<string>();
            foreach (SyncTask Config in Tasks)
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
            foreach (SyncTask Config in RemoveSyncJob)
            {
                Tasks.Remove(Config);
            }
            SaveEverything();
        }
        private static void SynchronizeDirectories()
        {
            Thread InitSync = new Thread(() => MirrorAllDirectoriesOfSyncJobs(Tasks));
            InitSync.Start();
            TryStartSyncing();
        }
        public static void RefreshTaskConfiguration()
        {
            HealthCheck();
            SaveTasks();
            RefreshLogicalDriveQueues();
            RefreshFileSystemWatchers();
            SynchronizeDirectories();
        }
        public static void AddConfiguration(SyncTask syncTask)
        {
            Tasks.Add(syncTask);
        }

        // SyncJob Informations

        public static void ShowSyncDiretories()
        {
            if (Tasks.Count > 0)
            {
                foreach (SyncTask syncTask in Tasks)
                {
                    int SyncJobNumber = Tasks.IndexOf(syncTask) + 1;
                    Console.WriteLine($"Sync Job Number {SyncJobNumber} || Root Directory:\t{syncTask.GetSourceDirectory()}");
                }
            }
            else { Console.WriteLine("There are no Sync SyncJobs configured at thist time!"); }
        }
        public static int GetAmountOfSyncJobs()
        {
            return Tasks.Count;
        }
        public static void ShowDirectoryDetails(int FolderIndex)
        {
            if (FolderIndex >= 0 && FolderIndex < Tasks.Count)
            {
                Console.WriteLine($"Root Directory: {Tasks[FolderIndex].GetSourceDirectory()}\n");

                Console.WriteLine("Target Folders:");
                int i = 1;
                foreach (String TargetFolder in Tasks[FolderIndex].TargetDirectories)
                {
                    Console.WriteLine($"{i})\t{TargetFolder}");
                    i++;
                }

                Console.WriteLine($"\nSync just root directory: {Tasks[FolderIndex].GetSyncJustRootDirectoryAttribute()}");

                if (Tasks[FolderIndex].ExcludedDiretories.Count > 0)
                {
                    Console.WriteLine("\nExcluded Folders:");
                    i = 1;
                    foreach (String ExcludedFolder in Tasks[FolderIndex].ExcludedDiretories)
                    {
                        Console.WriteLine($"{i})\t{ExcludedFolder}");
                        i++;
                    }
                }
            }
            else { Console.WriteLine("\nFolder with this index does not exist!"); }
        }

        // Queue operations

        public static void RefreshLogicalDriveQueues()
        {
            List<string> Paths = new List<string>();
            Tasks.Select(entry => entry.SourceDirectory).ToList().ForEach(entry => Paths.Add(entry));
            Tasks.ForEach(entry => entry.TargetDirectories.ForEach(entry => Paths.Add(entry)));
            List<string> UniqueDriveLetters = Paths.Select(entry => entry[0].ToString()).ToList().Distinct().ToList();
            List<string> DriveLettersWhichAreNotInQueues = UniqueDriveLetters.Where(entry => !Jobs.ToList().Any(entry2 => entry2.name == entry)).ToList();
            DriveLettersWhichAreNotInQueues.ForEach(entry => Jobs.Add(new JobQueue(entry)));
        }

        public static void TryStartSyncing()
        {
            Logger.WriteMessagesToScreen();
            foreach (var jobQueue in Jobs.Where(jobQueue => jobQueue.TryEnter()))
            {
                jobQueue.UnLock();
                Task.Run(() => DeQueue(jobQueue.name));
            }
        }

        public static void DeQueue(string Name)
        {
            JobQueue jobQueue = Jobs.First(entry => entry.name == Name);
            jobQueue.Lock();
            while (!jobQueue.SyncJobs.IsEmpty)
            {
                jobQueue.SyncJobs.TryDequeue(out IJob? Job);
                Job.DoJob();
            }
            jobQueue.UnLock();
        }

        // Create jobs and desicion making

        private static void CreateDeleteFileJob(string TargetFilePath)
        {
            string DiskLetter = TargetFilePath.ToString()[0].ToString();
            DeleteFileJob deleteFileJob = new DeleteFileJob(TargetFilePath);
            if (Configuration.ParallelSync && char.IsLetter(TargetFilePath[0]))
            {
                if (!Jobs.First(entry => entry.name.ToString() == DiskLetter).SyncJobs.Contains(deleteFileJob) && File.Exists(TargetFilePath))
                {
                    Logger.EnqueueQueueState(deleteFileJob.GetQueuedMessage());
                    Jobs.First(entry => entry.name.ToString() == DiskLetter).SyncJobs.Enqueue(deleteFileJob);
                }
            }
            else
            {
                if (!Jobs.First(entry => entry.name.ToString() == "NoParallelSync").SyncJobs.Contains(deleteFileJob) && File.Exists(TargetFilePath))
                {
                    Logger.EnqueueQueueState(deleteFileJob.GetQueuedMessage());
                    Jobs.First(entry => entry.name.ToString() == "NoParallelSync").SyncJobs.Enqueue(deleteFileJob);
                }
            }
        }
        public static void CreateCopyFileJob(string SourceFilePath, string SourcePath, string TargetPath)
        {
            string DiskLetter = TargetPath.ToString()[0].ToString();
            CopyFileJob copyFileJob = new CopyFileJob(SourceFilePath, SourcePath, TargetPath);
            if (Configuration.ParallelSync && char.IsLetter(TargetPath[0]))
            {
                if (!Jobs.First(entry => entry.name.ToString() == DiskLetter).SyncJobs.Contains(copyFileJob))
                {
                    Logger.EnqueueQueueState(copyFileJob.GetQueuedMessage());
                    Jobs.First(entry => entry.name.ToString() == DiskLetter).SyncJobs.Enqueue(copyFileJob);
                }
            }
            else
            {
                if (!Jobs.First(entry => entry.name.ToString() == "NoParallelSync").SyncJobs.Contains(copyFileJob))
                {
                    Logger.EnqueueQueueState(copyFileJob.GetQueuedMessage());
                    Jobs.First(entry => entry.name.ToString() == "NoParallelSync").SyncJobs.Enqueue(copyFileJob);
                }
            }
        }
        private static void CreateMakeDirectoryJob(string SourceDirectoryPath, string SourcePath, string TargetPath)
        {
            string DiskLetter = TargetPath.ToString()[0].ToString();
            CreateDirectory createDirectory = new CreateDirectory(SourceDirectoryPath, SourcePath, TargetPath);
            if (Configuration.ParallelSync && char.IsLetter(TargetPath[0]))
            {
                if (!Jobs.First(entry => entry.name.ToString() == DiskLetter).SyncJobs.Contains(createDirectory))
                {
                    Logger.EnqueueQueueState(createDirectory.GetQueuedMessage());
                    Jobs.First(entry => entry.name.ToString() == DiskLetter).SyncJobs.Enqueue(createDirectory);
                }
            }
            else
            {
                if (!Jobs.First(entry => entry.name.ToString() == "NoParallelSync").SyncJobs.Contains(createDirectory))
                {
                    Logger.EnqueueQueueState(createDirectory.GetQueuedMessage());
                    Jobs.First(entry => entry.name.ToString() == "NoParallelSync").SyncJobs.Enqueue(createDirectory);
                }
            }
        }
        private static void CreateDeleteDirectoryJob(string TargetDirectoryPath)
        {
            string DiskLetter = TargetDirectoryPath.ToString()[0].ToString();
            DeleteDirectory deleteDirectory = new DeleteDirectory(TargetDirectoryPath);
            if (Configuration.ParallelSync && char.IsLetter(TargetDirectoryPath[0]))
            {
                if (!Jobs.First(entry => entry.name.ToString() == DiskLetter).SyncJobs.Contains(deleteDirectory))
                {
                    Logger.EnqueueQueueState(deleteDirectory.GetQueuedMessage());
                    Jobs.First(entry => entry.name.ToString() == DiskLetter).SyncJobs.Enqueue(deleteDirectory);
                }
            }
            else
            {
                if (!Jobs.First(entry => entry.name.ToString() == "NoParallelSync").SyncJobs.Contains(deleteDirectory))
                {
                    Logger.EnqueueQueueState(deleteDirectory.GetQueuedMessage());
                    Jobs.First(entry => entry.name.ToString() == "NoParallelSync").SyncJobs.Enqueue(deleteDirectory);
                }
            }
        }
        private static void CreateCompareFilesJob(string FullPath, string SourcePath, string TargetPath)
        {
            string DiskLetter = TargetPath.ToString()[0].ToString();
            CompareFilesJop compareFilesJop = new CompareFilesJop(FullPath, SourcePath, TargetPath);
            if (Configuration.ParallelSync && char.IsLetter(TargetPath[0]))
            {
                if (!Jobs.First(entry => entry.name.ToString() == DiskLetter).SyncJobs.Contains(compareFilesJop))
                {
                    Logger.EnqueueQueueState(compareFilesJop.GetQueuedMessage());
                    Jobs.First(entry => entry.name.ToString() == DiskLetter).SyncJobs.Enqueue(compareFilesJop);
                }
            }
            else
            {
                if (!Jobs.First(entry => entry.name.ToString() == "NoParallelSync").SyncJobs.Contains(compareFilesJop))
                {
                    Logger.EnqueueQueueState(compareFilesJop.GetQueuedMessage());
                    Jobs.First(entry => entry.name.ToString() == "NoParallelSync").SyncJobs.Enqueue(compareFilesJop);
                }
            }
        }

        // FileSystemWatchers

        public static void RefreshFileSystemWatchers()
        {
            FileSystemWatchers.Clear();
            HealthCheck();
            GenerateFileSystemWatchers();
        }
        public static void GenerateFileSystemWatchers()
        {
            foreach (SyncTask syncTask in Tasks)
            {
                FileSystemWatchers.Add(MyWatcherFactory(syncTask));
            }
        }
        public static bool CheckIfJobsAreRunning()
        {
            bool Result = true;
            foreach (JobQueue jobQueue in Jobs)
            {
                if (jobQueue.TryEnter())
                {
                    jobQueue.UnLock();
                }
                else
                {
                    Result = false;
                    break;
                }
            }
            return Result;
        }
        public static FileSystemWatcher MyWatcherFactory(SyncTask syncTask)
        {
            FileSystemWatcher watcher = new FileSystemWatcher(syncTask.SourceDirectory);
            watcher.NotifyFilter = NotifyFilters.Attributes
                                         | NotifyFilters.CreationTime
                                         | NotifyFilters.DirectoryName
                                         | NotifyFilters.FileName
                                         | NotifyFilters.LastAccess
                                         | NotifyFilters.LastWrite
                                         | NotifyFilters.Security
                                         | NotifyFilters.Size;
            watcher.Changed += OnSourceChange;
            watcher.Created += OnSourceCreate;
            watcher.Renamed += OnSourceRename;
            watcher.Deleted += OnSourceDeleted;
            watcher.IncludeSubdirectories = syncTask.IncludeSubdiretories;
            watcher.EnableRaisingEvents = true;
            return watcher;
        }
        public static void OnSourceCreate(object sender, FileSystemEventArgs e)
        {
            try
            {
                foreach (SyncTask syncTask in Tasks)
                {
                    if (Path.GetDirectoryName(e.FullPath).Contains(syncTask.SourceDirectory))
                    {
                        foreach (string TargetDirectory in syncTask.TargetDirectories)
                        {
                            if (syncTask.ExcludedDiretories.Count > 0)
                            {
                                foreach (string ExcludedDiretory in syncTask.ExcludedDiretories)
                                {
                                    if (!e.FullPath.Contains(ExcludedDiretory + "\\"))
                                    {
                                        CreateCopyJobs(e.FullPath, syncTask.SourceDirectory, TargetDirectory);
                                    }
                                }
                            }
                            else
                            {
                                CreateCopyJobs(e.FullPath, syncTask.SourceDirectory, TargetDirectory);
                            }
                        }
                    }
                    TryStartSyncing();
                }
            }
            catch
            {
                TryStartSyncing();
                RefreshTaskConfiguration();
            }
        }
        public static void OnSourceRename(object sender, FileSystemEventArgs e)
        {
            try
            {
                SynchronizeDirectories();
            }
            catch
            {
                TryStartSyncing();
                RefreshTaskConfiguration();
            }
        }
        public static void OnSourceDeleted(object sender, FileSystemEventArgs e)
        {
            foreach (SyncTask syncJobConfiguration in Tasks)
            {
                if (Path.GetDirectoryName(e.FullPath).Contains(syncJobConfiguration.SourceDirectory))
                {
                    foreach (string TargetDirectory in syncJobConfiguration.TargetDirectories)
                    {
                        string RelativeFileName = e.FullPath.Replace(syncJobConfiguration.SourceDirectory, "");
                        string TargetPath = TargetDirectory + RelativeFileName;
                        if (!e.FullPath.Contains("."))
                        {
                            CreateDeleteDirectoryJob(TargetPath);
                        }
                        else
                        {
                            CreateDeleteFileJob(TargetPath);
                        }
                    }
                }
            }
            TryStartSyncing();
        }
        public static void CreateCopyJobs(string FullPath, string SourceDirectory, string TargetDirectory)
        {
            FileAttributes attr = File.GetAttributes(FullPath);
            if (attr.HasFlag(FileAttributes.Directory))
            {
                CreateMakeDirectoryJob(FullPath, SourceDirectory, TargetDirectory);
            }
            else
            {
                CreateCopyFileJob(FullPath, SourceDirectory, TargetDirectory);
            }
        }
        public static void OnSourceChange(object sender, FileSystemEventArgs e)
        {
            try
            {
                foreach (SyncTask syncTask in Tasks)
                {
                    if (Path.GetDirectoryName(e.FullPath).Contains(syncTask.SourceDirectory))
                    {
                        foreach (string TargetDirectory in syncTask.TargetDirectories)
                        {
                            if (syncTask.ExcludedDiretories.Count > 0)
                            {
                                foreach (string ExcludedDiretory in syncTask.ExcludedDiretories)
                                {
                                    if (!e.FullPath.Contains(ExcludedDiretory + "\\"))
                                    {
                                        string TargetFilePath = TargetDirectory + e.FullPath.Replace(syncTask.SourceDirectory, "");
                                        if (File.Exists(TargetFilePath))
                                        {
                                            FileInfo fileInfoSource = new FileInfo(e.FullPath);
                                            FileInfo fileInfoTarget = new FileInfo(TargetDirectory);
                                            if (fileInfoSource.Length != fileInfoTarget.Length)
                                            {
                                                CreateDeleteFileJob(TargetFilePath);
                                                CreateCopyFileJob(e.FullPath, syncTask.SourceDirectory, TargetDirectory);
                                            }
                                            if (fileInfoSource.Length == fileInfoTarget.Length)
                                            {
                                                CreateCompareFilesJob(e.FullPath, syncTask.SourceDirectory, TargetDirectory);
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                string TargetFilePath = TargetDirectory + e.FullPath.Replace(syncTask.SourceDirectory, "");
                                if (File.Exists(TargetFilePath))
                                {
                                    FileInfo fileInfoSource = new FileInfo(e.FullPath);
                                    FileInfo fileInfoTarget = new FileInfo(TargetDirectory);
                                    if (fileInfoSource.Length != fileInfoTarget.Length)
                                    {
                                        CreateDeleteFileJob(TargetFilePath);
                                        CreateCopyFileJob(e.FullPath, syncTask.SourceDirectory, TargetDirectory);
                                    }
                                    if (fileInfoSource.Length == fileInfoTarget.Length)
                                    {
                                        CreateCompareFilesJob(e.FullPath, syncTask.SourceDirectory, TargetDirectory);
                                    }
                                }
                            }
                        }
                    }
                    TryStartSyncing();
                }
            }
            catch
            {
                TryStartSyncing();
            }
        }
        // Mirror all Directories of SyncClient with Configuration

        public static void MirrorAllDirectoriesOfSyncJobs(List<SyncTask> syncJobConfigurations)
        {
            foreach (SyncTask syncJobConfiguration in syncJobConfigurations)
            {
                foreach (string TargetDirectory in syncJobConfiguration.TargetDirectories)
                {
                    MirrorFilesFromSourceToTarget(syncJobConfiguration.SourceDirectory, TargetDirectory, false, false);

                    if (syncJobConfiguration.IncludeSubdiretories)
                    {
                        string[] SourceDirectories = Directory.GetDirectories(syncJobConfiguration.SourceDirectory, "*", SearchOption.AllDirectories);
                        string[] DirectoriesInTargetDirectory = Directory.GetDirectories(TargetDirectory, "*", SearchOption.AllDirectories);

                        foreach (string SourceDirectory in SourceDirectories)
                        {
                            if (!syncJobConfiguration.ExcludedDiretories.Contains(SourceDirectory))
                            {
                                string SourceDirectoryName = SourceDirectory.Replace(syncJobConfiguration.SourceDirectory, "");
                                string SubTargetDirectory = TargetDirectory + SourceDirectoryName;

                                if (!Directory.Exists(SubTargetDirectory))
                                {
                                    CreateMakeDirectoryJob(SourceDirectory, syncJobConfiguration.SourceDirectory, TargetDirectory);
                                    TryStartSyncing();
                                }

                                while (!Directory.Exists(SubTargetDirectory)) ;

                                MirrorDirectoryContentFromSourceToTarget(SourceDirectory, SubTargetDirectory, false);
                            }
                        }
                        foreach (string TargetDir in DirectoriesInTargetDirectory)
                        {
                            if (!SourceDirectories.Contains(TargetDir.Replace(TargetDirectory, syncJobConfiguration.SourceDirectory)))
                            {
                                DeleteAllFilesFromDirectory(TargetDir);
                            }
                        }
                    }
                    DeleteAllEmptyDirectoriesWhichAreNotInSource(syncJobConfiguration.SourceDirectory, TargetDirectory, false);
                    TryStartSyncing();
                }
            }
        }
        public static void MirrorDirectoryContentFromSourceToTarget(string SourcePath, string TargetPath, bool JobWorker)
        {
            string[] Directories = Directory.GetDirectories(SourcePath, "*", SearchOption.AllDirectories);

            foreach (string Dir in Directories)
            {
                CreateMakeDirectoryJob(Dir, SourcePath, TargetPath);
            }

            MirrorFilesFromSourceToTarget(SourcePath, TargetPath, true, false);

            DeleteAllEmptyDirectoriesWhichAreNotInSource(SourcePath, TargetPath, false);

            if (JobWorker)
            {
                TryStartSyncing();
            }
        }
        public static void DeleteAllEmptyDirectoriesWhichAreNotInSource(string SourcePath, string TargetPath, bool JobWorker)
        {
            string[] SourceDirectories = Directory.GetDirectories(SourcePath, "*", SearchOption.AllDirectories);
            string[] TargetDirectories = Directory.GetDirectories(TargetPath, "*", SearchOption.AllDirectories);

            bool Remove = false;
            foreach (string TargetDirectory in TargetDirectories)
            {
                foreach (string SourceDirectory in SourceDirectories)
                {
                    if (TargetDirectory.Replace(TargetPath, "") == SourceDirectory.Replace(SourcePath, ""))
                    {
                        Remove = false; break;
                    }
                    else
                    {
                        Remove = true;
                    }
                }
                if (Remove || SourceDirectories.Count() == 0)
                {
                    CreateDeleteDirectoryJob(TargetDirectory);
                }
            }
            if (JobWorker)
            {
                TryStartSyncing();
            }
        }
        public static void MirrorFilesFromSourceToTarget(string SourcePath, string TargetPath, bool IncludeSubDirectories, bool JobWorker)
        {
            DirectoryInfo dir1 = new DirectoryInfo(SourcePath);
            DirectoryInfo dir2 = new DirectoryInfo(TargetPath);

            IEnumerable<FileInfo> SourceList = GetFilesOfDirectory(dir1, IncludeSubDirectories);

            IEnumerable<FileInfo> TargetList = GetFilesOfDirectory(dir2, IncludeSubDirectories);

            bool Remove = false;
            foreach (FileInfo TargetFile in TargetList)
            {
                foreach (FileInfo SourceFile in SourceList)
                {
                    if (TargetFile.FullName.Replace(TargetPath, "") == SourceFile.FullName.Replace(SourcePath, ""))
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
                    CreateDeleteFileJob(TargetFile.FullName);
                }
            }
            bool Add = true;
            foreach (FileInfo SourceFile in SourceList)
            {
                foreach (FileInfo TargetFile in TargetList)
                {
                    if (SourceFile.FullName.Replace(SourcePath, "") == TargetFile.FullName.Replace(TargetPath, ""))
                    {
                        SetAttributes(SourceFile.FullName, TargetFile.FullName);

                        if (SourceFile.Length == TargetFile.Length)
                        {
                            if (SourceFile.Length > SyncClient.Configuration.BlockSyncFileSize * 1000000)
                            {
                                CreateCompareFilesJob(SourceFile.FullName, SourcePath, TargetPath);
                            }
                        }
                        else
                        {
                            string TargetFilePath = TargetPath + SourceFile.FullName.Replace(SourcePath, "");
                            CreateDeleteFileJob(TargetFilePath);
                            CreateCopyFileJob(SourceFile.FullName, SourcePath, TargetPath);
                        }

                        Add = false; break;
                    }
                    else
                    {
                        Add = true;
                    }
                }
                if (Add || TargetList.Count() == 0)
                {
                    CreateCopyFileJob(SourceFile.FullName, SourcePath, TargetPath);
                }
            }
            if (JobWorker) { TryStartSyncing(); }
        }
        public static void DeleteAllFilesFromDirectory(string DirectoryPath)
        {
            DirectoryInfo Dir = new DirectoryInfo(DirectoryPath);

            IEnumerable<FileInfo> FileList = GetFilesOfDirectory(Dir, true);

            foreach (FileInfo DirectoryFile in FileList)
            {
                CreateDeleteFileJob(DirectoryFile.FullName);
            }
        }
        public static IEnumerable<FileInfo> GetFilesOfDirectory(DirectoryInfo Dir, bool IncludeSubDiretories)
        {
            if (IncludeSubDiretories)
            {
                return Dir.GetFiles("*.*", SearchOption.AllDirectories);
            }
            else
            {
                return Dir.GetFiles("*.*", SearchOption.TopDirectoryOnly);
            }
        }
        public static void SetAttributes(string SourcePath, string TargetPath)
        {
            if (File.GetAttributes(SourcePath) != File.GetAttributes(TargetPath))
            {
                File.SetAttributes(TargetPath, File.GetAttributes(SourcePath));
            }
        }
    }
}

