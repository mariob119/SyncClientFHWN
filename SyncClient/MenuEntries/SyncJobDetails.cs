using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncClient.MenuEntries
{
    internal static class SyncJobDetails
    {
        private static string Name = "Show sync details for choosen directory";
        private static string Info = "With this command, all the details of one sync folder is shown";
        private static string Command = "jobdetails";
        public static MenuEntryInfo GetMenuEntryInfo()
        {
            return new MenuEntryInfo(Name, Info, Command);
        }
        public static string GetCommand()
        {
            return Command;
        }
        public static void MainMethode()
        {
            Functions.WriteHeadLine("Sync Folder Details!");

            switch (SyncJobs.GetAmountOfSyncJobs())
            {
                case 1:
                    {
                        ShowOneSyncJob();
                        break; 
                    }
                case > 1:
                    {
                        ShowAllSyncJobs();
                        break;
                    }
                default:
                    {
                        ThereAreNoSyncJobs();
                        break;
                    }
            }
        }
        private static void ShowOneSyncJob()
        {
            SyncJobs.ShowDirectoryDetails(0);
            Console.WriteLine("\nThere is exactly one root directory configured, which is shown aboth!");
            Functions.PressAnyKeyToContinue();
        }
        private static void ShowAllSyncJobs()
        {
            SyncJobs.ShowSyncDiretories();
            Console.Write("\nEnter a sync job number for viewing its details: ");
            int SyncJobNumber = Functions.GetNumberBetween(0, SyncJobs.GetAmountOfSyncJobs() + 1) - 1;
            Console.Clear();
            Functions.WriteHeadLine("Sync Folder Details!");
            SyncJobs.ShowDirectoryDetails(SyncJobNumber);
            Functions.PressAnyKeyToContinue();
        }
        private static void ThereAreNoSyncJobs()
        {
            Console.WriteLine("There are no folders configured!");
            Functions.PressAnyKeyToContinue();
        }
    }
}
