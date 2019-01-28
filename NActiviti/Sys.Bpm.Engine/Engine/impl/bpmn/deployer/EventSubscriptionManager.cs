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
namespace org.activiti.engine.impl.bpmn.deployer
{

    using org.activiti.bpmn.model;
    using org.activiti.engine.impl.context;
    using org.activiti.engine.impl.@event;
    using org.activiti.engine.impl.interceptor;
    using org.activiti.engine.impl.persistence.entity;
    using org.activiti.engine.impl.util;

    /// <summary>
    /// Manages event subscriptions for newly-deployed process definitions and their previous versions.
    /// </summary>
    public class EventSubscriptionManager
    {

        protected internal virtual void removeObsoleteEventSubscriptionsImpl(IProcessDefinitionEntity processDefinition, string eventHandlerType)
        {
            // remove all subscriptions for the previous version
            IEventSubscriptionEntityManager eventSubscriptionEntityManager = Context.CommandContext.EventSubscriptionEntityManager;
            IList<IEventSubscriptionEntity> subscriptionsToDelete = eventSubscriptionEntityManager.findEventSubscriptionsByTypeAndProcessDefinitionId(eventHandlerType, processDefinition.Id, processDefinition.TenantId);

            foreach (IEventSubscriptionEntity eventSubscriptionEntity in subscriptionsToDelete)
            {
                eventSubscriptionEntityManager.delete(eventSubscriptionEntity);
            }
        }

        protected internal virtual void removeObsoleteMessageEventSubscriptions(IProcessDefinitionEntity previousProcessDefinition)
        {
            // remove all subscriptions for the previous version
            if (previousProcessDefinition != null)
            {
                removeObsoleteEventSubscriptionsImpl(previousProcessDefinition, MessageEventHandler.EVENT_HANDLER_TYPE);
            }
        }

        protected internal virtual void removeObsoleteSignalEventSubScription(IProcessDefinitionEntity previousProcessDefinition)
        {
            // remove all subscriptions for the previous version
            if (previousProcessDefinition != null)
            {
                removeObsoleteEventSubscriptionsImpl(previousProcessDefinition, SignalEventHandler.EVENT_HANDLER_TYPE);
            }
        }

        protected internal virtual void addMessageEventSubscriptions(IProcessDefinitionEntity processDefinition, Process process, BpmnModel bpmnModel)
        {
            if (CollectionUtil.IsNotEmpty(process.FlowElements))
            {
                foreach (FlowElement element in process.FlowElements)
                {
                    if (element is StartEvent)
                    {
                        StartEvent startEvent = (StartEvent)element;
                        if (CollectionUtil.IsNotEmpty(startEvent.EventDefinitions))
                        {
                            EventDefinition eventDefinition = startEvent.EventDefinitions[0];
                            if (eventDefinition is MessageEventDefinition)
                            {
                                MessageEventDefinition messageEventDefinition = (MessageEventDefinition)eventDefinition;
                                insertMessageEvent(messageEventDefinition, startEvent, processDefinition, bpmnModel);
                            }
                        }
                    }
                }
            }
        }

        protected internal virtual void insertMessageEvent(MessageEventDefinition messageEventDefinition, StartEvent startEvent, IProcessDefinitionEntity processDefinition, BpmnModel bpmnModel)
        {
            ICommandContext commandContext = Context.CommandContext;
            if (bpmnModel.containsMessageId(messageEventDefinition.MessageRef))
            {
                Message message = bpmnModel.getMessage(messageEventDefinition.MessageRef);
                messageEventDefinition.MessageRef = message.Name;
            }

            // look for subscriptions for the same name in db:
            IList<IEventSubscriptionEntity> subscriptionsForSameMessageName = commandContext.EventSubscriptionEntityManager.findEventSubscriptionsByName(MessageEventHandler.EVENT_HANDLER_TYPE, messageEventDefinition.MessageRef, processDefinition.TenantId);

            foreach (IEventSubscriptionEntity eventSubscriptionEntity in subscriptionsForSameMessageName)
            {
                // throw exception only if there's already a subscription as start event
                if (ReferenceEquals(eventSubscriptionEntity.ProcessInstanceId, null) || eventSubscriptionEntity.ProcessInstanceId.Length == 0)
                { 
                    // processInstanceId != null or not empty -> it's a message related to an execution
                    // the event subscription has no instance-id, so it's a message start event
                    throw new ActivitiException("Cannot deploy process definition '" + processDefinition.ResourceName + "': there already is a message event subscription for the message with name '" + messageEventDefinition.MessageRef + "'.");
                }
            }

            IMessageEventSubscriptionEntity newSubscription = commandContext.EventSubscriptionEntityManager.createMessageEventSubscription();
            newSubscription.EventName = messageEventDefinition.MessageRef;
            newSubscription.ActivityId = startEvent.Id;
            newSubscription.Configuration = processDefinition.Id;
            newSubscription.ProcessDefinitionId = processDefinition.Id;

            if (!ReferenceEquals(processDefinition.TenantId, null))
            {
                newSubscription.TenantId = processDefinition.TenantId;
            }

            commandContext.EventSubscriptionEntityManager.insert(newSubscription);
        }

        protected internal virtual void addSignalEventSubscriptions(ICommandContext commandContext, IProcessDefinitionEntity processDefinition, Process process, BpmnModel bpmnModel)
        {
            if (CollectionUtil.IsNotEmpty(process.FlowElements))
            {
                foreach (FlowElement element in process.FlowElements)
                {
                    if (element is StartEvent)
                    {
                        StartEvent startEvent = (StartEvent)element;
                        if (CollectionUtil.IsNotEmpty(startEvent.EventDefinitions))
                        {
                            EventDefinition eventDefinition = startEvent.EventDefinitions[0];
                            if (eventDefinition is SignalEventDefinition)
                            {
                                SignalEventDefinition signalEventDefinition = (SignalEventDefinition)eventDefinition;
                                ISignalEventSubscriptionEntity subscriptionEntity = commandContext.EventSubscriptionEntityManager.createSignalEventSubscription();
                                Signal signal = bpmnModel.getSignal(signalEventDefinition.SignalRef);
                                if (signal != null)
                                {
                                    subscriptionEntity.EventName = signal.Name;
                                }
                                else
                                {
                                    subscriptionEntity.EventName = signalEventDefinition.SignalRef;
                                }
                                subscriptionEntity.ActivityId = startEvent.Id;
                                subscriptionEntity.ProcessDefinitionId = processDefinition.Id;
                                if (!ReferenceEquals(processDefinition.TenantId, null))
                                {
                                    subscriptionEntity.TenantId = processDefinition.TenantId;
                                }

                                Context.CommandContext.EventSubscriptionEntityManager.insert(subscriptionEntity);
                            }
                        }
                    }
                }
            }
        }
    }


}