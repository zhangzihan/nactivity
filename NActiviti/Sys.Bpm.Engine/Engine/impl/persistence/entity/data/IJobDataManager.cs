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
namespace Sys.Workflow.engine.impl.persistence.entity.data
{

    using Sys.Workflow.engine.runtime;

    /// 
    public interface IJobDataManager : IDataManager<IJobEntity>
    {

        IList<IJobEntity> FindJobsToExecute(Page page);

        IList<IJobEntity> FindJobsByExecutionId(string executionId);

        IList<IJobEntity> FindJobsByProcessDefinitionId(string processDefinitionId);

        IList<IJobEntity> FindJobsByTypeAndProcessDefinitionId(string jobTypeTimer, string id);

        IList<IJobEntity> FindJobsByProcessInstanceId(string processInstanceId);

        IList<IJobEntity> FindExpiredJobs(Page page);

        IList<IJob> FindJobsByQueryCriteria(IJobQuery jobQuery, Page page);

        long FindJobCountByQueryCriteria(IJobQuery jobQuery);

        void UpdateJobTenantIdForDeployment(string deploymentId, string newTenantId);

        void ResetExpiredJob(string jobId);

    }

}