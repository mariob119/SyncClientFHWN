using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncClient.MenuEntries
{
    internal static class DeleteSyncJob
    {
        private static string Name = "Deletes a sync job";
        private static string Info = "With this command, you can delete a sync job!";
        private static string Command = "deletejob";
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
            Functions.WriteHeadLine("Delete a sync job");

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
            SyncJobs.ShowSyncDiretories();
            Console.WriteLine("\nThere is exactly one job configured! Type 'y' if you want to delete it!");
            string Input = Functions.EnterNotEmptyString();
            if(Input == "y")
            {
                SyncJobs.SyncJobConfigurations.RemoveAt(0);
                Console.WriteLine("\nSync job has been removed successfully!");
                SyncJobs.SaveConfigurations();
                SyncJobs.ReloadFileSystemWatchers();
            } 
            else
            {
                Console.WriteLine("\nSync job has not been removed!");
            }
            Functions.PressAnyKeyToContinue();
        }
        private static void ShowAllSyncJobs()
        {
            SyncJobs.ShowSyncDiretories();
            Console.Write("\nEnter a sync job which you want to delete: ");
            int SyncJobNumber = Functions.GetNumberBetween(0, SyncJobs.GetAmountOfSyncJobs() + 1) - 1;
            Console.WriteLine($"Do you want to delte the job with number {SyncJobNumber + 1} ('y' to delete)?");
            string Input = Functions.EnterNotEmptyString();
            if (Input == "y")
            {
                SyncJobs.SyncJobConfigurations.RemoveAt(SyncJobNumber);
                Console.WriteLine("\nSync job has been removed successfully!");
                SyncJobs.SaveConfigurations();
                SyncJobs.ReloadFileSystemWatchers();
            }
            else
            {
                Console.WriteLine("\nSync job has not been removed!");
            }
            Functions.PressAnyKeyToContinue();
        }
        private static void ThereAreNoSyncJobs()
        {
            Console.WriteLine("There are no folders configured!");
            Functions.PressAnyKeyToContinue();
        }
    }
}
