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

    using org.activiti.engine.impl.history;

    /// <summary>
    /// A single process variable containing the last value when its process instance has finished. It is only available when HISTORY_LEVEL is set >= VARIABLE
    /// 
    /// 
    /// 
    /// </summary>
    public interface IHistoricVariableInstance : IHistoricData
    {

        /// <summary>
        /// The unique DB id </summary>
        string Id { get; }

        string VariableName { get; }

        string VariableTypeName { get; }

        object Value { get; }

        /// <summary>
        /// The process instance reference. </summary>
        string ProcessInstanceId { get; }

        /// <returns> the task id of the task, in case this variable instance has been set locally on a task. Returns null, if this variable is not related to a task. </returns>
        string TaskId { get; }

        /// <summary>
        /// Returns the time when the variable was created.
        /// </summary>
        DateTime? CreateTime { get; }

        /// <summary>
        /// Returns the time when the value of the variable was last updated. Note that a <seealso cref="IHistoricVariableInstance"/> only contains the latest value of the variable. The actual different value and value
        /// changes are recorded in <seealso cref="IHistoricVariableUpdate"/> instances, which are captured on <seealso cref="HistoryLevel"/> FULL.
        /// </summary>
        DateTime? LastUpdatedTime { get; }

    }

}