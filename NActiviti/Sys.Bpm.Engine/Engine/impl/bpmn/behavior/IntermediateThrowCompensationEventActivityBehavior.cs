using System;
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

namespace org.activiti.engine.impl.bpmn.behavior
{

    using org.activiti.bpmn.model;
    using org.activiti.engine.impl.bpmn.helper;
    using org.activiti.engine.impl.context;
    using org.activiti.engine.impl.interceptor;
    using org.activiti.engine.impl.persistence.entity;
    using org.activiti.engine.impl.util;

    /// 
    /// 
    [Serializable]
    public class IntermediateThrowCompensationEventActivityBehavior : FlowNodeActivityBehavior
    {

        private const long serialVersionUID = 1L;

        protected internal readonly CompensateEventDefinition compensateEventDefinition;

        public IntermediateThrowCompensationEventActivityBehavior(CompensateEventDefinition compensateEventDefinition)
        {
            this.compensateEventDefinition = compensateEventDefinition;
        }

        public override void Execute(IExecutionEntity execution)
        {
            ThrowEvent throwEvent = (ThrowEvent)execution.CurrentFlowElement;

            /*
             * From the BPMN 2.0 spec:
             * 
             * The Activity to be compensated MAY be supplied.
             *  
             * If an Activity is not supplied, then the compensation is broadcast to all completed Activities in 
             * the current Sub- Process (if present), or the entire Process instance (if at the global level). This "throws" the compensation.
             */
            string activityRef = compensateEventDefinition.ActivityRef;

            ICommandContext commandContext = Context.CommandContext;
            IEventSubscriptionEntityManager eventSubscriptionEntityManager = commandContext.EventSubscriptionEntityManager;

            IList<ICompensateEventSubscriptionEntity> eventSubscriptions = new List<ICompensateEventSubscriptionEntity>();
            if (!string.IsNullOrWhiteSpace(activityRef))
            {
                // If an activity ref is provided, only that activity is compensated
                ((List<ICompensateEventSubscriptionEntity>)eventSubscriptions).AddRange(eventSubscriptionEntityManager.FindCompensateEventSubscriptionsByProcessInstanceIdAndActivityId(execution.ProcessInstanceId, activityRef));
            }
            else
            {
                // If no activity ref is provided, it is broadcast to the current sub process / process instance
                Process process = ProcessDefinitionUtil.GetProcess(execution.ProcessDefinitionId);

                IFlowElementsContainer flowElementsContainer;
                if (throwEvent.SubProcess == null)
                {
                    flowElementsContainer = process;
                }
                else
                {
                    flowElementsContainer = throwEvent.SubProcess;
                }

                foreach (FlowElement flowElement in flowElementsContainer.FlowElements)
                {
                    if (flowElement is Activity)
                    {
                        ((List<ICompensateEventSubscriptionEntity>)eventSubscriptions).AddRange(eventSubscriptionEntityManager.FindCompensateEventSubscriptionsByProcessInstanceIdAndActivityId(execution.ProcessInstanceId, flowElement.Id));
                    }
                }

            }

            if (eventSubscriptions.Count == 0)
            {
                Leave(execution);
            }
            else
            {
                // TODO: implement async (waitForCompletion=false in bpmn)
                ScopeUtil.ThrowCompensationEvent(eventSubscriptions, execution, false);
                Leave(execution);
            }
        }
    }

}