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
namespace Sys.Workflow.engine.impl.bpmn.behavior
{

    using Sys.Workflow.bpmn.model;
    using Sys.Workflow.engine.@delegate;
    using Sys.Workflow.engine.impl.context;
    using Sys.Workflow.engine.impl.persistence.entity;
    using Sys.Workflow.engine.impl.util;

    /// 
    [Serializable]
    public class BoundaryCompensateEventActivityBehavior : BoundaryEventActivityBehavior
    {

        private const long serialVersionUID = 1L;

        protected internal CompensateEventDefinition compensateEventDefinition;

        public BoundaryCompensateEventActivityBehavior(CompensateEventDefinition compensateEventDefinition, bool interrupting) : base(interrupting)
        {
            this.compensateEventDefinition = compensateEventDefinition;
        }

        public override void Execute(IExecutionEntity execution)
        {
            IExecutionEntity executionEntity = execution;
            BoundaryEvent boundaryEvent = (BoundaryEvent)execution.CurrentFlowElement;

            Process process = ProcessDefinitionUtil.GetProcess(execution.ProcessDefinitionId);
            if (process == null)
            {
                throw new ActivitiException("Process model (id = " + execution.Id + ") could not be found");
            }

            Activity compensationActivity = null;
            IList<Association> associations = process.FindAssociationsWithSourceRefRecursive(boundaryEvent.Id);
            foreach (Association association in associations)
            {
                FlowElement targetElement = process.GetFlowElement(association.TargetRef, true);
                if (targetElement is Activity activity)
                {
                    if (activity.ForCompensation)
                    {
                        compensationActivity = activity;
                        break;
                    }
                }
            }

            if (compensationActivity == null)
            {
                throw new ActivitiException("Compensation activity could not be found (or it is missing 'isForCompensation=\"true\"'");
            }

            // find SubProcess or Process instance execution
            IExecutionEntity scopeExecution = null;
            IExecutionEntity parentExecution = executionEntity.Parent;
            while (scopeExecution == null && parentExecution != null)
            {
                if (parentExecution.CurrentFlowElement is SubProcess)
                {
                    scopeExecution = parentExecution;

                }
                else if (parentExecution.ProcessInstanceType)
                {
                    scopeExecution = parentExecution;
                }
                else
                {
                    parentExecution = parentExecution.Parent;
                }
            }

            if (scopeExecution == null)
            {
                throw new ActivitiException("Could not find a scope execution for compensation boundary event " + boundaryEvent.Id);
            }

            Context.CommandContext.EventSubscriptionEntityManager.InsertCompensationEvent(scopeExecution, compensationActivity.Id);
        }

        public override void Trigger(IExecutionEntity execution, string triggerName, object triggerData, bool throwError = true)
        {
            BoundaryEvent boundaryEvent = (BoundaryEvent)execution.CurrentFlowElement;

            if (boundaryEvent.CancelActivity)
            {
                IEventSubscriptionEntityManager eventSubscriptionEntityManager = Context.CommandContext.EventSubscriptionEntityManager;
                IList<IEventSubscriptionEntity> eventSubscriptions = execution.EventSubscriptions;
                foreach (IEventSubscriptionEntity eventSubscription in eventSubscriptions)
                {
                    if (eventSubscription.EventType == CompensateEventSubscriptionEntityFields.EVENT_TYPE && eventSubscription.ActivityId.Equals(compensateEventDefinition.ActivityRef))
                    {
                        eventSubscriptionEntityManager.Delete(eventSubscription);
                    }
                }
            }

            base.Trigger(execution, triggerName, triggerData, throwError);
        }
    }

}