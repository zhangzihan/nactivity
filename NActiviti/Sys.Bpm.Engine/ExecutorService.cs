using System;
using System.Threading;

namespace java.util.concurrent
{
    public class ExecutorService
    {
        private ThreadStart runnable;

        public void Execute(ThreadStart runnable)
        {
            this.runnable = runnable;

            this.runnable.Invoke();
        }

        internal void Shutdown()
        {
            throw new NotImplementedException();
        }

        internal bool AwaitTermination(long secondsToWaitOnShutdown, TimeSpan timeout)
        {
            throw new NotImplementedException();
        }
    }
}