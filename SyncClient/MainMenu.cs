using SyncClient.MenuEntries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncClient
{
    internal static class MainMenu
    {
        public static void Run()
        {
            List<MenuEntryInfo> MenuEntries = new List<MenuEntryInfo>();
            MenuEntries.Add(ShowSyncJobs.GetMenuEntryInfo());
            MenuEntries.Add(AddSyncJob.GetMenuEntryInfo());
            MenuEntries.Add(SyncJobDetails.GetMenuEntryInfo());
            MenuEntries.Add(DeleteSyncJob.GetMenuEntryInfo());
            MenuEntries.Add(Exit.GetMenuEntryInfo());
            MenuEntries.Add(Help.GetMenuEntryInfo());

            List<String> Commands = new List<String>();
            foreach (MenuEntryInfo info in MenuEntries)
            {
                Commands.Add(info.Command);
            }

            bool Running = true;
            while (Running)
            {
                string Command = EnterACommand(Commands);
                if (Command == AddSyncJob.GetCommand())
                {
                    AddSyncJob.MainMethode();
                }
                if (Command == DeleteSyncJob.GetCommand())
                {
                    DeleteSyncJob.MainMethode();
                }
                if (Command == SyncJobDetails.GetCommand())
                {
                    SyncJobDetails.MainMethode();
                }
                if (Command == ShowSyncJobs.GetCommand())
                {
                    ShowSyncJobs.MainMethode();
                }
                if (Command == Help.GetCommand())
                {
                    Help.MainMethode(MenuEntries);
                }
                if (Command == Exit.GetCommand())
                {
                    Running = Exit.MainMethode();
                }
            }
        }
        private static string EnterACommand(List<String> Commands)
        {
            Functions.WriteHeadLine("Main Menu");
            Console.WriteLine("Enter a command or 'help':");
            string Command = Console.ReadLine();
            Console.Clear();
            while (!Commands.Contains(Command))
            {
                Functions.WriteHeadLine("Main Menu");
                Console.WriteLine("Please enter a valid command or enter 'help':");
                Command = Console.ReadLine();
                Console.Clear();
            }
            return Command;
        }
    }
}
