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

    using org.activiti.engine.impl.cfg;
    using org.activiti.engine.impl.cfg.multitenant;
    using org.activiti.engine.runtime;
    using System;

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
        protected internal ITenantInfoHolder tenantInfoHolder;

        protected internal IDictionary<string, Thread> timerJobAcquisitionThreads = new Dictionary<string, Thread>();
        protected internal IDictionary<string, TenantAwareAcquireTimerJobsRunnable> timerJobAcquisitionRunnables = new Dictionary<string, TenantAwareAcquireTimerJobsRunnable>();

        protected internal IDictionary<string, Thread> asyncJobAcquisitionThreads = new Dictionary<string, Thread>();
        protected internal IDictionary<string, TenantAwareAcquireAsyncJobsDueRunnable> asyncJobAcquisitionRunnables = new Dictionary<string, TenantAwareAcquireAsyncJobsDueRunnable>();

        protected internal IDictionary<string, Thread> resetExpiredJobsThreads = new Dictionary<string, Thread>();
        protected internal IDictionary<string, TenantAwareResetExpiredJobsRunnable> resetExpiredJobsRunnables = new Dictionary<string, TenantAwareResetExpiredJobsRunnable>();

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


            public virtual ThreadStart createExecuteAsyncRunnable(IJob job, ProcessEngineConfigurationImpl processEngineConfiguration)
            {

                // Here, the runnable will be created by for example the acquire thread, which has already set the current id.
                // But it will be executed later on, by the executorService and thus we need to set it explicitely again then
                throw new System.NotImplementedException();

                //return new TenantAwareExecuteAsyncRunnable(job, processEngineConfiguration, outerInstance.tenantInfoHolder, outerInstance.tenantInfoHolder.CurrentTenantId);
            }

        }

        public virtual ICollection<string> TenantIds
        {
            get
            {
                return timerJobAcquisitionRunnables.Keys;
            }
        }

        public virtual void addTenantAsyncExecutor(string tenantId, bool startExecutor)
        {

            TenantAwareAcquireTimerJobsRunnable timerRunnable = new TenantAwareAcquireTimerJobsRunnable(this, tenantInfoHolder, tenantId);
            timerJobAcquisitionRunnables[tenantId] = timerRunnable;
            timerJobAcquisitionThreads[tenantId] = new Thread(timerRunnable.Runable);

            TenantAwareAcquireAsyncJobsDueRunnable asyncJobsRunnable = new TenantAwareAcquireAsyncJobsDueRunnable(this, tenantInfoHolder, tenantId);
            asyncJobAcquisitionRunnables[tenantId] = asyncJobsRunnable;
            asyncJobAcquisitionThreads[tenantId] = new Thread(asyncJobsRunnable.Runable);

            TenantAwareResetExpiredJobsRunnable resetExpiredJobsRunnable = new TenantAwareResetExpiredJobsRunnable(this, tenantInfoHolder, tenantId);
            resetExpiredJobsRunnables[tenantId] = resetExpiredJobsRunnable;
            resetExpiredJobsThreads[tenantId] = new Thread(resetExpiredJobsRunnable.Runable);

            if (startExecutor)
            {
                startTimerJobAcquisitionForTenant(tenantId);
                startAsyncJobAcquisitionForTenant(tenantId);
                startResetExpiredJobsForTenant(tenantId);
            }
        }

        public virtual void removeTenantAsyncExecutor(string tenantId)
        {
            stopThreadsForTenant(tenantId);
        }

        public override void start()
        {
            foreach (string tenantId in timerJobAcquisitionRunnables.Keys)
            {
                startTimerJobAcquisitionForTenant(tenantId);
                startAsyncJobAcquisitionForTenant(tenantId);
                startResetExpiredJobsForTenant(tenantId);
            }
        }

        protected internal virtual void startTimerJobAcquisitionForTenant(string tenantId)
        {
            timerJobAcquisitionThreads[tenantId].Start();
        }

        protected internal virtual void startAsyncJobAcquisitionForTenant(string tenantId)
        {
            asyncJobAcquisitionThreads[tenantId].Start();
        }

        protected internal virtual void startResetExpiredJobsForTenant(string tenantId)
        {
            resetExpiredJobsThreads[tenantId].Start();
        }

        protected internal override void stopJobAcquisitionThread()
        {
            foreach (string tenantId in timerJobAcquisitionRunnables.Keys)
            {
                stopThreadsForTenant(tenantId);
            }
        }

        protected internal virtual void stopThreadsForTenant(string tenantId)
        {
            timerJobAcquisitionRunnables[tenantId].stop();
            asyncJobAcquisitionRunnables[tenantId].stop();
            resetExpiredJobsRunnables[tenantId].stop();

            try
            {
                timerJobAcquisitionThreads[tenantId].Join();
            }
            catch (Exception e)
            {
                //logger.warn("Interrupted while waiting for the timer job acquisition thread to terminate", e);
            }

            try
            {
                asyncJobAcquisitionThreads[tenantId].Join();
            }
            catch (Exception e)
            {
                //logger.warn("Interrupted while waiting for the timer job acquisition thread to terminate", e);
            }

            try
            {
                resetExpiredJobsThreads[tenantId].Join();
            }
            catch (Exception e)
            {
                //logger.warn("Interrupted while waiting for the reset expired jobs thread to terminate", e);
            }
        }

    }

}