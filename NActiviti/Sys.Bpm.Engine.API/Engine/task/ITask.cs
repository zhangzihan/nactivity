using System;

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
namespace org.activiti.engine.task
{

    /// <summary>
    /// Represents one task for a human user.
    /// 
    /// 
    /// 
    /// </summary>
    public interface ITask : ITaskInfo
    {

        /// <summary>
        /// Default value used for priority when a new <seealso cref="ITask"/> is created.
        /// </summary>

        /// <summary>
        /// Name or title of the task. </summary>
        new string Name { get; set; }

        /// <summary>
        /// Sets an optional localized name for the task. </summary>
        string LocalizedName { get; set; }

        /// <summary>
        /// Change the description of the task </summary>
        new string Description { get; set; }

        /// <summary>
        /// Sets an optional localized description for the task. </summary>
        string LocalizedDescription { get; set; }

        /// <summary>
        /// Sets the indication of how important/urgent this task is </summary>
        new int? Priority { get; set; }

        /// <summary>
        /// The <seealso cref="User.getId() userId"/> of the person that is responsible for this task.
        /// </summary>
        new string Owner { get; set; }

        /// <summary>
        /// The <seealso cref="User.getId() userId"/> of the person to which this task is delegated.
        /// </summary>
        new string Assignee { get; set; }

        /// <summary>
        /// The current <seealso cref="DelegationState"/> for this task. </summary>
        DelegationState? DelegationState { get; set; }

        /// <summary>
        /// Change due date of the task. </summary>
        new DateTime? DueDate { get; set; }

        /// <summary>
        /// Change the category of the task. This is an optional field and allows to 'tag' tasks as belonging to a certain category.
        /// </summary>
        new string Category { get; set; }

        /// <summary>
        /// the parent task for which this task is a subtask </summary>
        new string ParentTaskId { get; set; }

        /// <summary>
        /// Change the tenantId of the task </summary>
        new string TenantId { get; set; }

        /// <summary>
        /// Change the form key of the task </summary>
        new string FormKey { get; set; }

        /// <summary>
        /// Indicates whether this task is suspended or not. </summary>
        bool Suspended { get; }

    }

    public static class Task_Fields
    {
        public const int DEFAULT_PRIORITY = 50;
    }

}