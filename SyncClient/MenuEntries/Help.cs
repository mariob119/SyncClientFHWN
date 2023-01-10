using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncClient.MenuEntries
{
    internal static class Help
    {
        private static string Name = "Help Page";
        private static string Info = "With this command, the help page gets displayed";
        private static string Command = "help";
        public static MenuEntryInfo GetMenuEntryInfo()
        {
            return new MenuEntryInfo(Name, Info, Command);
        }
        public static string GetCommand()
        {
            return Command;
        }
        public static void MainMethode(List<MenuEntryInfo> menuEntryInfos)
        {
            Functions.WriteHeadLine("HELP-PAGE");

            List<MenuEntryInfo> SortedMenuEntryInfos = menuEntryInfos.OrderBy(m => m.Command).ToList();

            foreach (MenuEntryInfo menuEntry in SortedMenuEntryInfos)
            {
                Console.WriteLine($"Name: \t\t{menuEntry.Name}");
                Console.WriteLine($"Command: \t'{menuEntry.Command}'");
                Console.WriteLine($"Description: \t{menuEntry.Info}\n");
            }

            Functions.PressAnyKeyToContinue();
        }
    }
}
