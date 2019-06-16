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

namespace Sys.Workflow.Engine.History
{

    using Sys.Workflow.Engine.Tasks;

    /// <summary>
    /// Allows programmatic querying for <seealso cref="IHistoricTaskInstance"/>s.
    /// 
    /// 
    /// 
    /// </summary>
    public interface IHistoricTaskInstanceQuery : ITaskInfoQuery<IHistoricTaskInstanceQuery, IHistoricTaskInstance>
    {

        /// <summary>
        /// Only select historic task instances with the given task delete reason. </summary>
        IHistoricTaskInstanceQuery SetTaskDeleteReason(string taskDeleteReason);

        /// <summary>
        /// Only select historic task instances with a task description like the given value. The syntax that should be used is the same as in SQL, eg. %activiti%.
        /// </summary>
        IHistoricTaskInstanceQuery SetTaskDeleteReasonLike(string taskDeleteReasonLike);

        /// <summary>
        /// Only select historic task instances which are finished.
        /// </summary>
        IHistoricTaskInstanceQuery SetFinished();

        /// <summary>
        /// Only select historic task instances which aren't finished yet.
        /// </summary>
        IHistoricTaskInstanceQuery SetUnfinished();

        /// <summary>
        /// Only select historic task instances which are part of a process instance which is already finished.
        /// </summary>
        IHistoricTaskInstanceQuery SetProcessFinished();

        /// <summary>
        /// Only select historic task instances which are part of a process instance which is not finished yet.
        /// </summary>
        IHistoricTaskInstanceQuery SetProcessUnfinished();

        /// <summary>
        /// Only select subtasks of the given parent task </summary>
        IHistoricTaskInstanceQuery SetTaskParentTaskId(string parentTaskId);

        /// <summary>
        /// Only select select historic task instances which are completed on the given date
        /// </summary>
        IHistoricTaskInstanceQuery SetTaskCompletedOn(DateTime? endDate);

        /// <summary>
        /// Only select select historic task instances which are completed before the given date
        /// </summary>
        IHistoricTaskInstanceQuery SetTaskCompletedBefore(DateTime? endDate);

        /// <summary>
        /// Only select select historic task instances which are completed after the given date
        /// </summary>
        IHistoricTaskInstanceQuery SetTaskCompletedAfter(DateTime? endDate);

        // ORDERING

        /// <summary>
        /// Order by the historic activity instance id this task was used in (needs to be followed by <seealso cref="#asc()"/> or <seealso cref="#desc()"/>).
        /// </summary>
        IHistoricTaskInstanceQuery OrderByHistoricActivityInstanceId();

        /// <summary>
        /// Order by duration (needs to be followed by <seealso cref="#asc()"/> or <seealso cref="#desc()"/>).
        /// </summary>
        IHistoricTaskInstanceQuery OrderByHistoricTaskInstanceDuration();

        /// <summary>
        /// Order by end time (needs to be followed by <seealso cref="#asc()"/> or <seealso cref="#desc()"/>).
        /// </summary>
        IHistoricTaskInstanceQuery OrderByHistoricTaskInstanceEndTime();

        /// <summary>
        /// Order by start time (needs to be followed by <seealso cref="#asc()"/> or <seealso cref="#desc()"/>).
        /// </summary>
        IHistoricTaskInstanceQuery OrderByHistoricTaskInstanceStartTime();

        /// <summary>
        /// Order by task delete reason (needs to be followed by <seealso cref="#asc()"/> or <seealso cref="#desc()"/>).
        /// </summary>
        IHistoricTaskInstanceQuery OrderByDeleteReason();

    }

}