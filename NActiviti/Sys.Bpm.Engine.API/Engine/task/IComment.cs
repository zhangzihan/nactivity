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

namespace Sys.Workflow.Engine.Tasks
{

    using Sys.Workflow.Engine.History;

    /// <summary>
    /// User comments that form discussions around tasks.
    /// </summary>
    /// <seealso cref= {@link TaskService#getTaskComments(String)
    /// 
    ///  </seealso>
    public interface IComment : IHistoricData
    {

        /// <summary>
        /// unique identifier for this comment </summary>
        string Id { get; }

        /// <summary>
        /// reference to the user that made the comment </summary>
        string UserId { get; }

        /// <summary>
        /// time and date when the user made the comment </summary>
        new DateTime? Time { get; }

        /// <summary>
        /// reference to the task on which this comment was made </summary>
        string TaskId { get; }

        /// <summary>
        /// reference to the process instance on which this comment was made </summary>
        string ProcessInstanceId { get; }

        /// <summary>
        /// reference to the type given to the comment </summary>
        string Type { get; }

        /// <summary>
        /// the full comment message the user had related to the task and/or process instance
        /// </summary>
        /// <seealso cref= ITaskService.GetTaskComments(String) </seealso>
        string FullMessage { get; }
    }

}