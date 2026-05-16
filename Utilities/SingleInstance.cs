using System;
using System.Threading;

namespace LenovoBatteryTray.Utilities
{
    internal sealed class SingleInstance : IDisposable
    {
        private readonly Mutex mutex;
        private readonly bool ownsMutex;

        public SingleInstance(string mutexName)
        {
            mutex = new Mutex(true, mutexName, out ownsMutex);
        }

        public bool IsFirstInstance
        {
            get { return ownsMutex; }
        }

        public void Dispose()
        {
            if (ownsMutex)
            {
                mutex.ReleaseMutex();
            }

            mutex.Dispose();
        }
    }
}
