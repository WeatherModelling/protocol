using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GUI
{
    sealed class Log:IDisposable
    {
        // singleton
        public static readonly Log Instance = new Log();

        public enum RecordType
        {
            // General records
            Main,
            // Solver protocol exchange data
            Solver, 
            // Evolution thread events
            Evolution
        }
        struct Record {
            public RecordType type;
            public DateTime time;
            public string message;
        }
        
        SortedDictionary<DateTime, Record> log = new SortedDictionary<DateTime, Record>();
        ConcurrentQueue<Record> queue;
        public void Add(RecordType type, string message)
        {
            queue.Enqueue(new Record
            {
                type = type,
                time = DateTime.Now,
                message = message
            });
        }

        bool loggingStarted = true;
        private Log()
        {
            (new Thread(() =>
            {
                while (true)
                {

                }
            }
            )).Start();

        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~Log() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            Dispose(true);
        }
        #endregion

    }
}
