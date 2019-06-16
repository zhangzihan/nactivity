using System;
using System.Collections.Generic;
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
namespace Sys.Workflow.engine.impl.asyncexecutor
{
    using Microsoft.Extensions.Logging;
    using Sys.Workflow.engine.impl.persistence.entity;
    using Sys.Workflow.engine.runtime;
    using Sys.Workflow;

    /// <summary>
    /// Runnable that checks the <seealso cref="IJob"/> entities periodically for 'expired' jobs.
    /// 
    /// When a job is executed, it is first locked (lock owner and lock time is set).
    /// A job is expired when this lock time is exceeded. This can happen when an executor 
    /// goes down before completing a task.
    /// 
    /// This runnable will find such jobs and reset them, so they can be picked up again.
    /// 
    /// 
    /// </summary>
    public class ResetExpiredJobsRunnable
    {
        private static readonly ILogger log = ProcessEngineServiceProvider.LoggerService<ResetExpiredJobsRunnable>();

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
        protected internal readonly object MONITOR = new object();

        /// <summary>
        /// 
        /// </summary>
        protected internal bool isWaiting = false;// new AtomicBoolean(false);

        /// <summary>
        /// 
        /// </summary>
        protected internal ThreadStart Runable { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="asyncExecutor"></param>
        public ResetExpiredJobsRunnable(IAsyncExecutor asyncExecutor)
        {
            this.asyncExecutor = asyncExecutor;
            Runable += new ThreadStart(Run);
        }

        private readonly object syncRoot = new object();

        /// <summary>
        /// 
        /// </summary>
        public virtual void Run()
        {
            lock (syncRoot)
            {
                Thread.CurrentThread.Name = "activiti-reset-expired-jobs";
                log.LogInformation($"{Thread.CurrentThread.Name} starting to reset expired jobs");

                while (!isInterrupted)
                {
                    try
                    {
                        IList<IJobEntity> expiredJobs = asyncExecutor.ProcessEngineConfiguration.CommandExecutor.Execute(new FindExpiredJobsCmd(asyncExecutor.ResetExpiredJobsPageSize));

                        IList<string> expiredJobIds = new List<string>(expiredJobs.Count);
                        foreach (IJobEntity expiredJob in expiredJobs)
                        {
                            expiredJobIds.Add(expiredJob.Id);
                        }

                        if (expiredJobIds.Count > 0)
                        {
                            asyncExecutor.ProcessEngineConfiguration.CommandExecutor.Execute(new ResetExpiredJobsCmd(expiredJobIds));
                        }
                    }
                    catch (Exception e)
                    {
                        if (e is ActivitiOptimisticLockingException)
                        {
                            log.LogDebug($"Optmistic lock exception while resetting locked jobs {e.Message}");
                        }
                        else
                        {
                            log.LogError($"exception during resetting expired jobs {e.Message}");
                        }
                    }

                    // Sleep
                    try
                    {
                        lock (MONITOR)
                        {
                            if (!isInterrupted)
                            {
                                isWaiting = true;
                                Monitor.Wait(MONITOR, TimeSpan.FromMilliseconds(asyncExecutor.ResetExpiredJobsInterval));
                            }
                        }
                    }
                    catch (ThreadInterruptedException)
                    {
                        if (log.IsEnabled(LogLevel.Debug))
                        {
                            log.LogDebug("async reset expired jobs wait interrupted");
                        }
                    }
                    finally
                    {
                        isWaiting = false;//.set(false);
                    }

                }

                log.LogInformation("stopped resetting expired jobs");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void Stop()
        {
            lock (MONITOR)
            {
                isInterrupted = true;
                if (isWaiting == true)//.compareAndSet(true, false))
                {
                    Monitor.PulseAll(MONITOR);
                }
            }
        }
    }
}