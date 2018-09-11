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

namespace org.activiti.engine.impl.asyncexecutor.multitenant
{

    using org.activiti.engine.impl.cfg;
    using org.activiti.engine.impl.cfg.multitenant;
    using org.activiti.engine.runtime;

    /// <summary>
    /// An <seealso cref="IAsyncExecutor"/> that has one <seealso cref="IAsyncExecutor"/> per tenant.
    /// So each tenant has its own acquiring threads and it's own threadpool for executing jobs.
    /// 
    /// 
    /// </summary>
    public class ExecutorPerTenantAsyncExecutor : ITenantAwareAsyncExecutor
    {

        //private static readonly Logger logger = LoggerFactory.getLogger(typeof(ExecutorPerTenantAsyncExecutor));

        protected internal ITenantInfoHolder tenantInfoHolder;
        protected internal ITenantAwareAsyncExecutorFactory tenantAwareAyncExecutorFactory;

        protected internal IDictionary<string, IAsyncExecutor> tenantExecutors = new Dictionary<string, IAsyncExecutor>();

        protected internal ProcessEngineConfigurationImpl processEngineConfiguration;
        protected internal bool active;
        protected internal bool autoActivate;

        public ExecutorPerTenantAsyncExecutor(ITenantInfoHolder tenantInfoHolder) : this(tenantInfoHolder, null)
        {
        }

        public ExecutorPerTenantAsyncExecutor(ITenantInfoHolder tenantInfoHolder, ITenantAwareAsyncExecutorFactory tenantAwareAyncExecutorFactory)
        {
            this.tenantInfoHolder = tenantInfoHolder;
            this.tenantAwareAyncExecutorFactory = tenantAwareAyncExecutorFactory;
        }

        public virtual ICollection<string> TenantIds
        {
            get
            {
                return tenantExecutors.Keys;
            }
        }

        public virtual void addTenantAsyncExecutor(string tenantId, bool startExecutor)
        {
            IAsyncExecutor tenantExecutor = null;

            if (tenantAwareAyncExecutorFactory == null)
            {
                tenantExecutor = new DefaultAsyncJobExecutor();
            }
            else
            {
                tenantExecutor = tenantAwareAyncExecutorFactory.createAsyncExecutor(tenantId);
            }

            tenantExecutor.ProcessEngineConfiguration = processEngineConfiguration;

            if (tenantExecutor is DefaultAsyncJobExecutor)
            {
                DefaultAsyncJobExecutor defaultAsyncJobExecutor = (DefaultAsyncJobExecutor)tenantExecutor;
                defaultAsyncJobExecutor.AsyncJobsDueRunnable = new TenantAwareAcquireAsyncJobsDueRunnable(defaultAsyncJobExecutor, tenantInfoHolder, tenantId);
                defaultAsyncJobExecutor.TimerJobRunnable = new TenantAwareAcquireTimerJobsRunnable(defaultAsyncJobExecutor, tenantInfoHolder, tenantId);
                defaultAsyncJobExecutor.ExecuteAsyncRunnableFactory = new TenantAwareExecuteAsyncRunnableFactory(tenantInfoHolder, tenantId);
                defaultAsyncJobExecutor.ResetExpiredJobsRunnable = new TenantAwareResetExpiredJobsRunnable(defaultAsyncJobExecutor, tenantInfoHolder, tenantId);
            }

            tenantExecutors[tenantId] = tenantExecutor;

            if (startExecutor)
            {
                tenantExecutor.start();
            }
        }

        public virtual void removeTenantAsyncExecutor(string tenantId)
        {
            shutdownTenantExecutor(tenantId);
            tenantExecutors.Remove(tenantId);
        }

        protected internal virtual IAsyncExecutor determineAsyncExecutor()
        {
            return tenantExecutors[tenantInfoHolder.CurrentTenantId];
        }

        public virtual bool executeAsyncJob(IJob job)
        {
            return determineAsyncExecutor().executeAsyncJob(job);
        }

        public virtual IJobManager JobManager
        {
            get
            {
                // Should never be accessed on this class, should be accessed on the actual AsyncExecutor
                throw new System.NotSupportedException();
            }
        }

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


        public virtual bool Active
        {
            get
            {
                return active;
            }
        }

        public virtual void start()
        {
            foreach (IAsyncExecutor asyncExecutor in tenantExecutors.Values)
            {
                asyncExecutor.start();
            }
            active = true;
        }

        public virtual void shutdown()
        {
            lock (this)
            {
                foreach (string tenantId in tenantExecutors.Keys)
                {
                    shutdownTenantExecutor(tenantId);
                }
                active = false;
            }
        }

        protected internal virtual void shutdownTenantExecutor(string tenantId)
        {
            //logger.info("Shutting down async executor for tenant " + tenantId);
            tenantExecutors[tenantId].shutdown();
        }

        public virtual string LockOwner
        {
            get
            {
                return determineAsyncExecutor().LockOwner;
            }
        }

        public virtual int TimerLockTimeInMillis
        {
            get
            {
                return determineAsyncExecutor().TimerLockTimeInMillis;
            }
            set
            {
                foreach (IAsyncExecutor asyncExecutor in tenantExecutors.Values)
                {
                    asyncExecutor.TimerLockTimeInMillis = value;
                }
            }
        }


        public virtual int AsyncJobLockTimeInMillis
        {
            get
            {
                return determineAsyncExecutor().AsyncJobLockTimeInMillis;
            }
            set
            {
                foreach (IAsyncExecutor asyncExecutor in tenantExecutors.Values)
                {
                    asyncExecutor.AsyncJobLockTimeInMillis = value;
                }
            }
        }


        public virtual int DefaultTimerJobAcquireWaitTimeInMillis
        {
            get
            {
                return determineAsyncExecutor().DefaultTimerJobAcquireWaitTimeInMillis;
            }
            set
            {
                foreach (IAsyncExecutor asyncExecutor in tenantExecutors.Values)
                {
                    asyncExecutor.DefaultTimerJobAcquireWaitTimeInMillis = value;
                }
            }
        }


        public virtual int DefaultAsyncJobAcquireWaitTimeInMillis
        {
            get
            {
                return determineAsyncExecutor().DefaultAsyncJobAcquireWaitTimeInMillis;
            }
            set
            {
                foreach (IAsyncExecutor asyncExecutor in tenantExecutors.Values)
                {
                    asyncExecutor.DefaultAsyncJobAcquireWaitTimeInMillis = value;
                }
            }
        }


        public virtual int DefaultQueueSizeFullWaitTimeInMillis
        {
            get
            {
                return determineAsyncExecutor().DefaultQueueSizeFullWaitTimeInMillis;
            }
            set
            {
                foreach (IAsyncExecutor asyncExecutor in tenantExecutors.Values)
                {
                    asyncExecutor.DefaultQueueSizeFullWaitTimeInMillis = value;
                }
            }
        }


        public virtual int MaxAsyncJobsDuePerAcquisition
        {
            get
            {
                return determineAsyncExecutor().MaxAsyncJobsDuePerAcquisition;
            }
            set
            {
                foreach (IAsyncExecutor asyncExecutor in tenantExecutors.Values)
                {
                    asyncExecutor.MaxAsyncJobsDuePerAcquisition = value;
                }
            }
        }


        public virtual int MaxTimerJobsPerAcquisition
        {
            get
            {
                return determineAsyncExecutor().MaxTimerJobsPerAcquisition;
            }
            set
            {
                foreach (IAsyncExecutor asyncExecutor in tenantExecutors.Values)
                {
                    asyncExecutor.MaxTimerJobsPerAcquisition = value;
                }
            }
        }


        public virtual int RetryWaitTimeInMillis
        {
            get
            {
                return determineAsyncExecutor().RetryWaitTimeInMillis;
            }
            set
            {
                foreach (IAsyncExecutor asyncExecutor in tenantExecutors.Values)
                {
                    asyncExecutor.RetryWaitTimeInMillis = value;
                }
            }
        }


        public virtual int ResetExpiredJobsInterval
        {
            get
            {
                return determineAsyncExecutor().ResetExpiredJobsInterval;
            }
            set
            {
                foreach (IAsyncExecutor asyncExecutor in tenantExecutors.Values)
                {
                    asyncExecutor.ResetExpiredJobsInterval = value;
                }
            }
        }


        public virtual int ResetExpiredJobsPageSize
        {
            get
            {
                return determineAsyncExecutor().ResetExpiredJobsPageSize;
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