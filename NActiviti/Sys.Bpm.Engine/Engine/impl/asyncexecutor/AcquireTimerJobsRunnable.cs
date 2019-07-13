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

    /// <summary>
    /// 
    /// </summary>
    public class AcquireTimerJobsRunnable
    {
        private static readonly ILogger log = ProcessEngineServiceProvider.LoggerService<AcquireTimerJobsRunnable>();

        /// <summary>
        /// 
        /// </summary>
        protected internal readonly IAsyncExecutor asyncExecutor;

        /// <summary>
        /// 
        /// </summary>
        protected internal readonly IJobManager jobManager;

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
        protected internal bool isWaiting = false;

        /// <summary>
        /// 
        /// </summary>
        protected internal ThreadStart Runable { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        protected internal long millisToWait;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="asyncExecutor"></param>
        /// <param name="jobManager"></param>
        public AcquireTimerJobsRunnable(IAsyncExecutor asyncExecutor, IJobManager jobManager)
        {
            this.asyncExecutor = asyncExecutor;
            this.jobManager = jobManager;

            this.Runable += new ThreadStart(Run);
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void Run()
        {
            log.LogInformation("starting to acquire async jobs due");
            Thread.CurrentThread.Name = "activiti-acquire-timer-jobs";

            ICommandExecutor commandExecutor = asyncExecutor.ProcessEngineConfiguration.CommandExecutor;

            while (!isInterrupted)
            {
                try
                {
                    AcquiredTimerJobEntities acquiredJobs = commandExecutor.Execute(new AcquireTimerJobsCmd(asyncExecutor));

                    if (acquiredJobs is object)
                    {
                        commandExecutor.Execute(new CommandAnonymousInnerClass(this, acquiredJobs));

                        // if all jobs were executed
                        millisToWait = asyncExecutor.DefaultTimerJobAcquireWaitTimeInMillis;
                        int jobsAcquired = acquiredJobs.Size();
                        if (jobsAcquired >= asyncExecutor.MaxTimerJobsPerAcquisition)
                        {
                            millisToWait = 0;
                        }
                    }
                }
                catch (ActivitiOptimisticLockingException optimisticLockingException)
                {
                    if (log.IsEnabled(LogLevel.Debug))
                    {
                        log.LogDebug($@"Optimistic locking exception during timer job acquisition. 
If you have multiple timer executors running against the same database, 
this exception means that this thread tried to acquire a timer job, 
which already was acquired by another timer executor acquisition thread. 
This is expected behavior in a clustered environment. 
You can ignore this message if you indeed have multiple timer executor acquisition threads running against the same database. 
Exception message: {optimisticLockingException.Message}");
                    }
                }
                catch (ObjectDisposedException ex)
                {
                    if (log.IsEnabled(LogLevel.Debug))
                    {
                        log.LogDebug($@"Optimistic locking exception during timer job acquisition. 
If you have multiple timer executors running against the same database, 
this exception means that this thread tried to acquire a timer job, 
which already was acquired by another timer executor acquisition thread. 
This is expected behavior in a clustered environment. 
You can ignore this message if you indeed have multiple timer executor acquisition threads running against the same database. 
Exception message: {ex.Message}");
                    }
                }
                catch (Exception e)
                {
                    log.LogError($"exception during timer job acquisition: {e.Message}");
                    millisToWait = asyncExecutor.DefaultTimerJobAcquireWaitTimeInMillis;
                }

                if (millisToWait > 0)
                {
                    try
                    {
                        //if (log.IsEnabled(LogLevel.Debug))
                        //{
                        //    log.LogDebug($"timer job acquisition thread sleeping for {millisToWait} millis");
                        //}
                        lock (MONITOR)
                        {
                            if (!isInterrupted)
                            {
                                isWaiting = true;
                                Monitor.Wait(MONITOR, TimeSpan.FromMilliseconds(millisToWait));
                            }
                        }

                        //if (log.IsEnabled(LogLevel.Debug))
                        //{
                        //    log.LogDebug("timer job acquisition thread woke up");
                        //}
                    }
                    catch (Exception e)
                    {
                        if (log.IsEnabled(LogLevel.Debug))
                        {
                            log.LogDebug("timer job acquisition wait interrupted");
                        }
                    }
                    finally
                    {
                        isWaiting = false;
                    }
                }
            }

            log.LogInformation("stopped async job due acquisition");
        }

        private class CommandAnonymousInnerClass : ICommand<object>
        {
            private readonly AcquireTimerJobsRunnable outerInstance;

            private readonly AcquiredTimerJobEntities acquiredJobs;

            public CommandAnonymousInnerClass(AcquireTimerJobsRunnable outerInstance, AcquiredTimerJobEntities acquiredJobs)
            {
                this.outerInstance = outerInstance;
                this.acquiredJobs = acquiredJobs;
            }


            public virtual object Execute(ICommandContext commandContext)
            {
                try
                {
                    foreach (ITimerJobEntity job in acquiredJobs.Jobs)
                    {
                        outerInstance.jobManager.MoveTimerJobToExecutableJob(job);
                    }
                    return commandContext.GetResult();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
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
                if (isWaiting == true)
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