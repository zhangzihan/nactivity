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
namespace org.activiti.engine.impl.asyncexecutor
{
    using Microsoft.Extensions.Logging;
    using org.activiti.engine.impl.cmd;
    using org.activiti.engine.impl.interceptor;
    using org.activiti.engine.impl.persistence.entity;
    using Sys;

    public class AcquireTimerJobsRunnable
    {
        private static ILogger log = ProcessEngineServiceProvider.LoggerService<AcquireTimerJobsRunnable>();

        protected internal readonly IAsyncExecutor asyncExecutor;
        protected internal readonly IJobManager jobManager;

        protected internal volatile bool isInterrupted;
        protected internal readonly object MONITOR = new object();
        protected internal bool isWaiting = false;

        protected internal ThreadStart Runable { get; private set; }

        protected internal long millisToWait;

        public AcquireTimerJobsRunnable(IAsyncExecutor asyncExecutor, IJobManager jobManager)
        {
            this.asyncExecutor = asyncExecutor;
            this.jobManager = jobManager;

            this.Runable += new ThreadStart(run);
        }

        public virtual void run()
        {
            lock (this)
            {
                log.LogInformation("starting to acquire async jobs due");
                Thread.CurrentThread.Name = "activiti-acquire-timer-jobs";

                ICommandExecutor commandExecutor = asyncExecutor.ProcessEngineConfiguration.CommandExecutor;

                while (!isInterrupted)
                {
                    try
                    {
                        AcquiredTimerJobEntities acquiredJobs = commandExecutor.execute(new AcquireTimerJobsCmd(asyncExecutor));

                        commandExecutor.execute(new CommandAnonymousInnerClass(this, acquiredJobs));

                        // if all jobs were executed
                        millisToWait = asyncExecutor.DefaultTimerJobAcquireWaitTimeInMillis;
                        int jobsAcquired = acquiredJobs.size();
                        if (jobsAcquired >= asyncExecutor.MaxTimerJobsPerAcquisition)
                        {
                            millisToWait = 0;
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
                    catch (Exception e)
                    {
                        log.LogError($"exception during timer job acquisition: {e.Message}");
                        millisToWait = asyncExecutor.DefaultTimerJobAcquireWaitTimeInMillis;
                    }

                    if (millisToWait > 0)
                    {
                        try
                        {
                            if (log.IsEnabled(LogLevel.Debug))
                            {
                                log.LogDebug($"timer job acquisition thread sleeping for {millisToWait} millis");
                            }
                            lock (MONITOR)
                            {
                                if (!isInterrupted)
                                {
                                    isWaiting = true;
                                    Monitor.Wait(MONITOR, TimeSpan.FromMilliseconds(millisToWait));
                                }
                            }

                            if (log.IsEnabled(LogLevel.Debug))
                            {
                                log.LogDebug("timer job acquisition thread woke up");
                            }
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
        }

        private class CommandAnonymousInnerClass : ICommand<object>
        {
            private readonly AcquireTimerJobsRunnable outerInstance;

            private org.activiti.engine.impl.asyncexecutor.AcquiredTimerJobEntities acquiredJobs;

            public CommandAnonymousInnerClass(AcquireTimerJobsRunnable outerInstance, org.activiti.engine.impl.asyncexecutor.AcquiredTimerJobEntities acquiredJobs)
            {
                this.outerInstance = outerInstance;
                this.acquiredJobs = acquiredJobs;
            }


            public virtual object execute(ICommandContext commandContext)
            {
                foreach (ITimerJobEntity job in acquiredJobs.Jobs)
                {
                    outerInstance.jobManager.moveTimerJobToExecutableJob(job);
                }
                return commandContext.GetResult();
            }
        }

        public virtual void stop()
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