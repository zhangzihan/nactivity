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
namespace Sys.Workflow.engine.impl.persistence.entity.data.impl
{

    using Sys.Workflow.engine.impl.cfg;
    using Sys.Workflow.engine.impl.persistence.entity.data.impl.cachematcher;
    using Sys.Workflow.engine.runtime;

    /// 
    public class MybatisDeadLetterJobDataManager : AbstractDataManager<IDeadLetterJobEntity>, IDeadLetterJobDataManager
    {

        protected internal ICachedEntityMatcher<IDeadLetterJobEntity> deadLetterByExecutionIdMatcher = new DeadLetterJobsByExecutionIdMatcher();

        public MybatisDeadLetterJobDataManager(ProcessEngineConfigurationImpl processEngineConfiguration) : base(processEngineConfiguration)
        {
        }

        public override Type ManagedEntityClass
        {
            get
            {
                return typeof(DeadLetterJobEntityImpl);
            }
        }

        public override IDeadLetterJobEntity Create()
        {
            return new DeadLetterJobEntityImpl();
        }

        public override void Delete(IDeadLetterJobEntity entity)
        {
            DbSqlSession.Delete(entity);
        }

        public virtual IList<IJob> FindJobsByQueryCriteria(IDeadLetterJobQuery jobQuery, Page page)
        {
            string query = "selectDeadLetterJobByQueryCriteria";
            return DbSqlSession.SelectList<JobEntityImpl, IJob>(query, jobQuery, page);
        }

        public virtual long FindJobCountByQueryCriteria(IDeadLetterJobQuery jobQuery)
        {
            return DbSqlSession.SelectOne<DeadLetterJobEntityImpl, long?>("selectDeadLetterJobCountByQueryCriteria", jobQuery).GetValueOrDefault(0);
        }

        public virtual IList<IDeadLetterJobEntity> FindJobsByExecutionId(string executionId)
        {
            return (IList<IDeadLetterJobEntity>)GetList("selectDeadLetterJobsByExecutionId", new { executionId }, deadLetterByExecutionIdMatcher, true);
        }

        public virtual void UpdateJobTenantIdForDeployment(string deploymentId, string newTenantId)
        {
            Dictionary<string, object> @params = new Dictionary<string, object>
            {
                ["deploymentId"] = deploymentId,
                ["tenantId"] = newTenantId
            };
            DbSqlSession.Update<DeadLetterJobEntityImpl>("updateDeadLetterJobTenantIdForDeployment", @params);
        }

    }

}