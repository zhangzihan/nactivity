using Sys.Workflow;
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
namespace Sys.Workflow.engine.task
{
    /// 
    public interface ITaskInfo
    {

        /// <summary>
        /// DB id of the task. </summary>
        string Id { get; }

        /// <summary>
        /// Name or title of the task.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Free text description of the task.
        /// </summary>
        string Description { get; }

        /// <summary>
        /// 业务键值
        /// </summary>
        string BusinessKey { get; }

        /// <summary>
        /// Indication of how important/urgent this task is
        /// </summary>
        int? Priority { get; }

        /// <summary>
        /// The <seealso cref="IUserInfo.Id"/> of the person that is responsible for this task.
        /// </summary>
        string Owner { get; }

        /// <summary>
        /// The <seealso cref="IUserInfo.Id"/> of the person to which this task is delegated.
        /// </summary>
        string Assignee { get; }

        /// <summary>
        /// 
        /// </summary>
        string AssigneeUser { get; }

        /// <summary>
        /// 
        /// </summary>
        IUserInfo Assigner { get; }

        /// <summary>
        /// Reference to the process instance or null if it is not related to a process instance.
        /// </summary>
        string ProcessInstanceId { get; }

        /// <summary>
        /// Reference to the path of execution or null if it is not related to a process instance.
        /// </summary>
        string ExecutionId { get; }

        /// <summary>
        /// Reference to the process definition or null if it is not related to a process.
        /// </summary>
        string ProcessDefinitionId { get; }

        /// <summary>
        /// The date/time when this task was created </summary>
        DateTime? CreateTime { get; }

        /// <summary>
        /// The id of the activity in the process defining this task or null if this is not related to a process
        /// </summary>
        string TaskDefinitionKey { get; }

        /// <summary>
        /// Due date of the task.
        /// </summary>
        DateTime? DueDate { get; }

        /// <summary>
        /// The category of the task. This is an optional field and allows to 'tag' tasks as belonging to a certain category.
        /// </summary>
        string Category { get; }

        /// <summary>
        /// The parent task for which this task is a subtask
        /// </summary>
        string ParentTaskId { get; }

        /// <summary>
        /// The tenant identifier of this task
        /// </summary>
        string TenantId { get; }

        /// <summary>
        /// The form key for the user task
        /// </summary>
        string FormKey { get; }

        /// <summary>
        /// Returns the local task variables if requested in the task query
        /// </summary>
        IDictionary<string, object> TaskLocalVariables { get; }

        /// <summary>
        /// Returns the process variables if requested in the task query
        /// </summary>
        IDictionary<string, object> ProcessVariables { get; }

        /// <summary>
        /// The claim time of this task
        /// </summary>
        DateTime? ClaimTime { get; }

        /// <summary>
        /// 是否是其他人员追加给我的任务
        /// </summary>
        bool? IsAppend { get; }

        /// <summary>
        /// 是否是其他人员转办给我的任务
        /// </summary>
        bool? IsTransfer { get; }

        /// <summary>
        /// 是否允许任务执行人转给其他人员
        /// </summary>
        bool? CanTransfer { get; }

        /// <summary>
        /// 该任务只允许一个人执行
        /// </summary>
        bool? OnlyAssignee { get; }

        /// <summary>
        /// 是否是其他节点指定的人员
        /// </summary>
        bool? IsRuntime { get; }
    }
}