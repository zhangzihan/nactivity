using System;
using System.Threading;

/* Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *      http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
namespace Sys.Workflow.Engine.Impl.Asyncexecutor
{
    using Microsoft.Extensions.Logging;
    using Sys.Workflow.Engine.Impl.Cmd;
    using Sys.Workflow.Engine.Impl.Interceptor;
    using Sys.Workflow.Engine.Impl.Persistence.Entity;
    using Sys.Workflow;

    /// 
    /// 
    public class AcquireAsyncJobsDueRunnable
    {
        private static readonly ILogger log = ProcessEngineServiceProvider.LoggerService<AcquireAsyncJobsDueRunnable>();

        /// <summary>
        /// 
        /// </summary>
        protected internal readonly IAsyncExecutor asyncExecutor;

        /// <summary>
        /// 
        /// </summary>
        protected internal volatile bool isInterrupted;

        /// <summary>
        /// 
        /// </summary>
        private readonly object MONITOR = new object();

        /// <summary>
        /// 
        /// </summary>
        protected internal bool isWaiting = false;// new AtomicBoolean(false);

        /// <summary>
        /// 
        /// </summary>
        protected internal long millisToWait;

        /// <summary>
        /// 
        /// </summary>
        protected internal ThreadStart Runable { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="asyncExecutor"></param>
        public AcquireAsyncJobsDueRunnable(IAsyncExecutor asyncExecutor)
        {
            this.asyncExecutor = asyncExecutor;

            this.Runable += new ThreadStart(Run);
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void Run()
        {
            Thread.CurrentThread.Name = "activiti-acquire-async-jobs";
            log.LogInformation($"{Thread.CurrentThread.Name} starting to acquire async jobs due");

            ICommandExecutor commandExecutor = asyncExecutor.ProcessEngineConfiguration.CommandExecutor;

            while (!isInterrupted)
            {
                try
                {
                    AcquiredJobEntities acquiredJobs = commandExecutor.Execute(new AcquireJobsCmd(asyncExecutor));

                    if (acquiredJobs is object)
                    {
                        bool allJobsSuccessfullyOffered = true;
                        foreach (IJobEntity job in acquiredJobs.Jobs)
                        {
                            bool jobSuccessFullyOffered = asyncExecutor.ExecuteAsyncJob(job);
                            if (!jobSuccessFullyOffered)
                            {
                                allJobsSuccessfullyOffered = false;
                            }
                        }

                        // If all jobs are executed, we check if we got back the amount we expected
                        // If not, we will wait, as to not query the database needlessly. 
                        // Otherwise, we set the wait time to 0, as to query again immediately.
                        millisToWait = asyncExecutor.DefaultAsyncJobAcquireWaitTimeInMillis;
                        int jobsAcquired = acquiredJobs.Size();
                        if (jobsAcquired >= asyncExecutor.MaxAsyncJobsDuePerAcquisition)
                        {
                            millisToWait = 0;
                        }

                        // If the queue was full, we wait too (even if we got enough jobs back), as not overload the queue
                        if (millisToWait == 0 && !allJobsSuccessfullyOffered)
                        {
                            millisToWait = asyncExecutor.DefaultQueueSizeFullWaitTimeInMillis;
                        }
                    }
                }
                catch (ActivitiOptimisticLockingException optimisticLockingException)
                {
                    if (log.IsEnabled(LogLevel.Debug))
                    {
                        log.LogDebug($@"Optimistic locking exception during async job acquisition. 
If you have multiple async executors running against the same database, this exception means that this thread tried to acquire a due async job, 
which already was acquired by another async executor acquisition thread.
This is expected behavior in a clustered environment. 
You can ignore this message if you indeed have multiple async executor acquisition threads running against the same database. Exception message: {optimisticLockingException.Message}");
                    }
                }
                catch (Exception e)
                {
                    log.LogDebug($"exception during async job acquisition: {e.Message}");
                    millisToWait = asyncExecutor.DefaultAsyncJobAcquireWaitTimeInMillis;
                }

                if (millisToWait > 0)
                {
                    try
                    {
                        //if (log.IsEnabled(LogLevel.Debug))
                        //{
                        //    log.LogDebug($"async job acquisition thread sleeping for {millisToWait} millis");
                        //}
                        lock (MONITOR)
                        {
                            if (!isInterrupted)
                            {
                                isWaiting = true;//.set(true);
                                Monitor.Wait(MONITOR, TimeSpan.FromMilliseconds(millisToWait));
                            }
                        }

                        //if (log.IsEnabled(LogLevel.Debug))
                        //{
                        //    log.LogDebug("async job acquisition thread woke up");
                        //}
                    }
                    catch (ThreadInterruptedException e)
                    {
                        if (log.IsEnabled(LogLevel.Debug))
                        {
                            log.LogDebug("async job acquisition wait interrupted");
                        }
                    }
                    finally
                    {
                        isWaiting = false;//.set(false);
                    }
                }
            }

            log.LogInformation("stopped async job due acquisition");
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void Stop()
        {
            lock (MONITOR)
            {
                isInterrupted = true;
                if (isWaiting == true) //.compareAndSet(true, false))
                {
                    Monitor.PulseAll(MONITOR);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual long MillisToWait
        {
            get
            {
                return millisToWait;
            }
            set
            {
                this.millisToWait = value;
            }
        }
    }
}