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

	/// 
	public interface IVariableInstanceDataManager : IDataManager<IVariableInstanceEntity>
	{

	  IList<IVariableInstanceEntity> findVariableInstancesByTaskId(string taskId);

	  IList<IVariableInstanceEntity> findVariableInstancesByTaskIds(ISet<string> taskIds);

	  IList<IVariableInstanceEntity> findVariableInstancesByExecutionId(string executionId);

	  IList<IVariableInstanceEntity> findVariableInstancesByExecutionIds(ISet<string> executionIds);

	  IVariableInstanceEntity findVariableInstanceByExecutionAndName(string executionId, string variableName);

	  IList<IVariableInstanceEntity> findVariableInstancesByExecutionAndNames(string executionId, ICollection<string> names);

	  IVariableInstanceEntity findVariableInstanceByTaskAndName(string taskId, string variableName);

	  IList<IVariableInstanceEntity> findVariableInstancesByTaskAndNames(string taskId, ICollection<string> names);

	}

}