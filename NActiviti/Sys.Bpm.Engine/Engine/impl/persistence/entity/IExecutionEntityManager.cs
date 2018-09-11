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
namespace org.activiti.engine.impl.persistence.entity
{

    using org.activiti.engine.repository;
    using org.activiti.engine.runtime;

    /// 
    public interface IExecutionEntityManager : IEntityManager<IExecutionEntity>
	{

	  IExecutionEntity createProcessInstanceExecution(IProcessDefinition processDefinition, string businessKey, string tenantId, string initiatorVariableName);

	  IExecutionEntity createChildExecution(IExecutionEntity parentExecutionEntity);

	  IExecutionEntity createSubprocessInstance(IProcessDefinition processDefinition, IExecutionEntity superExecutionEntity, string businessKey);

	  /// <summary>
	  /// Finds the <seealso cref="IExecutionEntity"/> for the given root process instance id.
	  /// All children will have been fetched and initialized. 
	  /// </summary>
	  IExecutionEntity findByRootProcessInstanceId(string rootProcessInstanceId);

	  IExecutionEntity findSubProcessInstanceBySuperExecutionId(string superExecutionId);

	  IList<IExecutionEntity> findChildExecutionsByParentExecutionId(string parentExecutionId);

	  IList<IExecutionEntity> findChildExecutionsByProcessInstanceId(string processInstanceId);

	  IList<IExecutionEntity> findExecutionsByParentExecutionAndActivityIds(string parentExecutionId, ICollection<string> activityIds);

	  long findExecutionCountByQueryCriteria(ExecutionQueryImpl executionQuery);

	  IList<IExecutionEntity> findExecutionsByQueryCriteria(ExecutionQueryImpl executionQuery, Page page);

	  long findProcessInstanceCountByQueryCriteria(ProcessInstanceQueryImpl executionQuery);

	  IList<IProcessInstance> findProcessInstanceByQueryCriteria(ProcessInstanceQueryImpl executionQuery);

	  IList<IProcessInstance> findProcessInstanceAndVariablesByQueryCriteria(ProcessInstanceQueryImpl executionQuery);

	  ICollection<IExecutionEntity> findInactiveExecutionsByProcessInstanceId(string processInstanceId);

	  ICollection<IExecutionEntity> findInactiveExecutionsByActivityIdAndProcessInstanceId(string activityId, string processInstanceId);

	  IList<IExecution> findExecutionsByNativeQuery(IDictionary<string, object> parameterMap, int firstResult, int maxResults);

	  IList<IProcessInstance> findProcessInstanceByNativeQuery(IDictionary<string, object> parameterMap, int firstResult, int maxResults);

	  long findExecutionCountByNativeQuery(IDictionary<string, object> parameterMap);


	  /// <summary>
	  /// Returns all child executions of a given <seealso cref="IExecutionEntity"/>.
	  /// In the list, child executions will be behind parent executions. 
	  /// </summary>
	  IList<IExecutionEntity> collectChildren(IExecutionEntity executionEntity);

	  IExecutionEntity findFirstScope(IExecutionEntity executionEntity);

	  IExecutionEntity findFirstMultiInstanceRoot(IExecutionEntity executionEntity);


	  void updateExecutionTenantIdForDeployment(string deploymentId, string newTenantId);

	  string updateProcessInstanceBusinessKey(IExecutionEntity executionEntity, string businessKey);


	  void deleteProcessInstancesByProcessDefinition(string processDefinitionId, string deleteReason, bool cascade);

	  void deleteProcessInstance(string processInstanceId, string deleteReason, bool cascade);

	  void deleteProcessInstanceExecutionEntity(string processInstanceId, string currentFlowElementId, string deleteReason, bool cascade, bool cancel);

	  void deleteChildExecutions(IExecutionEntity executionEntity, string deleteReason, bool cancel);

	  void deleteExecutionAndRelatedData(IExecutionEntity executionEntity, string deleteReason, bool cancel);


	  void updateProcessInstanceLockTime(string processInstanceId);

	  void clearProcessInstanceLockTime(string processInstanceId);

	}
}