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

namespace Sys.Workflow.Engine.Impl.Events
{

    using Sys.Workflow.Bpmn.Models;
    using Sys.Workflow.Engine.Impl.Interceptor;
    using Sys.Workflow.Engine.Impl.Persistence.Entity;
    using Sys.Workflow.Engine.Impl.Util;
    using Sys.Workflow.Engine.Repository;

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

        public override void HandleEvent(IEventSubscriptionEntity eventSubscription, object payload, ICommandContext commandContext)
        {
            if (eventSubscription.ExecutionId is not null)
            {

                base.HandleEvent(eventSubscription, payload, commandContext);

            }
            else if (eventSubscription.ProcessDefinitionId is not null)
            {
                // Find initial flow element matching the signal start event
                string processDefinitionId = eventSubscription.ProcessDefinitionId;
                IProcessDefinition processDefinition = ProcessDefinitionUtil.GetProcessDefinition(processDefinitionId);

                if (processDefinition is null)
                {
                    throw new ActivitiObjectNotFoundException("No process definition found for id '" + processDefinitionId + "'", typeof(IProcessDefinition));
                }

                if (processDefinition.Suspended)
                {
                    throw new ActivitiException("Could not handle signal: process definition with id: " + processDefinitionId + " is suspended");
                }

                Process process = ProcessDefinitionUtil.GetProcess(processDefinitionId);
                FlowElement flowElement = process.GetFlowElement(eventSubscription.ActivityId, true);
                if (flowElement is null)
                {
                    throw new ActivitiException("Could not find matching FlowElement for activityId " + eventSubscription.ActivityId);
                }

                // Start process instance via that flow element
                IDictionary<string, object> variables = null;
                if (payload is System.Collections.IDictionary)
                {
                    variables = (IDictionary<string, object>)payload;
                }
                var processInstanceHelper = commandContext.ProcessEngineConfiguration.ProcessInstanceHelper;
                processInstanceHelper.CreateAndStartProcessInstanceWithInitialFlowElement(processDefinition, null, null, flowElement, process, variables, null, true);

            }
            else
            {
                throw new ActivitiException("Invalid signal handling: no execution nor process definition set");
            }
        }

    }

}