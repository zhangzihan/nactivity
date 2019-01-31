using System;
using System.Threading;

namespace java.util.concurrent
{
    public class ExecutorService
    {
        private ThreadStart runnable;

        public void execute(ThreadStart runnable)
        {
            this.runnable = runnable;

            this.runnable.Invoke();
        }

        internal void shutdown()
        {
            throw new NotImplementedException();
        }

        internal bool awaitTermination(long secondsToWaitOnShutdown, TimeSpan timeout)
        {
            throw new NotImplementedException();
        }
    }
}