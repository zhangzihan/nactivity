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
namespace org.activiti.engine.impl.asyncexecutor
{
    using Microsoft.Extensions.Logging;
    using org.activiti.engine.impl.cfg;
    using org.activiti.engine.impl.context;
    using org.activiti.engine.impl.interceptor;
    using org.activiti.engine.runtime;
    using Sys.Concurrent;
    using Sys.Workflow;
    using System;
    using System.Collections.Concurrent;

    /// 
    /// 
    public class DefaultAsyncJobExecutor : IAsyncExecutor
    {
        /// <summary>
        /// The minimal number of threads that are kept alive in the threadpool for job execution
        /// </summary>
        protected internal int corePoolSize = 2;

        /// <summary>
        /// The maximum number of threads that are kept alive in the threadpool for job execution
        /// </summary>
        protected internal int maxPoolSize = 10;

        /// <summary>
        /// The time (in milliseconds) a thread used for job execution must be kept alive before it is destroyed. Default setting is 0. Having a non-default setting of 0 takes resources, but in the case of
        /// many job executions it avoids creating new threads all the time.
        /// </summary>
        protected internal long keepAliveTime = 5000L;

        /// <summary>
        /// The size of the queue on which jobs to be executed are placed </summary>
        protected internal int queueSize = 100;

        /// <summary>
        /// The queue used for job execution work </summary>
        protected internal ConcurrentQueue<ThreadStart> threadPoolQueue;

        /// <summary>
        /// The executor service used for job execution </summary>
        protected internal IExecutorService executorService;

        /// <summary>
        /// The time (in seconds) that is waited to gracefully shut down the threadpool used for job execution
        /// </summary>
        protected internal long secondsToWaitOnShutdown = 60L;

        /// <summary>
        /// 
        /// </summary>
        protected internal Thread timerJobAcquisitionThread;

        /// <summary>
        /// 
        /// </summary>
        protected internal Thread asyncJobAcquisitionThread;

        /// <summary>
        /// 
        /// </summary>
        protected internal Thread resetExpiredJobThread;

        /// <summary>
        /// 
        /// </summary>
        protected internal AcquireTimerJobsRunnable timerJobRunnable;

        /// <summary>
        /// 
        /// </summary>
        protected internal AcquireAsyncJobsDueRunnable asyncJobsDueRunnable;

        /// <summary>
        /// 
        /// </summary>
        protected internal ResetExpiredJobsRunnable resetExpiredJobsRunnable;

        /// <summary>
        /// 
        /// </summary>
        protected internal IExecuteAsyncRunnableFactory executeAsyncRunnableFactory;

        /// <summary>
        /// 
        /// </summary>
        protected internal bool isAutoActivate;

        /// <summary>
        /// 
        /// </summary>
        protected internal bool isActive;

        /// <summary>
        /// 
        /// </summary>
        protected internal bool isMessageQueueMode;

        /// <summary>
        /// 
        /// </summary>
        protected internal int maxTimerJobsPerAcquisition = 1;

        /// <summary>
        /// 
        /// </summary>
        protected internal int maxAsyncJobsDuePerAcquisition = 1;

        /// <summary>
        /// 
        /// </summary>
        protected internal int defaultTimerJobAcquireWaitTimeInMillis = 10 * 1000;

        /// <summary>
        /// 
        /// </summary>
        protected internal int defaultAsyncJobAcquireWaitTimeInMillis = 10 * 1000;

        /// <summary>
        /// 
        /// </summary>
        protected internal int defaultQueueSizeFullWaitTime = 0;

        /// <summary>
        /// 
        /// </summary>
        protected internal string lockOwner = Guid.NewGuid().ToString();

        /// <summary>
        /// 
        /// </summary>
        protected internal int timerLockTimeInMillis = 5 * 60 * 1000;

        /// <summary>
        /// 
        /// </summary>
        protected internal int asyncJobLockTimeInMillis = 5 * 60 * 1000;

        /// <summary>
        /// 
        /// </summary>
        protected internal int retryWaitTimeInMillis = 500;

        /// <summary>
        /// 
        /// </summary>
        protected internal int resetExpiredJobsInterval = 60 * 1000;

        /// <summary>
        /// 
        /// </summary>
        protected internal int resetExpiredJobsPageSize = 3;

        /// <summary>
        /// Job queue used when async executor is not yet started and jobs are already added.
        /// This is mainly used for testing purpose.
        /// </summary>
        protected internal ConcurrentQueue<IJob> temporaryJobQueue = new ConcurrentQueue<IJob>();

        /// <summary>
        /// 
        /// </summary>
        protected internal ProcessEngineConfigurationImpl processEngineConfiguration;

        private static readonly ILogger logger = ProcessEngineServiceProvider.LoggerService<DefaultAsyncJobExecutor>();

        public DefaultAsyncJobExecutor()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="job"></param>
        /// <returns></returns>
        public virtual bool ExecuteAsyncJob(IJob job)
        {

            if (isMessageQueueMode)
            {
                // When running with a message queue based job executor,
                // the job is not executed here.
                return true;
            }

            if (isActive)
            {
                ThreadStart runnable = CreateRunnableForJob(job);

                try
                {
                    executorService.Execute(runnable);
                }
                catch (Exception e)
                {
                    // When a RejectedExecutionException is caught, this means that the queue for holding the jobs 
                    // that are to be executed is full and can't store more.
                    // The job is now 'unlocked', meaning that the lock owner/time is set to null,
                    // so other executors can pick the job up (or this async executor, the next time the 
                    // acquire query is executed.

                    // This can happen while already in a command context (for example in a transaction listener
                    // after the async executor has been hinted that a new async job is created)
                    // or not (when executed in the acquire thread runnable)

                    ICommandContext commandContext = Context.CommandContext;
                    if (commandContext != null)
                    {
                        commandContext.JobManager.Unacquire(job);
                    }
                    else
                    {
                        processEngineConfiguration.CommandExecutor.Execute(new CommandAnonymousInnerClass(this, job));
                    }

                    // Job queue full, returning true so (if wanted) the acquiring can be throttled
                    return false;
                }
            }
            else
            {
                temporaryJobQueue.Enqueue(job);
            }

            return true;
        }

        private class CommandAnonymousInnerClass : ICommand<object>
        {
            private readonly DefaultAsyncJobExecutor outerInstance;

            private readonly IJob job;

            public CommandAnonymousInnerClass(DefaultAsyncJobExecutor outerInstance, IJob job)
            {
                this.outerInstance = outerInstance;
                this.job = job;
            }

            public virtual object Execute(ICommandContext commandContext)
            {
                commandContext.JobManager.Unacquire(job);
                return commandContext.GetResult();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="job"></param>
        /// <returns></returns>
        protected internal virtual ThreadStart CreateRunnableForJob(IJob job)
        {
            if (executeAsyncRunnableFactory == null)
            {
                var runable = new ExecuteAsyncRunnable(job, processEngineConfiguration);

                return runable.Runable;
            }
            else
            {
                return executeAsyncRunnableFactory.CreateExecuteAsyncRunnable(job, processEngineConfiguration);
            }
        }

        /// <summary>
        /// Starts the async executor </summary>
        public virtual void Start()
        {
            if (isActive)
            {
                return;
            }

            logger.LogInformation($"Starting up the default async job executor [{this.GetType().FullName}].");

            if (timerJobRunnable == null)
            {
                timerJobRunnable = new AcquireTimerJobsRunnable(this, processEngineConfiguration.JobManager);
            }

            if (resetExpiredJobsRunnable == null)
            {
                resetExpiredJobsRunnable = new ResetExpiredJobsRunnable(this);
            }

            if (!isMessageQueueMode && asyncJobsDueRunnable == null)
            {
                asyncJobsDueRunnable = new AcquireAsyncJobsDueRunnable(this);
            }

            if (!isMessageQueueMode)
            {
                InitAsyncJobExecutionThreadPool();
                StartJobAcquisitionThread();
            }

            StartTimerAcquisitionThread();
            StartResetExpiredJobsThread();

            isActive = true;

            ExecuteTemporaryJobs();
        }

        /// <summary>
        /// 
        /// </summary>
        protected internal virtual void ExecuteTemporaryJobs()
        {
            while (temporaryJobQueue.IsEmpty == false)
            {
                if (temporaryJobQueue.TryDequeue(out IJob job))
                {
                    ExecuteAsyncJob(job);
                }
            }
        }

        private readonly object syncRoot = new object();

        /// <summary>
        /// Shuts down the whole job executor </summary>
        public virtual void Shutdown()
        {
            lock (syncRoot)
            {
                if (!isActive)
                {
                    return;
                }

                logger.LogInformation($"Shutting down the default async job executor [{this.GetType().FullName}].");

                if (timerJobRunnable != null)
                {
                    timerJobRunnable.Stop();
                }
                if (asyncJobsDueRunnable != null)
                {
                    asyncJobsDueRunnable.Stop();
                }
                if (resetExpiredJobsRunnable != null)
                {
                    resetExpiredJobsRunnable.Stop();
                }

                StopResetExpiredJobsThread();
                StopTimerAcquisitionThread();
                StopJobAcquisitionThread();
                StopExecutingAsyncJobs();

                timerJobRunnable = null;
                asyncJobsDueRunnable = null;
                resetExpiredJobsRunnable = null;

                isActive = false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected internal virtual void InitAsyncJobExecutionThreadPool()
        {
            if (threadPoolQueue == null)
            {
                logger.LogInformation($"Creating thread pool queue of size {queueSize}");
                threadPoolQueue = new ConcurrentQueue<ThreadStart>();
            }

            if (executorService == null)
            {
                logger.LogInformation($"Creating executor service with corePoolSize {corePoolSize}, maxPoolSize {maxPoolSize} and keepAliveTime {keepAliveTime}");
                //throw new NotImplementedException();
                //BasicThreadFactory threadFactory = (new BasicThreadFactory.Builder()).namingPattern("activiti-async-job-executor-thread-%d").build();
                executorService = new ThreadPoolExecutor(corePoolSize, maxPoolSize, keepAliveTime, threadPoolQueue);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected internal virtual void StopExecutingAsyncJobs()
        {
            if (executorService != null)
            {
                // Ask the thread pool to finish and exit
                executorService.Shutdown();

                // Waits for 1 minute to finish all currently executing jobs
                try
                {
                    if (!executorService.AwaitTermination(secondsToWaitOnShutdown, TimeSpan.FromSeconds(3000)))
                    {
                        logger.LogWarning($"Timeout during shutdown of async job executor. The current running jobs could not end within {secondsToWaitOnShutdown} seconds after shutdown operation.");
                    }
                }
                catch (ThreadInterruptedException e)
                {
                    logger.LogWarning($"Interrupted while shutting down the async job executor. {e}");
                }

                executorService = null;
            }
        }

        /// <summary>
        /// Starts the acquisition thread </summary>
        protected internal virtual void StartJobAcquisitionThread()
        {
            if (asyncJobAcquisitionThread == null)
            {
                asyncJobAcquisitionThread = new Thread(asyncJobsDueRunnable.Runable);
            }
            asyncJobAcquisitionThread.Start();
        }

        /// <summary>
        /// 
        /// </summary>
        protected internal virtual void StartTimerAcquisitionThread()
        {
            if (timerJobAcquisitionThread == null)
            {
                timerJobAcquisitionThread = new Thread(timerJobRunnable.Runable);
            }
            timerJobAcquisitionThread.Start();
        }

        /// <summary>
        /// Stops the acquisition thread </summary>
        protected internal virtual void StopJobAcquisitionThread()
        {
            if (asyncJobAcquisitionThread != null)
            {
                try
                {
                    asyncJobAcquisitionThread.Join();
                }
                catch (ThreadInterruptedException e)
                {
                    logger.LogWarning($"Interrupted while waiting for the async job acquisition thread to terminate {e}");
                }
                asyncJobAcquisitionThread = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected internal virtual void StopTimerAcquisitionThread()
        {
            if (timerJobAcquisitionThread != null)
            {
                try
                {
                    timerJobAcquisitionThread.Join();
                }
                catch (ThreadInterruptedException e)
                {
                    logger.LogWarning($"Interrupted while waiting for the timer job acquisition thread to terminate {e}");
                }
                timerJobAcquisitionThread = null;
            }
        }

        /// <summary>
        /// Starts the reset expired jobs thread </summary>
        protected internal virtual void StartResetExpiredJobsThread()
        {
            if (resetExpiredJobThread == null)
            {
                resetExpiredJobThread = new Thread(resetExpiredJobsRunnable.Runable);
            }
            resetExpiredJobThread.Start();
        }

        /// <summary>
        /// Stops the reset expired jobs thread </summary>
        protected internal virtual void StopResetExpiredJobsThread()
        {
            if (resetExpiredJobThread != null)
            {
                try
                {
                    resetExpiredJobThread.Join();
                }
                catch (ThreadInterruptedException e)
                {
                    logger.LogWarning($"Interrupted while waiting for the reset expired jobs thread to terminate {e}");
                }

                resetExpiredJobThread = null;
            }
        }

        /* getters and setters */

        /// <summary>
        /// 
        /// </summary>
        public virtual ProcessEngineConfigurationImpl ProcessEngineConfiguration
        {
            get
            {
                return processEngineConfiguration;
            }
            set
            {
                this.processEngineConfiguration = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual Thread TimerJobAcquisitionThread
        {
            get
            {
                return timerJobAcquisitionThread;
            }
            set
            {
                this.timerJobAcquisitionThread = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual Thread AsyncJobAcquisitionThread
        {
            get
            {
                return asyncJobAcquisitionThread;
            }
            set
            {
                this.asyncJobAcquisitionThread = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual Thread ResetExpiredJobThread
        {
            get
            {
                return resetExpiredJobThread;
            }
            set
            {
                this.resetExpiredJobThread = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual bool AutoActivate
        {
            get
            {
                return isAutoActivate;
            }
            set
            {
                this.isAutoActivate = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual bool Active
        {
            get
            {
                return isActive;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual bool MessageQueueMode
        {
            get
            {
                return isMessageQueueMode;
            }
            set
            {
                this.isMessageQueueMode = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual int QueueSize
        {
            get
            {
                return queueSize;
            }
            set
            {
                this.queueSize = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual int CorePoolSize
        {
            get
            {
                return corePoolSize;
            }
            set
            {
                this.corePoolSize = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual int MaxPoolSize
        {
            get
            {
                return maxPoolSize;
            }
            set
            {
                this.maxPoolSize = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual long KeepAliveTime
        {
            get
            {
                return keepAliveTime;
            }
            set
            {
                this.keepAliveTime = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual long SecondsToWaitOnShutdown
        {
            get
            {
                return secondsToWaitOnShutdown;
            }
            set
            {
                this.secondsToWaitOnShutdown = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual ConcurrentQueue<ThreadStart> ThreadPoolQueue
        {
            get
            {
                return threadPoolQueue;
            }
            set
            {
                this.threadPoolQueue = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IExecutorService ExecutorService
        {
            get
            {
                return executorService;
            }
            set
            {
                this.executorService = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual string LockOwner
        {
            get
            {
                return lockOwner;
            }
            set
            {
                this.lockOwner = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual int TimerLockTimeInMillis
        {
            get
            {
                return timerLockTimeInMillis;
            }
            set
            {
                this.timerLockTimeInMillis = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual int AsyncJobLockTimeInMillis
        {
            get
            {
                return asyncJobLockTimeInMillis;
            }
            set
            {
                this.asyncJobLockTimeInMillis = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual int MaxTimerJobsPerAcquisition
        {
            get
            {
                return maxTimerJobsPerAcquisition;
            }
            set
            {
                this.maxTimerJobsPerAcquisition = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual int MaxAsyncJobsDuePerAcquisition
        {
            get
            {
                return maxAsyncJobsDuePerAcquisition;
            }
            set
            {
                this.maxAsyncJobsDuePerAcquisition = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual int DefaultTimerJobAcquireWaitTimeInMillis
        {
            get
            {
                return defaultTimerJobAcquireWaitTimeInMillis;
            }
            set
            {
                this.defaultTimerJobAcquireWaitTimeInMillis = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual int DefaultAsyncJobAcquireWaitTimeInMillis
        {
            get
            {
                return defaultAsyncJobAcquireWaitTimeInMillis;
            }
            set
            {
                this.defaultAsyncJobAcquireWaitTimeInMillis = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual AcquireTimerJobsRunnable TimerJobRunnable
        {
            set
            {
                this.timerJobRunnable = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual int DefaultQueueSizeFullWaitTimeInMillis
        {
            get
            {
                return defaultQueueSizeFullWaitTime;
            }
            set
            {
                this.defaultQueueSizeFullWaitTime = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual AcquireAsyncJobsDueRunnable AsyncJobsDueRunnable
        {
            set
            {
                this.asyncJobsDueRunnable = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual ResetExpiredJobsRunnable ResetExpiredJobsRunnable
        {
            set
            {
                this.resetExpiredJobsRunnable = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual int RetryWaitTimeInMillis
        {
            get
            {
                return retryWaitTimeInMillis;
            }
            set
            {
                this.retryWaitTimeInMillis = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>

        public virtual int ResetExpiredJobsInterval
        {
            get
            {
                return resetExpiredJobsInterval;
            }
            set
            {
                this.resetExpiredJobsInterval = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>

        public virtual int ResetExpiredJobsPageSize
        {
            get
            {
                return resetExpiredJobsPageSize;
            }
            set
            {
                this.resetExpiredJobsPageSize = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IExecuteAsyncRunnableFactory ExecuteAsyncRunnableFactory
        {
            get
            {
                return executeAsyncRunnableFactory;
            }
            set
            {
                this.executeAsyncRunnableFactory = value;
            }
        }
    }
}