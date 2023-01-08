using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            while(Result <= Min || Result >= Max)
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
            while(Input == string.Empty)
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
            //while (!CheckIfDirectoryExists(Input))
            //{
            //    Console.Write($"Please enter a valid direcotry: {Prefix}\\");
            //    Input = Prefix + "\\" + EnterNotEmptyString();
            //}
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
            if(Input == "true") { return true; } else { return false; }
        }
    }
}
