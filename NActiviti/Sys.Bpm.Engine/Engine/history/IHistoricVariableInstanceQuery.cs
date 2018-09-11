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

namespace org.activiti.engine.history
{

    using org.activiti.engine.query;

    /// <summary>
    /// Programmatic querying for <seealso cref="IHistoricVariableInstance"/>s.
    /// 
    /// 
    /// 
    /// </summary>
    public interface IHistoricVariableInstanceQuery : IQuery<IHistoricVariableInstanceQuery, IHistoricVariableInstance>
    {

        /// <summary>
        /// Only select a historic variable with the given id. </summary>
        IHistoricVariableInstanceQuery id(string id);

        /// <summary>
        /// Only select historic process variables with the given process instance. </summary>
        IHistoricVariableInstanceQuery processInstanceId(string processInstanceId);

        /// <summary>
        /// Only select historic process variables with the given id. * </summary>
        IHistoricVariableInstanceQuery executionId(string executionId);

        /// <summary>
        /// Only select historic process variables whose id is in the given set of ids. </summary>
        IHistoricVariableInstanceQuery executionIds(ISet<string> executionIds);

        /// <summary>
        /// Only select historic process variables with the given task. </summary>
        IHistoricVariableInstanceQuery taskId(string taskId);

        /// <summary>
        /// Only select historic process variables whose id is in the given set of ids. </summary>
        IHistoricVariableInstanceQuery taskIds(ISet<string> taskIds);

        /// <summary>
        /// Only select historic process variables with the given variable name. </summary>
        IHistoricVariableInstanceQuery variableName(string variableName);

        /// <summary>
        /// Only select historic process variables where the given variable name is like. </summary>
        IHistoricVariableInstanceQuery variableNameLike(string variableNameLike);

        /// <summary>
        /// Only select historic process variables which were not set task-local. </summary>
        IHistoricVariableInstanceQuery excludeTaskVariables();

        /// <summary>
        /// Don't initialize variable values. This is foremost a way to deal with variable delete queries </summary>
        IHistoricVariableInstanceQuery excludeVariableInitialization();

        /// <summary>
        /// only select historic process variables with the given name and value </summary>
        IHistoricVariableInstanceQuery variableValueEquals(string variableName, object variableValue);

        /// <summary>
        /// only select historic process variables that don't have the given name and value
        /// </summary>
        IHistoricVariableInstanceQuery variableValueNotEquals(string variableName, object variableValue);

        /// <summary>
        /// only select historic process variables like the given name and value
        /// </summary>
        IHistoricVariableInstanceQuery variableValueLike(string variableName, string variableValue);

        /// <summary>
        /// only select historic process variables like the given name and value (case insensitive)
        /// </summary>
        IHistoricVariableInstanceQuery variableValueLikeIgnoreCase(string variableName, string variableValue);

        IHistoricVariableInstanceQuery orderByProcessInstanceId();

        IHistoricVariableInstanceQuery orderByVariableName();

    }

}