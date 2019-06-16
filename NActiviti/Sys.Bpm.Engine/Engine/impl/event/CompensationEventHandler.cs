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

namespace Sys.Workflow.engine.impl.@event
{
    using Sys.Workflow.bpmn.model;
    using Sys.Workflow.engine.@delegate.@event;
    using Sys.Workflow.engine.@delegate.@event.impl;
    using Sys.Workflow.engine.impl.bpmn.helper;
    using Sys.Workflow.engine.impl.context;
    using Sys.Workflow.engine.impl.interceptor;
    using Sys.Workflow.engine.impl.persistence.entity;
    using Sys.Workflow.engine.impl.util;
    using System;

    /// 
    public class CompensationEventHandler : IEventHandler
    {
        public virtual string EventHandlerType
        {
            get
            {
                return CompensateEventSubscriptionEntityFields.EVENT_TYPE;
            }
        }

        public virtual void HandleEvent(IEventSubscriptionEntity eventSubscription, object payload, ICommandContext commandContext)
        {
            string configuration = eventSubscription.Configuration;
            if (configuration is null)
            {
                throw new ActivitiException("Compensating execution not set for compensate event subscription with id " + eventSubscription.Id);
            }

            IExecutionEntity compensatingExecution = commandContext.ExecutionEntityManager.FindById<IExecutionEntity>(configuration);

            string processDefinitionId = compensatingExecution.ProcessDefinitionId;
            Process process = ProcessDefinitionUtil.GetProcess(processDefinitionId);
            if (process == null)
            {
                throw new ActivitiException("Cannot start process instance. Process model (id = " + processDefinitionId + ") could not be found");
            }

            FlowElement flowElement = process.GetFlowElement(eventSubscription.ActivityId, true);

            if (flowElement is SubProcess && !((SubProcess)flowElement).ForCompensation)
            {

                // descend into scope:
                compensatingExecution.IsScope = true;
                IList<ICompensateEventSubscriptionEntity> eventsForThisScope = commandContext.EventSubscriptionEntityManager.FindCompensateEventSubscriptionsByExecutionId(compensatingExecution.Id);
                ScopeUtil.ThrowCompensationEvent(eventsForThisScope, compensatingExecution, false);
            }
            else
            {
                try
                {
                    if (commandContext.ProcessEngineConfiguration.EventDispatcher.Enabled)
                    {
                        commandContext.ProcessEngineConfiguration.EventDispatcher.DispatchEvent(ActivitiEventBuilder.CreateActivityEvent(ActivitiEventType.ACTIVITY_COMPENSATE, flowElement.Id, flowElement.Name, compensatingExecution.Id, compensatingExecution.ProcessInstanceId, compensatingExecution.ProcessDefinitionId, flowElement));
                    }
                    compensatingExecution.CurrentFlowElement = flowElement;
                    Context.Agenda.PlanContinueProcessInCompensation(compensatingExecution);

                }
                catch (Exception e)
                {
                    throw new ActivitiException("Error while handling compensation event " + eventSubscription, e);
                }
            }
        }
    }
}