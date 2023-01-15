using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncClient.MenuEntries
{
    internal static class EditConfiguration
    {
        private static string Name = "Edit the configuration of the program";
        private static string Info = "With this command, you can edit the configuration of the Program!";
        private static string Command = "edit";
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
            var Config = SyncClient.Configuration;
            ShowConfig(true);

            bool Editing = true;
            while (Editing)
            {
                string Command = Functions.EnterNotEmptyString();
                switch (Command)
                {
                    case "1":
                        Console.WriteLine("Enter a value:");
                        bool WTF = Functions.EnterABooleanValue();
                        Config.WriteToLogFile = WTF;
                        Console.Clear();
                        ShowConfig(true);
                        break;
                    case "2":
                        Console.WriteLine("Enter a value:");
                        int LFS = Functions.GetAPositiveNumber();
                        Config.LogFileSize = Convert.ToDouble(LFS);
                        Console.Clear();
                        ShowConfig(true);
                        break;
                    case "3":
                        Console.WriteLine("Enter a name:");
                        string LFN = Functions.EnterNotEmptyString();
                        Config.LogFileName = LFN;
                        Console.Clear();
                        ShowConfig(true);
                        break;
                    case "4":
                        Console.WriteLine("Enter a name:");
                        string LFP = Functions.EnterAValidDirectory();
                        Config.LogFilePath = LFP;
                        Console.Clear();
                        ShowConfig(true);
                        break;
                    case "5":
                        Console.WriteLine("Enter a value:");
                        bool LTDP = Functions.EnterABooleanValue();
                        Config.LogToDifferentPath = LTDP;
                        Console.Clear();
                        ShowConfig(true);
                        break;
                    case "6":
                        Console.WriteLine("Enter a value:");
                        bool PS = Functions.EnterABooleanValue();
                        Config.ParallelSync = PS;
                        Console.Clear();
                        ShowConfig(true);
                        break;
                    case "7":
                        Console.WriteLine("Enter a value:");
                        int VQ = Functions.GetAPositiveNumber();
                        Config.VisualisedQueues = VQ;
                        Console.Clear();
                        ShowConfig(true);
                        break;
                    case "8":
                        Console.WriteLine("Enter a value:");
                        int VL = Functions.GetAPositiveNumber();
                        Config.VisualisedLogs = VL;
                        Console.Clear();
                        ShowConfig(true);
                        break;
                    case "9":
                        Console.WriteLine("Enter a value:");
                        int BSFS = Functions.GetAPositiveNumber();
                        Config.BlockSyncFileSize = BSFS;
                        Console.Clear();
                        ShowConfig(true);
                        break;
                    case "10":
                        Console.WriteLine("Enter a value:");
                        int BSBS = Functions.GetAPositiveNumber();
                        Config.BlockSyncBlockSize = BSBS;
                        Console.Clear();
                        ShowConfig(true);
                        break;
                    case "exit":
                        Editing = false;
                        break;
                    default:
                        Console.Clear();
                        ShowConfig(false);
                        break;
                }
            }
            SyncClient.SaveEverything();
            Console.Clear();
        }
        public static void ShowConfig(bool RightInput)
        {
            var Config = SyncClient.Configuration;
            Functions.WriteHeadLine("Edit Configuration");

            Console.WriteLine($"1)  Write to log file:\t\t\t{Config.WriteToLogFile}");
            Console.WriteLine($"2)  Log file size in MB:\t\t{Config.LogFileSize}");
            Console.WriteLine($"3)  Log file name:\t\t\t{Config.LogFileName}");
            Console.WriteLine($"4)  Log file path (5 must be ture):\t{Config.LogFilePath}");
            Console.WriteLine($"5)  Log to different path:\t\t{Config.LogToDifferentPath}");
            Console.WriteLine($"6)  Parallel sync:\t\t\t{Config.ParallelSync}");
            Console.WriteLine($"7)  Visualised Queues:\t\t\t{Config.VisualisedQueues}");
            Console.WriteLine($"8)  Visualised Logs:\t\t\t{Config.VisualisedLogs}");
            Console.WriteLine($"9)  Block sync file size in MB:\t\t{Config.BlockSyncFileSize}");
            Console.WriteLine($"10) Block sync block size in Bytes:\t{Config.BlockSyncBlockSize}");

            if (RightInput)
            {
                Console.WriteLine("\nEnter a number for editing the value, or enter 'exit' to go back to main menu!");
            }
            else
            {
                Console.WriteLine("\nThe number you have entered is not in the list! Please enter another one:");
            }
        }
    }
}
