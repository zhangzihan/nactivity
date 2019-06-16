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
    using Sys.Workflow.Engine.Delegate;
    using Sys.Workflow.Engine.Impl.Bpmn.Helper;
    using Sys.Workflow.Engine.Impl.Contexts;
    using Sys.Workflow.Engine.Impl.Interceptor;
    using Sys.Workflow.Engine.Impl.Persistence.Entity;

    /// 
    [Serializable]
    public class BoundaryCancelEventActivityBehavior : BoundaryEventActivityBehavior
    {

        private const long serialVersionUID = 1L;

        public override void Trigger(IExecutionEntity execution, string triggerName, object triggerData, bool throwError = true)
        {
            BoundaryEvent boundaryEvent = (BoundaryEvent)execution.CurrentFlowElement;

            ICommandContext commandContext = Context.CommandContext;
            IExecutionEntityManager executionEntityManager = commandContext.ExecutionEntityManager;

            IExecutionEntity subProcessExecution = null;
            // TODO: this can be optimized. A full search in the all executions shouldn't be needed
            IList<IExecutionEntity> processInstanceExecutions = executionEntityManager.FindChildExecutionsByProcessInstanceId(execution.ProcessInstanceId);
            foreach (IExecutionEntity childExecution in processInstanceExecutions)
            {
                if (childExecution.CurrentFlowElement != null && childExecution.CurrentFlowElement.Id.Equals(boundaryEvent.AttachedToRefId))
                {
                    subProcessExecution = childExecution;
                    break;
                }
            }

            if (subProcessExecution == null)
            {
                throw new ActivitiException("No execution found for sub process of boundary cancel event " + boundaryEvent.Id);
            }

            IEventSubscriptionEntityManager eventSubscriptionEntityManager = commandContext.EventSubscriptionEntityManager;
            IList<ICompensateEventSubscriptionEntity> eventSubscriptions = eventSubscriptionEntityManager.FindCompensateEventSubscriptionsByExecutionId(subProcessExecution.ParentId);

            if (eventSubscriptions.Count == 0)
            {
                Leave(execution);
            }
            else
            {

                string deleteReason = History.DeleteReasonFields.BOUNDARY_EVENT_INTERRUPTING + "(" + boundaryEvent.Id + ")";

                // cancel boundary is always sync
                ScopeUtil.ThrowCompensationEvent(eventSubscriptions, execution, false);
                executionEntityManager.DeleteExecutionAndRelatedData(subProcessExecution, deleteReason, false);
                if (subProcessExecution.CurrentFlowElement is Activity activity)
                {
                    if (activity.LoopCharacteristics != null)
                    {
                        IExecutionEntity miExecution = subProcessExecution.Parent;
                        IList<IExecutionEntity> miChildExecutions = executionEntityManager.FindChildExecutionsByParentExecutionId(miExecution.Id);
                        foreach (IExecutionEntity miChildExecution in miChildExecutions)
                        {
                            if (subProcessExecution.Id.Equals(miChildExecution.Id) == false && activity.Id.Equals(miChildExecution.CurrentActivityId))
                            {
                                executionEntityManager.DeleteExecutionAndRelatedData(miChildExecution, deleteReason, false);
                            }
                        }
                    }
                }
                Leave(execution);
            }
        }
    }

}