using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncClient.MenuEntries
{
    internal static class ShowSyncJobs
    {
        private static string Name = "Show sync jobs";
        private static string Info = "With this command, all the configured root directories are shown!";
        private static string Command = "jobs";
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
            Functions.WriteHeadLine("Sync Folders");

            SyncJobs.ShowSyncDiretories();

            Functions.PressAnyKeyToContinue();
        }
    }
}
