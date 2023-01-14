using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SyncClient.ConfigModels;

namespace SyncClient.MenuEntries
{
    internal static class AddSyncJob
    {
        private static string Name = "Add Synchonronisation Folder";
        private static string Info = "Adds a root directory which should be synchronised!";
        private static string Command = "addjob";
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
            Functions.WriteHeadLine("Add a sync job");

            SyncTask config = new SyncTask();

            Console.WriteLine("Enter a source direcotry:");
            string SourceDirectory = Functions.EnterAValidDirectory();
            config.SetSourceDirectory(SourceDirectory);

            List<string> TargetDirectories = new List<string>();
            foreach (SyncTask syncJobConfiguration in SyncClient.Tasks)
            {
                foreach (string TargetDirectory in syncJobConfiguration.TargetDirectories)
                {
                    TargetDirectories.Add(TargetDirectory);
                }
            }

            Console.Write("Enter how many target directories you want to add: ");
            int NumberOfTargetDirectories = Functions.GetAPositiveNumber();
            for (int i = 1; i < NumberOfTargetDirectories + 1; i++)
            {
                Console.Write($"Enter target directory number {i}: ");
                bool IsValid = false;
                string DirectoryPath = String.Empty;
                while (IsValid == false)
                {
                    DirectoryPath = Functions.EnterAValidDirectory();
                    if (config.TargetDirectories.Contains(DirectoryPath))
                    {
                        Console.WriteLine("The directory you have entered is already a target direcotry!\nPlease enter another directory!");
                    }
                    else if (TargetDirectories.Contains(DirectoryPath))
                    {
                        Console.WriteLine("The target directory you have entered is already used by another job!\nPlease enter another directory!");
                    }
                    else if (config.SourceDirectory == DirectoryPath)
                    {
                        Console.WriteLine("The source directory can not be choosen as target directory!\nPlease enter another directory!");
                    }
                    else { IsValid = true; }
                }

                config.AddTargetDirectory(DirectoryPath);
            }

            Console.WriteLine("Should subdirectories be included? (true or false)?");
            config.SetIndludeSubdirectoriesAttribute(Functions.EnterABooleanValue());

            Console.Write("Enter how many excluded directories you want to add (0 for none): ");
            int NumberOfExcludedDirectories = Functions.GetAPositiveNumberIncludingZero();
            for (int i = 1; i < NumberOfExcludedDirectories + 1; i++)
            {
                Console.Write($"Enter excluded directory number {i}: {config.GetSourceDirectory()}\\");
                config.AddExcludedDirectory(Functions.EnterADirectoryWithPrefix(config.GetSourceDirectory()));
            }

            SyncClient.AddConfiguration(config);
            SyncClient.RefreshTaskConfiguration();
            Console.WriteLine($"\nSync Job Number {SyncClient.Tasks.Count()} has been added successfully!");
            SyncClient.HealthCheck();
            Functions.PressAnyKeyToContinue();
        }
    }
}
