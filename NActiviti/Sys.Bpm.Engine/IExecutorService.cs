using System;
using System.Threading;

namespace Sys.Concurrent
{
    public interface IExecutorService
    {
        void Execute(ThreadStart runnable);

        void Shutdown();

        bool AwaitTermination(long secondsToWaitOnShutdown, TimeSpan timeout);
    }
}