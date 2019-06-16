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

	using Sys.Workflow.Engine.History;

	/// 
	public interface IHistoricTaskInstanceDataManager : IDataManager<IHistoricTaskInstanceEntity>
	{

	  IHistoricTaskInstanceEntity Create(ITaskEntity task, IExecutionEntity execution);

	  IList<IHistoricTaskInstanceEntity> FindHistoricTasksByParentTaskId(string parentTaskId);

	  IList<IHistoricTaskInstanceEntity> FindHistoricTaskInstanceByProcessInstanceId(string processInstanceId);

	  long FindHistoricTaskInstanceCountByQueryCriteria(IHistoricTaskInstanceQuery historicTaskInstanceQuery);

	  IList<IHistoricTaskInstance> FindHistoricTaskInstancesByQueryCriteria(IHistoricTaskInstanceQuery historicTaskInstanceQuery);

	  IList<IHistoricTaskInstance> FindHistoricTaskInstancesAndVariablesByQueryCriteria(IHistoricTaskInstanceQuery historicTaskInstanceQuery);

	  IList<IHistoricTaskInstance> FindHistoricTaskInstancesByNativeQuery(IDictionary<string, object> parameterMap, int firstResult, int maxResults);

	  long FindHistoricTaskInstanceCountByNativeQuery(IDictionary<string, object> parameterMap);

	}

}