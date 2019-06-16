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
    /// 
    public class MybatisTimerJobDataManager : AbstractDataManager<ITimerJobEntity>, ITimerJobDataManager
    {

        protected internal ICachedEntityMatcher<ITimerJobEntity> timerJobsByExecutionIdMatcher = new TimerJobsByExecutionIdMatcher();

        public MybatisTimerJobDataManager(ProcessEngineConfigurationImpl processEngineConfiguration) : base(processEngineConfiguration)
        {
        }

        public override Type ManagedEntityClass
        {
            get
            {
                return typeof(TimerJobEntityImpl);
            }
        }

        public override ITimerJobEntity Create()
        {
            return new TimerJobEntityImpl();
        }

        public virtual IList<IJob> FindJobsByQueryCriteria(ITimerJobQuery jobQuery, Page page)
        {
            string query = "selectTimerJobByQueryCriteria";
            return DbSqlSession.SelectList<TimerJobEntityImpl, IJob>(query, jobQuery, page);
        }

        public virtual long FindJobCountByQueryCriteria(ITimerJobQuery jobQuery)
        {
            return DbSqlSession.SelectOne<TimerJobEntityImpl, long?>("selectTimerJobCountByQueryCriteria", jobQuery).GetValueOrDefault();
        }

        public virtual IList<ITimerJobEntity> FindTimerJobsToExecute(Page page)
        {
            DateTime now = Clock.CurrentTime;
            return DbSqlSession.SelectList<TimerJobEntityImpl, ITimerJobEntity>("selectTimerJobsToExecute", new { duedate = now }, page);
        }

        public virtual IList<ITimerJobEntity> FindJobsByTypeAndProcessDefinitionId(string handlerType, string processDefinitionId)
        {
            return DbSqlSession.SelectList<TimerJobEntityImpl, ITimerJobEntity>("selectTimerJobByTypeAndProcessDefinitionId", new { handlerType, processDefinitionId });

        }

        public virtual IList<ITimerJobEntity> FindJobsByExecutionId(string executionId)
        {
            return (IList<ITimerJobEntity>)GetList("selectTimerJobsByExecutionId", new { executionId }, timerJobsByExecutionIdMatcher, true);
        }

        public virtual IList<ITimerJobEntity> FindJobsByProcessInstanceId(string processInstanceId)
        {
            return DbSqlSession.SelectList<TimerJobEntityImpl, ITimerJobEntity>("selectTimerJobsByProcessInstanceId", new { processInstanceId });
        }

        public virtual IList<ITimerJobEntity> FindJobsByTypeAndProcessDefinitionKeyNoTenantId(string handlerType, string processDefinitionKey)
        {
            return DbSqlSession.SelectList<TimerJobEntityImpl, ITimerJobEntity>("selectTimerJobByTypeAndProcessDefinitionKeyNoTenantId", new { handlerType, processDefinitionKey });
        }

        public virtual IList<ITimerJobEntity> FindJobsByTypeAndProcessDefinitionKeyAndTenantId(string handlerType, string processDefinitionKey, string tenantId)
        {
            return DbSqlSession.SelectList<TimerJobEntityImpl, ITimerJobEntity>("selectTimerJobByTypeAndProcessDefinitionKeyAndTenantId", new { handlerType, processDefinitionKey, tenantId });
        }

        public virtual void UpdateJobTenantIdForDeployment(string deploymentId, string newTenantId)
        {
            Dictionary<string, object> @params = new Dictionary<string, object>
            {
                ["deploymentId"] = deploymentId,
                ["tenantId"] = newTenantId
            };
            DbSqlSession.Update<TimerJobEntityImpl>("updateTimerJobTenantIdForDeployment", @params);
        }

    }

}