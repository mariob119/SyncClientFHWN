using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncClient.MenuEntries
{
    internal static class ShowConfiguration
    {
        static private string Name = "Show Configuration";
        static private string Info = "This is a test config!";
        static private string Command = "showconfig";
        static public MenuEntryInfo GetMenuEntryInfo()
        {
            return new MenuEntryInfo(Name, Info, Command);
        }
        static public string GetCommand()
        {
            return Command;
        }
        static public void MainMethode()
        {
            Console.WriteLine("Not Implemented!");
        }
    }
}
