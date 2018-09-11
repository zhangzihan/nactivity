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
namespace org.activiti.engine.impl.util
{
    using org.activiti.bpmn.model;
    using org.activiti.engine;
    using org.activiti.engine.@delegate.@event;
    using org.activiti.engine.@delegate.@event.impl;
    using org.activiti.engine.impl.context;
    using org.activiti.engine.impl.interceptor;
    using org.activiti.engine.impl.persistence.entity;
    using org.activiti.engine.repository;
    using org.activiti.engine.runtime;


    /// 
    /// 
    public class ProcessInstanceHelper
    {

        public virtual IProcessInstance createProcessInstance(IProcessDefinitionEntity processDefinition, string businessKey, string processInstanceName, IDictionary<string, object> variables, IDictionary<string, object> transientVariables)
        {

            return createAndStartProcessInstance(processDefinition, businessKey, processInstanceName, variables, transientVariables, false);
        }

        public virtual IProcessInstance createAndStartProcessInstance(IProcessDefinition processDefinition, string businessKey, string processInstanceName, IDictionary<string, object> variables, IDictionary<string, object> transientVariables)
        {

            return createAndStartProcessInstance(processDefinition, businessKey, processInstanceName, variables, transientVariables, true);
        }

        protected internal virtual IProcessInstance createAndStartProcessInstance(IProcessDefinition processDefinition, string businessKey, string processInstanceName, IDictionary<string, object> variables, IDictionary<string, object> transientVariables, bool startProcessInstance)
        {

            ICommandContext commandContext = Context.CommandContext; // Todo: ideally, context should be passed here

            // Do not start process a process instance if the process definition is suspended
            if (ProcessDefinitionUtil.isProcessDefinitionSuspended(processDefinition.Id))
            {
                throw new ActivitiException("Cannot start process instance. Process definition " + processDefinition.Name + " (id = " + processDefinition.Id + ") is suspended");
            }

            // Get model from cache
            Process process = ProcessDefinitionUtil.getProcess(processDefinition.Id);
            if (process == null)
            {
                throw new ActivitiException("Cannot start process instance. Process model " + processDefinition.Name + " (id = " + processDefinition.Id + ") could not be found");
            }

            FlowElement initialFlowElement = process.InitialFlowElement;
            if (initialFlowElement == null)
            {
                throw new ActivitiException("No start element found for process definition " + processDefinition.Id);
            }

            return createAndStartProcessInstanceWithInitialFlowElement(processDefinition, businessKey, processInstanceName, initialFlowElement, process, variables, transientVariables, startProcessInstance);
        }

        public virtual IProcessInstance createAndStartProcessInstanceByMessage(IProcessDefinition processDefinition, string messageName, IDictionary<string, object> variables, IDictionary<string, object> transientVariables)
        {

            ICommandContext commandContext = Context.CommandContext;
            // Do not start process a process instance if the process definition is suspended
            if (ProcessDefinitionUtil.isProcessDefinitionSuspended(processDefinition.Id))
            {
                throw new ActivitiException("Cannot start process instance. Process definition " + processDefinition.Name + " (id = " + processDefinition.Id + ") is suspended");
            }

            // Get model from cache
            Process process = ProcessDefinitionUtil.getProcess(processDefinition.Id);
            if (process == null)
            {
                throw new ActivitiException("Cannot start process instance. Process model " + processDefinition.Name + " (id = " + processDefinition.Id + ") could not be found");
            }

            FlowElement initialFlowElement = null;
            foreach (FlowElement flowElement in process.FlowElements)
            {
                if (flowElement is StartEvent)
                {
                    StartEvent startEvent = (StartEvent)flowElement;
                    if (CollectionUtil.IsNotEmpty(startEvent.EventDefinitions) && startEvent.EventDefinitions[0] is MessageEventDefinition)
                    {

                        MessageEventDefinition messageEventDefinition = (MessageEventDefinition)startEvent.EventDefinitions[0];
                        if (messageEventDefinition.MessageRef.Equals(messageName))
                        {
                            initialFlowElement = flowElement;
                            break;
                        }
                    }
                }
            }
            if (initialFlowElement == null)
            {
                throw new ActivitiException("No message start event found for process definition " + processDefinition.Id + " and message name " + messageName);
            }

            return createAndStartProcessInstanceWithInitialFlowElement(processDefinition, null, null, initialFlowElement, process, variables, transientVariables, true);
        }

        public virtual IProcessInstance createAndStartProcessInstanceWithInitialFlowElement(IProcessDefinition processDefinition, string businessKey, string processInstanceName, FlowElement initialFlowElement, Process process, IDictionary<string, object> variables, IDictionary<string, object> transientVariables, bool startProcessInstance)
        {

            ICommandContext commandContext = Context.CommandContext;

            // Create the process instance
            string initiatorVariableName = null;
            if (initialFlowElement is StartEvent)
            {
                initiatorVariableName = ((StartEvent)initialFlowElement).Initiator;
            }

            IExecutionEntity processInstance = commandContext.ExecutionEntityManager.createProcessInstanceExecution(processDefinition, businessKey, processDefinition.TenantId, initiatorVariableName);

            commandContext.HistoryManager.recordProcessInstanceStart(processInstance, initialFlowElement);

            processInstance.Variables = processDataObjects(process.DataObjects);

            // Set the variables passed into the start command
            if (variables != null)
            {
                foreach (string varName in variables.Keys)
                {
                    processInstance.setVariable(varName, variables[varName]);
                }
            }
            if (transientVariables != null)
            {
                foreach (string varName in transientVariables.Keys)
                {
                    processInstance.setTransientVariable(varName, transientVariables[varName]);
                }
            }

            // Set processInstance name
            if (!string.ReferenceEquals(processInstanceName, null))
            {
                processInstance.Name = processInstanceName;
                commandContext.HistoryManager.recordProcessInstanceNameChange(processInstance.Id, processInstanceName);
            }

            // Fire events
            if (Context.ProcessEngineConfiguration.EventDispatcher.Enabled)
            {
                Context.ProcessEngineConfiguration.EventDispatcher.dispatchEvent(ActivitiEventBuilder.createEntityWithVariablesEvent(ActivitiEventType.ENTITY_INITIALIZED, processInstance, variables, false));
            }

            // Create the first execution that will visit all the process definition elements
            IExecutionEntity execution = commandContext.ExecutionEntityManager.createChildExecution(processInstance);
            execution.CurrentFlowElement = initialFlowElement;

            if (startProcessInstance)
            {
                this.startProcessInstance(processInstance, commandContext, variables);
            }

            return processInstance;
        }

        public virtual void startProcessInstance(IExecutionEntity processInstance, ICommandContext commandContext, IDictionary<string, object> variables)
        {

            Process process = ProcessDefinitionUtil.getProcess(processInstance.ProcessDefinitionId);


            // Event sub process handling
            IList<IMessageEventSubscriptionEntity> messageEventSubscriptions = new List<IMessageEventSubscriptionEntity>();
            foreach (FlowElement flowElement in process.FlowElements)
            {
                if (flowElement is EventSubProcess)
                {
                    EventSubProcess eventSubProcess = (EventSubProcess)flowElement;
                    foreach (FlowElement subElement in eventSubProcess.FlowElements)
                    {
                        if (subElement is StartEvent)
                        {
                            StartEvent startEvent = (StartEvent)subElement;
                            if (CollectionUtil.IsNotEmpty(startEvent.EventDefinitions))
                            {
                                EventDefinition eventDefinition = startEvent.EventDefinitions[0];
                                if (eventDefinition is MessageEventDefinition)
                                {
                                    MessageEventDefinition messageEventDefinition = (MessageEventDefinition)eventDefinition;
                                    BpmnModel bpmnModel = ProcessDefinitionUtil.getBpmnModel(processInstance.ProcessDefinitionId);
                                    if (bpmnModel.containsMessageId(messageEventDefinition.MessageRef))
                                    {
                                        messageEventDefinition.MessageRef = bpmnModel.getMessage(messageEventDefinition.MessageRef).Name;
                                    }
                                    IExecutionEntity messageExecution = commandContext.ExecutionEntityManager.createChildExecution(processInstance);
                                    messageExecution.CurrentFlowElement = startEvent;
                                    messageExecution.IsEventScope = true;
                                    messageEventSubscriptions.Add(commandContext.EventSubscriptionEntityManager.insertMessageEvent(messageEventDefinition.MessageRef, messageExecution));
                                }
                            }
                        }
                    }
                }
            }

            IExecutionEntity execution = processInstance.Executions[0]; // There will always be one child execution created
            commandContext.Agenda.planContinueProcessOperation(execution);

            if (Context.ProcessEngineConfiguration.EventDispatcher.Enabled)
            {
                IActivitiEventDispatcher eventDispatcher = Context.ProcessEngineConfiguration.EventDispatcher;
                eventDispatcher.dispatchEvent(ActivitiEventBuilder.createProcessStartedEvent(execution, variables, false));

                foreach (IMessageEventSubscriptionEntity messageEventSubscription in messageEventSubscriptions)
                {
                    commandContext.ProcessEngineConfiguration.EventDispatcher.dispatchEvent(ActivitiEventBuilder.createMessageEvent(ActivitiEventType.ACTIVITY_MESSAGE_WAITING, messageEventSubscription.ActivityId, messageEventSubscription.EventName, null, messageEventSubscription.Execution.Id, messageEventSubscription.ProcessInstanceId, messageEventSubscription.ProcessDefinitionId));
                }
            }
        }

        protected internal virtual IDictionary<string, object> processDataObjects(ICollection<ValuedDataObject> dataObjects)
        {
            IDictionary<string, object> variablesMap = new Dictionary<string, object>();
            // convert data objects to process variables
            if (dataObjects != null)
            {
                foreach (ValuedDataObject dataObject in dataObjects)
                {
                    variablesMap[dataObject.Name] = dataObject.Value;
                }
            }
            return variablesMap;
        }
    }

}