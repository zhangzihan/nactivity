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
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    ///<inheritdoc />
    public class RuntimeServiceImpl : ServiceImpl, IRuntimeService
    {
        ///<inheritdoc />
        public virtual IProcessInstance StartProcessInstanceByKey(string processDefinitionKey)
        {
            return commandExecutor.Execute(new StartProcessInstanceCmd(processDefinitionKey, null, null, null));
        }

        ///<inheritdoc />
        public virtual IProcessInstance StartProcessInstanceByKey(string processDefinitionKey, string businessKey)
        {
            return commandExecutor.Execute(new StartProcessInstanceCmd(processDefinitionKey, null, businessKey, null));
        }

        ///<inheritdoc />
        public virtual IProcessInstance StartProcessInstanceByKey(string processDefinitionKey, IDictionary<string, object> variables)
        {
            return commandExecutor.Execute(new StartProcessInstanceCmd(processDefinitionKey, null, null, variables));
        }

        ///<inheritdoc />
        public virtual IProcessInstance StartProcessInstanceByKey(string processDefinitionKey, string businessKey, IDictionary<string, object> variables)
        {
            return commandExecutor.Execute(new StartProcessInstanceCmd(processDefinitionKey, null, businessKey, variables));
        }

        ///<inheritdoc />
        public virtual IProcessInstance StartProcessInstanceByKeyAndTenantId(string processDefinitionKey, string tenantId)
        {
            return commandExecutor.Execute(new StartProcessInstanceCmd(processDefinitionKey, null, null, null, tenantId));
        }

        ///<inheritdoc />
        public virtual IProcessInstance StartProcessInstanceByKeyAndTenantId(string processDefinitionKey, string businessKey, string tenantId)
        {
            return commandExecutor.Execute(new StartProcessInstanceCmd(processDefinitionKey, null, businessKey, null, tenantId));
        }

        ///<inheritdoc />
        public virtual IProcessInstance StartProcessInstanceByKeyAndTenantId(string processDefinitionKey, IDictionary<string, object> variables, string tenantId)
        {
            return commandExecutor.Execute(new StartProcessInstanceCmd(processDefinitionKey, null, null, variables, tenantId));
        }

        ///<inheritdoc />
        public virtual IProcessInstance StartProcessInstanceByKeyAndTenantId(string processDefinitionKey, string businessKey, IDictionary<string, object> variables, string tenantId)
        {
            return commandExecutor.Execute(new StartProcessInstanceCmd(processDefinitionKey, null, businessKey, variables, tenantId));
        }

        ///<inheritdoc />
        public virtual IProcessInstance StartProcessInstanceById(string processDefinitionId)
        {
            return commandExecutor.Execute(new StartProcessInstanceCmd(null, processDefinitionId, null, null));
        }

        ///<inheritdoc />
        public virtual IProcessInstance StartProcessInstanceById(string processDefinitionId, string businessKey)
        {
            return commandExecutor.Execute(new StartProcessInstanceCmd(null, processDefinitionId, businessKey, null));
        }

        ///<inheritdoc />
        public virtual IProcessInstance StartProcessInstanceById(string processDefinitionId, IDictionary<string, object> variables)
        {
            return commandExecutor.Execute(new StartProcessInstanceCmd(null, processDefinitionId, null, variables));
        }

        ///<inheritdoc />
        public virtual IProcessInstance StartProcessInstanceById(string processDefinitionId, string businessKey, IDictionary<string, object> variables)
        {
            return commandExecutor.Execute(new StartProcessInstanceCmd(null, processDefinitionId, businessKey, variables));
        }

        ///<inheritdoc />
        public virtual void DeleteProcessInstance(string processInstanceId, string deleteReason)
        {
            commandExecutor.Execute(new DeleteProcessInstanceCmd(processInstanceId, deleteReason));
        }

        ///<inheritdoc />
        public virtual void TerminateProcessInstance(string processInstanceId, string businessKey, string reason, IDictionary<string, object> variables)
        {
            commandExecutor.Execute(new TerminateProcessInstanceCmd(processInstanceId, businessKey, reason, variables));
        }

        ///<inheritdoc />
        public virtual IExecutionQuery CreateExecutionQuery()
        {
            return new ExecutionQueryImpl(commandExecutor);
        }

        ///<inheritdoc />
        public virtual INativeExecutionQuery CreateNativeExecutionQuery()
        {
            return new NativeExecutionQueryImpl(commandExecutor);
        }

        ///<inheritdoc />
        public virtual INativeProcessInstanceQuery CreateNativeProcessInstanceQuery()
        {
            return new NativeProcessInstanceQueryImpl(commandExecutor);
        }

        ///<inheritdoc />
        public virtual void UpdateBusinessKey(string processInstanceId, string businessKey)
        {
            commandExecutor.Execute(new SetProcessInstanceBusinessKeyCmd(processInstanceId, businessKey));
        }

        ///<inheritdoc />
        public virtual IDictionary<string, object> GetVariables(string executionId)
        {
            return commandExecutor.Execute(new GetExecutionVariablesCmd(executionId, null, false));
        }

        ///<inheritdoc />
        public virtual IDictionary<string, IVariableInstance> GetVariableInstances(string executionId)
        {
            return commandExecutor.Execute(new GetExecutionVariableInstancesCmd(executionId, null, false));
        }

        ///<inheritdoc />
        public virtual IList<IVariableInstance> GetVariableInstancesByExecutionIds(string[] executionIds)
        {
            return commandExecutor.Execute(new GetExecutionsVariablesCmd(executionIds));
        }

        ///<inheritdoc />
        public virtual IDictionary<string, object> GetVariablesLocal(string executionId)
        {
            return commandExecutor.Execute(new GetExecutionVariablesCmd(executionId, null, true));
        }

        ///<inheritdoc />
        public virtual IDictionary<string, IVariableInstance> GetVariableInstancesLocal(string executionId)
        {
            return commandExecutor.Execute(new GetExecutionVariableInstancesCmd(executionId, null, true));
        }

        ///<inheritdoc />
        public virtual IDictionary<string, object> GetVariables(string executionId, IEnumerable<string> variableNames)
        {
            return commandExecutor.Execute(new GetExecutionVariablesCmd(executionId, variableNames, false));
        }

        ///<inheritdoc />
        public virtual IDictionary<string, IVariableInstance> GetVariableInstances(string executionId, IEnumerable<string> variableNames)
        {
            return commandExecutor.Execute(new GetExecutionVariableInstancesCmd(executionId, variableNames, false));
        }

        ///<inheritdoc />
        public virtual IDictionary<string, object> GetVariablesLocal(string executionId, IEnumerable<string> variableNames)
        {
            return commandExecutor.Execute(new GetExecutionVariablesCmd(executionId, variableNames, true));
        }

        ///<inheritdoc />
        public virtual IDictionary<string, IVariableInstance> GetVariableInstancesLocal(string executionId, IEnumerable<string> variableNames)
        {
            return commandExecutor.Execute(new GetExecutionVariableInstancesCmd(executionId, variableNames, true));
        }

        ///<inheritdoc />
        public virtual object GetVariable(string executionId, string variableName)
        {
            return commandExecutor.Execute(new GetExecutionVariableCmd(executionId, variableName, false));
        }

        ///<inheritdoc />
        public virtual IVariableInstance GetVariableInstance(string executionId, string variableName)
        {
            return commandExecutor.Execute(new GetExecutionVariableInstanceCmd(executionId, variableName, false));
        }

        ///<inheritdoc />
        public virtual T GetVariable<T>(string executionId, string variableName)
        {
            return (T)GetVariable(executionId, variableName);
        }

        ///<inheritdoc />
        public virtual bool HasVariable(string executionId, string variableName)
        {
            return commandExecutor.Execute(new HasExecutionVariableCmd(executionId, variableName, false));
        }

        ///<inheritdoc />
        public virtual object GetVariableLocal(string executionId, string variableName)
        {
            return commandExecutor.Execute(new GetExecutionVariableCmd(executionId, variableName, true));
        }

        ///<inheritdoc />
        public virtual IVariableInstance GetVariableInstanceLocal(string executionId, string variableName)
        {
            return commandExecutor.Execute(new GetExecutionVariableInstanceCmd(executionId, variableName, true));
        }

        ///<inheritdoc />
        public virtual T GetVariableLocal<T>(string executionId, string variableName)
        {
            return (T)GetVariableLocal(executionId, variableName);
        }

        ///<inheritdoc />
        public virtual bool HasVariableLocal(string executionId, string variableName)
        {
            return commandExecutor.Execute(new HasExecutionVariableCmd(executionId, variableName, true));
        }

        ///<inheritdoc />
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

        ///<inheritdoc />
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

        ///<inheritdoc />
        public virtual void SetVariables<T1>(string executionId, IDictionary<string, T1> variables)
        {
            commandExecutor.Execute(new SetExecutionVariablesCmd(executionId, variables as IDictionary<string, object>, false));
        }

        ///<inheritdoc />
        public virtual void SetVariablesLocal<T1>(string executionId, IDictionary<string, T1> variables)
        {
            commandExecutor.Execute(new SetExecutionVariablesCmd(executionId, variables as IDictionary<string, object>, true));
        }

        ///<inheritdoc />
        public virtual void RemoveVariable(string executionId, string variableName)
        {
            IList<string> variableNames = new List<string>(1)
            {
                variableName
            };
            commandExecutor.Execute(new RemoveExecutionVariablesCmd(executionId, variableNames, false));
        }

        ///<inheritdoc />
        public virtual void RemoveVariableLocal(string executionId, string variableName)
        {
            IList<string> variableNames = new List<string>
            {
                variableName
            };
            commandExecutor.Execute(new RemoveExecutionVariablesCmd(executionId, variableNames, true));
        }

        ///<inheritdoc />
        public virtual void RemoveVariables(string executionId, IEnumerable<string> variableNames)
        {
            commandExecutor.Execute(new RemoveExecutionVariablesCmd(executionId, variableNames, false));
        }

        ///<inheritdoc />
        public virtual void RemoveVariablesLocal(string executionId, IEnumerable<string> variableNames)
        {
            commandExecutor.Execute(new RemoveExecutionVariablesCmd(executionId, variableNames, true));
        }

        ///<inheritdoc />
        public virtual IDictionary<string, IDataObject> GetDataObjects(string executionId)
        {
            return commandExecutor.Execute(new GetDataObjectsCmd(executionId, null, false));
        }

        ///<inheritdoc />
        public virtual IDictionary<string, IDataObject> GetDataObjects(string executionId, string locale, bool withLocalizationFallback)
        {
            return commandExecutor.Execute(new GetDataObjectsCmd(executionId, null, false, locale, withLocalizationFallback));
        }

        ///<inheritdoc />
        public virtual IDictionary<string, IDataObject> GetDataObjectsLocal(string executionId)
        {
            return commandExecutor.Execute(new GetDataObjectsCmd(executionId, null, true));
        }

        ///<inheritdoc />
        public virtual IDictionary<string, IDataObject> GetDataObjectsLocal(string executionId, string locale, bool withLocalizationFallback)
        {
            return commandExecutor.Execute(new GetDataObjectsCmd(executionId, null, true, locale, withLocalizationFallback));
        }

        ///<inheritdoc />
        public virtual IDictionary<string, IDataObject> GetDataObjects(string executionId, IEnumerable<string> dataObjectNames)
        {
            return commandExecutor.Execute(new GetDataObjectsCmd(executionId, dataObjectNames, false));
        }

        ///<inheritdoc />
        public virtual IDictionary<string, IDataObject> GetDataObjects(string executionId, IEnumerable<string> dataObjectNames, string locale, bool withLocalizationFallback)
        {
            return commandExecutor.Execute(new GetDataObjectsCmd(executionId, dataObjectNames, false, locale, withLocalizationFallback));
        }

        ///<inheritdoc />
        public virtual IDictionary<string, IDataObject> GetDataObjectsLocal(string executionId, IEnumerable<string> dataObjects)
        {
            return commandExecutor.Execute(new GetDataObjectsCmd(executionId, dataObjects, true));
        }

        ///<inheritdoc />
        public virtual IDictionary<string, IDataObject> GetDataObjectsLocal(string executionId, IEnumerable<string> dataObjectNames, string locale, bool withLocalizationFallback)
        {
            return commandExecutor.Execute(new GetDataObjectsCmd(executionId, dataObjectNames, true, locale, withLocalizationFallback));
        }

        ///<inheritdoc />
        public virtual IDataObject GetDataObject(string executionId, string dataObject)
        {
            return commandExecutor.Execute(new GetDataObjectCmd(executionId, dataObject, false));
        }

        ///<inheritdoc />
        public virtual IDataObject GetDataObject(string executionId, string dataObjectName, string locale, bool withLocalizationFallback)
        {
            return commandExecutor.Execute(new GetDataObjectCmd(executionId, dataObjectName, false, locale, withLocalizationFallback));
        }

        ///<inheritdoc />
        public virtual IDataObject GetDataObjectLocal(string executionId, string dataObjectName)
        {
            return commandExecutor.Execute(new GetDataObjectCmd(executionId, dataObjectName, true));
        }

        ///<inheritdoc />
        public virtual IDataObject GetDataObjectLocal(string executionId, string dataObjectName, string locale, bool withLocalizationFallback)
        {
            return commandExecutor.Execute(new GetDataObjectCmd(executionId, dataObjectName, true, locale, withLocalizationFallback));
        }

        ///<inheritdoc />
        public virtual void Signal(string executionId)
        {
            commandExecutor.Execute(new TriggerCmd(executionId, null));
        }

        ///<inheritdoc />
        public virtual void Trigger(string executionId)
        {
            commandExecutor.Execute(new TriggerCmd(executionId, null));
        }

        ///<inheritdoc />
        public virtual void Signal(string executionId, IDictionary<string, object> processVariables)
        {
            commandExecutor.Execute(new TriggerCmd(executionId, processVariables));
        }

        ///<inheritdoc />
        public virtual void Trigger(string executionId, IDictionary<string, object> processVariables)
        {
            commandExecutor.Execute(new TriggerCmd(executionId, processVariables));
        }

        ///<inheritdoc />
        public virtual void Trigger(string executionId, IDictionary<string, object> processVariables, IDictionary<string, object> transientVariables)
        {
            commandExecutor.Execute(new TriggerCmd(executionId, processVariables, transientVariables));
        }

        ///<inheritdoc />
        public virtual void AddUserIdentityLink(string processInstanceId, string userId, string identityLinkType)
        {
            commandExecutor.Execute(new AddIdentityLinkForProcessInstanceCmd(processInstanceId, userId, null, identityLinkType));
        }

        ///<inheritdoc />
        public virtual void AddGroupIdentityLink(string processInstanceId, string groupId, string identityLinkType)
        {
            commandExecutor.Execute(new AddIdentityLinkForProcessInstanceCmd(processInstanceId, null, groupId, identityLinkType));
        }

        ///<inheritdoc />
        public virtual void AddParticipantUser(string processInstanceId, string userId)
        {
            commandExecutor.Execute(new AddIdentityLinkForProcessInstanceCmd(processInstanceId, userId, null, IdentityLinkType.PARTICIPANT));
        }

        ///<inheritdoc />
        public virtual void AddParticipantGroup(string processInstanceId, string groupId)
        {
            commandExecutor.Execute(new AddIdentityLinkForProcessInstanceCmd(processInstanceId, null, groupId, IdentityLinkType.PARTICIPANT));
        }

        ///<inheritdoc />
        public virtual void DeleteParticipantUser(string processInstanceId, string userId)
        {
            commandExecutor.Execute(new DeleteIdentityLinkForProcessInstanceCmd(processInstanceId, userId, null, IdentityLinkType.PARTICIPANT));
        }

        ///<inheritdoc />
        public virtual void DeleteParticipantGroup(string processInstanceId, string groupId)
        {
            commandExecutor.Execute(new DeleteIdentityLinkForProcessInstanceCmd(processInstanceId, null, groupId, IdentityLinkType.PARTICIPANT));
        }

        ///<inheritdoc />
        public virtual void DeleteUserIdentityLink(string processInstanceId, string userId, string identityLinkType)
        {
            commandExecutor.Execute(new DeleteIdentityLinkForProcessInstanceCmd(processInstanceId, userId, null, identityLinkType));
        }

        ///<inheritdoc />
        public virtual void DeleteGroupIdentityLink(string processInstanceId, string groupId, string identityLinkType)
        {
            commandExecutor.Execute(new DeleteIdentityLinkForProcessInstanceCmd(processInstanceId, null, groupId, identityLinkType));
        }

        ///<inheritdoc />
        public virtual IList<IIdentityLink> GetIdentityLinksForProcessInstance(string processInstanceId)
        {
            return commandExecutor.Execute(new GetIdentityLinksForProcessInstanceCmd(processInstanceId));
        }

        ///<inheritdoc />
        public virtual IProcessInstanceQuery CreateProcessInstanceQuery()
        {
            return new ProcessInstanceQueryImpl(commandExecutor);
        }

        ///<inheritdoc />
        public virtual IList<string> GetActiveActivityIds(string executionId)
        {
            return commandExecutor.Execute(new FindActiveActivityIdsCmd(executionId));
        }

        ///<inheritdoc />
        public virtual void SuspendProcessInstanceById(string processInstanceId)
        {
            commandExecutor.Execute(new SuspendProcessInstanceCmd(processInstanceId));
        }

        ///<inheritdoc />
        public virtual void ActivateProcessInstanceById(string processInstanceId)
        {
            commandExecutor.Execute(new ActivateProcessInstanceCmd(processInstanceId));
        }

        ///<inheritdoc />
        public virtual IProcessInstance StartProcessInstanceByMessage(string messageName)
        {
            return commandExecutor.Execute(new StartProcessInstanceByMessageCmd(messageName, null, null, null));
        }

        ///<inheritdoc />
        public virtual IProcessInstance StartProcessInstanceByMessageAndTenantId(string messageName, string tenantId)
        {
            return commandExecutor.Execute(new StartProcessInstanceByMessageCmd(messageName, null, null, tenantId));
        }

        ///<inheritdoc />
        public virtual IProcessInstance StartProcessInstanceByMessage(string messageName, string businessKey)
        {
            return commandExecutor.Execute(new StartProcessInstanceByMessageCmd(messageName, businessKey, null, null));
        }

        ///<inheritdoc />
        public virtual IProcessInstance StartProcessInstanceByMessageAndTenantId(string messageName, string businessKey, string tenantId)
        {
            return commandExecutor.Execute(new StartProcessInstanceByMessageCmd(messageName, businessKey, null, tenantId));
        }

        ///<inheritdoc />
        public virtual IProcessInstance StartProcessInstanceByMessage(string messageName, IDictionary<string, object> processVariables)
        {
            return commandExecutor.Execute(new StartProcessInstanceByMessageCmd(messageName, null, processVariables, null));
        }

        ///<inheritdoc />
        public virtual IProcessInstance StartProcessInstanceByMessageAndTenantId(string messageName, IDictionary<string, object> processVariables, string tenantId)
        {
            return commandExecutor.Execute(new StartProcessInstanceByMessageCmd(messageName, null, processVariables, tenantId));
        }

        ///<inheritdoc />
        public virtual IProcessInstance StartProcessInstanceByMessage(string messageName, string businessKey, IDictionary<string, object> processVariables)
        {
            return commandExecutor.Execute(new StartProcessInstanceByMessageCmd(messageName, businessKey, processVariables, null));
        }

        ///<inheritdoc />
        public virtual IProcessInstance StartProcessInstanceByMessageAndTenantId(string messageName, string businessKey, IDictionary<string, object> processVariables, string tenantId)
        {
            return commandExecutor.Execute(new StartProcessInstanceByMessageCmd(messageName, businessKey, processVariables, tenantId));
        }

        ///<inheritdoc />
        public virtual void SignalEventReceived(string signalName)
        {
            commandExecutor.Execute(new SignalEventReceivedCmd(signalName, null, null, null));
        }

        ///<inheritdoc />
        public virtual void SignalEventReceivedWithTenantId(string signalName, string tenantId)
        {
            commandExecutor.Execute(new SignalEventReceivedCmd(signalName, null, null, tenantId));
        }

        ///<inheritdoc />
        public virtual void SignalEventReceivedAsync(string signalName)
        {
            commandExecutor.Execute(new SignalEventReceivedCmd(signalName, null, true, null));
        }

        ///<inheritdoc />
        public virtual void SignalEventReceivedAsyncWithTenantId(string signalName, string tenantId)
        {
            commandExecutor.Execute(new SignalEventReceivedCmd(signalName, null, true, tenantId));
        }

        ///<inheritdoc />
        public virtual void SignalEventReceived(string signalName, IDictionary<string, object> processVariables)
        {
            commandExecutor.Execute(new SignalEventReceivedCmd(signalName, null, processVariables, null));
        }

        ///<inheritdoc />
        public virtual void SignalEventReceivedWithTenantId(string signalName, IDictionary<string, object> processVariables, string tenantId)
        {
            commandExecutor.Execute(new SignalEventReceivedCmd(signalName, null, processVariables, tenantId));
        }

        ///<inheritdoc />
        public virtual void SignalEventReceived(string signalName, string executionId)
        {
            commandExecutor.Execute(new SignalEventReceivedCmd(signalName, executionId, null, null));
        }

        ///<inheritdoc />
        public virtual void SignalEventReceived(string signalName, string executionId, IDictionary<string, object> processVariables)
        {
            commandExecutor.Execute(new SignalEventReceivedCmd(signalName, executionId, processVariables, null));
        }

        ///<inheritdoc />
        public virtual void SignalEventReceivedAsync(string signalName, string executionId)
        {
            commandExecutor.Execute(new SignalEventReceivedCmd(signalName, executionId, true, null));
        }

        ///<inheritdoc />
        public virtual void MessageEventReceived(string messageName, string executionId)
        {
            commandExecutor.Execute(new MessageEventReceivedCmd(messageName, executionId, null));
        }

        ///<inheritdoc />
        public virtual void MessageEventReceived(string messageName, string executionId, IDictionary<string, object> processVariables)
        {
            commandExecutor.Execute(new MessageEventReceivedCmd(messageName, executionId, processVariables));
        }

        ///<inheritdoc />
        public virtual void MessageEventReceivedAsync(string messageName, string executionId)
        {
            commandExecutor.Execute(new MessageEventReceivedCmd(messageName, executionId, true));
        }

        ///<inheritdoc />
        public virtual void AddEventListener(IActivitiEventListener listenerToAdd)
        {
            commandExecutor.Execute(new AddEventListenerCommand(listenerToAdd));
        }

        ///<inheritdoc />
        public virtual void AddEventListener(IActivitiEventListener listenerToAdd, params ActivitiEventType[] types)
        {
            commandExecutor.Execute(new AddEventListenerCommand(listenerToAdd, types));
        }

        ///<inheritdoc />
        public virtual void RemoveEventListener(IActivitiEventListener listenerToRemove)
        {
            commandExecutor.Execute(new RemoveEventListenerCommand(listenerToRemove));
        }

        ///<inheritdoc />
        public virtual void DispatchEvent(IActivitiEvent @event)
        {
            commandExecutor.Execute(new DispatchEventCommand(@event));
        }

        ///<inheritdoc />
        public virtual void SetProcessInstanceName(string processInstanceId, string name)
        {
            commandExecutor.Execute(new SetProcessInstanceNameCmd(processInstanceId, name));
        }

        ///<inheritdoc />
        public virtual IList<Event> GetProcessInstanceEvents(string processInstanceId)
        {
            return commandExecutor.Execute(new GetProcessInstanceEventsCmd(processInstanceId)) as IList<Event>;
        }

        ///<inheritdoc />
        public virtual IList<FlowNode> GetEnabledActivitiesFromAdhocSubProcess(string executionId)
        {
            return commandExecutor.Execute(new GetEnabledActivitiesForAdhocSubProcessCmd(executionId));
        }

        ///<inheritdoc />
        public virtual IExecution ExecuteActivityInAdhocSubProcess(string executionId, string activityId)
        {
            return commandExecutor.Execute(new ExecuteActivityForAdhocSubProcessCmd(executionId, activityId));
        }

        ///<inheritdoc />
        public virtual void CompleteAdhocSubProcess(string executionId)
        {
            commandExecutor.Execute(new CompleteAdhocSubProcessCmd(executionId));
        }

        ///<inheritdoc />
        public virtual IProcessInstanceBuilder CreateProcessInstanceBuilder()
        {
            return new ProcessInstanceBuilderImpl(this);
        }

        ///<inheritdoc />
        public virtual IProcessInstance StartProcessInstance(ProcessInstanceBuilderImpl processInstanceBuilder)
        {
            if (string.IsNullOrWhiteSpace(processInstanceBuilder.MessageName) == false)
            {
                return commandExecutor.Execute(new StartProcessInstanceByMessageCmd(processInstanceBuilder));
            }
            else if (processInstanceBuilder.ProcessDefinitionId is not null || processInstanceBuilder.ProcessDefinitionKey is not null)
            {
                return commandExecutor.Execute(new StartProcessInstanceCmd(processInstanceBuilder));
            }
            else
            {
                throw new ActivitiIllegalArgumentException("No processDefinitionId, processDefinitionKey nor messageName provided");
            }
        }

        /// <inheritdoc />

        public IProcessInstance[] StartProcessInstanceByCmd(IStartProcessInstanceCmd[] cmds)
        {
            var instances = new List<IProcessInstance>(cmds.Length);
            foreach (var cmd in cmds)
            {
                var scmd = new StartProcessInstanceCmd(cmd as IStartProcessInstanceCmd);

                var instance = commandExecutor.Execute(scmd);

                instances.Add(instance);
            }

            return instances.ToArray();
        }

        /// <inheritdoc />
        public async Task<IProcessInstance[]> StartProcessInstanceByCmdAsync(IStartProcessInstanceCmd[] cmds)
        {
            IEnumerable<Task<IProcessInstance>> tasks = Enumerable.Select(cmds, (cmd) =>
            {
                TaskCompletionSource<IProcessInstance> tsc = new TaskCompletionSource<IProcessInstance>();

                Task.Factory.StartNew((command) =>
                {
                    var instance = StartProcessInstanceByCmd(command as IStartProcessInstanceCmd);

                    tsc.TrySetResult(instance);
                }, cmd);

                return tsc.Task;
            });

            //var instances = new List<IProcessInstance>(cmds.Length);
            //List<Task> tasks = new List<Task>();
            //foreach (var cmd in cmds)
            //{
            //    tasks.Add(Task.Factory.StartNew((command) =>
            //    {
            //        var instance = StartProcessInstanceByCmd(command as IStartProcessInstanceCmd);

            //        instances.Add(instance);
            //    }, cmd));
            //}

            IProcessInstance[] instances = await Task.WhenAll(tasks.ToArray()).ConfigureAwait(false);

            return instances;
        }

        /// <inheritdoc />

        public IProcessInstance StartProcessInstanceByCmd(IStartProcessInstanceCmd cmd)
        {
            IProcessInstance[] ins = StartProcessInstanceByCmd(new IStartProcessInstanceCmd[] { cmd });

            return ins[0];
        }
    }

}