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

	using org.activiti.engine.task;

	public interface ITaskEntityManager : IEntityManager<ITaskEntity>
	{

	  void insert(ITaskEntity taskEntity, IExecutionEntity execution);

	  void changeTaskAssignee(ITaskEntity taskEntity, string assignee);

	  void changeTaskAssigneeNoEvents(ITaskEntity taskEntity, string assignee);

	  void changeTaskOwner(ITaskEntity taskEntity, string owner);

	  IList<ITaskEntity> findTasksByExecutionId(string executionId);

	  IList<ITaskEntity> findTasksByProcessInstanceId(string processInstanceId);

	  IList<ITask> findTasksByQueryCriteria(TaskQueryImpl taskQuery);

	  IList<ITask> findTasksAndVariablesByQueryCriteria(TaskQueryImpl taskQuery);

	  long findTaskCountByQueryCriteria(TaskQueryImpl taskQuery);

	  IList<ITask> findTasksByNativeQuery(IDictionary<string, object> parameterMap, int firstResult, int maxResults);

	  long findTaskCountByNativeQuery(IDictionary<string, object> parameterMap);

	  IList<ITask> findTasksByParentTaskId(string parentTaskId);

	  void updateTaskTenantIdForDeployment(string deploymentId, string newTenantId);

	  void deleteTask(string taskId, string deleteReason, bool cascade);

	  void deleteTasksByProcessInstanceId(string processInstanceId, string deleteReason, bool cascade);

	  void deleteTask(ITaskEntity task, string deleteReason, bool cascade, bool cancel);

	}
}