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

    using Sys.Workflow.Engine.Tasks;

    public interface ITaskEntityManager : IEntityManager<ITaskEntity>
    {

        void Insert(ITaskEntity taskEntity, IExecutionEntity execution);

        void ChangeTaskAssignee(ITaskEntity taskEntity, string assignee, string assigneeUser);

        void ChangeTaskAssigneeNoEvents(ITaskEntity taskEntity, string assignee, string assigneeUser);

        void ChangeTaskOwner(ITaskEntity taskEntity, string owner);

        IList<ITaskEntity> FindTasksByExecutionId(string executionId);

        IList<ITaskEntity> FindTasksByProcessInstanceId(string processInstanceId);

        IList<ITask> FindTasksByQueryCriteria(ITaskQuery taskQuery);

        IList<ITask> FindTasksAndVariablesByQueryCriteria(ITaskQuery taskQuery);

        long FindTaskCountByQueryCriteria(ITaskQuery taskQuery);

        IList<ITask> FindTasksByNativeQuery(IDictionary<string, object> parameterMap, int firstResult, int maxResults);

        long FindTaskCountByNativeQuery(IDictionary<string, object> parameterMap);

        IList<ITask> FindTasksByParentTaskId(string parentTaskId);

        void UpdateTaskTenantIdForDeployment(string deploymentId, string newTenantId);

        void DeleteTask(string taskId, string deleteReason, bool cascade);

        void DeleteTasksByProcessInstanceId(string processInstanceId, string deleteReason, bool cascade);

        void DeleteTask(ITaskEntity task, string deleteReason, bool cascade, bool cancel);
    }
}