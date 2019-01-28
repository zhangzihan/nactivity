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
namespace org.activiti.engine.impl
{

    using org.activiti.bpmn.model;
    using org.activiti.engine.@delegate.@event;
    using org.activiti.engine.impl.cfg;
    using org.activiti.engine.impl.cmd;
    using org.activiti.engine.impl.persistence.entity;
    using org.activiti.engine.impl.runtime;
    using org.activiti.engine.runtime;
    using org.activiti.engine.task;


    public class RuntimeServiceImpl : ServiceImpl, IRuntimeService
    {
        public virtual IProcessInstance startProcessInstanceByKey(string processDefinitionKey)
        {
            return commandExecutor.execute(new StartProcessInstanceCmd<IProcessInstance>(processDefinitionKey, null, null, null));
        }

        public virtual IProcessInstance startProcessInstanceByKey(string processDefinitionKey, string businessKey)
        {
            return commandExecutor.execute(new StartProcessInstanceCmd<IProcessInstance>(processDefinitionKey, null, businessKey, null));
        }

        public virtual IProcessInstance startProcessInstanceByKey(string processDefinitionKey, IDictionary<string, object> variables)
        {
            return commandExecutor.execute(new StartProcessInstanceCmd<IProcessInstance>(processDefinitionKey, null, null, variables));
        }

        public virtual IProcessInstance startProcessInstanceByKey(string processDefinitionKey, string businessKey, IDictionary<string, object> variables)
        {
            return commandExecutor.execute(new StartProcessInstanceCmd<IProcessInstance>(processDefinitionKey, null, businessKey, variables));
        }

        public virtual IProcessInstance startProcessInstanceByKeyAndTenantId(string processDefinitionKey, string tenantId)
        {
            return commandExecutor.execute(new StartProcessInstanceCmd<IProcessInstance>(processDefinitionKey, null, null, null, tenantId));
        }

        public virtual IProcessInstance startProcessInstanceByKeyAndTenantId(string processDefinitionKey, string businessKey, string tenantId)
        {
            return commandExecutor.execute(new StartProcessInstanceCmd<IProcessInstance>(processDefinitionKey, null, businessKey, null, tenantId));
        }

        public virtual IProcessInstance startProcessInstanceByKeyAndTenantId(string processDefinitionKey, IDictionary<string, object> variables, string tenantId)
        {
            return commandExecutor.execute(new StartProcessInstanceCmd<IProcessInstance>(processDefinitionKey, null, null, variables, tenantId));
        }

        public virtual IProcessInstance startProcessInstanceByKeyAndTenantId(string processDefinitionKey, string businessKey, IDictionary<string, object> variables, string tenantId)
        {
            return commandExecutor.execute(new StartProcessInstanceCmd<IProcessInstance>(processDefinitionKey, null, businessKey, variables, tenantId));
        }

        public virtual IProcessInstance startProcessInstanceById(string processDefinitionId)
        {
            return commandExecutor.execute(new StartProcessInstanceCmd<IProcessInstance>(null, processDefinitionId, null, null));
        }

        public virtual IProcessInstance startProcessInstanceById(string processDefinitionId, string businessKey)
        {
            return commandExecutor.execute(new StartProcessInstanceCmd<IProcessInstance>(null, processDefinitionId, businessKey, null));
        }

        public virtual IProcessInstance startProcessInstanceById(string processDefinitionId, IDictionary<string, object> variables)
        {
            return commandExecutor.execute(new StartProcessInstanceCmd<IProcessInstance>(null, processDefinitionId, null, variables));
        }

        public virtual IProcessInstance startProcessInstanceById(string processDefinitionId, string businessKey, IDictionary<string, object> variables)
        {
            return commandExecutor.execute(new StartProcessInstanceCmd<IProcessInstance>(null, processDefinitionId, businessKey, variables));
        }

        public virtual void deleteProcessInstance(string processInstanceId, string deleteReason)
        {
            commandExecutor.execute(new DeleteProcessInstanceCmd(processInstanceId, deleteReason));
        }

        public virtual IExecutionQuery createExecutionQuery()
        {
            return new ExecutionQueryImpl(commandExecutor);
        }

        public virtual INativeExecutionQuery createNativeExecutionQuery()
        {
            return new NativeExecutionQueryImpl(commandExecutor);
        }

        public virtual INativeProcessInstanceQuery createNativeProcessInstanceQuery()
        {
            return new NativeProcessInstanceQueryImpl(commandExecutor);
        }

        public virtual void updateBusinessKey(string processInstanceId, string businessKey)
        {
            commandExecutor.execute(new SetProcessInstanceBusinessKeyCmd(processInstanceId, businessKey));
        }

        public virtual IDictionary<string, object> getVariables(string executionId)
        {
            return commandExecutor.execute(new GetExecutionVariablesCmd(executionId, null, false));
        }

        public virtual IDictionary<string, IVariableInstance> getVariableInstances(string executionId)
        {
            return commandExecutor.execute(new GetExecutionVariableInstancesCmd(executionId, null, false));
        }

        public virtual IList<IVariableInstance> getVariableInstancesByExecutionIds(ISet<string> executionIds)
        {
            return commandExecutor.execute(new GetExecutionsVariablesCmd(executionIds));
        }

        public virtual IDictionary<string, object> getVariablesLocal(string executionId)
        {
            return commandExecutor.execute(new GetExecutionVariablesCmd(executionId, null, true));
        }

        public virtual IDictionary<string, IVariableInstance> getVariableInstancesLocal(string executionId)
        {
            return commandExecutor.execute(new GetExecutionVariableInstancesCmd(executionId, null, true));
        }

        public virtual IDictionary<string, object> getVariables(string executionId, ICollection<string> variableNames)
        {
            return commandExecutor.execute(new GetExecutionVariablesCmd(executionId, variableNames, false));
        }

        public virtual IDictionary<string, IVariableInstance> getVariableInstances(string executionId, ICollection<string> variableNames)
        {
            return commandExecutor.execute(new GetExecutionVariableInstancesCmd(executionId, variableNames, false));
        }

        public virtual IDictionary<string, object> getVariablesLocal(string executionId, ICollection<string> variableNames)
        {
            return commandExecutor.execute(new GetExecutionVariablesCmd(executionId, variableNames, true));
        }

        public virtual IDictionary<string, IVariableInstance> getVariableInstancesLocal(string executionId, ICollection<string> variableNames)
        {
            return commandExecutor.execute(new GetExecutionVariableInstancesCmd(executionId, variableNames, true));
        }

        public virtual object getVariable(string executionId, string variableName)
        {
            return commandExecutor.execute(new GetExecutionVariableCmd(executionId, variableName, false));
        }

        public virtual IVariableInstance getVariableInstance(string executionId, string variableName)
        {
            return commandExecutor.execute(new GetExecutionVariableInstanceCmd(executionId, variableName, false));
        }

        public virtual T getVariable<T>(string executionId, string variableName)
        {
            return (T)getVariable(executionId, variableName);
        }

        public virtual bool hasVariable(string executionId, string variableName)
        {
            return commandExecutor.execute(new HasExecutionVariableCmd(executionId, variableName, false));
        }

        public virtual object getVariableLocal(string executionId, string variableName)
        {
            return commandExecutor.execute(new GetExecutionVariableCmd(executionId, variableName, true));
        }

        public virtual IVariableInstance getVariableInstanceLocal(string executionId, string variableName)
        {
            return commandExecutor.execute(new GetExecutionVariableInstanceCmd(executionId, variableName, true));
        }

        public virtual T getVariableLocal<T>(string executionId, string variableName)
        {
            return (T)getVariableLocal(executionId, variableName);
        }

        public virtual bool hasVariableLocal(string executionId, string variableName)
        {
            return commandExecutor.execute(new HasExecutionVariableCmd(executionId, variableName, true));
        }

        public virtual void setVariable(string executionId, string variableName, object value)
        {
            if (ReferenceEquals(variableName, null))
            {
                throw new ActivitiIllegalArgumentException("variableName is null");
            }
            IDictionary<string, object> variables = new Dictionary<string, object>();
            variables[variableName] = value;
            commandExecutor.execute(new SetExecutionVariablesCmd(executionId, variables, false));
        }

        public virtual void setVariableLocal(string executionId, string variableName, object value)
        {
            if (ReferenceEquals(variableName, null))
            {
                throw new ActivitiIllegalArgumentException("variableName is null");
            }
            IDictionary<string, object> variables = new Dictionary<string, object>();
            variables[variableName] = value;
            commandExecutor.execute(new SetExecutionVariablesCmd(executionId, variables, true));
        }

        public virtual void setVariables<T1>(string executionId, IDictionary<string, T1> variables)
        {
            commandExecutor.execute(new SetExecutionVariablesCmd(executionId, variables as IDictionary<string, object>, false));
        }

        public virtual void setVariablesLocal<T1>(string executionId, IDictionary<string, T1> variables)
        {
            commandExecutor.execute(new SetExecutionVariablesCmd(executionId, variables as IDictionary<string, object>, true));
        }

        public virtual void removeVariable(string executionId, string variableName)
        {
            ICollection<string> variableNames = new List<string>(1);
            variableNames.Add(variableName);
            commandExecutor.execute(new RemoveExecutionVariablesCmd(executionId, variableNames, false));
        }

        public virtual void removeVariableLocal(string executionId, string variableName)
        {
            ICollection<string> variableNames = new List<string>();
            variableNames.Add(variableName);
            commandExecutor.execute(new RemoveExecutionVariablesCmd(executionId, variableNames, true));
        }

        public virtual void removeVariables(string executionId, ICollection<string> variableNames)
        {
            commandExecutor.execute(new RemoveExecutionVariablesCmd(executionId, variableNames, false));
        }

        public virtual void removeVariablesLocal(string executionId, ICollection<string> variableNames)
        {
            commandExecutor.execute(new RemoveExecutionVariablesCmd(executionId, variableNames, true));
        }

        public virtual IDictionary<string, IDataObject> getDataObjects(string executionId)
        {
            return commandExecutor.execute(new GetDataObjectsCmd(executionId, null, false));
        }

        public virtual IDictionary<string, IDataObject> getDataObjects(string executionId, string locale, bool withLocalizationFallback)
        {
            return commandExecutor.execute(new GetDataObjectsCmd(executionId, null, false, locale, withLocalizationFallback));
        }

        public virtual IDictionary<string, IDataObject> getDataObjectsLocal(string executionId)
        {
            return commandExecutor.execute(new GetDataObjectsCmd(executionId, null, true));
        }

        public virtual IDictionary<string, IDataObject> getDataObjectsLocal(string executionId, string locale, bool withLocalizationFallback)
        {
            return commandExecutor.execute(new GetDataObjectsCmd(executionId, null, true, locale, withLocalizationFallback));
        }

        public virtual IDictionary<string, IDataObject> getDataObjects(string executionId, ICollection<string> dataObjectNames)
        {
            return commandExecutor.execute(new GetDataObjectsCmd(executionId, dataObjectNames, false));
        }

        public virtual IDictionary<string, IDataObject> getDataObjects(string executionId, ICollection<string> dataObjectNames, string locale, bool withLocalizationFallback)
        {
            return commandExecutor.execute(new GetDataObjectsCmd(executionId, dataObjectNames, false, locale, withLocalizationFallback));
        }

        public virtual IDictionary<string, IDataObject> getDataObjectsLocal(string executionId, ICollection<string> dataObjects)
        {
            return commandExecutor.execute(new GetDataObjectsCmd(executionId, dataObjects, true));
        }

        public virtual IDictionary<string, IDataObject> getDataObjectsLocal(string executionId, ICollection<string> dataObjectNames, string locale, bool withLocalizationFallback)
        {
            return commandExecutor.execute(new GetDataObjectsCmd(executionId, dataObjectNames, true, locale, withLocalizationFallback));
        }

        public virtual IDataObject getDataObject(string executionId, string dataObject)
        {
            return commandExecutor.execute(new GetDataObjectCmd(executionId, dataObject, false));
        }

        public virtual IDataObject getDataObject(string executionId, string dataObjectName, string locale, bool withLocalizationFallback)
        {
            return commandExecutor.execute(new GetDataObjectCmd(executionId, dataObjectName, false, locale, withLocalizationFallback));
        }

        public virtual IDataObject getDataObjectLocal(string executionId, string dataObjectName)
        {
            return commandExecutor.execute(new GetDataObjectCmd(executionId, dataObjectName, true));
        }

        public virtual IDataObject getDataObjectLocal(string executionId, string dataObjectName, string locale, bool withLocalizationFallback)
        {
            return commandExecutor.execute(new GetDataObjectCmd(executionId, dataObjectName, true, locale, withLocalizationFallback));
        }

        public virtual void signal(string executionId)
        {
            commandExecutor.execute(new TriggerCmd(executionId, null));
        }

        public virtual void trigger(string executionId)
        {
            commandExecutor.execute(new TriggerCmd(executionId, null));
        }

        public virtual void signal(string executionId, IDictionary<string, object> processVariables)
        {
            commandExecutor.execute(new TriggerCmd(executionId, processVariables));
        }

        public virtual void trigger(string executionId, IDictionary<string, object> processVariables)
        {
            commandExecutor.execute(new TriggerCmd(executionId, processVariables));
        }

        public virtual void trigger(string executionId, IDictionary<string, object> processVariables, IDictionary<string, object> transientVariables)
        {
            commandExecutor.execute(new TriggerCmd(executionId, processVariables, transientVariables));
        }

        public virtual void addUserIdentityLink(string processInstanceId, string userId, string identityLinkType)
        {
            commandExecutor.execute(new AddIdentityLinkForProcessInstanceCmd(processInstanceId, userId, null, identityLinkType));
        }

        public virtual void addGroupIdentityLink(string processInstanceId, string groupId, string identityLinkType)
        {
            commandExecutor.execute(new AddIdentityLinkForProcessInstanceCmd(processInstanceId, null, groupId, identityLinkType));
        }

        public virtual void addParticipantUser(string processInstanceId, string userId)
        {
            commandExecutor.execute(new AddIdentityLinkForProcessInstanceCmd(processInstanceId, userId, null, IdentityLinkType.PARTICIPANT));
        }

        public virtual void addParticipantGroup(string processInstanceId, string groupId)
        {
            commandExecutor.execute(new AddIdentityLinkForProcessInstanceCmd(processInstanceId, null, groupId, IdentityLinkType.PARTICIPANT));
        }

        public virtual void deleteParticipantUser(string processInstanceId, string userId)
        {
            commandExecutor.execute(new DeleteIdentityLinkForProcessInstanceCmd(processInstanceId, userId, null, IdentityLinkType.PARTICIPANT));
        }

        public virtual void deleteParticipantGroup(string processInstanceId, string groupId)
        {
            commandExecutor.execute(new DeleteIdentityLinkForProcessInstanceCmd(processInstanceId, null, groupId, IdentityLinkType.PARTICIPANT));
        }

        public virtual void deleteUserIdentityLink(string processInstanceId, string userId, string identityLinkType)
        {
            commandExecutor.execute(new DeleteIdentityLinkForProcessInstanceCmd(processInstanceId, userId, null, identityLinkType));
        }

        public virtual void deleteGroupIdentityLink(string processInstanceId, string groupId, string identityLinkType)
        {
            commandExecutor.execute(new DeleteIdentityLinkForProcessInstanceCmd(processInstanceId, null, groupId, identityLinkType));
        }

        public virtual IList<IIdentityLink> getIdentityLinksForProcessInstance(string processInstanceId)
        {
            return commandExecutor.execute(new GetIdentityLinksForProcessInstanceCmd(processInstanceId));
        }

        public virtual IProcessInstanceQuery createProcessInstanceQuery()
        {
            return new ProcessInstanceQueryImpl(commandExecutor);
        }

        public virtual IList<string> getActiveActivityIds(string executionId)
        {
            return commandExecutor.execute(new FindActiveActivityIdsCmd(executionId));
        }

        public virtual void suspendProcessInstanceById(string processInstanceId)
        {
            commandExecutor.execute(new SuspendProcessInstanceCmd(processInstanceId));
        }

        public virtual void activateProcessInstanceById(string processInstanceId)
        {
            commandExecutor.execute(new ActivateProcessInstanceCmd(processInstanceId));
        }

        public virtual IProcessInstance startProcessInstanceByMessage(string messageName)
        {
            return commandExecutor.execute(new StartProcessInstanceByMessageCmd(messageName, null, null, null));
        }

        public virtual IProcessInstance startProcessInstanceByMessageAndTenantId(string messageName, string tenantId)
        {
            return commandExecutor.execute(new StartProcessInstanceByMessageCmd(messageName, null, null, tenantId));
        }

        public virtual IProcessInstance startProcessInstanceByMessage(string messageName, string businessKey)
        {
            return commandExecutor.execute(new StartProcessInstanceByMessageCmd(messageName, businessKey, null, null));
        }

        public virtual IProcessInstance startProcessInstanceByMessageAndTenantId(string messageName, string businessKey, string tenantId)
        {
            return commandExecutor.execute(new StartProcessInstanceByMessageCmd(messageName, businessKey, null, tenantId));
        }

        public virtual IProcessInstance startProcessInstanceByMessage(string messageName, IDictionary<string, object> processVariables)
        {
            return commandExecutor.execute(new StartProcessInstanceByMessageCmd(messageName, null, processVariables, null));
        }

        public virtual IProcessInstance startProcessInstanceByMessageAndTenantId(string messageName, IDictionary<string, object> processVariables, string tenantId)
        {
            return commandExecutor.execute(new StartProcessInstanceByMessageCmd(messageName, null, processVariables, tenantId));
        }

        public virtual IProcessInstance startProcessInstanceByMessage(string messageName, string businessKey, IDictionary<string, object> processVariables)
        {
            return commandExecutor.execute(new StartProcessInstanceByMessageCmd(messageName, businessKey, processVariables, null));
        }

        public virtual IProcessInstance startProcessInstanceByMessageAndTenantId(string messageName, string businessKey, IDictionary<string, object> processVariables, string tenantId)
        {
            return commandExecutor.execute(new StartProcessInstanceByMessageCmd(messageName, businessKey, processVariables, tenantId));
        }

        public virtual void signalEventReceived(string signalName)
        {
            commandExecutor.execute(new SignalEventReceivedCmd(signalName, null, null, null));
        }

        public virtual void signalEventReceivedWithTenantId(string signalName, string tenantId)
        {
            commandExecutor.execute(new SignalEventReceivedCmd(signalName, null, null, tenantId));
        }

        public virtual void signalEventReceivedAsync(string signalName)
        {
            commandExecutor.execute(new SignalEventReceivedCmd(signalName, null, true, null));
        }

        public virtual void signalEventReceivedAsyncWithTenantId(string signalName, string tenantId)
        {
            commandExecutor.execute(new SignalEventReceivedCmd(signalName, null, true, tenantId));
        }

        public virtual void signalEventReceived(string signalName, IDictionary<string, object> processVariables)
        {
            commandExecutor.execute(new SignalEventReceivedCmd(signalName, null, processVariables, null));
        }

        public virtual void signalEventReceivedWithTenantId(string signalName, IDictionary<string, object> processVariables, string tenantId)
        {
            commandExecutor.execute(new SignalEventReceivedCmd(signalName, null, processVariables, tenantId));
        }

        public virtual void signalEventReceived(string signalName, string executionId)
        {
            commandExecutor.execute(new SignalEventReceivedCmd(signalName, executionId, null, null));
        }

        public virtual void signalEventReceived(string signalName, string executionId, IDictionary<string, object> processVariables)
        {
            commandExecutor.execute(new SignalEventReceivedCmd(signalName, executionId, processVariables, null));
        }

        public virtual void signalEventReceivedAsync(string signalName, string executionId)
        {
            commandExecutor.execute(new SignalEventReceivedCmd(signalName, executionId, true, null));
        }

        public virtual void messageEventReceived(string messageName, string executionId)
        {
            commandExecutor.execute(new MessageEventReceivedCmd(messageName, executionId, null));
        }

        public virtual void messageEventReceived(string messageName, string executionId, IDictionary<string, object> processVariables)
        {
            commandExecutor.execute(new MessageEventReceivedCmd(messageName, executionId, processVariables));
        }

        public virtual void messageEventReceivedAsync(string messageName, string executionId)
        {
            commandExecutor.execute(new MessageEventReceivedCmd(messageName, executionId, true));
        }

        public virtual void addEventListener(IActivitiEventListener listenerToAdd)
        {
            commandExecutor.execute(new AddEventListenerCommand(listenerToAdd));
        }

        public virtual void addEventListener(IActivitiEventListener listenerToAdd, params ActivitiEventType[] types)
        {
            commandExecutor.execute(new AddEventListenerCommand(listenerToAdd, types));
        }

        public virtual void removeEventListener(IActivitiEventListener listenerToRemove)
        {
            commandExecutor.execute(new RemoveEventListenerCommand(listenerToRemove));
        }

        public virtual void dispatchEvent(IActivitiEvent @event)
        {
            commandExecutor.execute(new DispatchEventCommand(@event));
        }

        public virtual void setProcessInstanceName(string processInstanceId, string name)
        {
            commandExecutor.execute(new SetProcessInstanceNameCmd(processInstanceId, name));
        }

        public virtual IList<Event> getProcessInstanceEvents(string processInstanceId)
        {
            return commandExecutor.execute(new GetProcessInstanceEventsCmd(processInstanceId)) as IList<Event>;
        }

        public virtual IList<FlowNode> getEnabledActivitiesFromAdhocSubProcess(string executionId)
        {
            return commandExecutor.execute(new GetEnabledActivitiesForAdhocSubProcessCmd(executionId));
        }

        public virtual IExecution executeActivityInAdhocSubProcess(string executionId, string activityId)
        {
            return commandExecutor.execute(new ExecuteActivityForAdhocSubProcessCmd(executionId, activityId));
        }

        public virtual void completeAdhocSubProcess(string executionId)
        {
            commandExecutor.execute(new CompleteAdhocSubProcessCmd(executionId));
        }

        public virtual IProcessInstanceBuilder createProcessInstanceBuilder()
        {
            return new ProcessInstanceBuilderImpl(this);
        }

        public virtual IProcessInstance startProcessInstance(ProcessInstanceBuilderImpl processInstanceBuilder)
        {
            if (!ReferenceEquals(processInstanceBuilder.ProcessDefinitionId, null) || !ReferenceEquals(processInstanceBuilder.ProcessDefinitionKey, null))
            {
                return commandExecutor.execute(new StartProcessInstanceCmd<IProcessInstance>(processInstanceBuilder));
            }
            else if (!ReferenceEquals(processInstanceBuilder.MessageName, null))
            {
                return commandExecutor.execute(new StartProcessInstanceByMessageCmd(processInstanceBuilder));
            }
            else
            {
                throw new ActivitiIllegalArgumentException("No processDefinitionId, processDefinitionKey nor messageName provided");
            }
        }
    }

}