using System.Collections.Generic;

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

namespace Sys.Workflow.Engine.Impl.Asyncexecutor.Multitenants
{
    using Microsoft.Extensions.Logging;
    using Sys.Workflow.Engine.Impl.Cfg;
    using Sys.Workflow.Engine.Impl.Cfg.Multitenants;
    using Sys.Workflow.Engine.Runtime;
    using Sys.Workflow;
    using System;
    using System.Collections.Concurrent;

    /// <summary>
    /// An <seealso cref="IAsyncExecutor"/> that has one <seealso cref="IAsyncExecutor"/> per tenant.
    /// So each tenant has its own acquiring threads and it's own threadpool for executing jobs.
    /// 
    /// 
    /// </summary>
    public class ExecutorPerTenantAsyncExecutor : ITenantAwareAsyncExecutor
    {
        private static readonly ILogger logger = ProcessEngineServiceProvider.LoggerService<ExecutorPerTenantAsyncExecutor>();

        /// <summary>
        /// 
        /// </summary>
        protected internal ITenantInfoHolder tenantInfoHolder;

        /// <summary>
        /// 
        /// </summary>
        protected internal ITenantAwareAsyncExecutorFactory tenantAwareAyncExecutorFactory;

        /// <summary>
        /// 
        /// </summary>
        protected internal ConcurrentDictionary<string, IAsyncExecutor> tenantExecutors = new ConcurrentDictionary<string, IAsyncExecutor>();

        /// <summary>
        /// 
        /// </summary>
        protected internal ProcessEngineConfigurationImpl processEngineConfiguration;

        /// <summary>
        /// 
        /// </summary>
        protected internal bool active;

        /// <summary>
        /// 
        /// </summary>
        protected internal bool autoActivate;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tenantInfoHolder"></param>
        public ExecutorPerTenantAsyncExecutor(ITenantInfoHolder tenantInfoHolder) : this(tenantInfoHolder, null)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tenantInfoHolder"></param>
        /// <param name="tenantAwareAyncExecutorFactory"></param>
        public ExecutorPerTenantAsyncExecutor(ITenantInfoHolder tenantInfoHolder, ITenantAwareAsyncExecutorFactory tenantAwareAyncExecutorFactory)
        {
            this.tenantInfoHolder = tenantInfoHolder;
            this.tenantAwareAyncExecutorFactory = tenantAwareAyncExecutorFactory;
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual ICollection<string> TenantIds
        {
            get
            {
                return tenantExecutors.Keys;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="startExecutor"></param>
        public virtual void AddTenantAsyncExecutor(string tenantId, bool startExecutor)
        {
            IAsyncExecutor tenantExecutor;
            if (tenantAwareAyncExecutorFactory == null)
            {
                tenantExecutor = new DefaultAsyncJobExecutor();
            }
            else
            {
                tenantExecutor = tenantAwareAyncExecutorFactory.CreateAsyncExecutor(tenantId);
            }

            tenantExecutor.ProcessEngineConfiguration = processEngineConfiguration;

            if (tenantExecutor is DefaultAsyncJobExecutor defaultAsyncJobExecutor)
            {
                defaultAsyncJobExecutor.AsyncJobsDueRunnable = new TenantAwareAcquireAsyncJobsDueRunnable(defaultAsyncJobExecutor, tenantInfoHolder, tenantId);
                defaultAsyncJobExecutor.TimerJobRunnable = new TenantAwareAcquireTimerJobsRunnable(defaultAsyncJobExecutor, tenantInfoHolder, tenantId);
                defaultAsyncJobExecutor.ExecuteAsyncRunnableFactory = new TenantAwareExecuteAsyncRunnableFactory(tenantInfoHolder, tenantId);
                defaultAsyncJobExecutor.ResetExpiredJobsRunnable = new TenantAwareResetExpiredJobsRunnable(defaultAsyncJobExecutor, tenantInfoHolder, tenantId);
            }

            tenantExecutors.AddOrUpdate(tenantId, tenantExecutor, (key, old) => tenantExecutor);

            if (startExecutor)
            {
                tenantExecutor.Start();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tenantId"></param>
        public virtual void RemoveTenantAsyncExecutor(string tenantId)
        {
            ShutdownTenantExecutor(tenantId);

            tenantExecutors.TryRemove(tenantId, out _);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected internal virtual IAsyncExecutor DetermineAsyncExecutor()
        {
            tenantExecutors.TryGetValue(tenantInfoHolder.CurrentTenantId, out var value);
            return value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="job"></param>
        /// <returns></returns>
        public virtual bool ExecuteAsyncJob(IJob job)
        {
            return DetermineAsyncExecutor().ExecuteAsyncJob(job);
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IJobManager JobManager
        {
            get
            {
                // Should never be accessed on this class, should be accessed on the actual AsyncExecutor
                throw new System.NotSupportedException();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual ProcessEngineConfigurationImpl ProcessEngineConfiguration
        {
            set
            {
                this.processEngineConfiguration = value;
                foreach (IAsyncExecutor asyncExecutor in tenantExecutors.Values)
                {
                    asyncExecutor.ProcessEngineConfiguration = value;
                }
            }
            get
            {
                throw new System.NotSupportedException();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual bool AutoActivate
        {
            get
            {
                return autoActivate;
            }
            set
            {
                autoActivate = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual bool Active
        {
            get
            {
                return active;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void Start()
        {
            foreach (IAsyncExecutor asyncExecutor in tenantExecutors.Values)
            {
                asyncExecutor.Start();
            }
            active = true;
        }

        private readonly object syncRoot = new object();

        /// <summary>
        /// 
        /// </summary>
        public virtual void Shutdown()
        {
            lock (syncRoot)
            {
                foreach (string tenantId in tenantExecutors.Keys)
                {
                    ShutdownTenantExecutor(tenantId);
                }
                active = false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tenantId"></param>
        protected internal virtual void ShutdownTenantExecutor(string tenantId)
        {
            logger.LogInformation("Shutting down async executor for tenant " + tenantId);
            if (tenantExecutors.TryGetValue(tenantId, out var value))
            {
                value.Shutdown();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual string LockOwner
        {
            get
            {
                return DetermineAsyncExecutor().LockOwner;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual int TimerLockTimeInMillis
        {
            get
            {
                return DetermineAsyncExecutor().TimerLockTimeInMillis;
            }
            set
            {
                foreach (IAsyncExecutor asyncExecutor in tenantExecutors.Values)
                {
                    asyncExecutor.TimerLockTimeInMillis = value;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual int AsyncJobLockTimeInMillis
        {
            get
            {
                return DetermineAsyncExecutor().AsyncJobLockTimeInMillis;
            }
            set
            {
                foreach (IAsyncExecutor asyncExecutor in tenantExecutors.Values)
                {
                    asyncExecutor.AsyncJobLockTimeInMillis = value;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual int DefaultTimerJobAcquireWaitTimeInMillis
        {
            get
            {
                return DetermineAsyncExecutor().DefaultTimerJobAcquireWaitTimeInMillis;
            }
            set
            {
                foreach (IAsyncExecutor asyncExecutor in tenantExecutors.Values)
                {
                    asyncExecutor.DefaultTimerJobAcquireWaitTimeInMillis = value;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual int DefaultAsyncJobAcquireWaitTimeInMillis
        {
            get
            {
                return DetermineAsyncExecutor().DefaultAsyncJobAcquireWaitTimeInMillis;
            }
            set
            {
                foreach (IAsyncExecutor asyncExecutor in tenantExecutors.Values)
                {
                    asyncExecutor.DefaultAsyncJobAcquireWaitTimeInMillis = value;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual int DefaultQueueSizeFullWaitTimeInMillis
        {
            get
            {
                return DetermineAsyncExecutor().DefaultQueueSizeFullWaitTimeInMillis;
            }
            set
            {
                foreach (IAsyncExecutor asyncExecutor in tenantExecutors.Values)
                {
                    asyncExecutor.DefaultQueueSizeFullWaitTimeInMillis = value;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual int MaxAsyncJobsDuePerAcquisition
        {
            get
            {
                return DetermineAsyncExecutor().MaxAsyncJobsDuePerAcquisition;
            }
            set
            {
                foreach (IAsyncExecutor asyncExecutor in tenantExecutors.Values)
                {
                    asyncExecutor.MaxAsyncJobsDuePerAcquisition = value;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual int MaxTimerJobsPerAcquisition
        {
            get
            {
                return DetermineAsyncExecutor().MaxTimerJobsPerAcquisition;
            }
            set
            {
                foreach (IAsyncExecutor asyncExecutor in tenantExecutors.Values)
                {
                    asyncExecutor.MaxTimerJobsPerAcquisition = value;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual int RetryWaitTimeInMillis
        {
            get
            {
                return DetermineAsyncExecutor().RetryWaitTimeInMillis;
            }
            set
            {
                foreach (IAsyncExecutor asyncExecutor in tenantExecutors.Values)
                {
                    asyncExecutor.RetryWaitTimeInMillis = value;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual int ResetExpiredJobsInterval
        {
            get
            {
                return DetermineAsyncExecutor().ResetExpiredJobsInterval;
            }
            set
            {
                foreach (IAsyncExecutor asyncExecutor in tenantExecutors.Values)
                {
                    asyncExecutor.ResetExpiredJobsInterval = value;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual int ResetExpiredJobsPageSize
        {
            get
            {
                return DetermineAsyncExecutor().ResetExpiredJobsPageSize;
            }
            set
            {
                foreach (IAsyncExecutor asyncExecutor in tenantExecutors.Values)
                {
                    asyncExecutor.ResetExpiredJobsPageSize = value;
                }
            }
        }
    }
}