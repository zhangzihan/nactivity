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
namespace Sys.Workflow.engine.impl.persistence.entity
{

    using Sys.Workflow.engine.impl.variable;

    /// 
    public interface IVariableInstanceEntityManager : IEntityManager<IVariableInstanceEntity>
    {

        IVariableInstanceEntity Create(string name, IVariableType type, object value);

        IList<IVariableInstanceEntity> FindVariableInstancesByTaskId(string taskId);

        IList<IVariableInstanceEntity> FindVariableInstancesByTaskIds(string[] taskIds);

        IList<IVariableInstanceEntity> FindVariableInstancesByExecutionId(string executionId);

        IList<IVariableInstanceEntity> FindVariableInstancesByExecutionIds(string[] executionIds);

        IVariableInstanceEntity FindVariableInstanceByExecutionAndName(string executionId, string variableName);

        IList<IVariableInstanceEntity> FindVariableInstancesByExecutionAndNames(string executionId, IEnumerable<string> names);

        IVariableInstanceEntity FindVariableInstanceByTaskAndName(string taskId, string variableName);

        IList<IVariableInstanceEntity> FindVariableInstancesByTaskAndNames(string taskId, IEnumerable<string> names);

        void DeleteVariableInstanceByTask(ITaskEntity task);
    }
}