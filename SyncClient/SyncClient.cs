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

namespace SyncClient
{
    class SyncClient
    {
        public static List<SyncTask>? Tasks { get; private set; }

        private static ConcurrentQueue<JobInstruction>? JobInstructions;
        private static List<FileSystemWatcher>? FileSystemWatchers;
        private static Timer? ScanDirectoriesTimer;
        public static ClientConfig? Configurations { get; set; }




        public Dictionary<string, ConcurrentQueue<IJob>>? Jobs;

        private List<char> LocicalDrives;


        public SyncClient()
        {
            Jobs = new Dictionary<string, ConcurrentQueue<IJob>>();
            LocicalDrives = new List<char>();

            Jobs.Add("NoParallelSync", new ConcurrentQueue<IJob>());
        }

        public static void Init()
        {
            Tasks = new List<SyncTask>();
            JobInstructions = new ConcurrentQueue<JobInstruction>();
            FileSystemWatchers = new List<FileSystemWatcher>();
            Configurations = new ClientConfig();

        }

        public static void SynchronizeDirectories()
        {
            Thread InitSync = new Thread(ScanDirectories);
            InitSync.Start();
            StartJobWorker();
        }
        public static void AddConfiguration(SyncTask syncJobConfiguration)
        {
            Tasks.Add(syncJobConfiguration);
        }



        public void GetLogicalDrives()
        {
            List<SyncTask> SyncConfigs = SyncClient.Tasks;
            List<char> DriveLetters = new List<char>();
            SyncConfigs.FindAll(entry => Char.IsLetter(entry.SourceDiretory[0])).ToList().ForEach(entry => DriveLetters.Add(entry.SourceDiretory[0]));
            SyncConfigs.ForEach(entry => entry.TargetDirectories.FindAll(entry => Char.IsLetter(entry[0])).ToList().ForEach(entry => DriveLetters.Add(entry[0])));
            LocicalDrives = DriveLetters.Distinct().ToList();
        }
        public void RegenerateLogicalDriveQueues()
        {
            List<string> Paths = new List<string>();
            SyncClient.Tasks.Select(entry => entry.SourceDiretory).ToList().ForEach(entry => Paths.Add(entry));
            SyncClient.Tasks.ForEach(entry => entry.TargetDirectories.ForEach(entry => Paths.Add(entry)));
            List<string> UniqueDriveLetters = Paths.Select(entry => entry[0].ToString()).ToList().Distinct().ToList();
            List<string> DriveLettersWhichAreNotInQueues = UniqueDriveLetters.Where(entry => !Jobs.ToList().Any(entry2 => entry2.Key == entry)).ToList();
            DriveLettersWhichAreNotInQueues.ForEach(entry => Jobs.Add(entry, new ConcurrentQueue<IJob>()));
        }




        // SyncJob Informations

        public static void ShowSyncDiretories()
        {
            if (Tasks.Count > 0)
            {
                foreach (SyncTask syncJobConfiguration in Tasks)
                {
                    int SyncJobNumber = Tasks.IndexOf(syncJobConfiguration) + 1;
                    Console.WriteLine($"Sync Job Number {SyncJobNumber} || Root Directory:\t{syncJobConfiguration.GetSourceDirectory()}");
                }
            }
            else { Console.WriteLine("There are no Sync Jobs configured at thist time!"); }
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

        // SyncJob Operations

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
        readonly static object _sync_locker = new object();
        public static void DoSyncInstruction(JobInstruction jobInstruction)
        {
            lock (_sync_locker)
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
                    case "DELETEDIRECTORY":
                        {
                            DeleteDirectory(jobInstruction.TargetDirectory);
                            break;
                        }
                }
            }
        }
        public static void ScanDirectories(object state)
        {
            MirrorAllDirectoriesOfSyncJobs(Tasks);
        }
        public static void StartJobWorker()
        {
            ThreadPool.QueueUserWorkItem(SyncData);
        }
        public static void StartSyncJobs()
        {
            foreach (SyncTask syncJobConfiguration in Tasks)
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
        public static bool CheckIfJobsAreRunning()
        {
            return Monitor.TryEnter(_sync_locker);
        }

        // Configuration Operations

        public static void LoadConfigurations()
        {
            if (File.Exists("syncjobs.json"))
            {
                string JsonSettings = File.ReadAllText("syncjobs.json");
                Tasks = JsonSerializer.Deserialize<List<SyncTask>>(JsonSettings)!;
            }
            if (File.Exists("configs.json"))
            {
                string JsonConfigurations = File.ReadAllText("configs.json");
                Configurations = JsonSerializer.Deserialize<ClientConfig>(JsonConfigurations)!;
            }
            if (!Directory.Exists(Configurations.LogFilePath))
            {
                Configurations.LogFilePath = "ApplicationDiretory";
            }
            if(Configurations.LogFilePath == "ApplicationDiretory")
            {
                Configurations.LogFilePath = Directory.GetCurrentDirectory();
            }
            if (Configurations.ScanDirectoriesRepeatly)
            {
                ScanDirectoriesTimer = new Timer(new TimerCallback(ScanDirectories), null, 1000, Configurations.ScanDiretoriesIntervalInMillis);
            }
        }
        public static void SaveConfigurations()
        {
            string JsonSettings = JsonSerializer.Serialize(Tasks);
            File.WriteAllText("syncjobs.json", JsonSettings);

            if (!Configurations.LogToDifferentPath)
            {
                Configurations.LogFilePath = "ApplicationDiretory";
            }

            string Configs = JsonSerializer.Serialize(Configurations);
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
            SyncClient.SaveConfigurations();
        }

        // File and Folder Operations

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
                    Logger.Log(CopyFileLogMessage(SourceFilePath, TargetFilePath));
                }
                catch
                {
                    Logger.Log($"File {SourceFilePath} is being used by another Process!");
                }
            }
        }
        readonly static object _lock_deletefile = new object();
        public static void DeleteFile(string TargetDirectory, string RelativeFileName)
        {
            lock (_lock_deletefile)
            {
                string FullFilePath = TargetDirectory + "\\" + RelativeFileName;
                try
                {
                    File.Delete(FullFilePath);
                    Logger.Log(DeleteFileLogMessage(FullFilePath));
                }
                catch
                {
                    Logger.Log($"File {FullFilePath} is being used by another Process!");
                }
            }
        }
        readonly static object _lock_copydirectory = new object();
        public static void CopyDirectory(string SyncJobSourceDirectory, string SourceDirectory, string TargetDirectory)
        {
            lock (_lock_copydirectory)
            {
                string DirectoryName = SourceDirectory.Replace(SyncJobSourceDirectory, "");
                Directory.CreateDirectory(TargetDirectory + DirectoryName);
                Logger.Log(CreateDirectoryLogMessage(TargetDirectory + DirectoryName));
            }
        }
        readonly static object _lock_delete_directory = new object();
        public static void DeleteDirectory(string DirectoryPath)
        {
            lock (_lock_delete_directory)
            {
                if (Directory.Exists(DirectoryPath))
                {
                    Directory.Delete(DirectoryPath, true);
                    Logger.Log(DeleteDirectoryLogMessage(DirectoryPath));
                }
            }
        }

        // Create File and Folder Jobs

        private static void CreateDeleteFileQueue(string TargetDirectory, string RelativeFileName)
        {
            JobInstruction jobInstruction = new JobInstruction(TargetDirectory, RelativeFileName);
            if (!JobInstructions.Contains<JobInstruction>(jobInstruction))
            {
                JobInstructions.Enqueue(jobInstruction);
            }
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
        private static void CreateCopyDirectoryQueue(string SyncJobSourceDirectory, string SyncJobTargetDirectory, string DirectoryPath)
        {
            JobInstruction jobInstruction = new JobInstruction(SyncJobSourceDirectory, DirectoryPath, SyncJobTargetDirectory, true);
            if (!JobInstructions.Contains<JobInstruction>(jobInstruction))
            {
                JobInstructions.Enqueue(jobInstruction);
            }
        }

        // Log Messages

        private static string LogMessageFormated(string Message)
        {
            string MessageFormated = DateTime.Now.ToString("HH:mm:ss ON dd.MM.yyyy") + "\n";
            MessageFormated += Message;
            MessageFormated += "===============================================================";
            return MessageFormated;
        }
        private static string CopyFileLogMessage(string SourceFileName, string TargetFileName)
        {
            string Message = $"Copied:\t\t{SourceFileName}\n";
            Message += $"To:\t\t{TargetFileName}\n";
            return LogMessageFormated(Message);
        }

        private static string DeleteFileLogMessage(string TargetFileName)
        {
            string Message = $"Deleted:\t{TargetFileName}\n";
            return LogMessageFormated(Message);
        }

        private static string CreateDirectoryLogMessage(string TargetDirectoryPath)
        {
            string Message = $"Created:\t{TargetDirectoryPath}\n";
            return LogMessageFormated(Message);
        }
        private static string DeleteDirectoryLogMessage(string TargetDirectoryPath)
        {
            string Message = $"Deleted:\t{TargetDirectoryPath}\n";
            return LogMessageFormated(Message);

        }

        // File System Watchers

        public static FileSystemWatcher MyWatcherFactory(SyncTask syncJobConfiguration)
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
            foreach (SyncTask syncJobConfiguration in Tasks)
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
                                    CreateCopyDirectoryQueue(syncJobConfiguration.SourceDiretory, TargetDirectory, e.FullPath);
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
            foreach (SyncTask syncJobConfiguration in Tasks)
            {
                if (Path.GetDirectoryName(e.FullPath).Contains(syncJobConfiguration.SourceDiretory))
                {
                    foreach (string TargetDirectory in syncJobConfiguration.TargetDirectories)
                    {
                        string RelativeFileName = e.FullPath.Replace(syncJobConfiguration.SourceDiretory, "");
                        //JobInstruction jobInstruction = new JobInstruction(TargetDirectory, RelativeFileName);
                        //if (!JobInstructions.Contains<JobInstruction>(jobInstruction))
                        //{
                        //    Console.WriteLine(e.FullPath);
                        //    JobInstructions.Enqueue(jobInstruction);
                        //}
                        CreateDeleteFileQueue(TargetDirectory, RelativeFileName);
                    }
                }
            }
            ThreadPool.QueueUserWorkItem(SyncData);
        }

        // Mirror all Directories of SyncClient with Configurations

        public static void MirrorAllDirectoriesOfSyncJobs(List<SyncTask> syncJobConfigurations)
        {
            foreach (SyncTask syncJobConfiguration in syncJobConfigurations)
            {
                foreach (string TargetDirectory in syncJobConfiguration.TargetDirectories)
                {
                    MirrorFilesFromSourceToTarget(syncJobConfiguration.SourceDiretory, TargetDirectory, false, false);

                    if (syncJobConfiguration.IncludeSubdiretories)
                    {
                        string[] SourceDirectories = Directory.GetDirectories(syncJobConfiguration.SourceDiretory, "*", SearchOption.AllDirectories);
                        string[] DirectoriesInTargetDirectory = Directory.GetDirectories(TargetDirectory, "*", SearchOption.AllDirectories);

                        foreach (string SourceDirectory in SourceDirectories)
                        {
                            if (!syncJobConfiguration.ExcludedDiretories.Contains(SourceDirectory))
                            {
                                string SourceDirectoryName = SourceDirectory.Replace(syncJobConfiguration.SourceDiretory, "");
                                string SubTargetDirectory = TargetDirectory + SourceDirectoryName;

                                if (!Directory.Exists(SubTargetDirectory))
                                {
                                    CreateCopyDirectoryQueue(syncJobConfiguration.SourceDiretory, TargetDirectory, SourceDirectory);
                                    StartJobWorker();
                                }

                                while (!Directory.Exists(SubTargetDirectory)) { }

                                MirrorDirectoryContentFromSourceToTarget(SourceDirectory, SubTargetDirectory, false);
                            }
                        }
                        foreach (string TargetDir in DirectoriesInTargetDirectory)
                        {
                            if (!SourceDirectories.Contains(TargetDir.Replace(TargetDirectory, syncJobConfiguration.SourceDiretory)))
                            {
                                DeleteAllFilesFromDirectory(TargetDir);
                            }
                        }
                    }
                    DeleteAllEmptyDirectoriesWhichAreNotInSource(syncJobConfiguration.SourceDiretory, TargetDirectory, false);
                    StartJobWorker();

                }
            }
        }
        public static void MirrorDirectoryContentFromSourceToTarget(string SourcePath, string TargetPath, bool JobWorker)
        {
            string[] Directories = Directory.GetDirectories(SourcePath, "*", SearchOption.AllDirectories);

            foreach (string Dir in Directories)
            {
                CreateCopyDirectoryQueue(SourcePath, TargetPath, Dir);
            }

            MirrorFilesFromSourceToTarget(SourcePath, TargetPath, true, false);

            DeleteAllEmptyDirectoriesWhichAreNotInSource(SourcePath, TargetPath, false);

            if (JobWorker)
            {
                StartJobWorker();
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
                    JobInstruction jobInstruction = new JobInstruction(TargetDirectory, true);
                    if (!JobInstructions.Contains<JobInstruction>(jobInstruction))
                    {
                        JobInstructions.Enqueue(jobInstruction);
                    }
                }
            }
            if (JobWorker)
            {
                StartJobWorker();
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
                    JobInstruction jobInstruction = new JobInstruction(TargetPath, TargetFile.FullName.Replace(TargetPath, ""));
                    if (!JobInstructions.Contains<JobInstruction>(jobInstruction))
                    {
                        JobInstructions.Enqueue(jobInstruction);
                    }
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
                        Add = false; break;
                    }
                    else
                    {
                        Add = true;
                    }
                }
                if (Add || TargetList.Count() == 0)
                {
                    JobInstruction jobInstruction = new JobInstruction(SourcePath, TargetPath, SourceFile.FullName);
                    if (!JobInstructions.Contains<JobInstruction>(jobInstruction))
                    {
                        JobInstructions.Enqueue(jobInstruction);
                    }
                }
            }
            if (JobWorker) { StartJobWorker(); }
        }
        public static void DeleteAllFilesFromDirectory(string DirectoryPath)
        {
            DirectoryInfo Dir = new DirectoryInfo(DirectoryPath);

            IEnumerable<FileInfo> FileList = GetFilesOfDirectory(Dir, true);

            foreach (FileInfo DirectoryFile in FileList)
            {
                CreateDeleteFileQueue(DirectoryPath, DirectoryFile.FullName.Replace(DirectoryPath, ""));
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
