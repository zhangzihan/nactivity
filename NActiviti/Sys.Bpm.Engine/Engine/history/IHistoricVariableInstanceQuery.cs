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

namespace Sys.Workflow.engine.history
{

    using Sys.Workflow.engine.query;

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
        IHistoricVariableInstanceQuery SetId(string id);

        /// <summary>
        /// Only select historic process variables with the given process instance. </summary>
        IHistoricVariableInstanceQuery SetProcessInstanceId(string processInstanceId);

        /// <summary>
        /// Only select historic process variables with the given id. * </summary>
        IHistoricVariableInstanceQuery SetExecutionId(string executionId);

        /// <summary>
        /// Only select historic process variables whose id is in the given set of ids. </summary>
        IHistoricVariableInstanceQuery SetExecutionIds(string[] executionIds);

        /// <summary>
        /// Only select historic process variables with the given task. </summary>
        IHistoricVariableInstanceQuery SetTaskId(string taskId);

        /// <summary>
        /// Only select historic process variables whose id is in the given set of ids. </summary>
        IHistoricVariableInstanceQuery SetTaskIds(string[] taskIds);

        /// <summary>
        /// Only select historic process variables with the given variable name. </summary>
        IHistoricVariableInstanceQuery SetVariableName(string variableName);

        /// <summary>
        /// Only select historic process variables where the given variable name is like. </summary>
        IHistoricVariableInstanceQuery SetVariableNameLike(string variableNameLike);

        /// <summary>
        /// Only select historic process variables which were not set task-local. </summary>
        IHistoricVariableInstanceQuery SetExcludeTaskVariables();

        /// <summary>
        /// Don't initialize variable values. This is foremost a way to deal with variable delete queries </summary>
        IHistoricVariableInstanceQuery SetExcludeVariableInitialization();

        /// <summary>
        /// only select historic process variables with the given name and value </summary>
        IHistoricVariableInstanceQuery VariableValueEquals(string variableName, object variableValue);

        /// <summary>
        /// only select historic process variables that don't have the given name and value
        /// </summary>
        IHistoricVariableInstanceQuery VariableValueNotEquals(string variableName, object variableValue);

        /// <summary>
        /// only select historic process variables like the given name and value
        /// </summary>
        IHistoricVariableInstanceQuery VariableValueLike(string variableName, string variableValue);

        /// <summary>
        /// only select historic process variables like the given name and value (case insensitive)
        /// </summary>
        IHistoricVariableInstanceQuery VariableValueLikeIgnoreCase(string variableName, string variableValue);

        IHistoricVariableInstanceQuery OrderByProcessInstanceId();

        IHistoricVariableInstanceQuery OrderByVariableName();
    }
}