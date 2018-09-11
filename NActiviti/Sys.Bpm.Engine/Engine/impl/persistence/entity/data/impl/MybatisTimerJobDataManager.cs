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

        public override ITimerJobEntity create()
        {
            return new TimerJobEntityImpl();
        }

        public virtual IList<IJob> findJobsByQueryCriteria(TimerJobQueryImpl jobQuery, Page page)
        {
            string query = "selectTimerJobByQueryCriteria";
            return DbSqlSession.selectList<TimerJobEntityImpl, IJob>(query, jobQuery, page);
        }

        public virtual long findJobCountByQueryCriteria(TimerJobQueryImpl jobQuery)
        {
            return ((long?)DbSqlSession.selectOne<TimerJobEntityImpl, long?>("selectTimerJobCountByQueryCriteria", jobQuery)).GetValueOrDefault();
        }

        public virtual IList<ITimerJobEntity> findTimerJobsToExecute(Page page)
        {
            DateTime now = Clock.CurrentTime;
            return DbSqlSession.selectList<TimerJobEntityImpl, ITimerJobEntity>("selectTimerJobsToExecute", new KeyValuePair<string, object>("duedate", now), page);
        }

        public virtual IList<ITimerJobEntity> findJobsByTypeAndProcessDefinitionId(string jobHandlerType, string processDefinitionId)
        {
            IDictionary<string, string> @params = new Dictionary<string, string>(2);
            @params["handlerType"] = jobHandlerType;
            @params["processDefinitionId"] = processDefinitionId;
            return DbSqlSession.selectList<TimerJobEntityImpl, ITimerJobEntity>("selectTimerJobByTypeAndProcessDefinitionId", @params);

        }

        public virtual IList<ITimerJobEntity> findJobsByExecutionId(string executionId)
        {
            return (IList<ITimerJobEntity>)getList("selectTimerJobsByExecutionId", new KeyValuePair<string, object>("executionId", executionId), timerJobsByExecutionIdMatcher, true);
        }

        public virtual IList<ITimerJobEntity> findJobsByProcessInstanceId(string processInstanceId)
        {
            return DbSqlSession.selectList<TimerJobEntityImpl, ITimerJobEntity>("selectTimerJobsByProcessInstanceId", new KeyValuePair<string, object>("processInstanceId", processInstanceId));
        }

        public virtual IList<ITimerJobEntity> findJobsByTypeAndProcessDefinitionKeyNoTenantId(string jobHandlerType, string processDefinitionKey)
        {
            IDictionary<string, string> @params = new Dictionary<string, string>(2);
            @params["handlerType"] = jobHandlerType;
            @params["processDefinitionKey"] = processDefinitionKey;
            return DbSqlSession.selectList<TimerJobEntityImpl, ITimerJobEntity>("selectTimerJobByTypeAndProcessDefinitionKeyNoTenantId", @params);
        }

        public virtual IList<ITimerJobEntity> findJobsByTypeAndProcessDefinitionKeyAndTenantId(string jobHandlerType, string processDefinitionKey, string tenantId)
        {
            IDictionary<string, string> @params = new Dictionary<string, string>(3);
            @params["handlerType"] = jobHandlerType;
            @params["processDefinitionKey"] = processDefinitionKey;
            @params["tenantId"] = tenantId;
            return DbSqlSession.selectList<TimerJobEntityImpl, ITimerJobEntity>("selectTimerJobByTypeAndProcessDefinitionKeyAndTenantId", @params);
        }

        public virtual void updateJobTenantIdForDeployment(string deploymentId, string newTenantId)
        {
            Dictionary<string, object> @params = new Dictionary<string, object>();
            @params["deploymentId"] = deploymentId;
            @params["tenantId"] = newTenantId;
            DbSqlSession.update<TimerJobEntityImpl>("updateTimerJobTenantIdForDeployment", @params);
        }

    }

}