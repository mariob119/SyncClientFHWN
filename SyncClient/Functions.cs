using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#pragma warning disable CS8602

namespace SyncClient
{
    internal static class Functions
    {
        public static int GetNumber()
        {
            int Result = 0;
            string? Input = Console.ReadLine();
            while (!Int32.TryParse(Input, out Result))
            {
                Console.WriteLine("Please enter a valid number: ");
                Input = Console.ReadLine();
            }
            return Result;
        }
        public static int GetAPositiveNumber()
        {
            int Result = GetNumber();
            while (Result < 1)
            {
                Console.WriteLine($"Please enter a number greater than 0!");
                Result = GetNumber();
            }
            return Result;
        }
        public static int GetAPositiveNumberIncludingZero()
        {
            int Result = GetNumber();
            while (Result < 0)
            {
                Console.WriteLine($"Please enter a number greater or equal to 0!");
                Result = GetNumber();
            }
            return Result;
        }
        public static int GetNumberBetween(int Min, int Max)
        {
            int Result = GetNumber();
            while (Result <= Min || Result >= Max)
            {
                Console.WriteLine($"Please enter a number between {Min} and {Max}");
                Result = GetNumber();
            }
            return Result;
        }
        public static void WriteHeadLine(string Headline)
        {
            Console.WriteLine("=========================\n");
            Console.WriteLine(Headline);
            Console.WriteLine("\n=========================\n");
        }
        public static void PressAnyKeyToContinue()
        {
            Console.WriteLine("\nPress any key to go back to the main menu!");
            Console.ReadKey();
            Console.Clear();
        }
        public static string EnterNotEmptyString()
        {
            string? Input = Console.ReadLine();
            while (Input == string.Empty)
            {
                Console.WriteLine("Please do not enter an empty string!");
                Input = Console.ReadLine();
            }
            return Input;
        }
        public static string EnterAValidDirectory()
        {
            string Input = EnterNotEmptyString();
            while (!CheckIfDirectoryExists(Input))
            {
                Console.WriteLine("Please enter a valid direcotry: ");
                Input = EnterNotEmptyString();
            }
            return Input;
        }
        public static string EnterADirectoryWithPrefix(string Prefix)
        {
            string Input = Prefix + "\\" + EnterNotEmptyString();
            return Input;
        }
        public static bool CheckIfDirectoryExists(string DirectoryPath)
        {
            return Directory.Exists(DirectoryPath);
        }
        public static bool EnterABooleanValue()
        {
            string Input = EnterNotEmptyString();
            while (!(Input == "true" || Input == "false"))
            {
                Console.WriteLine("Enter true or false!");
                Input = EnterNotEmptyString();
            }
            if (Input == "true") { return true; } else { return false; }
        }
        public static bool EnterYesOrNo()
        {
            string Input = EnterNotEmptyString();
            while (!(Input == "y" || Input == "n"))
            {
                Console.WriteLine("Enter 'y' (yes) or 'n' (no)!");
                Input = EnterNotEmptyString();
            }
            if (Input == "y") { return true; } else { return false; }
        }
        public static bool IsFileLocked(FileInfo file)
        {
            try
            {
                using (FileStream stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    stream.Close();
                }
            }
            catch (IOException)
            {
                return true;
            }
            return false;
        }
        public static string ShortenFileName(string FileName)
        {
            string FileExtension = Path.GetExtension(FileName);
            if (FileName.Length > 15)
            {
                FileName = FileName.Substring(0, 15);
                return FileName + "...  *" + FileExtension;
            }
            else
            {
                return FileName;
            }
        }
        public static string ShortenPath(string DirectoryPath)
        {
            string RootFolder = Directory.GetDirectoryRoot(DirectoryPath);
            string ChildFolder = Path.GetDirectoryName(DirectoryPath);
            if (DirectoryPath.Length > 15)
            {
                return RootFolder + "..." + ChildFolder;
            }
            else
            {
                return DirectoryPath;
            }
        }
    }
}
