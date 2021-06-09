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

namespace Sys.Workflow.Engine.Impl.Bpmn.Behavior
{

    using Sys.Workflow.Bpmn.Models;
    using Sys.Workflow.Engine.Impl.Bpmn.Helper;
    using Sys.Workflow.Engine.Impl.Contexts;
    using Sys.Workflow.Engine.Impl.Interceptor;
    using Sys.Workflow.Engine.Impl.Persistence.Entity;
    using Sys.Workflow.Engine.Impl.Util;

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

            List<ICompensateEventSubscriptionEntity> eventSubscriptions = new List<ICompensateEventSubscriptionEntity>();
            if (!string.IsNullOrWhiteSpace(activityRef))
            {
                // If an activity ref is provided, only that activity is compensated
                eventSubscriptions.AddRange(eventSubscriptionEntityManager.FindCompensateEventSubscriptionsByProcessInstanceIdAndActivityId(execution.ProcessInstanceId, activityRef));
            }
            else
            {
                eventSubscriptions.AddRange(eventSubscriptionEntityManager.FindCompensateEventSubscriptionsByProcessInstanceIdAndActivityId(execution.ProcessInstanceId, null));

                // If no activity ref is provided, it is broadcast to the current sub process / process instance
                //IFlowElementsContainer process = ProcessDefinitionUtil.GetProcess(execution.ProcessDefinitionId);

                //IFlowElementsContainer flowElementsContainer;
                //if (throwEvent.SubProcess is null)
                //{
                //    flowElementsContainer = process;
                //}
                //else
                //{
                //    flowElementsContainer = throwEvent.SubProcess;
                //}

                ////if (flowElementsContainer != process)
                ////{
                ////    foreach (FlowElement flowElement in process.FlowElements)
                ////    {
                ////        if (flowElement is Activity)
                ////        {
                ////            IList<ICompensateEventSubscriptionEntity> compensateEvents = eventSubscriptionEntityManager.FindCompensateEventSubscriptionsByProcessInstanceIdAndActivityId(execution.ProcessInstanceId, flowElement.Id);

                ////            eventSubscriptions.AddRange(compensateEvents);
                ////        }
                ////    }
                ////}

                //foreach (FlowElement flowElement in flowElementsContainer.FlowElements)
                //{
                //    if (flowElement is Activity)
                //    {
                //        IList<ICompensateEventSubscriptionEntity> compensateEvents = eventSubscriptionEntityManager.FindCompensateEventSubscriptionsByProcessInstanceIdAndActivityId(execution.ProcessInstanceId, flowElement.Id);

                //        eventSubscriptions.AddRange(compensateEvents);
                //    }
                //}
            }

            Leave(execution);

            if (eventSubscriptions.Count > 0)
            {
                // TODO: implement async (waitForCompletion=false in bpmn)
                ScopeUtil.ThrowCompensationEvent(eventSubscriptions, execution, false);
            }
        }
    }
}