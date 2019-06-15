using System;
using System.Collections.Concurrent;
using System.Threading;
using Sys.Concurrent;

namespace org.activiti.engine.impl.asyncexecutor
{
    /// <summary>
    /// 
    /// </summary>
    public class ThreadPoolExecutor : IExecutorService
    {
        private int corePoolSize;
        private int maxPoolSize;
        private long keepAliveTime;
        private object mILLISECONDS;

        private ConcurrentQueue<ThreadStart> threadPoolQueue;

        private readonly object syncRoot = new object();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="corePoolSize"></param>
        /// <param name="maxPoolSize"></param>
        /// <param name="keepAliveTime"></param>
        /// <param name="threadPoolQueue"></param>
        public ThreadPoolExecutor(int corePoolSize, int maxPoolSize, long keepAliveTime, ConcurrentQueue<ThreadStart> threadPoolQueue)
        {
            this.corePoolSize = corePoolSize;
            this.maxPoolSize = maxPoolSize;
            this.keepAliveTime = keepAliveTime;
            this.threadPoolQueue = threadPoolQueue;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="secondsToWaitOnShutdown"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public bool AwaitTermination(long secondsToWaitOnShutdown, TimeSpan timeout)
        {
            //throw new NotImplementedException();
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="runnable"></param>
        public void Execute(ThreadStart runnable)
        {
            runnable.Invoke();
        }

        /// <summary>
        /// 
        /// </summary>
        public void Shutdown()
        {
            //throw new NotImplementedException();
        }
    }
}