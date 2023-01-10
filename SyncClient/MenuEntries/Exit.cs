using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncClient.MenuEntries
{
    internal static class Exit
    {
        private static string Name = "Exit program";
        private static string Info = "With this command, you can exit the Program!";
        private static string Command = "exit";
        public static MenuEntryInfo GetMenuEntryInfo()
        {
            return new MenuEntryInfo(Name, Info, Command);
        }
        public static string GetCommand()
        {
            return Command;
        }
        public static bool MainMethode()
        {
            bool Quit = true;
            bool Running = true;
            if (!SyncJobs.CheckIfJobsAreRunning())
            {
                Console.WriteLine("There are still some jobs running! Do you want to quit?\n");
                Console.WriteLine("Enter 'y' (yes) or 'n' (no)!\n");
                Quit = Functions.EnterYesOrNo();
                Console.Clear();
            }
            if (Quit)
            {
                Running = false;
            }
            else
            {
                Running = true;
            }
            return Running;
        }
    }
}
