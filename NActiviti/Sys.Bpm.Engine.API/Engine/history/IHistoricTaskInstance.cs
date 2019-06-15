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

namespace org.activiti.engine.history
{

    using org.activiti.engine.task;

    /// <summary>
    /// Represents a historic task instance (waiting, finished or deleted) that is stored permanent for statistics, audit and other business intelligence purposes.
    /// 
    /// 
    /// 
    /// </summary>
    public interface IHistoricTaskInstance : ITaskInfo, IHistoricData
    {
        /// <summary>
        /// The reason why this task was deleted {'completed' | 'deleted' | any other user defined string }.
        /// </summary>
        string DeleteReason { get; }

        /// <summary>
        /// Time when the task started. </summary>
        DateTime? StartTime { get; }

        /// <summary>
        /// Time when the task was deleted or completed. </summary>
        DateTime? EndTime { get; }

        /// <summary>
        /// Difference between <seealso cref="EndTime"/> and <seealso cref="StartTime"/> in milliseconds.
        /// </summary>
        long? DurationInMillis { get; }

        /// <summary>
        /// Difference between <seealso cref="EndTime"/> and <seealso cref="ClaimTime"/> in milliseconds.
        /// </summary>
        long? WorkTimeInMillis { get; }

        /// <summary>
        /// Time when the task was claimed. </summary>
        new DateTime? ClaimTime { get; set; }

        /// <inheritdoc />
        new bool? IsAppend { get; set; }

        /// <inheritdoc />
        new bool? IsTransfer { get; set; }

        /// <inheritdoc />
        new bool? CanTransfer { get; set; }

        /// <inheritdoc />
        new bool? OnlyAssignee { get; set; }

    }

}