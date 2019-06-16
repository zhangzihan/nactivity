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
namespace Sys.Workflow.Engine.Impl.Persistence.Entity.Data
{

    using Sys.Workflow.Engine.Runtime;

    /// 
    public interface IExecutionDataManager : IDataManager<IExecutionEntity>
    {
        IExecutionEntity FindSubProcessInstanceBySuperExecutionId(string superExecutionId);

        IList<IExecutionEntity> FindChildExecutionsByParentExecutionId(string parentExecutionId);

        IList<IExecutionEntity> FindChildExecutionsByProcessInstanceId(string processInstanceId);

        IList<IExecutionEntity> FindExecutionsByParentExecutionAndActivityIds(string parentExecutionId, ICollection<string> activityIds);

        long FindExecutionCountByQueryCriteria(IExecutionQuery executionQuery);

        IList<IExecutionEntity> FindExecutionsByQueryCriteria(IExecutionQuery executionQuery, Page page);

        long FindProcessInstanceCountByQueryCriteria(IProcessInstanceQuery executionQuery);

        IList<IProcessInstance> FindProcessInstanceByQueryCriteria(IProcessInstanceQuery executionQuery);

        IList<IExecutionEntity> FindExecutionsByRootProcessInstanceId(string rootProcessInstanceId);

        IList<IExecutionEntity> FindExecutionsByProcessInstanceId(string processInstanceId);

        IList<IProcessInstance> FindProcessInstanceAndVariablesByQueryCriteria(IProcessInstanceQuery executionQuery);

        ICollection<IExecutionEntity> FindInactiveExecutionsByProcessInstanceId(string processInstanceId);

        ICollection<IExecutionEntity> FindInactiveExecutionsByActivityIdAndProcessInstanceId(string activityId, string processInstanceId);

        IList<string> FindProcessInstanceIdsByProcessDefinitionId(string processDefinitionId);

        IList<IExecution> FindExecutionsByNativeQuery(IDictionary<string, object> parameterMap, int firstResult, int maxResults);

        IList<IProcessInstance> FindProcessInstanceByNativeQuery(IDictionary<string, object> parameterMap, int firstResult, int maxResults);

        long FindExecutionCountByNativeQuery(IDictionary<string, object> parameterMap);

        void UpdateExecutionTenantIdForDeployment(string deploymentId, string newTenantId);

        void UpdateProcessInstanceLockTime(string processInstanceId, DateTime lockDate, DateTime expirationTime);

        void UpdateAllExecutionRelatedEntityCountFlags(bool newValue);

        void ClearProcessInstanceLockTime(string processInstanceId);
    }
}