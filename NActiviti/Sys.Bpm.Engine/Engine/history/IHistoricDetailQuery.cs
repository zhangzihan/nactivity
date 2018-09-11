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
    using org.activiti.engine.query;
    using org.activiti.engine.runtime;

    /// <summary>
    /// Programmatic querying for <seealso cref="IHistoricDetail"/>s.
    /// 
    /// </summary>
    public interface IHistoricDetailQuery : IQuery<IHistoricDetailQuery, IHistoricDetail>
    {

        /// <summary>
        /// Only select historic info with the given id. </summary>
        IHistoricDetailQuery id(string id);

        /// <summary>
        /// Only select historic variable updates with the given process instance. {@link ProcessInstance) ids and <seealso cref="IHistoricProcessInstance"/> ids match.
        /// </summary>
        IHistoricDetailQuery processInstanceId(string processInstanceId);

        /// <summary>
        /// Only select historic variable updates with the given execution. Note that <seealso cref="IExecution"/> ids are not stored in the history as first class citizen, only process instances are.
        /// </summary>
        IHistoricDetailQuery executionId(string executionId);

        /// <summary>
        /// Only select historic variable updates associated to the given <seealso cref="IHistoricActivityInstance activity instance"/>.
        /// </summary>
        IHistoricDetailQuery activityInstanceId(string activityInstanceId);

        /// <summary>
        /// Only select historic variable updates associated to the given <seealso cref="IHistoricTaskInstance historic task instance"/>.
        /// </summary>
        IHistoricDetailQuery taskId(string taskId);

        /// <summary>
        /// Only select <seealso cref="IHistoricVariableUpdate"/>s. </summary>
        IHistoricDetailQuery variableUpdates();

        /// <summary>
        /// Exclude all task-related <seealso cref="IHistoricDetail"/>s, so only items which have no task-id set will be selected. When used together with <seealso cref="#taskId(String)"/>, this call is ignored task details are
        /// NOT excluded.
        /// </summary>
        IHistoricDetailQuery excludeTaskDetails();

        IHistoricDetailQuery orderByProcessInstanceId();

        IHistoricDetailQuery orderByVariableName();

        IHistoricDetailQuery orderByFormPropertyId();

        IHistoricDetailQuery orderByVariableType();

        IHistoricDetailQuery orderByVariableRevision();

        IHistoricDetailQuery orderByTime();
    }

}