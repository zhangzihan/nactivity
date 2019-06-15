using System;

/*
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *       http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

namespace org.activiti.engine.history
{

    /// <summary>
    /// Represents one execution of an activity and it's stored permanent for statistics, audit and other business intelligence purposes.
    /// 
    /// 
    /// 
    /// </summary>
    public interface IHistoricActivityInstance : IHistoricData
    {

        /// <summary>
        /// The unique identifier of this historic activity instance. </summary>
        string Id { get; }

        /// <summary>
        /// The unique identifier of the activity in the process </summary>
        string ActivityId { get; }

        /// <summary>
        /// The display name for the activity </summary>
        string ActivityName { get; }

        /// <summary>
        /// The XML tag of the activity as in the process file </summary>
        string ActivityType { get; }

        /// <summary>
        /// Process definition reference </summary>
        string ProcessDefinitionId { get; }

        /// <summary>
        /// Process instance reference </summary>
        string ProcessInstanceId { get; }

        /// <summary>
        /// Execution reference </summary>
        string ExecutionId { get; }

        /// <summary>
        /// The corresponding task in case of task activity </summary>
        string TaskId { get; }

        /// <summary>
        /// The called process instance in case of call activity </summary>
        string CalledProcessInstanceId { get; }

        /// <summary>
        /// Assignee in case of user task activity </summary>
        string Assignee { get; }

        /// <summary>
        /// Assignee in case of user task activity </summary>
        string AssigneeUser { get; }

        /// <summary>
        /// Time when the activity instance started </summary>
        DateTime? StartTime { get; }

        /// <summary>
        /// Time when the activity instance ended </summary>
        DateTime? EndTime { get; }

        /// <summary>
        /// Difference between <seealso cref="EndTime"/> and <seealso cref="StartTime"/>. </summary>
        long? DurationInMillis { get; }

        /// <summary>
        /// Returns the delete reason for this activity, if any was set (if completed normally, no delete reason is set) </summary>
        string DeleteReason { get; }

        /// <summary>
        /// Returns the tenant identifier for the historic activity </summary>
        string TenantId { get; }
    }

}