using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncClient.MenuEntries
{
    internal class MenuEntryInfo
    {
        public string Name { get; set; }
        public string Info { get; set; }
        public string Command { get; set; }
        public MenuEntryInfo(string name, string info, string command)
        {
            Name = name;
            Info = info;
            Command = command;
        }
    }
}
