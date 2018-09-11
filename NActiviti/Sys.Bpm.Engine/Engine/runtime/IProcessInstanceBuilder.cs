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
namespace org.activiti.engine.runtime
{


    /// <summary>
    /// Helper for starting new ProcessInstance.
    /// 
    /// An instance can be obtained through <seealso cref="org.activiti.engine.IRuntimeService#createProcessInstanceBuilder()"/>.
    /// 
    /// processDefinitionId or processDefinitionKey should be set before calling <seealso cref="#start()"/> to start a process instance.
    /// 
    /// 
    /// 
    /// 
    /// </summary>
    public interface IProcessInstanceBuilder
    {

        /// <summary>
        /// Set the id of the process definition 
        /// 
        /// </summary>
        IProcessInstanceBuilder processDefinitionId(string processDefinitionId);

        /// <summary>
        /// Set the key of the process definition, latest version of the process definition with the given key. 
        /// If processDefinitionId was set this will be ignored
        /// 
        /// </summary>
        IProcessInstanceBuilder processDefinitionKey(string processDefinitionKey);

        /// <summary>
        /// Set the message name that needs to be used to look up the process definition that needs to be used to start the process instance.
        /// </summary>
        IProcessInstanceBuilder messageName(string messageName);

        /// <summary>
        /// Set the name of process instance 
        /// 
        /// </summary>
        IProcessInstanceBuilder name(string processInstanceName);

        /// <summary>
        /// Set the businessKey of process instance 
        /// 
        /// </summary>
        IProcessInstanceBuilder businessKey(string businessKey);

        /// <summary>
        /// Set the tenantId of process instance 
        /// 
        /// </summary>
        IProcessInstanceBuilder tenantId(string tenantId);

        /// <summary>
        /// Sets the process variables
        /// </summary>
        IProcessInstanceBuilder variables(IDictionary<string, object> variables);

        /// <summary>
        /// Adds a variable to the process instance 
        /// 
        /// </summary>
        IProcessInstanceBuilder variable(string variableName, object value);

        /// <summary>
        /// Sets the transient variables
        /// </summary>
        IProcessInstanceBuilder transientVariables(IDictionary<string, object> transientVariables);

        /// <summary>
        /// Adds a transient variable to the process instance
        /// </summary>
        IProcessInstanceBuilder transientVariable(string variableName, object value);

        /// <summary>
        /// Start the process instance
        /// </summary>
        /// <exception cref="ActivitiIllegalArgumentException">
        ///           if processDefinitionKey and processDefinitionId are null </exception>
        /// <exception cref="ActivitiObjectNotFoundException">
        ///           when no process definition is deployed with the given processDefinitionKey or processDefinitionId
        /// * </exception>
        IProcessInstance start();

    }

}