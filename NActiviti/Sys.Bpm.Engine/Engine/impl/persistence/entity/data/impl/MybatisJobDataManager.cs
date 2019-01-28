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
    public class MybatisJobDataManager : AbstractDataManager<IJobEntity>, IJobDataManager
    {

        protected internal ICachedEntityMatcher<IJobEntity> jobsByExecutionIdMatcher = new JobsByExecutionIdMatcher();

        public MybatisJobDataManager(ProcessEngineConfigurationImpl processEngineConfiguration) : base(processEngineConfiguration)
        {
        }

        public override Type ManagedEntityClass
        {
            get
            {
                return typeof(JobEntityImpl);
            }
        }

        public override IJobEntity create()
        {
            return new JobEntityImpl();
        }


        public virtual IList<IJobEntity> findJobsToExecute(Page page)
        {
            return DbSqlSession.selectList<JobEntityImpl, IJobEntity>("selectJobsToExecute", null, page);
        }

        public virtual IList<IJobEntity> findJobsByExecutionId(string executionId)
        {
            return (IList<IJobEntity>)getList("selectJobsByExecutionId", new { executionId }, jobsByExecutionIdMatcher, true);
        }

        public virtual IList<IJobEntity> findJobsByProcessDefinitionId(string processDefinitionId)
        {
            IDictionary<string, string> @params = new Dictionary<string, string>(1);
            @params["processDefinitionId"] = processDefinitionId;
            return DbSqlSession.selectList<JobEntityImpl, IJobEntity>("selectJobByProcessDefinitionId", @params);
        }

        public virtual IList<IJobEntity> findJobsByTypeAndProcessDefinitionId(string jobHandlerType, string processDefinitionId)
        {
            IDictionary<string, string> @params = new Dictionary<string, string>(2);
            @params["handlerType"] = jobHandlerType;
            @params["processDefinitionId"] = processDefinitionId;
            return DbSqlSession.selectList<JobEntityImpl, IJobEntity>("selectJobByTypeAndProcessDefinitionId", @params);
        }

        public virtual IList<IJobEntity> findJobsByProcessInstanceId(string processInstanceId)
        {
            return DbSqlSession.selectList<JobEntityImpl, IJobEntity>("selectJobsByProcessInstanceId", new { processInstanceId });
        }

        public virtual IList<IJobEntity> findExpiredJobs(Page page)
        {
            DateTime now = Clock.CurrentTime;
            return DbSqlSession.selectList<JobEntityImpl, IJobEntity>("selectExpiredJobs", new { lockExpTime = now }, page);
        }

        public virtual IList<IJob> findJobsByQueryCriteria(JobQueryImpl jobQuery, Page page)
        {
            const string query = "selectJobByQueryCriteria";
            return DbSqlSession.selectList<JobEntityImpl, IJob>(query, jobQuery, page);
        }

        public virtual long findJobCountByQueryCriteria(JobQueryImpl jobQuery)
        {
            return ((long?)DbSqlSession.selectOne<JobEntityImpl, long?>("selectJobCountByQueryCriteria", jobQuery)).GetValueOrDefault();
        }

        public virtual void updateJobTenantIdForDeployment(string deploymentId, string newTenantId)
        {
            Dictionary<string, object> @params = new Dictionary<string, object>();
            @params["deploymentId"] = deploymentId;
            @params["tenantId"] = newTenantId;
            DbSqlSession.update<JobEntityImpl>("updateJobTenantIdForDeployment", @params);
        }

        public virtual void resetExpiredJob(string jobId)
        {
            IDictionary<string, object> @params = new Dictionary<string, object>(2);
            @params["id"] = jobId;
            DbSqlSession.update<JobEntityImpl>("resetExpiredJob", @params);
        }

    }

}