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

namespace org.activiti.engine.impl.@event
{
    using org.activiti.bpmn.model;
    using org.activiti.engine.@delegate.@event;
    using org.activiti.engine.@delegate.@event.impl;
    using org.activiti.engine.impl.bpmn.helper;
    using org.activiti.engine.impl.context;
    using org.activiti.engine.impl.interceptor;
    using org.activiti.engine.impl.persistence.entity;
    using org.activiti.engine.impl.util;
    using System;

    /// 
    public class CompensationEventHandler : IEventHandler
    {

        public virtual string EventHandlerType
        {
            get
            {
                return CompensateEventSubscriptionEntity_Fields.EVENT_TYPE;
            }
        }

        public virtual void handleEvent(IEventSubscriptionEntity eventSubscription, object payload, ICommandContext commandContext)
        {

            string configuration = eventSubscription.Configuration;
            if (ReferenceEquals(configuration, null))
            {
                throw new ActivitiException("Compensating execution not set for compensate event subscription with id " + eventSubscription.Id);
            }

            IExecutionEntity compensatingExecution = commandContext.ExecutionEntityManager.findById<IExecutionEntity>(configuration);

            string processDefinitionId = compensatingExecution.ProcessDefinitionId;
            Process process = ProcessDefinitionUtil.getProcess(processDefinitionId);
            if (process == null)
            {
                throw new ActivitiException("Cannot start process instance. Process model (id = " + processDefinitionId + ") could not be found");
            }

            FlowElement flowElement = process.getFlowElement(eventSubscription.ActivityId, true);

            if (flowElement is SubProcess && !((SubProcess)flowElement).ForCompensation)
            {

                // descend into scope:
                compensatingExecution.IsScope = true;
                IList<ICompensateEventSubscriptionEntity> eventsForThisScope = commandContext.EventSubscriptionEntityManager.findCompensateEventSubscriptionsByExecutionId(compensatingExecution.Id);
                ScopeUtil.throwCompensationEvent(eventsForThisScope, compensatingExecution, false);

            }
            else
            {

                try
                {

                    if (commandContext.ProcessEngineConfiguration.EventDispatcher.Enabled)
                    {
                        commandContext.ProcessEngineConfiguration.EventDispatcher.dispatchEvent(ActivitiEventBuilder.createActivityEvent(ActivitiEventType.ACTIVITY_COMPENSATE, flowElement.Id, flowElement.Name, compensatingExecution.Id, compensatingExecution.ProcessInstanceId, compensatingExecution.ProcessDefinitionId, flowElement));
                    }
                    compensatingExecution.CurrentFlowElement = flowElement;
                    Context.Agenda.planContinueProcessInCompensation(compensatingExecution);

                }
                catch (Exception e)
                {
                    throw new ActivitiException("Error while handling compensation event " + eventSubscription, e);
                }

            }
        }

    }

}