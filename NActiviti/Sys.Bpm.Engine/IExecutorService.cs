using System;
using System.Threading;

namespace Sys.Concurrent
{
    public interface IExecutorService
    {
        void execute(ThreadStart runnable);

        void shutdown();

        bool awaitTermination(long secondsToWaitOnShutdown, TimeSpan timeout);
    }
}