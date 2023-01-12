using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncClient
{
    internal class JobQueue
    {
        public string name { get; private set; }
        public ConcurrentQueue<IJob> Jobs { get; set; }
        readonly object _lock = new object();
        public JobQueue(string name)
        {
            this.name = name;
            Jobs = new ConcurrentQueue<IJob>();
        }
    }
}
