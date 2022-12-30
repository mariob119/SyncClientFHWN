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
        public static void Start()
        {
            List<MenuEntryInfo> MenuEntries = new List<MenuEntryInfo>();
            MenuEntries.Add(ShowConfiguration.GetMenuEntryInfo());

            List<String> Commands = new List<String>();
            Commands.Add("help");
            Commands.Add("exit");
            foreach(MenuEntryInfo info in MenuEntries)
            {
                Commands.Add(info.Command);
            }

            string a = "b";

            Console.WriteLine("=========================\n");
            Console.WriteLine("Welcome to Sync Client!\n");
            Console.WriteLine("=========================\n");

            bool Running = true;
            while (Running)
            {
                string Command = EnterACommand(Commands);
                if (Command == ShowConfiguration.GetCommand())
                {
                    ShowConfiguration.MainMethode();
                }
                if(Command == "help")
                {
                    PrintHelp(MenuEntries);
                }
                if(Command == "exit")
                {
                    Running = false;
                }
            }
        }
        private static string EnterACommand(List<String> Commands) {
            Console.WriteLine("Enter a command or 'help':");
            string Command = Console.ReadLine();
            Console.Clear();
            while (!Commands.Contains(Command))
            {
                Console.WriteLine("Please enter a valid command or enter 'help':");
                Command = Console.ReadLine();
                Console.Clear();
            }
            return Command;
        }
        private static void PrintHelp(List<MenuEntryInfo> menuEntryInfos)
        {
            Console.WriteLine("=========================\n");
            Console.WriteLine("HELP-PAGE\n");
            Console.WriteLine("=========================\n");

            foreach(MenuEntryInfo menuEntry in menuEntryInfos)
            {
                Console.WriteLine($"Name: \t\t{menuEntry.Name}");
                Console.WriteLine($"Command: \t'{menuEntry.Command}'");
                Console.WriteLine($"Description: \t{menuEntry.Info}\n");
            }

            Console.WriteLine($"Name: \t\tExit the program");
            Console.WriteLine($"Command: \t'exit'");
            Console.WriteLine($"Description: \tExiting the program!\n");

            Console.WriteLine("Press any key to continue!");
            Console.ReadKey();
            Console.Clear();
        }
    }
}
