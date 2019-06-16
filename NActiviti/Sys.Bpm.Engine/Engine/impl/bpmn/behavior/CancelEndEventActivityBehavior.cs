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
    using Sys.Workflow.Engine.Impl.Util;


    /// 
    [Serializable]
    public class CancelEndEventActivityBehavior : FlowNodeActivityBehavior
    {

        private const long serialVersionUID = 1L;

        public override void Execute(IExecutionEntity execution)
        {
            IExecutionEntity executionEntity = execution;
            ICommandContext commandContext = Context.CommandContext;
            IExecutionEntityManager executionEntityManager = commandContext.ExecutionEntityManager;

            // find cancel boundary event:
            IExecutionEntity parentScopeExecution = null;
            IExecutionEntity currentlyExaminedExecution = executionEntityManager.FindById<IExecutionEntity>(executionEntity.ParentId);
            while (currentlyExaminedExecution != null && parentScopeExecution == null)
            {
                if (currentlyExaminedExecution.CurrentFlowElement is SubProcess)
                {
                    parentScopeExecution = currentlyExaminedExecution;
                    SubProcess subProcess = (SubProcess)currentlyExaminedExecution.CurrentFlowElement;
                    if (subProcess.LoopCharacteristics != null)
                    {
                        IExecutionEntity miExecution = parentScopeExecution.Parent;
                        FlowElement miElement = miExecution.CurrentFlowElement;
                        if (miElement != null && miElement.Id.Equals(subProcess.Id))
                        {
                            parentScopeExecution = miExecution;
                        }
                    }

                }
                else
                {
                    currentlyExaminedExecution = executionEntityManager.FindById<IExecutionEntity>(currentlyExaminedExecution.ParentId);
                }
            }

            if (parentScopeExecution == null)
            {
                throw new ActivitiException("No sub process execution found for cancel end event " + executionEntity.CurrentActivityId);
            }

            {
                SubProcess subProcess = (SubProcess)parentScopeExecution.CurrentFlowElement;
                BoundaryEvent cancelBoundaryEvent = null;
                if (CollectionUtil.IsNotEmpty(subProcess.BoundaryEvents))
                {
                    foreach (BoundaryEvent boundaryEvent in subProcess.BoundaryEvents)
                    {
                        if (CollectionUtil.IsNotEmpty(boundaryEvent.EventDefinitions) && boundaryEvent.EventDefinitions[0] is CancelEventDefinition)
                        {

                            cancelBoundaryEvent = boundaryEvent;
                            break;
                        }
                    }
                }

                if (cancelBoundaryEvent == null)
                {
                    throw new ActivitiException("Could not find cancel boundary event for cancel end event " + executionEntity.CurrentActivityId);
                }

                IExecutionEntity newParentScopeExecution = null;
                currentlyExaminedExecution = executionEntityManager.FindById<IExecutionEntity>(parentScopeExecution.ParentId);
                while (currentlyExaminedExecution != null && newParentScopeExecution == null)
                {
                    if (currentlyExaminedExecution.IsScope)
                    {
                        newParentScopeExecution = currentlyExaminedExecution;
                    }
                    else
                    {
                        currentlyExaminedExecution = executionEntityManager.FindById<IExecutionEntity>(currentlyExaminedExecution.ParentId);
                    }
                }

                ScopeUtil.CreateCopyOfSubProcessExecutionForCompensation(parentScopeExecution);

                if (subProcess.LoopCharacteristics != null)
                {
                    IList<IExecutionEntity> multiInstanceExecutions = parentScopeExecution.Executions;
                    IList<IExecutionEntity> executionsToDelete = new List<IExecutionEntity>();
                    foreach (IExecutionEntity multiInstanceExecution in multiInstanceExecutions)
                    {
                        if (!multiInstanceExecution.Id.Equals(parentScopeExecution.Id))
                        {
                            ScopeUtil.CreateCopyOfSubProcessExecutionForCompensation(multiInstanceExecution);

                            // end all executions in the scope of the transaction
                            executionsToDelete.Add(multiInstanceExecution);
                            DeleteChildExecutions(multiInstanceExecution, executionEntity, commandContext, History.DeleteReasonFields.TRANSACTION_CANCELED);

                        }
                    }

                    foreach (IExecutionEntity executionEntityToDelete in executionsToDelete)
                    {
                        DeleteChildExecutions(executionEntityToDelete, executionEntity, commandContext, History.DeleteReasonFields.TRANSACTION_CANCELED);
                    }
                }

                // The current activity is finished (and will not be ended in the deleteChildExecutions)
                commandContext.HistoryManager.RecordActivityEnd(executionEntity, null);

                // set new parent for boundary event execution
                executionEntity.Parent = newParentScopeExecution ?? throw new ActivitiException("Programmatic error: no parent scope execution found for boundary event " + cancelBoundaryEvent.Id);
                executionEntity.CurrentFlowElement = cancelBoundaryEvent;

                // end all executions in the scope of the transaction
                DeleteChildExecutions(parentScopeExecution, executionEntity, commandContext, History.DeleteReasonFields.TRANSACTION_CANCELED);
                commandContext.HistoryManager.RecordActivityEnd(parentScopeExecution, History.DeleteReasonFields.TRANSACTION_CANCELED);

                Context.Agenda.PlanTriggerExecutionOperation(executionEntity);
            }
        }

        protected internal virtual void DeleteChildExecutions(IExecutionEntity parentExecution, IExecutionEntity notToDeleteExecution, ICommandContext commandContext, string deleteReason)
        {
            // Delete all child executions
            IExecutionEntityManager executionEntityManager = commandContext.ExecutionEntityManager;
            ICollection<IExecutionEntity> childExecutions = executionEntityManager.FindChildExecutionsByParentExecutionId(parentExecution.Id);
            if (CollectionUtil.IsNotEmpty(childExecutions))
            {
                foreach (IExecutionEntity childExecution in childExecutions)
                {
                    if (!(childExecution.Id.Equals(notToDeleteExecution.Id)))
                    {
                        DeleteChildExecutions(childExecution, notToDeleteExecution, commandContext, deleteReason);
                    }
                }
            }

            executionEntityManager.DeleteExecutionAndRelatedData(parentExecution, deleteReason, false);
        }

    }

}