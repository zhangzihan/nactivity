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
namespace Sys.Workflow.Engine.Impl.Persistence.Entity
{

    using Sys.Workflow.Engine.Repository;
    using Sys.Workflow.Engine.Runtime;

    /// 
    public interface IExecutionEntityManager : IEntityManager<IExecutionEntity>
	{

	  IExecutionEntity CreateProcessInstanceExecution(IProcessDefinition processDefinition, string businessKey, string tenantId, string initiatorVariableName);

	  IExecutionEntity CreateChildExecution(IExecutionEntity parentExecutionEntity);

	  IExecutionEntity CreateSubprocessInstance(IProcessDefinition processDefinition, IExecutionEntity superExecutionEntity, string businessKey);

	  /// <summary>
	  /// Finds the <seealso cref="IExecutionEntity"/> for the given root process instance id.
	  /// All children will have been fetched and initialized. 
	  /// </summary>
	  IExecutionEntity FindByRootProcessInstanceId(string rootProcessInstanceId);

	  IExecutionEntity FindSubProcessInstanceBySuperExecutionId(string superExecutionId);

	  IList<IExecutionEntity> FindChildExecutionsByParentExecutionId(string parentExecutionId);

	  IList<IExecutionEntity> FindChildExecutionsByProcessInstanceId(string processInstanceId);

	  IList<IExecutionEntity> FindExecutionsByParentExecutionAndActivityIds(string parentExecutionId, ICollection<string> activityIds);

	  long FindExecutionCountByQueryCriteria(IExecutionQuery executionQuery);

	  IList<IExecutionEntity> FindExecutionsByQueryCriteria(IExecutionQuery executionQuery, Page page);

	  long FindProcessInstanceCountByQueryCriteria(IProcessInstanceQuery executionQuery);

	  IList<IProcessInstance> FindProcessInstanceByQueryCriteria(IProcessInstanceQuery executionQuery);

	  IList<IProcessInstance> FindProcessInstanceAndVariablesByQueryCriteria(ProcessInstanceQueryImpl executionQuery);

	  ICollection<IExecutionEntity> FindInactiveExecutionsByProcessInstanceId(string processInstanceId);

	  ICollection<IExecutionEntity> FindInactiveExecutionsByActivityIdAndProcessInstanceId(string activityId, string processInstanceId);

	  IList<IExecution> FindExecutionsByNativeQuery(IDictionary<string, object> parameterMap, int firstResult, int maxResults);

	  IList<IProcessInstance> FindProcessInstanceByNativeQuery(IDictionary<string, object> parameterMap, int firstResult, int maxResults);

	  long FindExecutionCountByNativeQuery(IDictionary<string, object> parameterMap);


	  /// <summary>
	  /// Returns all child executions of a given <seealso cref="IExecutionEntity"/>.
	  /// In the list, child executions will be behind parent executions. 
	  /// </summary>
	  IList<IExecutionEntity> CollectChildren(IExecutionEntity executionEntity);

	  IExecutionEntity FindFirstScope(IExecutionEntity executionEntity);

	  IExecutionEntity FindFirstMultiInstanceRoot(IExecutionEntity executionEntity);


	  void UpdateExecutionTenantIdForDeployment(string deploymentId, string newTenantId);

	  string UpdateProcessInstanceBusinessKey(IExecutionEntity executionEntity, string businessKey);


	  void DeleteProcessInstancesByProcessDefinition(string processDefinitionId, string deleteReason, bool cascade);

	  void DeleteProcessInstance(string processInstanceId, string deleteReason, bool cascade);

	  void DeleteProcessInstanceExecutionEntity(string processInstanceId, string currentFlowElementId, string deleteReason, bool cascade, bool cancel);

	  void DeleteChildExecutions(IExecutionEntity executionEntity, string deleteReason, bool cancel);

	  void DeleteExecutionAndRelatedData(IExecutionEntity executionEntity, string deleteReason, bool cancel);

	  void UpdateProcessInstanceLockTime(string processInstanceId);

	  void ClearProcessInstanceLockTime(string processInstanceId);
	}
}