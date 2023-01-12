using SyncClient.ConfigModels;
using SyncClient.JobTypes;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncClient
{
    internal class SyncClient
    {
        public Dictionary<string, ConcurrentQueue<IJob>>? Jobs;

        private List<char> LocicalDrives;
        public SyncClient()
        {
            Jobs = new Dictionary<string, ConcurrentQueue<IJob>>();
            LocicalDrives = new List<char>();

            Jobs.Add("NoParallelSync", new ConcurrentQueue<IJob>());
        }
        public void GetLogicalDrives()
        {
            List<SyncTask> SyncConfigs = SyncTasks.Tasks;
            List<char> DriveLetters = new List<char>();
            SyncConfigs.FindAll(entry => Char.IsLetter(entry.SourceDiretory[0])).ToList().ForEach(entry => DriveLetters.Add(entry.SourceDiretory[0]));
            SyncConfigs.ForEach(entry => entry.TargetDirectories.FindAll(entry => Char.IsLetter(entry[0])).ToList().ForEach(entry => DriveLetters.Add(entry[0])));
            LocicalDrives = DriveLetters.Distinct().ToList();
        }
        public void RegenerateLogicalDriveQueues()
        {
            List<string> Paths = new List<string>();
            SyncTasks.Tasks.Select(entry => entry.SourceDiretory).ToList().ForEach(entry => Paths.Add(entry));
            SyncTasks.Tasks.ForEach(entry => entry.TargetDirectories.ForEach(entry => Paths.Add(entry)));
            List<string> UniqueDriveLetters = Paths.Select(entry => entry[0].ToString()).ToList().Distinct().ToList();
            List<string> DriveLettersWhichAreNotInQueues = UniqueDriveLetters.Where(entry => !Jobs.ToList().Any(entry2 => entry2.Key == entry)).ToList();
            DriveLettersWhichAreNotInQueues.ForEach(entry => Jobs.Add(entry, new ConcurrentQueue<IJob>()));
        }
    }
}
