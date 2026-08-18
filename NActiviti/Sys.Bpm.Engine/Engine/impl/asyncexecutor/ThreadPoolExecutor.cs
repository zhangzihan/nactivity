using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.Logging;
using Sys.Concurrent;

namespace Sys.Workflow.Engine.Impl.Asyncexecutor
{
    /// <summary>
    /// 真实的有界线程池执行器，对应 Java 版 Activiti 中 <c>java.util.concurrent.ThreadPoolExecutor</c> 的语义：
    /// 任务先进入有界队列（容量为 queueSize），核心线程常驻消费；队列满且线程数未达上限时扩容工作线程；
    /// 队列满且线程数已达 maxPoolSize 时抛出 <see cref="RejectedExecutionException"/>。
    /// </summary>
    public class ThreadPoolExecutor : IExecutorService
    {
        private readonly int corePoolSize;
        private readonly int maxPoolSize;
        private readonly int keepAliveTimeMillis;

        /// <summary>
        /// 有界任务队列，容量由 queueSize 决定
        /// </summary>
        private readonly BlockingCollection<ThreadStart> taskQueue;

        /// <summary>
        /// 当前存活的工作线程
        /// </summary>
        private readonly List<Thread> workers = new List<Thread>();

        private readonly object syncRoot = new object();

        private volatile bool isShutdown;

        private int workerSequence;

        private static readonly ILogger logger = ProcessEngineServiceProvider.LoggerService<ThreadPoolExecutor>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="corePoolSize">常驻核心线程数</param>
        /// <param name="maxPoolSize">最大工作线程数</param>
        /// <param name="keepAliveTime">超出核心线程数的工作线程空闲存活时间（毫秒）</param>
        /// <param name="threadPoolQueue">保留参数，兼容原构造签名，任务实际进入内部有界队列</param>
        /// <param name="queueSize">任务队列容量，队列满且线程数达到上限时新任务将被拒绝</param>
        public ThreadPoolExecutor(int corePoolSize, int maxPoolSize, long keepAliveTime, ConcurrentQueue<ThreadStart> threadPoolQueue, int queueSize = int.MaxValue)
        {
            this.corePoolSize = Math.Max(corePoolSize, 1);
            this.maxPoolSize = Math.Max(maxPoolSize, this.corePoolSize);
            this.keepAliveTimeMillis = (int)Math.Max(1, Math.Min(keepAliveTime, int.MaxValue));
            this.taskQueue = queueSize > 0 && queueSize < int.MaxValue
                ? new BlockingCollection<ThreadStart>(queueSize)
                : new BlockingCollection<ThreadStart>();
        }

        /// <summary>
        /// 提交任务：队列未满则入队，否则在线程数未达上限时扩容线程直接执行，否则拒绝
        /// </summary>
        /// <param name="runnable"></param>
        public void Execute(ThreadStart runnable)
        {
            if (runnable is null)
            {
                throw new ArgumentNullException(nameof(runnable));
            }

            if (!isShutdown)
            {
                try
                {
                    if (taskQueue.TryAdd(runnable))
                    {
                        EnsureCoreWorker();
                        return;
                    }
                }
                catch (InvalidOperationException)
                {
                    // 队列已完成添加（Shutdown 竞态），按拒绝处理
                }
            }

            // 队列已满，若线程数未达上限则新增工作线程直接执行该任务
            if (TryAddWorker(runnable))
            {
                return;
            }

            throw new RejectedExecutionException($"异步作业执行器已饱和：任务队列已满或已关闭，且工作线程数已达上限 {maxPoolSize}，任务被拒绝");
        }

        /// <summary>
        /// 队列中有积压任务且核心线程尚未拉满时，补充核心工作线程
        /// </summary>
        private void EnsureCoreWorker()
        {
            lock (syncRoot)
            {
                if (isShutdown || taskQueue.Count == 0 || workers.Count >= corePoolSize)
                {
                    return;
                }

                StartWorkerLocked(null);
            }
        }

        /// <summary>
        /// 在线程数未达上限且未关闭时，新增一个工作线程；firstTask 不为空时由该线程直接执行
        /// </summary>
        private bool TryAddWorker(ThreadStart firstTask)
        {
            lock (syncRoot)
            {
                if (isShutdown || workers.Count >= maxPoolSize)
                {
                    return false;
                }

                StartWorkerLocked(firstTask);
                return true;
            }
        }

        /// <summary>
        /// 调用方必须已持有 syncRoot 锁
        /// </summary>
        private void StartWorkerLocked(ThreadStart firstTask)
        {
            workerSequence++;
            Thread worker = new Thread(() => WorkerLoop(firstTask))
            {
                // 后台线程，避免阻塞进程退出
                IsBackground = true,
                Name = $"activiti-async-job-executor-thread-{workerSequence}"
            };
            workers.Add(worker);
            worker.Start();
        }

        /// <summary>
        /// 工作线程主循环：优先执行首任务，之后持续从队列取任务执行；
        /// 空闲超过 keepAliveTime 后，仅核心线程继续阻塞等待，其余线程退出
        /// </summary>
        private void WorkerLoop(ThreadStart firstTask)
        {
            ThreadStart task = firstTask;
            try
            {
                while (true)
                {
                    if (task is null)
                    {
                        bool taken;
                        try
                        {
                            taken = taskQueue.TryTake(out task, keepAliveTimeMillis);
                        }
                        catch (ObjectDisposedException)
                        {
                            // 队列已释放（ObjectDisposedException 派生自 InvalidOperationException，须先捕获）
                            break;
                        }
                        catch (InvalidOperationException)
                        {
                            // 队列已关闭且为空
                            break;
                        }

                        if (!taken)
                        {
                            // 取任务超时：已关闭或超出核心线程数时退出，否则继续等待
                            lock (syncRoot)
                            {
                                if (isShutdown || workers.Count > corePoolSize)
                                {
                                    break;
                                }
                            }
                            continue;
                        }
                    }

                    try
                    {
                        task();
                    }
                    catch (Exception e)
                    {
                        // 单个任务异常不能杀死工作线程
                        logger.LogError(e, "异步作业执行线程中的任务抛出未处理异常");
                    }

                    task = null;
                }
            }
            finally
            {
                lock (syncRoot)
                {
                    workers.Remove(Thread.CurrentThread);
                }
            }
        }

        /// <summary>
        /// 停止接受新任务，已入队的任务继续执行直至排空
        /// </summary>
        public void Shutdown()
        {
            lock (syncRoot)
            {
                if (isShutdown)
                {
                    return;
                }

                isShutdown = true;
            }

            taskQueue.CompleteAdding();
        }

        /// <summary>
        /// 等待所有工作线程结束（队列排空后退出），超时返回 false
        /// </summary>
        /// <param name="secondsToWaitOnShutdown">最长等待秒数</param>
        /// <returns>所有工作线程均已结束返回 true，超时返回 false</returns>
        public bool AwaitTermination(long secondsToWaitOnShutdown)
        {
            DateTime deadline = DateTime.UtcNow.AddSeconds(secondsToWaitOnShutdown);

            while (true)
            {
                Thread[] snapshot;
                lock (syncRoot)
                {
                    if (workers.Count == 0)
                    {
                        return true;
                    }

                    snapshot = workers.ToArray();
                }

                foreach (Thread worker in snapshot)
                {
                    TimeSpan remaining = deadline - DateTime.UtcNow;
                    if (remaining <= TimeSpan.Zero || !worker.Join(remaining))
                    {
                        return false;
                    }
                }
            }
        }
    }
}
