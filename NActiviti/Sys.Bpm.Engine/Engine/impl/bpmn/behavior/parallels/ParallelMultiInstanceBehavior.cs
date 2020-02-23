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
    using Sys.Workflow.Engine.Impl.Cmd;
    using Sys.Workflow.Engine.Impl.Contexts;
    using Sys.Workflow.Engine.Impl.Interceptor;
    using Sys.Workflow.Engine.Impl.Persistence.Entity;
    using Sys.Workflow.Engine.Impl.Util;

    /// 
    /// 
    [Serializable]
    public class ParallelMultiInstanceBehavior : MultiInstanceActivityBehavior
    {
        private const long serialVersionUID = 1L;

        private readonly object syncRoot = new object();

        public ParallelMultiInstanceBehavior(Activity activity, AbstractBpmnActivityBehavior originalActivityBehavior) : base(activity, originalActivityBehavior)
        {
        }

        /// <summary>
        /// Handles the parallel case of spawning the instances. Will create child executions accordingly for every instance needed.
        /// </summary>
        protected internal override int CreateInstances(IExecutionEntity execution)
        {
            int nrOfInstances = ResolveNrOfInstances(execution);
            if (nrOfInstances < 0)
            {
                throw new ActivitiIllegalArgumentException("Invalid number of instances: must be non-negative integer value" + ", but was " + nrOfInstances);
            }

            execution.IsMultiInstanceRoot = true;

            SetLoopVariable(execution, NUMBER_OF_INSTANCES, nrOfInstances);
            SetLoopVariable(execution, NUMBER_OF_COMPLETED_INSTANCES, 0);
            SetLoopVariable(execution, NUMBER_OF_ACTIVE_INSTANCES, nrOfInstances);

            IList<IExecutionEntity> concurrentExecutions = new List<IExecutionEntity>();
            for (int loopCounter = 0; loopCounter < nrOfInstances; loopCounter++)
            {
                IExecutionEntity concurrentExecution = Context.CommandContext.ExecutionEntityManager.CreateChildExecution(execution);
                concurrentExecution.CurrentFlowElement = activity;
                concurrentExecution.IsActive = true;
                concurrentExecution.IsScope = false;

                concurrentExecutions.Add(concurrentExecution);
                LogLoopDetails(concurrentExecution, "initialized", loopCounter, 0, nrOfInstances, nrOfInstances);
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
                    SetLoopVariable(concurrentExecution, CollectionElementIndexVariable, loopCounter);
                    ExecuteOriginalBehavior(concurrentExecution, loopCounter);
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

            if (!string.IsNullOrWhiteSpace(completedPolicy.CompleteConditionVarName))
            {
                execution.SetVariable(completedPolicy.CompleteConditionVarName, false);
            }

            return nrOfInstances;
        }

        /// <summary>
        /// Called when the wrapped <seealso cref="ActivityBehavior"/> calls the <seealso cref="AbstractBpmnActivityBehavior#leave(ActivityExecution)"/> method. Handles the completion of one of the parallel instances
        /// </summary>
        public override void Leave(IExecutionEntity execution)
        {
            Leave(execution, null);
        }

        public override void Leave(IExecutionEntity execution, object signalData)
        {
            lock (syncRoot)
            {
                ICommandContext commandContext = Context.CommandContext;
                if (commandContext == null)
                {
                    throw new ActivitiException("lazy loading outside command context");
                }

                completedPolicy.LeaveExection = execution;

                bool zeroNrOfInstances = false;
                if (ResolveNrOfInstances(execution) == 0)
                {
                    // Empty collection, just leave.
                    zeroNrOfInstances = true;
                    RemoveLocalLoopVariable(execution, CollectionElementIndexVariable);
                    base.Leave(execution, signalData); // Plan the default leave
                    execution.IsMultiInstanceRoot = false;
                }

                int loopCounter = GetLoopVariable(execution, CollectionElementIndexVariable).GetValueOrDefault(0);
                int nrOfInstances = GetLoopVariable(execution, NUMBER_OF_INSTANCES).GetValueOrDefault(0);
                int nrOfCompletedInstances = GetLoopVariable(execution, NUMBER_OF_COMPLETED_INSTANCES).GetValueOrDefault(0) + 1;
                int nrOfActiveInstances = GetLoopVariable(execution, NUMBER_OF_ACTIVE_INSTANCES).GetValueOrDefault(0) - 1;

                Context.CommandContext.HistoryManager.RecordActivityEnd(execution, null);
                CallActivityEndListeners(execution);

                if (zeroNrOfInstances)
                {
                    return;
                }

                IExecutionEntity miRootExecution = GetMultiInstanceRootExecution(execution);
                if (miRootExecution != null)
                { // will be null in case of empty collection
                    SetLoopVariable(miRootExecution, NUMBER_OF_COMPLETED_INSTANCES, nrOfCompletedInstances);
                    SetLoopVariable(miRootExecution, NUMBER_OF_ACTIVE_INSTANCES, nrOfActiveInstances < 0 ? 0 : nrOfActiveInstances);
                }

                //ExecuteCompensationBoundaryEvents(execution.CurrentFlowElement, execution);

                LogLoopDetails(execution, "instance completed", loopCounter, nrOfCompletedInstances, nrOfActiveInstances, nrOfActiveInstances < 0 ? 0 : nrOfActiveInstances);

                if (execution.Parent != null)
                {
                    execution.Inactivate();
                    LockFirstParentScope(execution);

                    if (CompletionConditionSatisfied(execution.Parent, signalData) || nrOfCompletedInstances >= nrOfInstances)
                    {
                        IExecutionEntity executionToUse;
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
                        else if (activity is SubProcess subProcess)
                        {
                            foreach (FlowElement subElement in subProcess.FlowElements)
                            {
                                if (subElement is Activity subActivity)
                                {
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
                            ScopeUtil.CreateCopyOfSubProcessExecutionForCompensation(executionToUse);
                        }

                        if (activity is CallActivity)
                        {
                            IExecutionEntityManager executionEntityManager = Context.CommandContext.ExecutionEntityManager;
                            if (executionToUse != null)
                            {
                                IList<string> callActivityExecutionIds = new List<string>();

                                // Find all execution entities that are at the call activity
                                IList<IExecutionEntity> childExecutions = executionEntityManager.CollectChildren(executionToUse);
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

                                            executionEntityManager.DeleteProcessInstanceExecutionEntity(childExecution.Id, activity.Id, "call activity completion condition met", true, false);
                                        }
                                    }

                                }
                            }
                        }

                        DeleteChildExecutions(executionToUse, false, Context.CommandContext);
                        RemoveLocalLoopVariable(executionToUse, CollectionElementIndexVariable);
                        executionToUse.IsScope = false;
                        executionToUse.IsMultiInstanceRoot = false;
                        Context.Agenda.PlanTakeOutgoingSequenceFlowsOperation(executionToUse, true);
                    }
                }
                else
                {
                    RemoveLocalLoopVariable(execution, CollectionElementIndexVariable);
                    execution.IsMultiInstanceRoot = false;
                    base.Leave(execution, signalData);
                }
            }
        }

        protected internal virtual void LockFirstParentScope(IExecutionEntity execution)
        {
            IExecutionEntityManager executionEntityManager = Context.CommandContext.ExecutionEntityManager;

            bool found = false;
            IExecutionEntity parentScopeExecution = null;
            IExecutionEntity currentExecution = execution;
            while (!found && currentExecution is object && currentExecution.ParentId is object)
            {
                parentScopeExecution = executionEntityManager.FindById<IExecutionEntity>(currentExecution.ParentId);
                if (parentScopeExecution != null && parentScopeExecution.IsScope)
                {
                    found = true;
                }
                currentExecution = parentScopeExecution;
            }

            parentScopeExecution.ForceUpdate();
        }

        // TODO: can the ExecutionManager.deleteChildExecution not be used?
        protected internal virtual void DeleteChildExecutions(IExecutionEntity parentExecution, bool deleteExecution, ICommandContext commandContext)
        {
            // Delete all child executions
            IExecutionEntityManager executionEntityManager = commandContext.ExecutionEntityManager;
            ICollection<IExecutionEntity> childExecutions = executionEntityManager.FindChildExecutionsByParentExecutionId(parentExecution.Id);
            if (CollectionUtil.IsNotEmpty(childExecutions))
            {
                foreach (IExecutionEntity childExecution in childExecutions)
                {
                    DeleteChildExecutions(childExecution, true, commandContext);
                }
            }

            if (deleteExecution)
            {
                executionEntityManager.DeleteExecutionAndRelatedData(parentExecution, null, false);
            }
        }
    }
}