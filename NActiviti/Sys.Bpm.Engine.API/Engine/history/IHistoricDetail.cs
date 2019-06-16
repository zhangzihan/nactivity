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

    /// <summary>
    /// Base class for all kinds of information that is related to either a <seealso cref="IHistoricProcessInstance"/> or a <seealso cref="IHistoricActivityInstance"/>.
    /// 
    /// 
    /// </summary>
    public interface IHistoricDetail : IHistoricData
    {

        /// <summary>
        /// The unique DB id for this historic detail </summary>
        string Id { get; }

        /// <summary>
        /// The process instance reference. </summary>
        string ProcessInstanceId { get; }

        /// <summary>
        /// The activity reference in case this detail is related to an activity instance.
        /// </summary>
        string ActivityInstanceId { get; }

        /// <summary>
        /// The identifier for the path of execution. </summary>
        string ExecutionId { get; }

        /// <summary>
        /// The identifier for the task. </summary>
        string TaskId { get; }

        /// <summary>
        /// The time when this detail occurred </summary>
        new DateTime? Time { get; }
    }

}