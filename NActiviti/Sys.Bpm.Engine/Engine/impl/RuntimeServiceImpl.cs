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
namespace Sys.Workflow.Engine.Impl
{

    using Sys.Workflow.Bpmn.Models;
    using Sys.Workflow.Engine.Delegate.Events;
    using Sys.Workflow.Engine.Impl.Cmd;
    using Sys.Workflow.Engine.Impl.Persistence.Entity;
    using Sys.Workflow.Engine.Impl.Runtimes;
    using Sys.Workflow.Engine.Runtime;
    using Sys.Workflow.Engine.Tasks;
    using Sys.Workflow.Services.Api.Commands;
    using System.Linq;
    using System.Threading.Tasks;

    public class RuntimeServiceImpl : ServiceImpl, IRuntimeService
    {
        public virtual IProcessInstance StartProcessInstanceByKey(string processDefinitionKey)
        {
            return commandExecutor.Execute(new StartProcessInstanceCmd(processDefinitionKey, null, null, null));
        }

        public virtual IProcessInstance StartProcessInstanceByKey(string processDefinitionKey, string businessKey)
        {
            return commandExecutor.Execute(new StartProcessInstanceCmd(processDefinitionKey, null, businessKey, null));
        }

        public virtual IProcessInstance StartProcessInstanceByKey(string processDefinitionKey, IDictionary<string, object> variables)
        {
            return commandExecutor.Execute(new StartProcessInstanceCmd(processDefinitionKey, null, null, variables));
        }

        public virtual IProcessInstance StartProcessInstanceByKey(string processDefinitionKey, string businessKey, IDictionary<string, object> variables)
        {
            return commandExecutor.Execute(new StartProcessInstanceCmd(processDefinitionKey, null, businessKey, variables));
        }

        public virtual IProcessInstance StartProcessInstanceByKeyAndTenantId(string processDefinitionKey, string tenantId)
        {
            return commandExecutor.Execute(new StartProcessInstanceCmd(processDefinitionKey, null, null, null, tenantId));
        }

        public virtual IProcessInstance StartProcessInstanceByKeyAndTenantId(string processDefinitionKey, string businessKey, string tenantId)
        {
            return commandExecutor.Execute(new StartProcessInstanceCmd(processDefinitionKey, null, businessKey, null, tenantId));
        }

        public virtual IProcessInstance StartProcessInstanceByKeyAndTenantId(string processDefinitionKey, IDictionary<string, object> variables, string tenantId)
        {
            return commandExecutor.Execute(new StartProcessInstanceCmd(processDefinitionKey, null, null, variables, tenantId));
        }

        public virtual IProcessInstance StartProcessInstanceByKeyAndTenantId(string processDefinitionKey, string businessKey, IDictionary<string, object> variables, string tenantId)
        {
            return commandExecutor.Execute(new StartProcessInstanceCmd(processDefinitionKey, null, businessKey, variables, tenantId));
        }

        public virtual IProcessInstance StartProcessInstanceById(string processDefinitionId)
        {
            return commandExecutor.Execute(new StartProcessInstanceCmd(null, processDefinitionId, null, null));
        }

        public virtual IProcessInstance StartProcessInstanceById(string processDefinitionId, string businessKey)
        {
            return commandExecutor.Execute(new StartProcessInstanceCmd(null, processDefinitionId, businessKey, null));
        }

        public virtual IProcessInstance StartProcessInstanceById(string processDefinitionId, IDictionary<string, object> variables)
        {
            return commandExecutor.Execute(new StartProcessInstanceCmd(null, processDefinitionId, null, variables));
        }

        public virtual IProcessInstance StartProcessInstanceById(string processDefinitionId, string businessKey, IDictionary<string, object> variables)
        {
            return commandExecutor.Execute(new StartProcessInstanceCmd(null, processDefinitionId, businessKey, variables));
        }

        private readonly object syncRoot = new object();

        public virtual void DeleteProcessInstance(string processInstanceId, string deleteReason)
        {
            lock (syncRoot)
            {
                commandExecutor.Execute(new DeleteProcessInstanceCmd(processInstanceId, deleteReason));
            }
        }

        public virtual void TerminateProcessInstance(string processInstanceId, string businessKey, string reason)
        {
            lock (syncRoot)
            {
                commandExecutor.Execute(new TerminateProcessInstanceCmd(processInstanceId, businessKey, reason));
            }
        }


        public virtual IExecutionQuery CreateExecutionQuery()
        {
            return new ExecutionQueryImpl(commandExecutor);
        }

        public virtual INativeExecutionQuery CreateNativeExecutionQuery()
        {
            return new NativeExecutionQueryImpl(commandExecutor);
        }

        public virtual INativeProcessInstanceQuery CreateNativeProcessInstanceQuery()
        {
            return new NativeProcessInstanceQueryImpl(commandExecutor);
        }

        public virtual void UpdateBusinessKey(string processInstanceId, string businessKey)
        {
            commandExecutor.Execute(new SetProcessInstanceBusinessKeyCmd(processInstanceId, businessKey));
        }

        public virtual IDictionary<string, object> GetVariables(string executionId)
        {
            return commandExecutor.Execute(new GetExecutionVariablesCmd(executionId, null, false));
        }

        public virtual IDictionary<string, IVariableInstance> GetVariableInstances(string executionId)
        {
            return commandExecutor.Execute(new GetExecutionVariableInstancesCmd(executionId, null, false));
        }

        public virtual IList<IVariableInstance> GetVariableInstancesByExecutionIds(string[] executionIds)
        {
            return commandExecutor.Execute(new GetExecutionsVariablesCmd(executionIds));
        }

        public virtual IDictionary<string, object> GetVariablesLocal(string executionId)
        {
            return commandExecutor.Execute(new GetExecutionVariablesCmd(executionId, null, true));
        }

        public virtual IDictionary<string, IVariableInstance> GetVariableInstancesLocal(string executionId)
        {
            return commandExecutor.Execute(new GetExecutionVariableInstancesCmd(executionId, null, true));
        }

        public virtual IDictionary<string, object> GetVariables(string executionId, IEnumerable<string> variableNames)
        {
            return commandExecutor.Execute(new GetExecutionVariablesCmd(executionId, variableNames, false));
        }

        public virtual IDictionary<string, IVariableInstance> GetVariableInstances(string executionId, IEnumerable<string> variableNames)
        {
            return commandExecutor.Execute(new GetExecutionVariableInstancesCmd(executionId, variableNames, false));
        }

        public virtual IDictionary<string, object> GetVariablesLocal(string executionId, IEnumerable<string> variableNames)
        {
            return commandExecutor.Execute(new GetExecutionVariablesCmd(executionId, variableNames, true));
        }

        public virtual IDictionary<string, IVariableInstance> GetVariableInstancesLocal(string executionId, IEnumerable<string> variableNames)
        {
            return commandExecutor.Execute(new GetExecutionVariableInstancesCmd(executionId, variableNames, true));
        }

        public virtual object GetVariable(string executionId, string variableName)
        {
            return commandExecutor.Execute(new GetExecutionVariableCmd(executionId, variableName, false));
        }

        public virtual IVariableInstance GetVariableInstance(string executionId, string variableName)
        {
            return commandExecutor.Execute(new GetExecutionVariableInstanceCmd(executionId, variableName, false));
        }

        public virtual T GetVariable<T>(string executionId, string variableName)
        {
            return (T)GetVariable(executionId, variableName);
        }

        public virtual bool HasVariable(string executionId, string variableName)
        {
            return commandExecutor.Execute(new HasExecutionVariableCmd(executionId, variableName, false));
        }

        public virtual object GetVariableLocal(string executionId, string variableName)
        {
            return commandExecutor.Execute(new GetExecutionVariableCmd(executionId, variableName, true));
        }

        public virtual IVariableInstance GetVariableInstanceLocal(string executionId, string variableName)
        {
            return commandExecutor.Execute(new GetExecutionVariableInstanceCmd(executionId, variableName, true));
        }

        public virtual T GetVariableLocal<T>(string executionId, string variableName)
        {
            return (T)GetVariableLocal(executionId, variableName);
        }

        public virtual bool HasVariableLocal(string executionId, string variableName)
        {
            return commandExecutor.Execute(new HasExecutionVariableCmd(executionId, variableName, true));
        }

        public virtual void SetVariable(string executionId, string variableName, object value)
        {
            if (variableName is null)
            {
                throw new ActivitiIllegalArgumentException("variableName is null");
            }
            IDictionary<string, object> variables = new Dictionary<string, object>
            {
                [variableName] = value
            };
            commandExecutor.Execute(new SetExecutionVariablesCmd(executionId, variables, false));
        }

        public virtual void SetVariableLocal(string executionId, string variableName, object value)
        {
            if (variableName is null)
            {
                throw new ActivitiIllegalArgumentException("variableName is null");
            }
            IDictionary<string, object> variables = new Dictionary<string, object>
            {
                [variableName] = value
            };
            commandExecutor.Execute(new SetExecutionVariablesCmd(executionId, variables, true));
        }

        public virtual void SetVariables<T1>(string executionId, IDictionary<string, T1> variables)
        {
            commandExecutor.Execute(new SetExecutionVariablesCmd(executionId, variables as IDictionary<string, object>, false));
        }

        public virtual void SetVariablesLocal<T1>(string executionId, IDictionary<string, T1> variables)
        {
            commandExecutor.Execute(new SetExecutionVariablesCmd(executionId, variables as IDictionary<string, object>, true));
        }

        public virtual void RemoveVariable(string executionId, string variableName)
        {
            IList<string> variableNames = new List<string>(1)
            {
                variableName
            };
            commandExecutor.Execute(new RemoveExecutionVariablesCmd(executionId, variableNames, false));
        }

        public virtual void RemoveVariableLocal(string executionId, string variableName)
        {
            IList<string> variableNames = new List<string>
            {
                variableName
            };
            commandExecutor.Execute(new RemoveExecutionVariablesCmd(executionId, variableNames, true));
        }

        public virtual void RemoveVariables(string executionId, IEnumerable<string> variableNames)
        {
            commandExecutor.Execute(new RemoveExecutionVariablesCmd(executionId, variableNames, false));
        }

        public virtual void RemoveVariablesLocal(string executionId, IEnumerable<string> variableNames)
        {
            commandExecutor.Execute(new RemoveExecutionVariablesCmd(executionId, variableNames, true));
        }

        public virtual IDictionary<string, IDataObject> GetDataObjects(string executionId)
        {
            return commandExecutor.Execute(new GetDataObjectsCmd(executionId, null, false));
        }

        public virtual IDictionary<string, IDataObject> GetDataObjects(string executionId, string locale, bool withLocalizationFallback)
        {
            return commandExecutor.Execute(new GetDataObjectsCmd(executionId, null, false, locale, withLocalizationFallback));
        }

        public virtual IDictionary<string, IDataObject> GetDataObjectsLocal(string executionId)
        {
            return commandExecutor.Execute(new GetDataObjectsCmd(executionId, null, true));
        }

        public virtual IDictionary<string, IDataObject> GetDataObjectsLocal(string executionId, string locale, bool withLocalizationFallback)
        {
            return commandExecutor.Execute(new GetDataObjectsCmd(executionId, null, true, locale, withLocalizationFallback));
        }

        public virtual IDictionary<string, IDataObject> GetDataObjects(string executionId, IEnumerable<string> dataObjectNames)
        {
            return commandExecutor.Execute(new GetDataObjectsCmd(executionId, dataObjectNames, false));
        }

        public virtual IDictionary<string, IDataObject> GetDataObjects(string executionId, IEnumerable<string> dataObjectNames, string locale, bool withLocalizationFallback)
        {
            return commandExecutor.Execute(new GetDataObjectsCmd(executionId, dataObjectNames, false, locale, withLocalizationFallback));
        }

        public virtual IDictionary<string, IDataObject> GetDataObjectsLocal(string executionId, IEnumerable<string> dataObjects)
        {
            return commandExecutor.Execute(new GetDataObjectsCmd(executionId, dataObjects, true));
        }

        public virtual IDictionary<string, IDataObject> GetDataObjectsLocal(string executionId, IEnumerable<string> dataObjectNames, string locale, bool withLocalizationFallback)
        {
            return commandExecutor.Execute(new GetDataObjectsCmd(executionId, dataObjectNames, true, locale, withLocalizationFallback));
        }

        public virtual IDataObject GetDataObject(string executionId, string dataObject)
        {
            return commandExecutor.Execute(new GetDataObjectCmd(executionId, dataObject, false));
        }

        public virtual IDataObject GetDataObject(string executionId, string dataObjectName, string locale, bool withLocalizationFallback)
        {
            return commandExecutor.Execute(new GetDataObjectCmd(executionId, dataObjectName, false, locale, withLocalizationFallback));
        }

        public virtual IDataObject GetDataObjectLocal(string executionId, string dataObjectName)
        {
            return commandExecutor.Execute(new GetDataObjectCmd(executionId, dataObjectName, true));
        }

        public virtual IDataObject GetDataObjectLocal(string executionId, string dataObjectName, string locale, bool withLocalizationFallback)
        {
            return commandExecutor.Execute(new GetDataObjectCmd(executionId, dataObjectName, true, locale, withLocalizationFallback));
        }

        public virtual void Signal(string executionId)
        {
            commandExecutor.Execute(new TriggerCmd(executionId, null));
        }

        public virtual void Trigger(string executionId)
        {
            commandExecutor.Execute(new TriggerCmd(executionId, null));
        }

        public virtual void Signal(string executionId, IDictionary<string, object> processVariables)
        {
            commandExecutor.Execute(new TriggerCmd(executionId, processVariables));
        }

        public virtual void Trigger(string executionId, IDictionary<string, object> processVariables)
        {
            commandExecutor.Execute(new TriggerCmd(executionId, processVariables));
        }

        public virtual void Trigger(string executionId, IDictionary<string, object> processVariables, IDictionary<string, object> transientVariables)
        {
            commandExecutor.Execute(new TriggerCmd(executionId, processVariables, transientVariables));
        }

        public virtual void AddUserIdentityLink(string processInstanceId, string userId, string identityLinkType)
        {
            commandExecutor.Execute(new AddIdentityLinkForProcessInstanceCmd(processInstanceId, userId, null, identityLinkType));
        }

        public virtual void AddGroupIdentityLink(string processInstanceId, string groupId, string identityLinkType)
        {
            commandExecutor.Execute(new AddIdentityLinkForProcessInstanceCmd(processInstanceId, null, groupId, identityLinkType));
        }

        public virtual void AddParticipantUser(string processInstanceId, string userId)
        {
            commandExecutor.Execute(new AddIdentityLinkForProcessInstanceCmd(processInstanceId, userId, null, IdentityLinkType.PARTICIPANT));
        }

        public virtual void AddParticipantGroup(string processInstanceId, string groupId)
        {
            commandExecutor.Execute(new AddIdentityLinkForProcessInstanceCmd(processInstanceId, null, groupId, IdentityLinkType.PARTICIPANT));
        }

        public virtual void DeleteParticipantUser(string processInstanceId, string userId)
        {
            commandExecutor.Execute(new DeleteIdentityLinkForProcessInstanceCmd(processInstanceId, userId, null, IdentityLinkType.PARTICIPANT));
        }

        public virtual void DeleteParticipantGroup(string processInstanceId, string groupId)
        {
            commandExecutor.Execute(new DeleteIdentityLinkForProcessInstanceCmd(processInstanceId, null, groupId, IdentityLinkType.PARTICIPANT));
        }

        public virtual void DeleteUserIdentityLink(string processInstanceId, string userId, string identityLinkType)
        {
            commandExecutor.Execute(new DeleteIdentityLinkForProcessInstanceCmd(processInstanceId, userId, null, identityLinkType));
        }

        public virtual void DeleteGroupIdentityLink(string processInstanceId, string groupId, string identityLinkType)
        {
            commandExecutor.Execute(new DeleteIdentityLinkForProcessInstanceCmd(processInstanceId, null, groupId, identityLinkType));
        }

        public virtual IList<IIdentityLink> GetIdentityLinksForProcessInstance(string processInstanceId)
        {
            return commandExecutor.Execute(new GetIdentityLinksForProcessInstanceCmd(processInstanceId));
        }

        public virtual IProcessInstanceQuery CreateProcessInstanceQuery()
        {
            return new ProcessInstanceQueryImpl(commandExecutor);
        }

        public virtual IList<string> GetActiveActivityIds(string executionId)
        {
            return commandExecutor.Execute(new FindActiveActivityIdsCmd(executionId));
        }

        public virtual void SuspendProcessInstanceById(string processInstanceId)
        {
            commandExecutor.Execute(new SuspendProcessInstanceCmd(processInstanceId));
        }

        public virtual void ActivateProcessInstanceById(string processInstanceId)
        {
            commandExecutor.Execute(new ActivateProcessInstanceCmd(processInstanceId));
        }

        public virtual IProcessInstance StartProcessInstanceByMessage(string messageName)
        {
            return commandExecutor.Execute(new StartProcessInstanceByMessageCmd(messageName, null, null, null));
        }

        public virtual IProcessInstance StartProcessInstanceByMessageAndTenantId(string messageName, string tenantId)
        {
            return commandExecutor.Execute(new StartProcessInstanceByMessageCmd(messageName, null, null, tenantId));
        }

        public virtual IProcessInstance StartProcessInstanceByMessage(string messageName, string businessKey)
        {
            return commandExecutor.Execute(new StartProcessInstanceByMessageCmd(messageName, businessKey, null, null));
        }

        public virtual IProcessInstance StartProcessInstanceByMessageAndTenantId(string messageName, string businessKey, string tenantId)
        {
            return commandExecutor.Execute(new StartProcessInstanceByMessageCmd(messageName, businessKey, null, tenantId));
        }

        public virtual IProcessInstance StartProcessInstanceByMessage(string messageName, IDictionary<string, object> processVariables)
        {
            return commandExecutor.Execute(new StartProcessInstanceByMessageCmd(messageName, null, processVariables, null));
        }

        public virtual IProcessInstance StartProcessInstanceByMessageAndTenantId(string messageName, IDictionary<string, object> processVariables, string tenantId)
        {
            return commandExecutor.Execute(new StartProcessInstanceByMessageCmd(messageName, null, processVariables, tenantId));
        }

        public virtual IProcessInstance StartProcessInstanceByMessage(string messageName, string businessKey, IDictionary<string, object> processVariables)
        {
            return commandExecutor.Execute(new StartProcessInstanceByMessageCmd(messageName, businessKey, processVariables, null));
        }

        public virtual IProcessInstance StartProcessInstanceByMessageAndTenantId(string messageName, string businessKey, IDictionary<string, object> processVariables, string tenantId)
        {
            return commandExecutor.Execute(new StartProcessInstanceByMessageCmd(messageName, businessKey, processVariables, tenantId));
        }

        public virtual void SignalEventReceived(string signalName)
        {
            commandExecutor.Execute(new SignalEventReceivedCmd(signalName, null, null, null));
        }

        public virtual void SignalEventReceivedWithTenantId(string signalName, string tenantId)
        {
            commandExecutor.Execute(new SignalEventReceivedCmd(signalName, null, null, tenantId));
        }

        public virtual void SignalEventReceivedAsync(string signalName)
        {
            commandExecutor.Execute(new SignalEventReceivedCmd(signalName, null, true, null));
        }

        public virtual void SignalEventReceivedAsyncWithTenantId(string signalName, string tenantId)
        {
            commandExecutor.Execute(new SignalEventReceivedCmd(signalName, null, true, tenantId));
        }

        public virtual void SignalEventReceived(string signalName, IDictionary<string, object> processVariables)
        {
            commandExecutor.Execute(new SignalEventReceivedCmd(signalName, null, processVariables, null));
        }

        public virtual void SignalEventReceivedWithTenantId(string signalName, IDictionary<string, object> processVariables, string tenantId)
        {
            commandExecutor.Execute(new SignalEventReceivedCmd(signalName, null, processVariables, tenantId));
        }

        public virtual void SignalEventReceived(string signalName, string executionId)
        {
            commandExecutor.Execute(new SignalEventReceivedCmd(signalName, executionId, null, null));
        }

        public virtual void SignalEventReceived(string signalName, string executionId, IDictionary<string, object> processVariables)
        {
            commandExecutor.Execute(new SignalEventReceivedCmd(signalName, executionId, processVariables, null));
        }

        public virtual void SignalEventReceivedAsync(string signalName, string executionId)
        {
            commandExecutor.Execute(new SignalEventReceivedCmd(signalName, executionId, true, null));
        }

        public virtual void MessageEventReceived(string messageName, string executionId)
        {
            commandExecutor.Execute(new MessageEventReceivedCmd(messageName, executionId, null));
        }

        public virtual void MessageEventReceived(string messageName, string executionId, IDictionary<string, object> processVariables)
        {
            commandExecutor.Execute(new MessageEventReceivedCmd(messageName, executionId, processVariables));
        }

        public virtual void MessageEventReceivedAsync(string messageName, string executionId)
        {
            commandExecutor.Execute(new MessageEventReceivedCmd(messageName, executionId, true));
        }

        public virtual void AddEventListener(IActivitiEventListener listenerToAdd)
        {
            commandExecutor.Execute(new AddEventListenerCommand(listenerToAdd));
        }

        public virtual void AddEventListener(IActivitiEventListener listenerToAdd, params ActivitiEventType[] types)
        {
            commandExecutor.Execute(new AddEventListenerCommand(listenerToAdd, types));
        }

        public virtual void RemoveEventListener(IActivitiEventListener listenerToRemove)
        {
            commandExecutor.Execute(new RemoveEventListenerCommand(listenerToRemove));
        }

        public virtual void DispatchEvent(IActivitiEvent @event)
        {
            commandExecutor.Execute(new DispatchEventCommand(@event));
        }

        public virtual void SetProcessInstanceName(string processInstanceId, string name)
        {
            commandExecutor.Execute(new SetProcessInstanceNameCmd(processInstanceId, name));
        }

        public virtual IList<Event> GetProcessInstanceEvents(string processInstanceId)
        {
            return commandExecutor.Execute(new GetProcessInstanceEventsCmd(processInstanceId)) as IList<Event>;
        }

        public virtual IList<FlowNode> GetEnabledActivitiesFromAdhocSubProcess(string executionId)
        {
            return commandExecutor.Execute(new GetEnabledActivitiesForAdhocSubProcessCmd(executionId));
        }

        public virtual IExecution ExecuteActivityInAdhocSubProcess(string executionId, string activityId)
        {
            return commandExecutor.Execute(new ExecuteActivityForAdhocSubProcessCmd(executionId, activityId));
        }

        public virtual void CompleteAdhocSubProcess(string executionId)
        {
            commandExecutor.Execute(new CompleteAdhocSubProcessCmd(executionId));
        }

        public virtual IProcessInstanceBuilder CreateProcessInstanceBuilder()
        {
            return new ProcessInstanceBuilderImpl(this);
        }

        public virtual IProcessInstance StartProcessInstance(ProcessInstanceBuilderImpl processInstanceBuilder)
        {
            if (processInstanceBuilder.ProcessDefinitionId is object || processInstanceBuilder.ProcessDefinitionKey is object)
            {
                return commandExecutor.Execute(new StartProcessInstanceCmd(processInstanceBuilder));
            }
            else if (processInstanceBuilder.MessageName is object)
            {
                return commandExecutor.Execute(new StartProcessInstanceByMessageCmd(processInstanceBuilder));
            }
            else
            {
                throw new ActivitiIllegalArgumentException("No processDefinitionId, processDefinitionKey nor messageName provided");
            }
        }

        /// <inheritdoc />

        public IProcessInstance[] StartProcessInstanceByCmd(IStartProcessInstanceCmd[] cmds)
        {
            IList<IProcessInstance> instances = new List<IProcessInstance>();
            IList<Task> tasks = new List<Task>();
            foreach (IStartProcessInstanceCmd cmd in cmds)
            {
                tasks.Add(Task.Factory.StartNew(() =>
                {
                    var scmd = new StartProcessInstanceCmd(cmd);

                    instances.Add(commandExecutor.Execute(scmd));
                }));
            }

            Task.WaitAll(tasks.ToArray());

            return instances.ToArray();
        }

        /// <inheritdoc />

        public IProcessInstance StartProcessInstanceByCmd(IStartProcessInstanceCmd cmd)
        {
            IProcessInstance[] ins = StartProcessInstanceByCmd(new IStartProcessInstanceCmd[] { cmd });

            return ins[0];
        }
    }

}