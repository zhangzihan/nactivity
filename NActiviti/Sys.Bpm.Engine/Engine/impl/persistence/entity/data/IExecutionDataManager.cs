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
namespace org.activiti.engine.impl.persistence.entity.data
{

    using org.activiti.engine.runtime;

    /// 
    public interface IExecutionDataManager : IDataManager<IExecutionEntity>
    {
        IExecutionEntity findSubProcessInstanceBySuperExecutionId(string superExecutionId);

        IList<IExecutionEntity> findChildExecutionsByParentExecutionId(string parentExecutionId);

        IList<IExecutionEntity> findChildExecutionsByProcessInstanceId(string processInstanceId);

        IList<IExecutionEntity> findExecutionsByParentExecutionAndActivityIds(string parentExecutionId, ICollection<string> activityIds);

        long findExecutionCountByQueryCriteria(ExecutionQueryImpl executionQuery);

        IList<IExecutionEntity> findExecutionsByQueryCriteria(ExecutionQueryImpl executionQuery, Page page);

        long findProcessInstanceCountByQueryCriteria(ProcessInstanceQueryImpl executionQuery);

        IList<IProcessInstance> findProcessInstanceByQueryCriteria(ProcessInstanceQueryImpl executionQuery);

        IList<IExecutionEntity> findExecutionsByRootProcessInstanceId(string rootProcessInstanceId);

        IList<IExecutionEntity> findExecutionsByProcessInstanceId(string processInstanceId);

        IList<IProcessInstance> findProcessInstanceAndVariablesByQueryCriteria(ProcessInstanceQueryImpl executionQuery);

        ICollection<IExecutionEntity> findInactiveExecutionsByProcessInstanceId(string processInstanceId);

        ICollection<IExecutionEntity> findInactiveExecutionsByActivityIdAndProcessInstanceId(string activityId, string processInstanceId);

        IList<string> findProcessInstanceIdsByProcessDefinitionId(string processDefinitionId);

        IList<IExecution> findExecutionsByNativeQuery(IDictionary<string, object> parameterMap, int firstResult, int maxResults);

        IList<IProcessInstance> findProcessInstanceByNativeQuery(IDictionary<string, object> parameterMap, int firstResult, int maxResults);

        long findExecutionCountByNativeQuery(IDictionary<string, object> parameterMap);

        void updateExecutionTenantIdForDeployment(string deploymentId, string newTenantId);

        void updateProcessInstanceLockTime(string processInstanceId, DateTime lockDate, DateTime expirationTime);

        void updateAllExecutionRelatedEntityCountFlags(bool newValue);

        void clearProcessInstanceLockTime(string processInstanceId);

    }

}