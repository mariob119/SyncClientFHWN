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
        public ConcurrentQueue<IJob> SyncJobs { get; set; }
        readonly object _lock = new object();
        public JobQueue(string name)
        {
            this.name = name;
            SyncJobs = new ConcurrentQueue<IJob>();
        }
        public bool TryEnter()
        {
            return Monitor.TryEnter(_lock);
        }
        public void UnLock()
        {
            Monitor.Exit(_lock);
        }
        public void Lock()
        {
            Monitor.Enter(_lock);
        }
    }
}
