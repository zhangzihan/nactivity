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

        public override IDeadLetterJobEntity create()
        {
            return new DeadLetterJobEntityImpl();
        }

        public override void delete(IDeadLetterJobEntity entity)
        {
            DbSqlSession.delete(entity);
        }

        public virtual IList<IJob> findJobsByQueryCriteria(DeadLetterJobQueryImpl jobQuery, Page page)
        {
            string query = "selectDeadLetterJobByQueryCriteria";
            return DbSqlSession.selectList<JobEntityImpl, IJob>(query, jobQuery, page);
        }

        public virtual long findJobCountByQueryCriteria(DeadLetterJobQueryImpl jobQuery)
        {
            return DbSqlSession.selectOne<DeadLetterJobEntityImpl, long?>("selectDeadLetterJobCountByQueryCriteria", jobQuery).GetValueOrDefault(0);
        }

        public virtual IList<IDeadLetterJobEntity> findJobsByExecutionId(string executionId)
        {
            return (IList<IDeadLetterJobEntity>)getList("selectDeadLetterJobsByExecutionId", new { executionId }, deadLetterByExecutionIdMatcher, true);
        }

        public virtual void updateJobTenantIdForDeployment(string deploymentId, string newTenantId)
        {
            Dictionary<string, object> @params = new Dictionary<string, object>();
            @params["deploymentId"] = deploymentId;
            @params["tenantId"] = newTenantId;
            DbSqlSession.update<DeadLetterJobEntityImpl>("updateDeadLetterJobTenantIdForDeployment", @params);
        }

    }

}