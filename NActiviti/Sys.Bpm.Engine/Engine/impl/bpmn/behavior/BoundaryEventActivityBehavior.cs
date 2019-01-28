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
    using org.activiti.engine.@delegate;
    using org.activiti.engine.impl.context;
    using org.activiti.engine.impl.interceptor;
    using org.activiti.engine.impl.persistence.entity;
    using org.activiti.engine.impl.util;

    /// 
    [Serializable]
    public class BoundaryEventActivityBehavior : FlowNodeActivityBehavior
    {

        private const long serialVersionUID = 1L;

        protected internal bool interrupting;

        public BoundaryEventActivityBehavior()
        {
        }

        public BoundaryEventActivityBehavior(bool interrupting)
        {
            this.interrupting = interrupting;
        }

        public override void execute(IExecutionEntity execution)
        {
            // Overridden by subclasses
        }

        public override void trigger(IExecutionEntity execution, string triggerName, object triggerData)
        {

            IExecutionEntity executionEntity = execution;

            ICommandContext commandContext = Context.CommandContext;

            if (interrupting)
            {
                executeInterruptingBehavior(executionEntity, commandContext);
            }
            else
            {
                executeNonInterruptingBehavior(executionEntity, commandContext);
            }
        }

        protected internal virtual void executeInterruptingBehavior(IExecutionEntity executionEntity, ICommandContext commandContext)
        {

            // The destroy scope operation will look for the parent execution and
            // destroy the whole scope, and leave the boundary event using this parent execution.
            //
            // The take outgoing seq flows operation below (the non-interrupting else clause) on the other hand uses the
            // child execution to leave, which keeps the scope alive.
            // Which is what we need here.

            IExecutionEntityManager executionEntityManager = commandContext.ExecutionEntityManager;
            IExecutionEntity attachedRefScopeExecution = executionEntityManager.findById<IExecutionEntity>(new KeyValuePair<string, object>("id", executionEntity.ParentId));

            IExecutionEntity parentScopeExecution = null;
            IExecutionEntity currentlyExaminedExecution = executionEntityManager.findById<IExecutionEntity>(new KeyValuePair<string, object>("id", attachedRefScopeExecution.ParentId));
            while (currentlyExaminedExecution != null && parentScopeExecution == null)
            {
                if (currentlyExaminedExecution.IsScope)
                {
                    parentScopeExecution = currentlyExaminedExecution;
                }
                else
                {
                    currentlyExaminedExecution = executionEntityManager.findById<IExecutionEntity>(new KeyValuePair<string, object>("id", currentlyExaminedExecution.ParentId));
                }
            }

            if (parentScopeExecution == null)
            {
                throw new ActivitiException("Programmatic error: no parent scope execution found for boundary event");
            }

            deleteChildExecutions(attachedRefScopeExecution, executionEntity, commandContext);

            // set new parent for boundary event execution
            executionEntity.Parent = parentScopeExecution;

            Context.Agenda.planTakeOutgoingSequenceFlowsOperation(executionEntity, true);
        }

        protected internal virtual void executeNonInterruptingBehavior(IExecutionEntity executionEntity, ICommandContext commandContext)
        {

            // Non-interrupting: the current execution is given the first parent
            // scope (which isn't its direct parent)
            //
            // Why? Because this execution does NOT have anything to do with
            // the current parent execution (the one where the boundary event is on): when it is deleted or whatever,
            // this does not impact this new execution at all, it is completely independent in that regard.

            // Note: if the parent of the parent does not exists, this becomes a concurrent execution in the process instance!

            IExecutionEntityManager executionEntityManager = commandContext.ExecutionEntityManager;

            IExecutionEntity parentExecutionEntity = executionEntityManager.findById<IExecutionEntity>(new KeyValuePair<string, object>("id", executionEntity.ParentId));

            IExecutionEntity scopeExecution = null;
            IExecutionEntity currentlyExaminedExecution = executionEntityManager.findById<IExecutionEntity>(new KeyValuePair<string, object>("id", parentExecutionEntity.ParentId));
            while (currentlyExaminedExecution != null && scopeExecution == null)
            {
                if (currentlyExaminedExecution.IsScope)
                {
                    scopeExecution = currentlyExaminedExecution;
                }
                else
                {
                    currentlyExaminedExecution = executionEntityManager.findById<IExecutionEntity>(new KeyValuePair<string, object>("id", currentlyExaminedExecution.ParentId));
                }
            }

            if (scopeExecution == null)
            {
                throw new ActivitiException("Programmatic error: no parent scope execution found for boundary event");
            }

            IExecutionEntity nonInterruptingExecution = executionEntityManager.createChildExecution(scopeExecution);
            nonInterruptingExecution.CurrentFlowElement = executionEntity.CurrentFlowElement;

            Context.Agenda.planTakeOutgoingSequenceFlowsOperation(nonInterruptingExecution, true);
        }

        protected internal virtual void deleteChildExecutions(IExecutionEntity parentExecution, IExecutionEntity notToDeleteExecution, ICommandContext commandContext)
        {

            // TODO: would be good if this deleteChildExecutions could be removed and the one on the executionEntityManager is used
            // The problem however, is that the 'notToDeleteExecution' is passed here.
            // This could be solved by not reusing an execution, but creating a new

            // Delete all child executions
            IExecutionEntityManager executionEntityManager = commandContext.ExecutionEntityManager;
            ICollection<IExecutionEntity> childExecutions = executionEntityManager.findChildExecutionsByParentExecutionId(parentExecution.Id);
            if (CollectionUtil.IsNotEmpty(childExecutions))
            {
                foreach (IExecutionEntity childExecution in childExecutions)
                {
                    if (childExecution.Id.Equals(notToDeleteExecution.Id) == false)
                    {
                        deleteChildExecutions(childExecution, notToDeleteExecution, commandContext);
                    }
                }
            }

            string deleteReason = engine.history.DeleteReason_Fields.BOUNDARY_EVENT_INTERRUPTING + " (" + notToDeleteExecution.CurrentActivityId + ")";
            if (parentExecution.CurrentFlowElement is CallActivity)
            {
                IExecutionEntity subProcessExecution = executionEntityManager.findSubProcessInstanceBySuperExecutionId(parentExecution.Id);
                if (subProcessExecution != null)
                {
                    executionEntityManager.deleteProcessInstanceExecutionEntity(subProcessExecution.Id, subProcessExecution.CurrentActivityId, deleteReason, true, true);
                }
            }

            executionEntityManager.deleteExecutionAndRelatedData(parentExecution, deleteReason, false);
        }

        public virtual bool Interrupting
        {
            get
            {
                return interrupting;
            }
            set
            {
                this.interrupting = value;
            }
        }


    }

}