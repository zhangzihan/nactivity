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
namespace org.activiti.engine.impl.persistence.entity.data
{

    using org.activiti.engine.runtime;

    /// 
    public interface IJobDataManager : IDataManager<IJobEntity>
    {

        IList<IJobEntity> findJobsToExecute(Page page);

        IList<IJobEntity> findJobsByExecutionId(string executionId);

        IList<IJobEntity> findJobsByProcessDefinitionId(string processDefinitionId);

        IList<IJobEntity> findJobsByTypeAndProcessDefinitionId(string jobTypeTimer, string id);

        IList<IJobEntity> findJobsByProcessInstanceId(string processInstanceId);

        IList<IJobEntity> findExpiredJobs(Page page);

        IList<IJob> findJobsByQueryCriteria(JobQueryImpl jobQuery, Page page);

        long findJobCountByQueryCriteria(JobQueryImpl jobQuery);

        void updateJobTenantIdForDeployment(string deploymentId, string newTenantId);

        void resetExpiredJob(string jobId);

    }

}