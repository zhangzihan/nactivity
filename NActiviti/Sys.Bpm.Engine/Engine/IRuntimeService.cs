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
namespace org.activiti.engine
{

    using org.activiti.bpmn.model;
    using org.activiti.engine.@delegate.@event;
    using org.activiti.engine.impl.persistence.entity;
    using org.activiti.engine.runtime;
    using org.activiti.engine.task;

    /// 
    /// 
    /// 
    /// 
    public interface IRuntimeService
    {

        /// <summary>
        /// Create a <seealso cref="IProcessInstanceBuilder"/>, that allows to set various options for starting a process instance,
        /// as an alternative to the various startProcessInstanceByXX methods. 
        /// </summary>
        IProcessInstanceBuilder createProcessInstanceBuilder();

        /// <summary>
        /// Starts a new process instance in the latest version of the process definition with the given key.
        /// </summary>
        /// <param name="processDefinitionKey">
        ///          key of process definition, cannot be null. </param>
        /// <exception cref="ActivitiObjectNotFoundException">
        ///           when no process definition is deployed with the given key. </exception>
        IProcessInstance startProcessInstanceByKey(string processDefinitionKey);

        /// <summary>
        /// Starts a new process instance in the latest version of the process definition with the given key.
        /// 
        /// A business key can be provided to associate the process instance with a certain identifier that has a clear business meaning. For example in an order process, the business key could be an order
        /// id. This business key can then be used to easily look up that process instance , see <seealso cref="IProcessInstanceQuery#processInstanceBusinessKey(String)"/>. Providing such a business key is definitely a
        /// best practice.
        /// </summary>
        /// <param name="processDefinitionKey">
        ///          key of process definition, cannot be null. </param>
        /// <param name="businessKey">
        ///          a key that uniquely identifies the process instance in the context or the given process definition. </param>
        /// <exception cref="ActivitiObjectNotFoundException">
        ///           when no process definition is deployed with the given key. </exception>
        IProcessInstance startProcessInstanceByKey(string processDefinitionKey, string businessKey);

        /// <summary>
        /// Starts a new process instance in the latest version of the process definition with the given key
        /// </summary>
        /// <param name="processDefinitionKey">
        ///          key of process definition, cannot be null. </param>
        /// <param name="variables">
        ///          the variables to pass, can be null. </param>
        /// <exception cref="ActivitiObjectNotFoundException">
        ///           when no process definition is deployed with the given key. </exception>
        IProcessInstance startProcessInstanceByKey(string processDefinitionKey, IDictionary<string, object> variables);

        /// <summary>
        /// Starts a new process instance in the latest version of the process definition with the given key.
        /// 
        /// A business key can be provided to associate the process instance with a certain identifier that has a clear business meaning. For example in an order process, the business key could be an order
        /// id. This business key can then be used to easily look up that process instance , see <seealso cref="IProcessInstanceQuery#processInstanceBusinessKey(String)"/>. Providing such a business key is definitely a
        /// best practice.
        /// 
        /// The combination of processdefinitionKey-businessKey must be unique.
        /// </summary>
        /// <param name="processDefinitionKey">
        ///          key of process definition, cannot be null. </param>
        /// <param name="variables">
        ///          the variables to pass, can be null. </param>
        /// <param name="businessKey">
        ///          a key that uniquely identifies the process instance in the context or the given process definition. </param>
        /// <exception cref="ActivitiObjectNotFoundException">
        ///           when no process definition is deployed with the given key. </exception>
        IProcessInstance startProcessInstanceByKey(string processDefinitionKey, string businessKey, IDictionary<string, object> variables);

        /// <summary>
        /// Similar to <seealso cref="#startProcessInstanceByKey(String)"/>, but using a specific tenant identifier.
        /// </summary>
        IProcessInstance startProcessInstanceByKeyAndTenantId(string processDefinitionKey, string tenantId);

        /// <summary>
        /// Similar to <seealso cref="#startProcessInstanceByKey(String, String)"/>, but using a specific tenant identifier.
        /// </summary>
        IProcessInstance startProcessInstanceByKeyAndTenantId(string processDefinitionKey, string businessKey, string tenantId);

        /// <summary>
        /// Similar to <seealso cref="#startProcessInstanceByKey(String, Map)"/>, but using a specific tenant identifier.
        /// </summary>
        IProcessInstance startProcessInstanceByKeyAndTenantId(string processDefinitionKey, IDictionary<string, object> variables, string tenantId);

        /// <summary>
        /// Similar to <seealso cref="#startProcessInstanceByKey(String, String, Map)"/>, but using a specific tenant identifier.
        /// </summary>
        IProcessInstance startProcessInstanceByKeyAndTenantId(string processDefinitionKey, string businessKey, IDictionary<string, object> variables, string tenantId);

        /// <summary>
        /// Starts a new process instance in the exactly specified version of the process definition with the given id.
        /// </summary>
        /// <param name="processDefinitionId">
        ///          the id of the process definition, cannot be null. </param>
        /// <exception cref="ActivitiObjectNotFoundException">
        ///           when no process definition is deployed with the given key. </exception>
        IProcessInstance startProcessInstanceById(string processDefinitionId);

        /// <summary>
        /// Starts a new process instance in the exactly specified version of the process definition with the given id.
        /// 
        /// A business key can be provided to associate the process instance with a certain identifier that has a clear business meaning. For example in an order process, the business key could be an order
        /// id. This business key can then be used to easily look up that process instance , see <seealso cref="IProcessInstanceQuery#processInstanceBusinessKey(String)"/>. Providing such a business key is definitely a
        /// best practice.
        /// </summary>
        /// <param name="processDefinitionId">
        ///          the id of the process definition, cannot be null. </param>
        /// <param name="businessKey">
        ///          a key that uniquely identifies the process instance in the context or the given process definition. </param>
        /// <exception cref="ActivitiObjectNotFoundException">
        ///           when no process definition is deployed with the given key. </exception>
        IProcessInstance startProcessInstanceById(string processDefinitionId, string businessKey);

        /// <summary>
        /// Starts a new process instance in the exactly specified version of the process definition with the given id.
        /// </summary>
        /// <param name="processDefinitionId">
        ///          the id of the process definition, cannot be null. </param>
        /// <param name="variables">
        ///          variables to be passed, can be null </param>
        /// <exception cref="ActivitiObjectNotFoundException">
        ///           when no process definition is deployed with the given key. </exception>
        IProcessInstance startProcessInstanceById(string processDefinitionId, IDictionary<string, object> variables);

        /// <summary>
        /// Starts a new process instance in the exactly specified version of the process definition with the given id.
        /// 
        /// A business key can be provided to associate the process instance with a certain identifier that has a clear business meaning. For example in an order process, the business key could be an order
        /// id. This business key can then be used to easily look up that process instance , see <seealso cref="IProcessInstanceQuery#processInstanceBusinessKey(String)"/>. Providing such a business key is definitely a
        /// best practice.
        /// </summary>
        /// <param name="processDefinitionId">
        ///          the id of the process definition, cannot be null. </param>
        /// <param name="variables">
        ///          variables to be passed, can be null </param>
        /// <exception cref="ActivitiObjectNotFoundException">
        ///           when no process definition is deployed with the given key. </exception>
        IProcessInstance startProcessInstanceById(string processDefinitionId, string businessKey, IDictionary<string, object> variables);

        /// <summary>
        /// <para>
        /// Signals the process engine that a message is received and starts a new <seealso cref="IProcessInstance"/>.
        /// </para>
        /// 
        /// <para>
        /// Calling this method can have two different outcomes:
        /// <ul>
        /// <li>If the message name is associated with a message start event, a new process instance is started.</li>
        /// <li>If no subscription to a message with the given name exists, <seealso cref="ActivitiException"/> is thrown</li>
        /// </ul>
        /// </para>
        /// </summary>
        /// <param name="messageName">
        ///          the 'name' of the message as specified as an attribute on the bpmn20 {@code <message name="messageName" />} element.
        /// </param>
        /// <returns> the <seealso cref="IProcessInstance"/> object representing the started process instance
        /// </returns>
        /// <exception cref="ActivitiException">
        ///           if no subscription to a message with the given name exists
        /// 
        /// @since 5.9 </exception>
        IProcessInstance startProcessInstanceByMessage(string messageName);

        /// <summary>
        /// Similar to <seealso cref="IRuntimeService#startProcessInstanceByMessage(String)"/>, but with tenant context.
        /// </summary>
        IProcessInstance startProcessInstanceByMessageAndTenantId(string messageName, string tenantId);

        /// <summary>
        /// <para>
        /// Signals the process engine that a message is received and starts a new <seealso cref="IProcessInstance"/>.
        /// </para>
        /// 
        /// See <seealso cref="#startProcessInstanceByMessage(String, Map)"/>. This method allows specifying a business key.
        /// </summary>
        /// <param name="messageName">
        ///          the 'name' of the message as specified as an attribute on the bpmn20 {@code <message name="messageName" />} element. </param>
        /// <param name="businessKey">
        ///          the business key which is added to the started process instance
        /// </param>
        /// <exception cref="ActivitiException">
        ///           if no subscription to a message with the given name exists
        /// 
        /// @since 5.10 </exception>
        IProcessInstance startProcessInstanceByMessage(string messageName, string businessKey);

        /// <summary>
        /// Similar to <seealso cref="IRuntimeService#startProcessInstanceByMessage(String, String)"/>, but with tenant context.
        /// </summary>
        IProcessInstance startProcessInstanceByMessageAndTenantId(string messageName, string businessKey, string tenantId);

        /// <summary>
        /// <para>
        /// Signals the process engine that a message is received and starts a new <seealso cref="IProcessInstance"/>.
        /// </para>
        /// 
        /// See <seealso cref="#startProcessInstanceByMessage(String)"/>. In addition, this method allows specifying a the payload of the message as a map of process variables.
        /// </summary>
        /// <param name="messageName">
        ///          the 'name' of the message as specified as an attribute on the bpmn20 {@code <message name="messageName" />} element. </param>
        /// <param name="processVariables">
        ///          the 'payload' of the message. The variables are added as processes variables to the started process instance. </param>
        /// <returns> the <seealso cref="IProcessInstance"/> object representing the started process instance
        /// </returns>
        /// <exception cref="ActivitiException">
        ///           if no subscription to a message with the given name exists
        /// 
        /// @since 5.9 </exception>
        IProcessInstance startProcessInstanceByMessage(string messageName, IDictionary<string, object> processVariables);

        /// <summary>
        /// Similar to <seealso cref="IRuntimeService#startProcessInstanceByMessage(String, Map<String, Object>)"/>, but with tenant context.
        /// </summary>
        IProcessInstance startProcessInstanceByMessageAndTenantId(string messageName, IDictionary<string, object> processVariables, string tenantId);

        /// <summary>
        /// <para>
        /// Signals the process engine that a message is received and starts a new <seealso cref="IProcessInstance"/>.
        /// </para>
        /// 
        /// See <seealso cref="#startProcessInstanceByMessage(String, Map)"/>. In addition, this method allows specifying a business key.
        /// </summary>
        /// <param name="messageName">
        ///          the 'name' of the message as specified as an attribute on the bpmn20 {@code <message name="messageName" />} element. </param>
        /// <param name="businessKey">
        ///          the business key which is added to the started process instance </param>
        /// <param name="processVariables">
        ///          the 'payload' of the message. The variables are added as processes variables to the started process instance. </param>
        /// <returns> the <seealso cref="IProcessInstance"/> object representing the started process instance
        /// </returns>
        /// <exception cref="ActivitiException">
        ///           if no subscription to a message with the given name exists
        /// 
        /// @since 5.9 </exception>
        IProcessInstance startProcessInstanceByMessage(string messageName, string businessKey, IDictionary<string, object> processVariables);

        /// <summary>
        /// Similar to <seealso cref="IRuntimeService#startProcessInstanceByMessage(String, String, Map<String, Object>)"/>, but with tenant context.
        /// </summary>
        IProcessInstance startProcessInstanceByMessageAndTenantId(string messageName, string businessKey, IDictionary<string, object> processVariables, string tenantId);

        /// <summary>
        /// Delete an existing runtime process instance.
        /// </summary>
        /// <param name="processInstanceId">
        ///          id of process instance to delete, cannot be null. </param>
        /// <param name="deleteReason">
        ///          reason for deleting, can be null. </param>
        /// <exception cref="ActivitiObjectNotFoundException">
        ///           when no process instance is found with the given id. </exception>
        void deleteProcessInstance(string processInstanceId, string deleteReason);

        /// <summary>
        /// Finds the activity ids for all executions that are waiting in activities. This is a list because a single activity can be active multiple times.
        /// </summary>
        /// <param name="executionId">
        ///          id of the execution, cannot be null. </param>
        /// <exception cref="ActivitiObjectNotFoundException">
        ///           when no execution exists with the given executionId. </exception>
        IList<string> getActiveActivityIds(string executionId);

        /// <summary>
        /// Sends an external trigger to an activity instance that is waiting inside the given execution.
        /// </summary>
        /// <param name="executionId">
        ///          id of execution to signal, cannot be null. </param>
        /// <exception cref="ActivitiObjectNotFoundException">
        ///           when no execution is found for the given executionId. </exception>
        void trigger(string executionId);

        /// <summary>
        /// Sends an external trigger to an activity instance that is waiting inside the given execution.
        /// </summary>
        /// <param name="executionId">
        ///          id of execution to signal, cannot be null. </param>
        /// <param name="processVariables">
        ///          a map of process variables </param>
        /// <exception cref="ActivitiObjectNotFoundException">
        ///           when no execution is found for the given executionId. </exception>
        void trigger(string executionId, IDictionary<string, object> processVariables);


        /// <summary>
        /// Similar to <seealso cref="#trigger(String, Map)"/>, but with an extra parameter that allows to pass
        /// transient variables.
        /// </summary>
        void trigger(string executionId, IDictionary<string, object> processVariables, IDictionary<string, object> transientVariables);

        /// <summary>
        /// Updates the business key for the provided process instance
        /// </summary>
        /// <param name="processInstanceId">
        ///          id of the process instance to set the business key, cannot be null </param>
        /// <param name="businessKey">
        ///          new businessKey value </param>
        void updateBusinessKey(string processInstanceId, string businessKey);

        // Identity Links
        // ///////////////////////////////////////////////////////////////

        /// <summary>
        /// Involves a user with a process instance. The type of identity link is defined by the given identityLinkType.
        /// </summary>
        /// <param name="processInstanceId">
        ///          id of the process instance, cannot be null. </param>
        /// <param name="userId">
        ///          id of the user involve, cannot be null. </param>
        /// <param name="identityLinkType"> </param>
        ///          type of identityLink, cannot be null (<seealso cref= <seealso cref="IdentityLinkType"/>). </seealso>
        /// <exception cref="ActivitiObjectNotFoundException">
        ///           when the process instance doesn't exist. </exception>
        void addUserIdentityLink(string processInstanceId, string userId, string identityLinkType);

        /// <summary>
        /// Involves a group with a process instance. The type of identityLink is defined by the given identityLink.
        /// </summary>
        /// <param name="processInstanceId">
        ///          id of the process instance, cannot be null. </param>
        /// <param name="groupId">
        ///          id of the group to involve, cannot be null. </param>
        /// <param name="identityLinkType"> </param>
        ///          type of identity, cannot be null (<seealso cref= <seealso cref="IdentityLinkType"/>). </seealso>
        /// <exception cref="ActivitiObjectNotFoundException">
        ///           when the process instance or group doesn't exist. </exception>
        void addGroupIdentityLink(string processInstanceId, string groupId, string identityLinkType);

        /// <summary>
        /// Convenience shorthand for <seealso cref="#addUserIdentityLink(String, String, String)"/>; with type <seealso cref="IdentityLinkType#CANDIDATE"/>
        /// </summary>
        /// <param name="processInstanceId">
        ///          id of the process instance, cannot be null. </param>
        /// <param name="userId">
        ///          id of the user to use as candidate, cannot be null. </param>
        /// <exception cref="ActivitiObjectNotFoundException">
        ///           when the task or user doesn't exist. </exception>
        void addParticipantUser(string processInstanceId, string userId);

        /// <summary>
        /// Convenience shorthand for <seealso cref="#addGroupIdentityLink(String, String, String)"/>; with type <seealso cref="IdentityLinkType#CANDIDATE"/>
        /// </summary>
        /// <param name="processInstanceId">
        ///          id of the process instance, cannot be null. </param>
        /// <param name="groupId">
        ///          id of the group to use as candidate, cannot be null. </param>
        /// <exception cref="ActivitiObjectNotFoundException">
        ///           when the task or group doesn't exist. </exception>
        void addParticipantGroup(string processInstanceId, string groupId);

        /// <summary>
        /// Convenience shorthand for <seealso cref="#deleteUserIdentityLink(String, String, String)"/>; with type <seealso cref="IdentityLinkType#CANDIDATE"/>
        /// </summary>
        /// <param name="processInstanceId">
        ///          id of the process instance, cannot be null. </param>
        /// <param name="userId">
        ///          id of the user to use as candidate, cannot be null. </param>
        /// <exception cref="ActivitiObjectNotFoundException">
        ///           when the task or user doesn't exist. </exception>
        void deleteParticipantUser(string processInstanceId, string userId);

        /// <summary>
        /// Convenience shorthand for <seealso cref="#deleteGroupIdentityLink(String, String, String)"/>; with type <seealso cref="IdentityLinkType#CANDIDATE"/>
        /// </summary>
        /// <param name="processInstanceId">
        ///          id of the process instance, cannot be null. </param>
        /// <param name="groupId">
        ///          id of the group to use as candidate, cannot be null. </param>
        /// <exception cref="ActivitiObjectNotFoundException">
        ///           when the task or group doesn't exist. </exception>
        void deleteParticipantGroup(string processInstanceId, string groupId);

        /// <summary>
        /// Removes the association between a user and a process instance for the given identityLinkType.
        /// </summary>
        /// <param name="processInstanceId">
        ///          id of the process instance, cannot be null. </param>
        /// <param name="userId">
        ///          id of the user involve, cannot be null. </param>
        /// <param name="identityLinkType"> </param>
        ///          type of identityLink, cannot be null (<seealso cref= <seealso cref="IdentityLinkType"/>). </seealso>
        /// <exception cref="ActivitiObjectNotFoundException">
        ///           when the task or user doesn't exist. </exception>
        void deleteUserIdentityLink(string processInstanceId, string userId, string identityLinkType);

        /// <summary>
        /// Removes the association between a group and a process instance for the given identityLinkType.
        /// </summary>
        /// <param name="processInstanceId">
        ///          id of the process instance, cannot be null. </param>
        /// <param name="groupId">
        ///          id of the group to involve, cannot be null. </param>
        /// <param name="identityLinkType"> </param>
        ///          type of identity, cannot be null (<seealso cref= <seealso cref="IdentityLinkType"/>). </seealso>
        /// <exception cref="ActivitiObjectNotFoundException">
        ///           when the task or group doesn't exist. </exception>
        void deleteGroupIdentityLink(string processInstanceId, string groupId, string identityLinkType);

        /// <summary>
        /// Retrieves the <seealso cref="IIdentityLink"/>s associated with the given process instance. Such an <seealso cref="IIdentityLink"/> informs how a certain user is involved with a process instance.
        /// </summary>
        IList<IIdentityLink> getIdentityLinksForProcessInstance(string instanceId);

        // Variables
        // ////////////////////////////////////////////////////////////////////

        /// <summary>
        /// All variables visible from the given execution scope (including parent scopes).
        /// </summary>
        /// <param name="executionId">
        ///          id of execution, cannot be null. </param>
        /// <returns> the variables or an empty map if no such variables are found. </returns>
        /// <exception cref="ActivitiObjectNotFoundException">
        ///           when no execution is found for the given executionId. </exception>
        IDictionary<string, object> getVariables(string executionId);

        /// <summary>
        /// All variables visible from the given execution scope (including parent scopes).
        /// </summary>
        /// <param name="executionId">
        ///          id of execution, cannot be null. </param>
        /// <returns> the variable instances or an empty map if no such variables are found. </returns>
        /// <exception cref="ActivitiObjectNotFoundException">
        ///           when no execution is found for the given executionId. </exception>
        IDictionary<string, IVariableInstance> getVariableInstances(string executionId);

        /// <summary>
        /// All variables visible from the given execution scope (including parent
        /// scopes).
        /// </summary>
        /// <param name="executionIds">
        ///          ids of execution, cannot be null. </param>
        /// <returns> the variables. </returns>
        IList<IVariableInstance> getVariableInstancesByExecutionIds(ISet<string> executionIds);

        /// <summary>
        /// All variable values that are defined in the execution scope, without taking outer scopes into account. If you have many task local variables and you only need a few, consider using
        /// <seealso cref="#getVariablesLocal(String, Collection)"/> for better performance.
        /// </summary>
        /// <param name="executionId">
        ///          id of execution, cannot be null. </param>
        /// <returns> the variables or an empty map if no such variables are found. </returns>
        /// <exception cref="ActivitiObjectNotFoundException">
        ///           when no execution is found for the given executionId. </exception>
        IDictionary<string, object> getVariablesLocal(string executionId);

        /// <summary>
        /// All variable values that are defined in the execution scope, without taking outer scopes into account. If you have many task local variables and you only need a few, consider using
        /// <seealso cref="#getVariableInstancesLocal(String, Collection)"/> for better performance.
        /// </summary>
        /// <param name="executionId">
        ///          id of execution, cannot be null. </param>
        /// <returns> the variables or an empty map if no such variables are found. </returns>
        /// <exception cref="ActivitiObjectNotFoundException">
        ///           when no execution is found for the given executionId. </exception>
        IDictionary<string, IVariableInstance> getVariableInstancesLocal(string executionId);

        /// <summary>
        /// The variable values for all given variableNames, takes all variables into account which are visible from the given execution scope (including parent scopes).
        /// </summary>
        /// <param name="executionId">
        ///          id of execution, cannot be null. </param>
        /// <param name="variableNames">
        ///          the collection of variable names that should be retrieved. </param>
        /// <returns> the variables or an empty map if no such variables are found. </returns>
        /// <exception cref="ActivitiObjectNotFoundException">
        ///           when no execution is found for the given executionId. </exception>
        IDictionary<string, object> getVariables(string executionId, ICollection<string> variableNames);

        /// <summary>
        /// The variable values for all given variableNames, takes all variables into account which are visible from the given execution scope (including parent scopes).
        /// </summary>
        /// <param name="executionId">
        ///          id of execution, cannot be null. </param>
        /// <param name="variableNames">
        ///          the collection of variable names that should be retrieved. </param>
        /// <returns> the variables or an empty map if no such variables are found. </returns>
        /// <exception cref="ActivitiObjectNotFoundException">
        ///           when no execution is found for the given executionId. </exception>
        IDictionary<string, IVariableInstance> getVariableInstances(string executionId, ICollection<string> variableNames);

        /// <summary>
        /// The variable values for the given variableNames only taking the given execution scope into account, not looking in outer scopes.
        /// </summary>
        /// <param name="executionId">
        ///          id of execution, cannot be null. </param>
        /// <param name="variableNames">
        ///          the collection of variable names that should be retrieved. </param>
        /// <returns> the variables or an empty map if no such variables are found. </returns>
        /// <exception cref="ActivitiObjectNotFoundException">
        ///           when no execution is found for the given executionId. </exception>
        IDictionary<string, object> getVariablesLocal(string executionId, ICollection<string> variableNames);

        /// <summary>
        /// The variable values for the given variableNames only taking the given execution scope into account, not looking in outer scopes.
        /// </summary>
        /// <param name="executionId">
        ///          id of execution, cannot be null. </param>
        /// <param name="variableNames">
        ///          the collection of variable names that should be retrieved. </param>
        /// <returns> the variables or an empty map if no such variables are found. </returns>
        /// <exception cref="ActivitiObjectNotFoundException">
        ///           when no execution is found for the given executionId. </exception>
        IDictionary<string, IVariableInstance> getVariableInstancesLocal(string executionId, ICollection<string> variableNames);

        /// <summary>
        /// The variable value. Searching for the variable is done in all scopes that are visible to the given execution (including parent scopes). Returns null when no variable value is found with the given
        /// name or when the value is set to null.
        /// </summary>
        /// <param name="executionId">
        ///          id of execution, cannot be null. </param>
        /// <param name="variableName">
        ///          name of variable, cannot be null. </param>
        /// <returns> the variable value or null if the variable is undefined or the value of the variable is null. </returns>
        /// <exception cref="ActivitiObjectNotFoundException">
        ///           when no execution is found for the given executionId. </exception>
        object getVariable(string executionId, string variableName);

        /// <summary>
        /// The variable. Searching for the variable is done in all scopes that are visible to the given execution (including parent scopes). Returns null when no variable value is found with the given
        /// name or when the value is set to null.
        /// </summary>
        /// <param name="executionId">
        ///          id of execution, cannot be null. </param>
        /// <param name="variableName">
        ///          name of variable, cannot be null. </param>
        /// <returns> the variable or null if the variable is undefined. </returns>
        /// <exception cref="ActivitiObjectNotFoundException">
        ///           when no execution is found for the given executionId. </exception>
        IVariableInstance getVariableInstance(string executionId, string variableName);

        /// <summary>
        /// The variable value. Searching for the variable is done in all scopes that are visible to the given execution (including parent scopes). Returns null when no variable value is found with the given
        /// name or when the value is set to null. Throws ClassCastException when cannot cast variable to given class
        /// </summary>
        /// <param name="executionId">
        ///          id of execution, cannot be null. </param>
        /// <param name="variableName">
        ///          name of variable, cannot be null. </param>
        /// <param name="variableClass">
        ///          name of variable, cannot be null. </param>
        /// <returns> the variable value or null if the variable is undefined or the value of the variable is null. </returns>
        /// <exception cref="ActivitiObjectNotFoundException">
        ///           when no execution is found for the given executionId. </exception>
        T getVariable<T>(string executionId, string variableName);

        /// <summary>
        /// Check whether or not this execution has variable set with the given name, Searching for the variable is done in all scopes that are visible to the given execution (including parent scopes).
        /// </summary>
        bool hasVariable(string executionId, string variableName);

        /// <summary>
        /// The variable value for an execution. Returns the value when the variable is set for the execution (and not searching parent scopes). Returns null when no variable value is found with the given
        /// name or when the value is set to null.
        /// </summary>
        object getVariableLocal(string executionId, string variableName);

        /// <summary>
        /// The variable for an execution. Returns the variable when it is set for the execution (and not searching parent scopes). Returns null when no variable is found with the given
        /// name or when the value is set to null.
        /// </summary>
        /// <param name="executionId">
        ///          id of execution, cannot be null. </param>
        /// <param name="variableName">
        ///          name of variable, cannot be null. </param>
        /// <returns> the variable or null if the variable is undefined. </returns>
        /// <exception cref="ActivitiObjectNotFoundException">
        ///           when no execution is found for the given executionId. </exception>
        IVariableInstance getVariableInstanceLocal(string executionId, string variableName);

        /// <summary>
        /// The variable value for an execution. Returns the value casted to given class when the variable is set for the execution (and not searching parent scopes). Returns null when no variable value is
        /// found with the given name or when the value is set to null.
        /// </summary>
        T getVariableLocal<T>(string executionId, string variableName);

        /// <summary>
        /// Check whether or not this execution has a local variable set with the given name.
        /// </summary>
        bool hasVariableLocal(string executionId, string variableName);

        /// <summary>
        /// Update or create a variable for an execution.
        /// 
        /// <para>
        /// The variable is set according to the algorithm as documented for <seealso cref="IVariableScope#setVariable(String, Object)"/>.
        /// 
        /// </para>
        /// </summary>
        /// <seealso cref= IVariableScope#setVariable(String, Object) <seealso cref="IVariableScope#setVariable(String, Object)"/>
        /// </seealso>
        /// <param name="executionId">
        ///          id of execution to set variable in, cannot be null. </param>
        /// <param name="variableName">
        ///          name of variable to set, cannot be null. </param>
        /// <param name="value">
        ///          value to set. When null is passed, the variable is not removed, only it's value will be set to null. </param>
        /// <exception cref="ActivitiObjectNotFoundException">
        ///           when no execution is found for the given executionId. </exception>
        void setVariable(string executionId, string variableName, object value);

        /// <summary>
        /// Update or create a variable for an execution (not considering parent scopes). If the variable is not already existing, it will be created in the given execution.
        /// </summary>
        /// <param name="executionId">
        ///          id of execution to set variable in, cannot be null. </param>
        /// <param name="variableName">
        ///          name of variable to set, cannot be null. </param>
        /// <param name="value">
        ///          value to set. When null is passed, the variable is not removed, only it's value will be set to null. </param>
        /// <exception cref="ActivitiObjectNotFoundException">
        ///           when no execution is found for the given executionId. </exception>
        void setVariableLocal(string executionId, string variableName, object value);

        /// <summary>
        /// Update or create given variables for an execution (including parent scopes).
        /// <para>
        /// Variables are set according to the algorithm as documented for <seealso cref="IVariableScope#setVariables(Map)"/>, applied separately to each variable.
        /// 
        /// </para>
        /// </summary>
        /// <seealso cref= IVariableScope#setVariables(Map) <seealso cref="IVariableScope#setVariables(Map)"/>
        /// </seealso>
        /// <param name="executionId">
        ///          id of the execution, cannot be null. </param>
        /// <param name="variables">
        ///          map containing name (key) and value of variables, can be null. </param>
        /// <exception cref="ActivitiObjectNotFoundException">
        ///           when no execution is found for the given executionId. </exception>
        void setVariables<T1>(string executionId, IDictionary<string, T1> variables);

        /// <summary>
        /// Update or create given variables for an execution (not considering parent scopes). If the variables are not already existing, it will be created in the given execution.
        /// </summary>
        /// <param name="executionId">
        ///          id of the execution, cannot be null. </param>
        /// <param name="variables">
        ///          map containing name (key) and value of variables, can be null. </param>
        /// <exception cref="ActivitiObjectNotFoundException">
        ///           when no execution is found for the given executionId. </exception>
        void setVariablesLocal<T1>(string executionId, IDictionary<string, T1> variables);

        /// <summary>
        /// Removes a variable for an execution.
        /// </summary>
        /// <param name="executionId">
        ///          id of execution to remove variable in. </param>
        /// <param name="variableName">
        ///          name of variable to remove. </param>
        void removeVariable(string executionId, string variableName);

        /// <summary>
        /// Removes a variable for an execution (not considering parent scopes).
        /// </summary>
        /// <param name="executionId">
        ///          id of execution to remove variable in. </param>
        /// <param name="variableName">
        ///          name of variable to remove. </param>
        void removeVariableLocal(string executionId, string variableName);

        /// <summary>
        /// Removes variables for an execution.
        /// </summary>
        /// <param name="executionId">
        ///          id of execution to remove variable in. </param>
        /// <param name="variableNames">
        ///          collection containing name of variables to remove. </param>
        void removeVariables(string executionId, ICollection<string> variableNames);

        /// <summary>
        /// Remove variables for an execution (not considering parent scopes).
        /// </summary>
        /// <param name="executionId">
        ///          id of execution to remove variable in. </param>
        /// <param name="variableNames">
        ///          collection containing name of variables to remove. </param>
        void removeVariablesLocal(string executionId, ICollection<string> variableNames);

        /// <summary>
        /// All DataObjects visible from the given execution scope (including parent scopes).
        /// </summary>
        /// <param name="executionId">
        ///          id of execution, cannot be null. </param>
        /// <returns> the DataObjects or an empty map if no such variables are found. </returns>
        /// <exception cref="ActivitiObjectNotFoundException">
        ///           when no execution is found for the given executionId. </exception>
        IDictionary<string, IDataObject> getDataObjects(string executionId);

        /// <summary>
        /// All DataObjects visible from the given execution scope (including parent scopes).
        /// </summary>
        /// <param name="executionId">
        ///          id of execution, cannot be null. </param>
        /// <param name="locale">
        ///          locale the IDataObject name and description should be returned in (if available). </param>
        /// <param name="withLocalizationFallback">
        ///          When true localization will fallback to more general locales if the specified locale is not found. </param>
        /// <returns> the DataObjects or an empty map if no DataObjects are found. </returns>
        /// <exception cref="ActivitiObjectNotFoundException">
        ///           when no execution is found for the given executionId. </exception>
        IDictionary<string, IDataObject> getDataObjects(string executionId, string locale, bool withLocalizationFallback);


        /// <summary>
        /// All IDataObject values that are defined in the execution scope, without taking outer scopes into account. If you have many local DataObjects and you only need a few, consider using
        /// <seealso cref="#getDataObjectsLocal(String, Collection)"/> for better performance.
        /// </summary>
        /// <param name="executionId">
        ///          id of execution, cannot be null. </param>
        /// <returns> the DataObjects or an empty map if no DataObjects are found. </returns>
        /// <exception cref="ActivitiObjectNotFoundException">
        ///           when no execution is found for the given executionId. </exception>
        IDictionary<string, IDataObject> getDataObjectsLocal(string executionId);

        /// <summary>
        /// All IDataObject values that are defined in the execution scope, without taking outer scopes into account. If you have many local DataObjects and you only need a few, consider using
        /// <seealso cref="#getDataObjectsLocal(String, Collection)"/> for better performance.
        /// </summary>
        /// <param name="executionId">
        ///          id of execution, cannot be null. </param>
        /// <param name="locale">
        ///          locale the IDataObject name and description should be returned in (if available). </param>
        /// <param name="withLocalizationFallback">
        ///          When true localization will fallback to more general locales if the specified locale is not found. </param>
        /// <returns> the DataObjects or an empty map if no such variables are found. </returns>
        /// <exception cref="ActivitiObjectNotFoundException">
        ///           when no execution is found for the given executionId. </exception>
        IDictionary<string, IDataObject> getDataObjectsLocal(string executionId, string locale, bool withLocalizationFallback);

        /// <summary>
        /// The DataObjects for all given dataObjectNames, takes all dataObjects into account which are visible from the given execution scope (including parent scopes).
        /// </summary>
        /// <param name="executionId">
        ///          id of execution, cannot be null. </param>
        /// <param name="dataObjectNames">
        ///          the collection of IDataObject names that should be retrieved. </param>
        /// <returns> the IDataObject or an empty map if no DataObjects are found. </returns>
        /// <exception cref="ActivitiObjectNotFoundException">
        ///           when no execution is found for the given executionId. </exception>
        IDictionary<string, IDataObject> getDataObjects(string executionId, ICollection<string> dataObjectNames);

        /// <summary>
        /// The DataObjects for all given dataObjectNames, takes all dataObjects into account which are visible from the given execution scope (including parent scopes).
        /// </summary>
        /// <param name="executionId">
        ///          id of execution, cannot be null. </param>
        /// <param name="dataObjectNames">
        ///          the collection of IDataObject names that should be retrieved. </param>
        /// <param name="locale">
        ///          locale the IDataObject name and description should be returned in (if available). </param>
        /// <param name="withLocalizationFallback">
        ///          When true localization will fallback to more general locales if the specified locale is not found. </param>
        /// <returns> the DataObjects or an empty map if no such dataObjects are found. </returns>
        /// <exception cref="ActivitiObjectNotFoundException">
        ///           when no execution is found for the given executionId. </exception>
        IDictionary<string, IDataObject> getDataObjects(string executionId, ICollection<string> dataObjectNames, string locale, bool withLocalizationFallback);

        /// <summary>
        /// The DataObjects for the given dataObjectNames only taking the given execution scope into account, not looking in outer scopes.
        /// </summary>
        /// <param name="executionId">
        ///          id of execution, cannot be null. </param>
        /// <param name="dataObjectNames">
        ///          the collection of IDataObject names that should be retrieved. </param>
        /// <returns> the DataObjects or an empty map if no DataObjects are found. </returns>
        /// <exception cref="ActivitiObjectNotFoundException">
        ///           when no execution is found for the given executionId. </exception>
        IDictionary<string, IDataObject> getDataObjectsLocal(string executionId, ICollection<string> dataObjects);

        /// <summary>
        /// The DataObjects for the given dataObjectNames only taking the given execution scope into account, not looking in outer scopes.
        /// </summary>
        /// <param name="executionId">
        ///          id of execution, cannot be null. </param>
        /// <param name="dataObjectNames">
        ///          the collection of IDataObject names that should be retrieved. </param>
        /// <param name="locale">
        ///          locale the IDataObject name and description should be returned in (if available). </param>
        /// <param name="withLocalizationFallback">
        ///          When true localization will fallback to more general locales if the specified locale is not found. </param>
        /// <returns> the DataObjects or an empty map if no DataObjects are found. </returns>
        /// <exception cref="ActivitiObjectNotFoundException">
        ///           when no execution is found for the given executionId. </exception>
        IDictionary<string, IDataObject> getDataObjectsLocal(string executionId, ICollection<string> dataObjectNames, string locale, bool withLocalizationFallback);

        /// <summary>
        /// The IDataObject. Searching for the IDataObject is done in all scopes that are visible to the given execution (including parent scopes). Returns null when no IDataObject value is found with the given
        /// name or when the value is set to null.
        /// </summary>
        /// <param name="executionId">
        ///          id of execution, cannot be null. </param>
        /// <param name="dataObjectName">
        ///          name of IDataObject, cannot be null. </param>
        /// <returns> the IDataObject or null if the variable is undefined. </returns>
        /// <exception cref="ActivitiObjectNotFoundException">
        ///           when no execution is found for the given executionId. </exception>
        IDataObject getDataObject(string executionId, string dataObject);

        /// <summary>
        /// The IDataObject. Searching for the IDataObject is done in all scopes that are visible to the given execution (including parent scopes). Returns null when no IDataObject value is found with the given
        /// name or when the value is set to null.
        /// </summary>
        /// <param name="executionId">
        ///          id of execution, cannot be null. </param>
        /// <param name="dataObjectName">
        ///          name of IDataObject, cannot be null. </param>
        /// <param name="locale">
        ///          locale the IDataObject name and description should be returned in (if available). </param>
        /// <param name="withLocalizationFallback">
        ///          When true localization will fallback to more general locales including the default locale of the JVM if the specified locale is not found. </param>
        /// <returns> the IDataObject or null if the IDataObject is undefined. </returns>
        /// <exception cref="ActivitiObjectNotFoundException">
        ///           when no execution is found for the given executionId. </exception>
        IDataObject getDataObject(string executionId, string dataObjectName, string locale, bool withLocalizationFallback);

        /// <summary>
        /// The IDataObject for an execution. Returns the IDataObject when it is set for the execution (and not searching parent scopes). Returns null when no IDataObject is found with the given
        /// name.
        /// </summary>
        /// <param name="executionId">
        ///          id of execution, cannot be null. </param>
        /// <param name="dataObjectName">
        ///          name of IDataObject, cannot be null. </param>
        /// <returns> the IDataObject or null if the IDataObject is undefined. </returns>
        /// <exception cref="ActivitiObjectNotFoundException">
        ///           when no execution is found for the given executionId. </exception>
        IDataObject getDataObjectLocal(string executionId, string dataObjectName);

        /// <summary>
        /// The IDataObject for an execution. Returns the IDataObject when it is set for the execution (and not searching parent scopes). Returns null when no IDataObject is found with the given
        /// name.
        /// </summary>
        /// <param name="executionId">
        ///          id of execution, cannot be null. </param>
        /// <param name="dataObjectName">
        ///          name of IDataObject, cannot be null. </param>
        /// <param name="locale">
        ///          locale the IDataObject name and description should be returned in (if available). </param>
        /// <param name="withLocalizationFallback">
        ///          When true localization will fallback to more general locales if the specified locale is not found. </param>
        /// <returns> the IDataObject or null if the IDataObject is undefined. </returns>
        /// <exception cref="ActivitiObjectNotFoundException">
        ///           when no execution is found for the given executionId. </exception>
        IDataObject getDataObjectLocal(string executionId, string dataObjectName, string locale, bool withLocalizationFallback);


        // Queries ////////////////////////////////////////////////////////

        /// <summary>
        /// Creates a new <seealso cref="IExecutionQuery"/> instance, that can be used to query the executions and process instances.
        /// </summary>
        IExecutionQuery createExecutionQuery();

        /// <summary>
        /// creates a new <seealso cref="INativeExecutionQuery"/> to query <seealso cref="IExecution"/>s by SQL directly
        /// </summary>
        INativeExecutionQuery createNativeExecutionQuery();

        /// <summary>
        /// Creates a new <seealso cref="IProcessInstanceQuery"/> instance, that can be used to query process instances.
        /// </summary>
        IProcessInstanceQuery createProcessInstanceQuery();

        /// <summary>
        /// creates a new <seealso cref="INativeProcessInstanceQuery"/> to query <seealso cref="IProcessInstance"/>s by SQL directly
        /// </summary>
        INativeProcessInstanceQuery createNativeProcessInstanceQuery();

        // Process instance state //////////////////////////////////////////

        /// <summary>
        /// Suspends the process instance with the given id.
        /// 
        /// If a process instance is in state suspended, activiti will not execute jobs (timers, messages) associated with this instance.
        /// 
        /// If you have a process instance hierarchy, suspending one process instance form the hierarchy will not suspend other process instances form that hierarchy.
        /// </summary>
        /// <exception cref="ActivitiObjectNotFoundException">
        ///           if no such processInstance can be found. </exception>
        /// <exception cref="ActivitiException">
        ///           the process instance is already in state suspended. </exception>
        void suspendProcessInstanceById(string processInstanceId);

        /// <summary>
        /// Activates the process instance with the given id.
        /// 
        /// If you have a process instance hierarchy, suspending one process instance form the hierarchy will not suspend other process instances form that hierarchy.
        /// </summary>
        /// <exception cref="ActivitiObjectNotFoundException">
        ///           if no such processInstance can be found. </exception>
        /// <exception cref="ActivitiException">
        ///           if the process instance is already in state active. </exception>
        void activateProcessInstanceById(string processInstanceId);

        // Events
        // ////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Notifies the process engine that a signal event of name 'signalName' has been received. This method delivers the signal to all executions waiting on the signal.
        /// <p/>
        /// 
        /// <strong>NOTE:</strong> The waiting executions are notified synchronously.
        /// </summary>
        /// <param name="signalName">
        ///          the name of the signal event </param>
        void signalEventReceived(string signalName);

        /// <summary>
        /// Similar to <seealso cref="#signalEventReceived(String)"/>, but within the context of one tenant.
        /// </summary>
        void signalEventReceivedWithTenantId(string signalName, string tenantId);

        /// <summary>
        /// Notifies the process engine that a signal event of name 'signalName' has been received. This method delivers the signal to all executions waiting on the signal.
        /// <p/>
        /// </summary>
        /// <param name="signalName">
        ///          the name of the signal event </param>
        void signalEventReceivedAsync(string signalName);

        /// <summary>
        /// Similar to <seealso cref="#signalEventReceivedAsync(String)"/>, but within the context of one tenant.
        /// </summary>
        void signalEventReceivedAsyncWithTenantId(string signalName, string tenantId);

        /// <summary>
        /// Notifies the process engine that a signal event of name 'signalName' has been received. This method delivers the signal to all executions waiting on the signal.
        /// <p/>
        /// 
        /// <strong>NOTE:</strong> The waiting executions are notified synchronously.
        /// </summary>
        /// <param name="signalName">
        ///          the name of the signal event </param>
        /// <param name="processVariables">
        ///          a map of variables added to the execution(s) </param>
        void signalEventReceived(string signalName, IDictionary<string, object> processVariables);

        /// <summary>
        /// Similar to <seealso cref="#signalEventReceived(String, Map<String, Object>)"/>, but within the context of one tenant.
        /// </summary>
        void signalEventReceivedWithTenantId(string signalName, IDictionary<string, object> processVariables, string tenantId);

        /// <summary>
        /// Notifies the process engine that a signal event of name 'signalName' has been received. This method delivers the signal to a single execution, being the execution referenced by 'executionId'. The
        /// waiting execution is notified synchronously.
        /// </summary>
        /// <param name="signalName">
        ///          the name of the signal event </param>
        /// <param name="executionId">
        ///          the id of the execution to deliver the signal to </param>
        /// <exception cref="ActivitiObjectNotFoundException">
        ///           if no such execution exists. </exception>
        /// <exception cref="ActivitiException">
        ///           if the execution has not subscribed to the signal. </exception>
        void signalEventReceived(string signalName, string executionId);

        /// <summary>
        /// Notifies the process engine that a signal event of name 'signalName' has been received. This method delivers the signal to a single execution, being the execution referenced by 'executionId'. The
        /// waiting execution is notified synchronously.
        /// </summary>
        /// <param name="signalName">
        ///          the name of the signal event </param>
        /// <param name="executionId">
        ///          the id of the execution to deliver the signal to </param>
        /// <param name="processVariables">
        ///          a map of variables added to the execution(s) </param>
        /// <exception cref="ActivitiObjectNotFoundException">
        ///           if no such execution exists. </exception>
        /// <exception cref="ActivitiException">
        ///           if the execution has not subscribed to the signal </exception>
        void signalEventReceived(string signalName, string executionId, IDictionary<string, object> processVariables);

        /// <summary>
        /// Notifies the process engine that a signal event of name 'signalName' has been received. This method delivers the signal to a single execution, being the execution referenced by 'executionId'. The
        /// waiting execution is notified <strong>asynchronously</strong>.
        /// </summary>
        /// <param name="signalName">
        ///          the name of the signal event </param>
        /// <param name="executionId">
        ///          the id of the execution to deliver the signal to </param>
        /// <exception cref="ActivitiObjectNotFoundException">
        ///           if no such execution exists. </exception>
        /// <exception cref="ActivitiException">
        ///           if the execution has not subscribed to the signal. </exception>
        void signalEventReceivedAsync(string signalName, string executionId);

        /// <summary>
        /// Notifies the process engine that a message event with name 'messageName' has been received and has been correlated to an execution with id 'executionId'.
        /// 
        /// The waiting execution is notified synchronously.
        /// </summary>
        /// <param name="messageName">
        ///          the name of the message event </param>
        /// <param name="executionId">
        ///          the id of the execution to deliver the message to </param>
        /// <exception cref="ActivitiObjectNotFoundException">
        ///           if no such execution exists. </exception>
        /// <exception cref="ActivitiException">
        ///           if the execution has not subscribed to the signal </exception>
        void messageEventReceived(string messageName, string executionId);

        /// <summary>
        /// Notifies the process engine that a message event with the name 'messageName' has been received and has been correlated to an execution with id 'executionId'.
        /// 
        /// The waiting execution is notified synchronously.
        /// 
        /// <para>
        /// Variables are set for the scope of the execution of the message event subscribed to the message name. For example:
        /// </para>
        /// <para>
        /// <li>The scope for an intermediate message event in the main process is that of the process instance</li>
        /// <li>The scope for an intermediate message event in a subprocess is that of the subprocess</li>
        /// <li>The scope for a boundary message event is that of the execution for the Activity the event is attached to</li>
        /// </para>
        /// <para>
        /// Variables are set according to the algorithm as documented for <seealso cref="IVariableScope#setVariables(Map)"/>, applied separately to each variable.
        /// 
        /// </para>
        /// </summary>
        /// <seealso cref= IVariableScope#setVariables(Map) <seealso cref="IVariableScope#setVariables(Map)"/>
        /// </seealso>
        /// <param name="messageName">
        ///          the name of the message event </param>
        /// <param name="executionId">
        ///          the id of the execution to deliver the message to </param>
        /// <param name="processVariables">
        ///          a map of variables added to the execution </param>
        /// <exception cref="ActivitiObjectNotFoundException">
        ///           if no such execution exists. </exception>
        /// <exception cref="ActivitiException">
        ///           if the execution has not subscribed to the signal </exception>
        void messageEventReceived(string messageName, string executionId, IDictionary<string, object> processVariables);

        /// <summary>
        /// Notifies the process engine that a message event with the name 'messageName' has been received and has been correlated to an execution with id 'executionId'.
        /// 
        /// The waiting execution is notified <strong>asynchronously</strong>.
        /// </summary>
        /// <param name="messageName">
        ///          the name of the message event </param>
        /// <param name="executionId">
        ///          the id of the execution to deliver the message to </param>
        /// <exception cref="ActivitiObjectNotFoundException">
        ///           if no such execution exists. </exception>
        /// <exception cref="ActivitiException">
        ///           if the execution has not subscribed to the signal </exception>
        void messageEventReceivedAsync(string messageName, string executionId);

        /// <summary>
        /// Adds an event-listener which will be notified of ALL events by the dispatcher.
        /// </summary>
        /// <param name="listenerToAdd">
        ///          the listener to add </param>
        void addEventListener(IActivitiEventListener listenerToAdd);

        /// <summary>
        /// Adds an event-listener which will only be notified when an event occurs, which type is in the given types.
        /// </summary>
        /// <param name="listenerToAdd">
        ///          the listener to add </param>
        /// <param name="types">
        ///          types of events the listener should be notified for </param>
        void addEventListener(IActivitiEventListener listenerToAdd, params ActivitiEventType[] types);

        /// <summary>
        /// Removes the given listener from this dispatcher. The listener will no longer be notified, regardless of the type(s) it was registered for in the first place.
        /// </summary>
        /// <param name="listenerToRemove">
        ///          listener to remove </param>
        void removeEventListener(IActivitiEventListener listenerToRemove);

        /// <summary>
        /// Dispatches the given event to any listeners that are registered.
        /// </summary>
        /// <param name="event">
        ///          event to dispatch.
        /// </param>
        /// <exception cref="ActivitiException">
        ///           if an exception occurs when dispatching the event or when the <seealso cref="IActivitiEventDispatcher"/> is disabled. </exception>
        /// <exception cref="ActivitiIllegalArgumentException">
        ///           when the given event is not suitable for dispatching. </exception>
        void dispatchEvent(IActivitiEvent @event);

        /// <summary>
        /// Sets the name for the process instance with the given id.
        /// </summary>
        /// <param name="processInstanceId">
        ///          id of the process instance to update </param>
        /// <param name="name">
        ///          new name for the process instance </param>
        /// <exception cref="ActivitiObjectNotFoundException">
        ///           when the given process instance does not exist. </exception>
        void setProcessInstanceName(string processInstanceId, string name);

        /// <summary>
        /// Gets enabled activities from ad-hoc sub process
        /// </summary>
        /// <param name="executionId">
        ///          id of the execution that has an ad-hoc sub process as current flow element </param>
        /// <returns> a list of enabled activities </returns>
        IList<FlowNode> getEnabledActivitiesFromAdhocSubProcess(string executionId);

        /// <summary>
        /// Executes an activity in a ad-hoc sub process
        /// </summary>
        /// <param name="executionId">
        ///          id of the execution that has an ad-hoc sub process as current flow element </param>
        /// <param name="activityId">
        ///          id of the activity id to enable </param>
        /// <returns> the newly created execution of the enabled activity </returns>
        IExecution executeActivityInAdhocSubProcess(string executionId, string activityId);

        /// <summary>
        /// Completes the ad-hoc sub process
        /// </summary>
        /// <param name="executionId">
        ///          id of the execution that has an ad-hoc sub process as current flow element </param>
        void completeAdhocSubProcess(string executionId);

        /// <summary>
        /// The all events related to the given Process Instance. </summary>
        IList<Event> getProcessInstanceEvents(string processInstanceId);

    }
}