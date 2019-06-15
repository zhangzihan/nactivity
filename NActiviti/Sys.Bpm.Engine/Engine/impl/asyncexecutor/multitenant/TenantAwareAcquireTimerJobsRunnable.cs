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
    using org.activiti.engine.impl.cfg.multitenant;

    /// <summary>
    /// Extends the default <seealso cref="AcquireTimerJobsRunnable"/> by setting the 'tenant' context before executing.
    /// 
    /// 
    /// </summary>
    public class TenantAwareAcquireTimerJobsRunnable : AcquireTimerJobsRunnable
    {
        /// <summary>
        /// 
        /// </summary>
        protected internal ITenantInfoHolder tenantInfoHolder;

        /// <summary>
        /// 
        /// </summary>
        protected internal string tenantId;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="asyncExecutor"></param>
        /// <param name="tenantInfoHolder"></param>
        /// <param name="tenantId"></param>
        public TenantAwareAcquireTimerJobsRunnable(IAsyncExecutor asyncExecutor, ITenantInfoHolder tenantInfoHolder, string tenantId) : base(asyncExecutor, asyncExecutor.ProcessEngineConfiguration.JobManager)
        {
            this.tenantInfoHolder = tenantInfoHolder;
            this.tenantId = tenantId;
        }

        /// <summary>
        /// 
        /// </summary>
        protected internal virtual ExecutorPerTenantAsyncExecutor TenantAwareAsyncExecutor
        {
            get
            {
                return (ExecutorPerTenantAsyncExecutor)asyncExecutor;
            }
        }

        private readonly object syncRoot = new object();

        /// <summary>
        /// 
        /// </summary>
        public override void Run()
        {
            lock (syncRoot)
            {
                tenantInfoHolder.CurrentTenantId = tenantId;
                base.Run();
                tenantInfoHolder.ClearCurrentTenantId();
            }
        }
    }
}