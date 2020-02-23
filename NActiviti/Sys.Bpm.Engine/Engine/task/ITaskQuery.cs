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
namespace Sys.Workflow.Engine.Tasks
{

    /// <summary>
    /// Allows programmatic querying of <seealso cref="ITask"/>s;
    /// 
    /// 
    /// 
    /// 
    /// </summary>
    public interface ITaskQuery : ITaskInfoQuery<ITaskQuery, ITask>
    {

        /// <summary>
        /// Only select tasks which don't have an assignee. </summary>
        ITaskQuery SetTaskUnassigned();

        /// <summary>
        /// Only select tasks with the given <seealso cref="DelegationState"/>. </summary>
        ITaskQuery SetTaskDelegationState(DelegationState? delegationState);

        /// <summary>
        /// Select tasks that has been claimed or assigned to user or waiting to claim by user (candidate user or groups). You can invoke <seealso cref="#taskCandidateGroupIn(List)"/> to include tasks that can be
        /// claimed by a user in the given groups while set property <strong>dbIdentityUsed</strong> to <strong>false</strong> in process engine configuration or using custom session factory of
        /// GroupIdentityManager.
        /// </summary>
        ITaskQuery SetTaskCandidateOrAssigned(string userIdForCandidateAndAssignee);

        /// <summary>
        /// Select tasks that has been claimed or assigned to user or waiting to claim by user (candidate user or groups).
        /// </summary>
        ITaskQuery SetTaskCandidateOrAssigned(string userIdForCandidateAndAssignee, IList<string> usersGroups);

        /// <summary>
        /// Only select tasks that have no parent (i.e. do not select subtasks). </summary>
        ITaskQuery SetExcludeSubtasks();

        /// <summary>
        /// Only selects tasks which are suspended, because its process instance was suspended.
        /// </summary>
        ITaskQuery SetSuspended();

        /// <summary>
        /// Only selects tasks which are active (ie. not suspended)
        /// </summary>
        ITaskQuery SetActive();

        /// <summary>
        /// 仅查询与任务关联的businessKey，子活动和子流程可以使用不同的businessKey
        /// </summary>
        /// <param name="isTaskBusinessKey"></param>
        /// <returns></returns>
        ITaskQuery SetIsTaskBusinessKey(bool isTaskBusinessKey);
    }

}