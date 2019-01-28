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
    using org.activiti.engine.impl.interceptor;
    using org.activiti.engine.impl.persistence.entity;
    using org.activiti.engine.impl.util;
    using org.activiti.engine.repository;

    /// 
    /// 
    public class SignalEventHandler : AbstractEventHandler
    {

        public const string EVENT_HANDLER_TYPE = "signal";

        public override string EventHandlerType
        {
            get
            {
                return EVENT_HANDLER_TYPE;
            }
        }

        public override void handleEvent(IEventSubscriptionEntity eventSubscription, object payload, ICommandContext commandContext)
        {
            if (!ReferenceEquals(eventSubscription.ExecutionId, null))
            {

                base.handleEvent(eventSubscription, payload, commandContext);

            }
            else if (!ReferenceEquals(eventSubscription.ProcessDefinitionId, null))
            {

                // Find initial flow element matching the signal start event
                string processDefinitionId = eventSubscription.ProcessDefinitionId;
                IProcessDefinition processDefinition = ProcessDefinitionUtil.getProcessDefinition(processDefinitionId);

                if (processDefinition == null)
                {
                    throw new ActivitiObjectNotFoundException("No process definition found for id '" + processDefinitionId + "'", typeof(IProcessDefinition));
                }

                if (processDefinition.Suspended)
                {
                    throw new ActivitiException("Could not handle signal: process definition with id: " + processDefinitionId + " is suspended");
                }

                org.activiti.bpmn.model.Process process = ProcessDefinitionUtil.getProcess(processDefinitionId);
                FlowElement flowElement = process.getFlowElement(eventSubscription.ActivityId, true);
                if (flowElement == null)
                {
                    throw new ActivitiException("Could not find matching FlowElement for activityId " + eventSubscription.ActivityId);
                }

                // Start process instance via that flow element
                IDictionary<string, object> variables = null;
                if (payload is System.Collections.IDictionary)
                {
                    variables = (IDictionary<string, object>)payload;
                }
                ProcessInstanceHelper processInstanceHelper = commandContext.ProcessEngineConfiguration.ProcessInstanceHelper;
                processInstanceHelper.createAndStartProcessInstanceWithInitialFlowElement(processDefinition, null, null, flowElement, process, variables, null, true);

            }
            else
            {
                throw new ActivitiException("Invalid signal handling: no execution nor process definition set");
            }
        }

    }

}