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
    public class ParallelMultiInstanceBehavior : MultiInstanceActivityBehavior
    {

        private const long serialVersionUID = 1L;

        public ParallelMultiInstanceBehavior(Activity activity, AbstractBpmnActivityBehavior originalActivityBehavior) : base(activity, originalActivityBehavior)
        {
        }

        /// <summary>
        /// Handles the parallel case of spawning the instances. Will create child executions accordingly for every instance needed.
        /// </summary>
        protected internal override int createInstances(IExecutionEntity execution)
        {
            int nrOfInstances = resolveNrOfInstances(execution);
            if (nrOfInstances < 0)
            {
                throw new ActivitiIllegalArgumentException("Invalid number of instances: must be non-negative integer value" + ", but was " + nrOfInstances);
            }

            execution.IsMultiInstanceRoot = true;

            setLoopVariable(execution, NUMBER_OF_INSTANCES, nrOfInstances);
            setLoopVariable(execution, NUMBER_OF_COMPLETED_INSTANCES, 0);
            setLoopVariable(execution, NUMBER_OF_ACTIVE_INSTANCES, nrOfInstances);

            IList<IExecutionEntity> concurrentExecutions = new List<IExecutionEntity>();
            for (int loopCounter = 0; loopCounter < nrOfInstances; loopCounter++)
            {
                IExecutionEntity concurrentExecution = Context.CommandContext.ExecutionEntityManager.createChildExecution(execution);
                concurrentExecution.CurrentFlowElement = activity;
                concurrentExecution.IsActive = true;
                concurrentExecution.IsScope = false;

                concurrentExecutions.Add(concurrentExecution);
                logLoopDetails(concurrentExecution, "initialized", loopCounter, 0, nrOfInstances, nrOfInstances);
            }

            // Before the activities are executed, all executions MUST be created up front
            // Do not try to merge this loop with the previous one, as it will lead
            // to bugs, due to possible child execution pruning.
            for (int loopCounter = 0; loopCounter < nrOfInstances; loopCounter++)
            {
                IExecutionEntity concurrentExecution = concurrentExecutions[loopCounter];
                // executions can be inactive, if instances are all automatics
                // (no-waitstate) and completionCondition has been met in the meantime
                if (concurrentExecution.IsActive && !concurrentExecution.Ended && concurrentExecution.Parent.IsActive && !concurrentExecution.Parent.Ended)
                {
                    setLoopVariable(concurrentExecution, CollectionElementIndexVariable, loopCounter);
                    executeOriginalBehavior(concurrentExecution, loopCounter);
                }
            }

            // See ACT-1586: ExecutionQuery returns wrong results when using multi
            // instance on a receive task The parent execution must be set to false, so it wouldn't show up in
            // the execution query when using .activityId(something). Do not we cannot nullify the
            // activityId (that would have been a better solution), as it would break boundary event behavior.
            if (concurrentExecutions.Count > 0)
            {
                IExecutionEntity executionEntity = execution;
                executionEntity.IsActive = false;
            }

            return nrOfInstances;
        }

        /// <summary>
        /// Called when the wrapped <seealso cref="ActivityBehavior"/> calls the <seealso cref="AbstractBpmnActivityBehavior#leave(ActivityExecution)"/> method. Handles the completion of one of the parallel instances
        /// </summary>
        public override void leave(IExecutionEntity execution)
        {

            bool zeroNrOfInstances = false;
            if (resolveNrOfInstances(execution) == 0)
            {
                // Empty collection, just leave.
                zeroNrOfInstances = true;
                removeLocalLoopVariable(execution, CollectionElementIndexVariable);
                base.leave(execution); // Plan the default leave
                execution.IsMultiInstanceRoot = false;
            }

            int loopCounter = getLoopVariable(execution, CollectionElementIndexVariable).Value;
            int nrOfInstances = getLoopVariable(execution, NUMBER_OF_INSTANCES).Value;
            int nrOfCompletedInstances = getLoopVariable(execution, NUMBER_OF_COMPLETED_INSTANCES).GetValueOrDefault(0) + 1;
            int nrOfActiveInstances = getLoopVariable(execution, NUMBER_OF_ACTIVE_INSTANCES).GetValueOrDefault(0) - 1;

            Context.CommandContext.HistoryManager.recordActivityEnd(execution, null);
            callActivityEndListeners(execution);

            if (zeroNrOfInstances)
            {
                return;
            }

            IExecutionEntity miRootExecution = getMultiInstanceRootExecution(execution);
            if (miRootExecution != null)
            { // will be null in case of empty collection
                setLoopVariable(miRootExecution, NUMBER_OF_COMPLETED_INSTANCES, nrOfCompletedInstances);
                setLoopVariable(miRootExecution, NUMBER_OF_ACTIVE_INSTANCES, nrOfActiveInstances);
            }

            //executeCompensationBoundaryEvents(execution.getCurrentFlowElement(), execution);

            logLoopDetails(execution, "instance completed", loopCounter, nrOfCompletedInstances, nrOfActiveInstances, nrOfInstances);

            if (execution.Parent != null)
            {

                execution.inactivate();
                lockFirstParentScope(execution);

                if (nrOfCompletedInstances >= nrOfInstances || completionConditionSatisfied(execution.Parent))
                {

                    IExecutionEntity executionToUse = null;
                    if (nrOfInstances > 0)
                    {
                        executionToUse = execution.Parent;
                    }
                    else
                    {
                        executionToUse = execution;
                    }

                    bool hasCompensation = false;
                    Activity activity = (Activity)execution.CurrentFlowElement;
                    if (activity is Transaction)
                    {
                        hasCompensation = true;
                    }
                    else if (activity is SubProcess)
                    {
                        SubProcess subProcess = (SubProcess)activity;
                        foreach (FlowElement subElement in subProcess.FlowElements)
                        {
                            if (subElement is Activity)
                            {
                                Activity subActivity = (Activity)subElement;
                                if (CollectionUtil.IsNotEmpty(subActivity.BoundaryEvents))
                                {
                                    foreach (BoundaryEvent boundaryEvent in subActivity.BoundaryEvents)
                                    {
                                        if (CollectionUtil.IsNotEmpty(boundaryEvent.EventDefinitions) && boundaryEvent.EventDefinitions[0] is CompensateEventDefinition)
                                        {

                                            hasCompensation = true;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (hasCompensation)
                    {
                        ScopeUtil.createCopyOfSubProcessExecutionForCompensation(executionToUse);
                    }

                    if (activity is CallActivity)
                    {
                        IExecutionEntityManager executionEntityManager = Context.CommandContext.ExecutionEntityManager;
                        if (executionToUse != null)
                        {
                            IList<string> callActivityExecutionIds = new List<string>();

                            // Find all execution entities that are at the call activity
                            IList<IExecutionEntity> childExecutions = executionEntityManager.collectChildren(executionToUse);
                            if (childExecutions != null)
                            {
                                foreach (IExecutionEntity childExecution in childExecutions)
                                {
                                    if (activity.Id.Equals(childExecution.CurrentActivityId))
                                    {
                                        callActivityExecutionIds.Add(childExecution.Id);
                                    }
                                }

                                // Now all call activity executions have been collected, loop again and check which should be removed
                                for (int i = childExecutions.Count - 1; i >= 0; i--)
                                {
                                    IExecutionEntity childExecution = childExecutions[i];
                                    if (!string.IsNullOrWhiteSpace(childExecution.SuperExecutionId) && callActivityExecutionIds.Contains(childExecution.SuperExecutionId))
                                    {

                                        executionEntityManager.deleteProcessInstanceExecutionEntity(childExecution.Id, activity.Id, "call activity completion condition met", true, false);
                                    }
                                }

                            }
                        }
                    }

                    deleteChildExecutions(executionToUse, false, Context.CommandContext);
                    removeLocalLoopVariable(executionToUse, CollectionElementIndexVariable);
                    executionToUse.IsScope = false;
                    executionToUse.IsMultiInstanceRoot = false;
                    Context.Agenda.planTakeOutgoingSequenceFlowsOperation(executionToUse, true);
                }

            }
            else
            {
                removeLocalLoopVariable(execution, CollectionElementIndexVariable);
                execution.IsMultiInstanceRoot = false;
                base.leave(execution);
            }
        }

        protected internal virtual void lockFirstParentScope(IExecutionEntity execution)
        {

            IExecutionEntityManager executionEntityManager = Context.CommandContext.ExecutionEntityManager;

            bool found = false;
            IExecutionEntity parentScopeExecution = null;
            IExecutionEntity currentExecution = (IExecutionEntity)execution;
            while (!found && currentExecution != null && !string.ReferenceEquals(currentExecution.ParentId, null))
            {
                parentScopeExecution = executionEntityManager.findById<IExecutionEntity>(new KeyValuePair<string, object>("id", currentExecution.ParentId));
                if (parentScopeExecution != null && parentScopeExecution.IsScope)
                {
                    found = true;
                }
                currentExecution = parentScopeExecution;
            }

            parentScopeExecution.forceUpdate();
        }

        // TODO: can the ExecutionManager.deleteChildExecution not be used?
        protected internal virtual void deleteChildExecutions(IExecutionEntity parentExecution, bool deleteExecution, ICommandContext commandContext)
        {
            // Delete all child executions
            IExecutionEntityManager executionEntityManager = commandContext.ExecutionEntityManager;
            ICollection<IExecutionEntity> childExecutions = executionEntityManager.findChildExecutionsByParentExecutionId(parentExecution.Id);
            if (CollectionUtil.IsNotEmpty(childExecutions))
            {
                foreach (IExecutionEntity childExecution in childExecutions)
                {
                    deleteChildExecutions(childExecution, true, commandContext);
                }
            }

            if (deleteExecution)
            {
                executionEntityManager.deleteExecutionAndRelatedData(parentExecution, null, false);
            }
        }

    }

}