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
    using Sys.Workflow.Engine.Impl.Cfg.Multitenants;

    /// <summary>
    /// 
    /// </summary>
    public class TenantAwareResetExpiredJobsRunnable : ResetExpiredJobsRunnable
    {
        /// <summary>
        /// 
        /// </summary>
        protected internal ITenantInfoHolder tenantInfoHolder;

        /// <summary>
        /// 
        /// </summary>
        protected internal string tenantId;

        private readonly object syncRoot = new object();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="asyncExecutor"></param>
        /// <param name="tenantInfoHolder"></param>
        /// <param name="tenantId"></param>
        public TenantAwareResetExpiredJobsRunnable(IAsyncExecutor asyncExecutor, ITenantInfoHolder tenantInfoHolder, string tenantId) : base(asyncExecutor)
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