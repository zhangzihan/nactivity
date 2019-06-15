using System;
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
namespace org.activiti.engine.impl.persistence.entity.data.impl
{

    using org.activiti.engine.impl.cfg;
    using org.activiti.engine.impl.persistence.entity.data.impl.cachematcher;
    using org.activiti.engine.runtime;

    /// 
    public class MybatisSuspendedJobDataManager : AbstractDataManager<ISuspendedJobEntity>, ISuspendedJobDataManager
    {

        protected internal ICachedEntityMatcher<ISuspendedJobEntity> suspendedJobsByExecutionIdMatcher = new SuspendedJobsByExecutionIdMatcher();

        public MybatisSuspendedJobDataManager(ProcessEngineConfigurationImpl processEngineConfiguration) : base(processEngineConfiguration)
        {
        }

        public override Type ManagedEntityClass
        {
            get
            {
                return typeof(SuspendedJobEntityImpl);
            }
        }

        public override ISuspendedJobEntity Create()
        {
            return new SuspendedJobEntityImpl();
        }

        public virtual IList<IJob> FindJobsByQueryCriteria(ISuspendedJobQuery jobQuery, Page page)
        {
            string query = "selectSuspendedJobByQueryCriteria";
            return DbSqlSession.SelectList<SuspendedJobEntityImpl, IJob>(query, jobQuery, page);
        }

        public virtual long FindJobCountByQueryCriteria(ISuspendedJobQuery jobQuery)
        {
            return DbSqlSession.SelectOne<SuspendedJobEntityImpl, long?>("selectSuspendedJobCountByQueryCriteria", jobQuery).GetValueOrDefault();
        }

        public virtual IList<ISuspendedJobEntity> FindJobsByExecutionId(string executionId)
        {
            return (IList<ISuspendedJobEntity>)GetList("selectSuspendedJobsByExecutionId", new { executionId }, suspendedJobsByExecutionIdMatcher, true);
        }

        public virtual IList<ISuspendedJobEntity> FindJobsByProcessInstanceId(string processInstanceId)
        {
            return DbSqlSession.SelectList<SuspendedJobEntityImpl, ISuspendedJobEntity>("selectSuspendedJobsByProcessInstanceId", new { processInstanceId });
        }

        public virtual void UpdateJobTenantIdForDeployment(string deploymentId, string newTenantId)
        {
            Dictionary<string, object> @params = new Dictionary<string, object>
            {
                ["deploymentId"] = deploymentId,
                ["tenantId"] = newTenantId
            };
            DbSqlSession.Update<SuspendedJobEntityImpl>("updateSuspendedJobTenantIdForDeployment", @params);
        }
    }

}