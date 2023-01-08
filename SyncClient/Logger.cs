using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncClient
{
    internal static class Logger
    {
        public static int LastCursorPosition = 0;
        public static int EntryNumber = 1;
        public static object _lock = new object();
        public static void log(string Message)
        {
            lock (_lock)
            {
                var pos = Console.GetCursorPosition();
                Console.SetCursorPosition(0, LastCursorPosition);
                Console.WriteLine($"Entry {EntryNumber}:\t" + Message);
                //for(int x = 0; x < 5; x++)
                //{
                //    Console.WriteLine("test");
                //}
                if(LastCursorPosition > 5) { LastCursorPosition = 0; }
                else { LastCursorPosition += 1; }
                EntryNumber += 1;
                Console.SetCursorPosition(0, pos.Top);
                //Console.WriteLine(Console.GetCursorPosition());
                //Console.WriteLine(Message);
            }
        }
        public static void lognow()
        {
            log(DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString());
        }
    }
}
