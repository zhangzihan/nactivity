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
namespace Sys.Workflow.Engine.Impl.Persistence.Entity.Data.Impl
{

    using Sys.Workflow.Engine.Impl.Cfg;
    using Sys.Workflow.Engine.Impl.Persistence.Entity.Data.Impl.Cachematcher;
    using Sys.Workflow.Engine.Runtime;

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

        public override IJobEntity Create()
        {
            return new JobEntityImpl();
        }


        public virtual IList<IJobEntity> FindJobsToExecute(Page page)
        {
            return DbSqlSession.SelectList<JobEntityImpl, IJobEntity>("selectJobsToExecute", null, page);
        }

        public virtual IList<IJobEntity> FindJobsByExecutionId(string executionId)
        {
            return (IList<IJobEntity>)GetList("selectJobsByExecutionId", new { executionId }, jobsByExecutionIdMatcher, true);
        }

        public virtual IList<IJobEntity> FindJobsByProcessDefinitionId(string processDefinitionId)
        {
            IDictionary<string, string> @params = new Dictionary<string, string>(1)
            {
                ["processDefinitionId"] = processDefinitionId
            };
            return DbSqlSession.SelectList<JobEntityImpl, IJobEntity>("selectJobByProcessDefinitionId", @params);
        }

        public virtual IList<IJobEntity> FindJobsByTypeAndProcessDefinitionId(string jobHandlerType, string processDefinitionId)
        {
            IDictionary<string, string> @params = new Dictionary<string, string>(2)
            {
                ["handlerType"] = jobHandlerType,
                ["processDefinitionId"] = processDefinitionId
            };
            return DbSqlSession.SelectList<JobEntityImpl, IJobEntity>("selectJobByTypeAndProcessDefinitionId", @params);
        }

        public virtual IList<IJobEntity> FindJobsByProcessInstanceId(string processInstanceId)
        {
            return DbSqlSession.SelectList<JobEntityImpl, IJobEntity>("selectJobsByProcessInstanceId", new { processInstanceId });
        }

        public virtual IList<IJobEntity> FindExpiredJobs(Page page)
        {
            DateTime now = Clock.CurrentTime;
            return DbSqlSession.SelectList<JobEntityImpl, IJobEntity>("selectExpiredJobs", new { lockExpTime = now }, page);
        }

        public virtual IList<IJob> FindJobsByQueryCriteria(IJobQuery jobQuery, Page page)
        {
            const string query = "selectJobByQueryCriteria";
            return DbSqlSession.SelectList<JobEntityImpl, IJob>(query, jobQuery, page);
        }

        public virtual long FindJobCountByQueryCriteria(IJobQuery jobQuery)
        {
            return DbSqlSession.SelectOne<JobEntityImpl, long?>("selectJobCountByQueryCriteria", jobQuery).GetValueOrDefault();
        }

        public virtual void UpdateJobTenantIdForDeployment(string deploymentId, string newTenantId)
        {
            Dictionary<string, object> @params = new Dictionary<string, object>
            {
                ["deploymentId"] = deploymentId,
                ["tenantId"] = newTenantId
            };
            DbSqlSession.Update<JobEntityImpl>("updateJobTenantIdForDeployment", @params);
        }

        public virtual void ResetExpiredJob(string jobId)
        {
            IDictionary<string, object> @params = new Dictionary<string, object>(2)
            {
                ["id"] = jobId
            };
            DbSqlSession.Update<JobEntityImpl>("resetExpiredJob", @params);
        }
    }
}