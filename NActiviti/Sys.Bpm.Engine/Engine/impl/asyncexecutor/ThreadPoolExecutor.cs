using System;
using System.Collections.Concurrent;
using System.Threading;
using Sys.Concurrent;

namespace org.activiti.engine.impl.asyncexecutor
{
    public class ThreadPoolExecutor : IExecutorService
    {
        private int corePoolSize;
        private int maxPoolSize;
        private long keepAliveTime;
        private object mILLISECONDS;
        private ConcurrentQueue<ThreadStart> threadPoolQueue;

        public ThreadPoolExecutor(int corePoolSize, int maxPoolSize, long keepAliveTime, ConcurrentQueue<ThreadStart> threadPoolQueue)
        {
            this.corePoolSize = corePoolSize;
            this.maxPoolSize = maxPoolSize;
            this.keepAliveTime = keepAliveTime;
            this.threadPoolQueue = threadPoolQueue;
        }

        public bool awaitTermination(long secondsToWaitOnShutdown, TimeSpan timeout)
        {
            //throw new NotImplementedException();
            return true;
        }

        public void execute(ThreadStart runnable)
        {
            runnable.Invoke();
        }

        public void shutdown()
        {
            //throw new NotImplementedException();
        }
    }
}