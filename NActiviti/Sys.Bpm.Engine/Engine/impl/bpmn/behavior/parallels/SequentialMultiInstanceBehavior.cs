using System;

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
    using org.activiti.engine.impl.persistence.entity;

    /// 
    /// 
    [Serializable]
    public class SequentialMultiInstanceBehavior : MultiInstanceActivityBehavior
    {

        private const long serialVersionUID = 1L;

        public SequentialMultiInstanceBehavior(Activity activity, AbstractBpmnActivityBehavior innerActivityBehavior) : base(activity, innerActivityBehavior)
        {
        }

        /// <summary>
        /// Handles the sequential case of spawning the instances. Will only create one instance, since at most one instance can be active.
        /// </summary>
        protected internal override int CreateInstances(IExecutionEntity multiInstanceExecution)
        {
            int nrOfInstances = ResolveNrOfInstances(multiInstanceExecution);
            if (nrOfInstances == 0)
            {
                return nrOfInstances;
            }
            else if (nrOfInstances < 0)
            {
                throw new ActivitiIllegalArgumentException("Invalid number of instances: must be a non-negative integer value" + ", but was " + nrOfInstances);
            }

            // Create child execution that will execute the inner behavior
            IExecutionEntity childExecution = Context.CommandContext.ExecutionEntityManager.CreateChildExecution(multiInstanceExecution);
            childExecution.CurrentFlowElement = multiInstanceExecution.CurrentFlowElement;
            multiInstanceExecution.IsMultiInstanceRoot = true;
            multiInstanceExecution.IsActive = false;

            // Set Multi-instance variables
            SetLoopVariable(multiInstanceExecution, NUMBER_OF_INSTANCES, nrOfInstances);
            SetLoopVariable(multiInstanceExecution, NUMBER_OF_COMPLETED_INSTANCES, 0);
            SetLoopVariable(multiInstanceExecution, NUMBER_OF_ACTIVE_INSTANCES, 1);
            SetLoopVariable(childExecution, CollectionElementIndexVariable, 0);
            LogLoopDetails(multiInstanceExecution, "initialized", 0, 0, 1, nrOfInstances);

            if (nrOfInstances > 0)
            {
                ExecuteOriginalBehavior(childExecution, 0);
            }
            return nrOfInstances;
        }

        /// <summary>
        /// Called when the wrapped <seealso cref="ActivityBehavior"/> calls the <seealso cref="AbstractBpmnActivityBehavior#leave(ActivityExecution)"/> method. Handles the completion of one instance, and executes the logic for
        /// the sequential behavior.
        /// </summary>
        public override void Leave(IExecutionEntity childExecution)
        {
            Leave(childExecution, null);
        }

        public override void Leave(IExecutionEntity childExecution, object signalData)
        {
            completedPolicy.LeaveExection = childExecution;

            IExecutionEntity multiInstanceRootExecution = GetMultiInstanceRootExecution(childExecution);
            int nrOfInstances = GetLoopVariable(multiInstanceRootExecution, NUMBER_OF_INSTANCES).GetValueOrDefault(0);
            int loopCounter = GetLoopVariable(childExecution, CollectionElementIndexVariable).GetValueOrDefault(0) + 1;
            int nrOfCompletedInstances = GetLoopVariable(multiInstanceRootExecution, NUMBER_OF_COMPLETED_INSTANCES).GetValueOrDefault(0) + 1;
            int nrOfActiveInstances = GetLoopVariable(multiInstanceRootExecution, NUMBER_OF_ACTIVE_INSTANCES).GetValueOrDefault(0);

            SetLoopVariable(multiInstanceRootExecution, NUMBER_OF_COMPLETED_INSTANCES, nrOfCompletedInstances);
            SetLoopVariable(childExecution, CollectionElementIndexVariable, loopCounter);
            LogLoopDetails(childExecution, "instance completed", loopCounter, nrOfCompletedInstances, nrOfActiveInstances, nrOfInstances);

            Context.CommandContext.HistoryManager.RecordActivityEnd(childExecution, null);
            CallActivityEndListeners(childExecution);

            //executeCompensationBoundaryEvents(execution.getCurrentFlowElement(), execution);

            if (loopCounter >= nrOfInstances || CompletionConditionSatisfied(multiInstanceRootExecution, signalData))
            {
                RemoveLocalLoopVariable(childExecution, CollectionElementIndexVariable);
                multiInstanceRootExecution.IsMultiInstanceRoot = false;
                multiInstanceRootExecution.IsScope = false;
                multiInstanceRootExecution.CurrentFlowElement = childExecution.CurrentFlowElement;
                Context.CommandContext.ExecutionEntityManager.DeleteChildExecutions(multiInstanceRootExecution, "MI_END", false);
                base.Leave(multiInstanceRootExecution, signalData);
            }
            else
            {
                try
                {
                    if (childExecution.CurrentFlowElement is SubProcess)
                    {
                        IExecutionEntityManager executionEntityManager = Context.CommandContext.ExecutionEntityManager;
                        IExecutionEntity executionToContinue = executionEntityManager.CreateChildExecution(multiInstanceRootExecution);
                        executionToContinue.CurrentFlowElement = childExecution.CurrentFlowElement;
                        executionToContinue.IsScope = true;
                        SetLoopVariable(executionToContinue, CollectionElementIndexVariable, loopCounter);
                        ExecuteOriginalBehavior(executionToContinue, loopCounter);
                    }
                    else
                    {
                        ExecuteOriginalBehavior(childExecution, loopCounter);
                    }
                }
                catch (BpmnError error)
                {
                    // re-throw business fault so that it can be caught by an Error
                    // Intermediate Event or Error Event Sub-Process in the process
                    throw error;
                }
                catch (Exception e)
                {
                    throw new ActivitiException("Could not execute inner activity behavior of multi instance behavior", e);
                }
            }
        }
    }
}