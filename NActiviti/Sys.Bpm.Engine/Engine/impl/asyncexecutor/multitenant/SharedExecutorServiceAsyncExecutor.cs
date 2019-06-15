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
namespace org.activiti.engine.impl.asyncexecutor.multitenant
{
    using Microsoft.Extensions.Logging;
    using org.activiti.engine.impl.cfg;
    using org.activiti.engine.impl.cfg.multitenant;
    using org.activiti.engine.runtime;
    using Sys.Workflow;
    using System;
    using System.Collections.Concurrent;

    /// <summary>
    /// Multi tenant <seealso cref="IAsyncExecutor"/>.
    /// 
    /// For each tenant, there will be acquire threads, but only one <seealso cref="ExecutorService"/> will be used
    /// once the jobs are acquired.
    /// 
    /// 
    /// </summary>
    public class SharedExecutorServiceAsyncExecutor : DefaultAsyncJobExecutor, ITenantAwareAsyncExecutor
    {
        private static readonly ILogger logger = ProcessEngineServiceProvider.LoggerService<SharedExecutorServiceAsyncExecutor>();

        /// <summary>
        /// 
        /// </summary>
        protected internal ITenantInfoHolder tenantInfoHolder;

        private readonly ConcurrentDictionary<string, Thread> timerJobAcquisitionThreads = new ConcurrentDictionary<string, Thread>();

        private readonly ConcurrentDictionary<string, TenantAwareAcquireTimerJobsRunnable> timerJobAcquisitionRunnables = new ConcurrentDictionary<string, TenantAwareAcquireTimerJobsRunnable>();

        private readonly ConcurrentDictionary<string, Thread> asyncJobAcquisitionThreads = new ConcurrentDictionary<string, Thread>();

        private readonly ConcurrentDictionary<string, TenantAwareAcquireAsyncJobsDueRunnable> asyncJobAcquisitionRunnables = new ConcurrentDictionary<string, TenantAwareAcquireAsyncJobsDueRunnable>();

        private readonly ConcurrentDictionary<string, Thread> resetExpiredJobsThreads = new ConcurrentDictionary<string, Thread>();

        private readonly ConcurrentDictionary<string, TenantAwareResetExpiredJobsRunnable> resetExpiredJobsRunnables = new ConcurrentDictionary<string, TenantAwareResetExpiredJobsRunnable>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tenantInfoHolder"></param>
        public SharedExecutorServiceAsyncExecutor(ITenantInfoHolder tenantInfoHolder)
        {
            this.tenantInfoHolder = tenantInfoHolder;

            ExecuteAsyncRunnableFactory = new ExecuteAsyncRunnableFactoryAnonymousInnerClass(this);
        }

        private class ExecuteAsyncRunnableFactoryAnonymousInnerClass : IExecuteAsyncRunnableFactory
        {
            private readonly SharedExecutorServiceAsyncExecutor outerInstance;

            public ExecuteAsyncRunnableFactoryAnonymousInnerClass(SharedExecutorServiceAsyncExecutor outerInstance)
            {
                this.outerInstance = outerInstance;
            }

            public virtual ThreadStart CreateExecuteAsyncRunnable(IJob job, ProcessEngineConfigurationImpl processEngineConfiguration)
            {

                // Here, the runnable will be created by for example the acquire thread, which has already set the current id.
                // But it will be executed later on, by the executorService and thus we need to set it explicitely again then
                throw new System.NotImplementedException();

                //return new TenantAwareExecuteAsyncRunnable(job, processEngineConfiguration, outerInstance.tenantInfoHolder, outerInstance.tenantInfoHolder.CurrentTenantId);
            }

        }

        /// <summary>
        /// 
        /// </summary>
        public virtual ICollection<string> TenantIds
        {
            get
            {
                return timerJobAcquisitionRunnables.Keys;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="startExecutor"></param>
        public virtual void AddTenantAsyncExecutor(string tenantId, bool startExecutor)
        {
            TenantAwareAcquireTimerJobsRunnable timerRunnable = new TenantAwareAcquireTimerJobsRunnable(this, tenantInfoHolder, tenantId);
            timerJobAcquisitionRunnables.AddOrUpdate(tenantId, timerRunnable, (key, old) => timerRunnable);
            Thread run = new Thread(timerRunnable.Runable);
            timerJobAcquisitionThreads.AddOrUpdate(tenantId, run, (key, old) => run);

            TenantAwareAcquireAsyncJobsDueRunnable asyncJobsRunnable = new TenantAwareAcquireAsyncJobsDueRunnable(this, tenantInfoHolder, tenantId);
            asyncJobAcquisitionRunnables.AddOrUpdate(tenantId, asyncJobsRunnable, (key, old) => asyncJobsRunnable);
            Thread run1 = new Thread(asyncJobsRunnable.Runable);
            asyncJobAcquisitionThreads.AddOrUpdate(tenantId, run1, (key, old) => run1);

            TenantAwareResetExpiredJobsRunnable resetExpiredJobsRunnable = new TenantAwareResetExpiredJobsRunnable(this, tenantInfoHolder, tenantId);
            resetExpiredJobsRunnables.AddOrUpdate(tenantId, resetExpiredJobsRunnable, (key, old) => resetExpiredJobsRunnable);
            Thread run2 = new Thread(resetExpiredJobsRunnable.Runable);
            resetExpiredJobsThreads.AddOrUpdate(tenantId, run2, (key, old) => run2);

            if (startExecutor)
            {
                StartTimerJobAcquisitionForTenant(tenantId);
                StartAsyncJobAcquisitionForTenant(tenantId);
                StartResetExpiredJobsForTenant(tenantId);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tenantId"></param>
        public virtual void RemoveTenantAsyncExecutor(string tenantId)
        {
            StopThreadsForTenant(tenantId);
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Start()
        {
            foreach (string tenantId in timerJobAcquisitionRunnables.Keys)
            {
                StartTimerJobAcquisitionForTenant(tenantId);
                StartAsyncJobAcquisitionForTenant(tenantId);
                StartResetExpiredJobsForTenant(tenantId);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tenantId"></param>
        protected internal virtual void StartTimerJobAcquisitionForTenant(string tenantId)
        {
            timerJobAcquisitionThreads.TryGetValue(tenantId, out Thread runable);
            runable.Start();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tenantId"></param>
        protected internal virtual void StartAsyncJobAcquisitionForTenant(string tenantId)
        {
            asyncJobAcquisitionThreads.TryGetValue(tenantId, out Thread runable);
            runable.Start();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tenantId"></param>
        protected internal virtual void StartResetExpiredJobsForTenant(string tenantId)
        {
            resetExpiredJobsThreads.TryGetValue(tenantId, out Thread runable);
            runable.Start();
        }

        /// <summary>
        /// 
        /// </summary>
        protected internal override void StopJobAcquisitionThread()
        {
            foreach (string tenantId in timerJobAcquisitionRunnables.Keys)
            {
                StopThreadsForTenant(tenantId);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tenantId"></param>
        protected internal virtual void StopThreadsForTenant(string tenantId)
        {
            timerJobAcquisitionRunnables.TryGetValue(tenantId, out TenantAwareAcquireTimerJobsRunnable timerJobRunnable);
            timerJobRunnable.Stop();

            asyncJobAcquisitionRunnables.TryGetValue(tenantId, out TenantAwareAcquireAsyncJobsDueRunnable asyncJobRunnable);
            asyncJobRunnable.Stop();

            resetExpiredJobsRunnables.TryGetValue(tenantId, out TenantAwareResetExpiredJobsRunnable resetExpiredRunable);
            resetExpiredRunable.Stop();

            try
            {
                timerJobAcquisitionThreads.TryGetValue(tenantId, out Thread timerJobThread);
                timerJobThread.Join();
            }
            catch (Exception e)
            {
                logger.LogWarning($"Interrupted while waiting for the timer job acquisition thread to terminate {e.Message}");
            }

            try
            {
                asyncJobAcquisitionThreads.TryGetValue(tenantId, out Thread asyncJobThread);
                asyncJobThread.Join();
            }
            catch (Exception e)
            {
                logger.LogWarning($"Interrupted while waiting for the timer job acquisition thread to terminate {e.Message}");
            }

            try
            {
                resetExpiredJobsThreads.TryGetValue(tenantId, out Thread resetJobThread);
                resetJobThread.Join();
            }
            catch (Exception e)
            {
                logger.LogWarning($"Interrupted while waiting for the reset expired jobs thread to terminate {e.Message}");
            }
        }
    }
}