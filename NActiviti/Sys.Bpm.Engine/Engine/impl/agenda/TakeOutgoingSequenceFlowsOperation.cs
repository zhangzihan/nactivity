using System.Collections.Generic;

namespace Sys.Workflow.Engine.Impl.Agenda
{
    using Microsoft.Extensions.Logging;
    using Sys.Workflow.Bpmn.Models;
    using Sys.Workflow.Engine.Delegate;
    using Sys.Workflow.Engine.Delegate.Events;
    using Sys.Workflow.Engine.Delegate.Events.Impl;
    using Sys.Workflow.Engine.Impl.Bpmn.Helper;
    using Sys.Workflow.Engine.Impl.Contexts;
    using Sys.Workflow.Engine.Impl.Interceptor;
    using Sys.Workflow.Engine.Impl.Persistence.Entity;
    using Sys.Workflow.Engine.Impl.Util;
    using Sys.Workflow.Engine.Impl.Util.Conditions;
    using Sys.Expressions;
    using Sys.Workflow;
    using System;

    /// <summary>
    /// Operation that leaves the <seealso cref="FlowElement"/> where the <seealso cref="IExecutionEntity"/> is currently at
    /// and leaves it following the sequence flow.
    /// </summary>
    public class TakeOutgoingSequenceFlowsOperation : AbstractOperation
    {
        private static readonly ILogger<TakeOutgoingSequenceFlowsOperation> log = ProcessEngineServiceProvider.LoggerService<TakeOutgoingSequenceFlowsOperation>();

        /// <summary>
        /// 
        /// </summary>
        protected internal bool evaluateConditions;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="commandContext"></param>
        /// <param name="executionEntity"></param>
        /// <param name="evaluateConditions"></param>
        public TakeOutgoingSequenceFlowsOperation(ICommandContext commandContext, IExecutionEntity executionEntity, bool evaluateConditions) : base(commandContext, executionEntity)
        {
            this.evaluateConditions = evaluateConditions;
        }

        /// <inheritdoc />
        protected override void RunOperation()
        {
            try
            {
                FlowElement currentFlowElement = GetCurrentFlowElement(execution);

                // Compensation check
                if ((currentFlowElement is Activity) && (((Activity)currentFlowElement)).ForCompensation)
                {
                    /*
                     * If the current flow element is part of a compensation, we don't always
                     * want to follow the regular rules of leaving an activity.
                     * More specifically, if there are no outgoing sequenceflow, we simply must stop
                     * the execution there and don't go up in the scopes as we usually do
                     * to find the outgoing sequenceflow
                     */

                    CleanupCompensation();

                    return;
                }

                // When leaving the current activity, we need to delete any related execution (eg active boundary events)
                CleanupExecutions(currentFlowElement);

                if (currentFlowElement is FlowNode)
                {
                    HandleFlowNode((FlowNode)currentFlowElement);
                }
                else if (currentFlowElement is SequenceFlow)
                {
                    HandleSequenceFlow();
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="flowNode"></param>
        protected internal virtual void HandleFlowNode(FlowNode flowNode)
        {
            HandleActivityEnd(flowNode);
            if (flowNode.ParentContainer != null && flowNode.ParentContainer is AdhocSubProcess)
            {
                HandleAdhocSubProcess(flowNode);
            }
            else
            {
                LeaveFlowNode(flowNode);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="flowNode"></param>
        protected internal virtual void HandleActivityEnd(FlowNode flowNode)
        {
            // a process instance execution can never leave a flow node, but it can pass here whilst cleaning up
            // hence the check for NOT being a process instance
            if (!execution.ProcessInstanceType)
            {
                if (CollectionUtil.IsNotEmpty(flowNode.ExecutionListeners))
                {
                    ExecuteExecutionListeners(flowNode, BaseExecutionListenerFields.EVENTNAME_END);
                }

                commandContext.HistoryManager.RecordActivityEnd(execution, null);

                if (!(execution.CurrentFlowElement is SubProcess))
                {
                    Context.ProcessEngineConfiguration.EventDispatcher.DispatchEvent(ActivitiEventBuilder.CreateActivityEvent(ActivitiEventType.ACTIVITY_COMPLETED, flowNode.Id, flowNode.Name, execution.Id, execution.ProcessInstanceId, execution.ProcessDefinitionId, flowNode));
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="flowNode"></param>
        protected internal virtual void LeaveFlowNode(FlowNode flowNode)
        {
            log.LogDebug($"Leaving flow node {flowNode.GetType()} with id '{flowNode.Id}' by following it's {flowNode.OutgoingFlows.Count} outgoing sequenceflow");

            // Get default sequence flow (if set)
            string defaultSequenceFlowId = null;
            if (flowNode is Activity)
            {
                defaultSequenceFlowId = ((Activity)flowNode).DefaultFlow;
            }
            else if (flowNode is Gateway)
            {
                defaultSequenceFlowId = ((Gateway)flowNode).DefaultFlow;
            }

            // Determine which sequence flows can be used for leaving
            IList<SequenceFlow> outgoingSequenceFlows = new List<SequenceFlow>();
            foreach (SequenceFlow sequenceFlow in flowNode.OutgoingFlows)
            {
                string skipExpressionString = sequenceFlow.SkipExpression;
                if (!SkipExpressionUtil.IsSkipExpressionEnabled(execution, skipExpressionString))
                {
                    if (!evaluateConditions || (evaluateConditions && ConditionUtil.HasTrueCondition(sequenceFlow, execution) && (defaultSequenceFlowId is null || !defaultSequenceFlowId.Equals(sequenceFlow.Id))))
                    {
                        outgoingSequenceFlows.Add(sequenceFlow);
                    }
                }
                else if (flowNode.OutgoingFlows.Count == 1 || SkipExpressionUtil.ShouldSkipFlowElement(commandContext, execution, skipExpressionString))
                {
                    // The 'skip' for a sequence flow means that we skip the condition, not the sequence flow.
                    outgoingSequenceFlows.Add(sequenceFlow);
                }
            }

            // Check if there is a default sequence flow
            if (outgoingSequenceFlows.Count == 0 && evaluateConditions)
            { // The elements that set this to false also have no support for default sequence flow
                if (defaultSequenceFlowId is object)
                {
                    foreach (SequenceFlow sequenceFlow in flowNode.OutgoingFlows)
                    {
                        if (defaultSequenceFlowId.Equals(sequenceFlow.Id))
                        {
                            outgoingSequenceFlows.Add(sequenceFlow);
                            break;
                        }
                    }
                }
            }

            // No outgoing found. Ending the execution
            if (outgoingSequenceFlows.Count == 0)
            {
                if (flowNode.OutgoingFlows == null || flowNode.OutgoingFlows.Count == 0)
                {
                    log.LogDebug($"No outgoing sequence flow found for flow node '{flowNode.Id}'.");
                    Context.Agenda.PlanEndExecutionOperation(execution);
                }
                else
                {
                    throw new ActivitiException("No outgoing sequence flow of element '" + flowNode.Id + "' could be selected for continuing the process");
                }
            }
            else
            {

                // Leave, and reuse the incoming sequence flow, make executions for all the others (if applicable)

                IExecutionEntityManager executionEntityManager = commandContext.ExecutionEntityManager;
                IList<IExecutionEntity> outgoingExecutions = new List<IExecutionEntity>(flowNode.OutgoingFlows.Count);

                SequenceFlow sequenceFlow = outgoingSequenceFlows[0];

                // Reuse existing one
                execution.CurrentFlowElement = sequenceFlow;
                execution.IsActive = true;
                outgoingExecutions.Add(execution);

                // Executions for all the other one
                if (outgoingSequenceFlows.Count > 1)
                {
                    for (int i = 1; i < outgoingSequenceFlows.Count; i++)
                    {
                        IExecutionEntity parent = execution.ParentId is object ? execution.Parent : execution;
                        IExecutionEntity outgoingExecutionEntity = commandContext.ExecutionEntityManager.CreateChildExecution(parent);

                        SequenceFlow outgoingSequenceFlow = outgoingSequenceFlows[i];
                        outgoingExecutionEntity.CurrentFlowElement = outgoingSequenceFlow;

                        executionEntityManager.Insert(outgoingExecutionEntity);
                        outgoingExecutions.Add(outgoingExecutionEntity);
                    }
                }

                // Leave (only done when all executions have been made, since some queries depend on this)
                foreach (IExecutionEntity outgoingExecution in outgoingExecutions)
                {
                    Context.Agenda.PlanContinueProcessOperation(outgoingExecution);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="flowNode"></param>
        protected internal virtual void HandleAdhocSubProcess(FlowNode flowNode)
        {
            bool completeAdhocSubProcess = false;
            AdhocSubProcess adhocSubProcess = (AdhocSubProcess)flowNode.ParentContainer;
            if (adhocSubProcess.CompletionCondition is object)
            {
                IExpression expr = Context.ProcessEngineConfiguration.ExpressionManager.CreateExpression(adhocSubProcess.CompletionCondition);
                bool adHoc = Convert.ToBoolean(expr.GetValue(execution));
                if (adHoc)
                {
                    completeAdhocSubProcess = true;
                }
                //ICondition condition = new UelExpressionCondition(expression);
                //if (condition.evaluate(adhocSubProcess.Id, execution))
                //{
                //    completeAdhocSubProcess = true;
                //}
            }

            if (flowNode.OutgoingFlows.Count > 0)
            {
                LeaveFlowNode(flowNode);
            }
            else
            {
                commandContext.ExecutionEntityManager.DeleteExecutionAndRelatedData(execution, null, false);
            }

            if (completeAdhocSubProcess)
            {
                bool endAdhocSubProcess = true;
                if (!adhocSubProcess.CancelRemainingInstances)
                {
                    IList<IExecutionEntity> childExecutions = commandContext.ExecutionEntityManager.FindChildExecutionsByParentExecutionId(execution.ParentId);
                    foreach (IExecutionEntity executionEntity in childExecutions)
                    {
                        if (!executionEntity.Id.Equals(execution.Id))
                        {
                            endAdhocSubProcess = false;
                            break;
                        }
                    }
                }

                if (endAdhocSubProcess)
                {
                    Context.Agenda.PlanEndExecutionOperation(execution.Parent);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected internal virtual void HandleSequenceFlow()
        {
            commandContext.HistoryManager.RecordActivityEnd(execution, null);
            Context.Agenda.PlanContinueProcessOperation(execution);
        }

        /// <summary>
        /// 
        /// </summary>
        protected internal virtual void CleanupCompensation()
        {
            // The compensation is at the end here. Simply stop the execution.
            commandContext.HistoryManager.RecordActivityEnd(execution, null);
            commandContext.ExecutionEntityManager.DeleteExecutionAndRelatedData(execution, null, false);

            IExecutionEntity parentExecutionEntity = execution.Parent;
            if (parentExecutionEntity.IsScope && !parentExecutionEntity.ProcessInstanceType)
            {

                if (AllChildExecutionsEnded(parentExecutionEntity, null))
                {
                    // Go up the hierarchy to check if the next scope is ended too.
                    // This could happen if only the compensation activity is still active, but the
                    // main process is already finished.

                    IExecutionEntity executionEntityToEnd = parentExecutionEntity;
                    IExecutionEntity scopeExecutionEntity = FindNextParentScopeExecutionWithAllEndedChildExecutions(parentExecutionEntity, parentExecutionEntity);
                    while (scopeExecutionEntity != null)
                    {
                        executionEntityToEnd = scopeExecutionEntity;
                        scopeExecutionEntity = FindNextParentScopeExecutionWithAllEndedChildExecutions(scopeExecutionEntity, parentExecutionEntity);
                    }

                    if (executionEntityToEnd.ProcessInstanceType)
                    {
                        Context.Agenda.PlanEndExecutionOperation(executionEntityToEnd);
                    }
                    else
                    {
                        Context.Agenda.PlanDestroyScopeOperation(executionEntityToEnd);
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="currentFlowElement"></param>
        protected internal virtual void CleanupExecutions(FlowElement currentFlowElement)
        {
            if (execution.ParentId is object && execution.IsScope)
            {
                // If the execution is a scope (and not a process instance), the scope must first be
                // destroyed before we can continue and follow the sequence flow
                Context.Agenda.PlanDestroyScopeOperation(execution);
            }
            else if (currentFlowElement is Activity activity)
            {
                // If the current activity is an activity, we need to remove any currently active boundary events

                if (CollectionUtil.IsNotEmpty(activity.BoundaryEvents))
                {
                    // Cancel events are not removed
                    IList<string> notToDeleteEvents = new List<string>();
                    foreach (BoundaryEvent @event in activity.BoundaryEvents)
                    {
                        if (CollectionUtil.IsNotEmpty(@event.EventDefinitions) && @event.EventDefinitions[0] is CancelEventDefinition)
                        {
                            notToDeleteEvents.Add(@event.Id);
                        }
                    }

                    // Delete all child executions
                    ICollection<IExecutionEntity> childExecutions = commandContext.ExecutionEntityManager.FindChildExecutionsByParentExecutionId(execution.Id);
                    foreach (IExecutionEntity childExecution in childExecutions)
                    {
                        if (childExecution.CurrentFlowElement == null || !notToDeleteEvents.Contains(childExecution.CurrentFlowElement.Id))
                        {
                            commandContext.ExecutionEntityManager.DeleteExecutionAndRelatedData(childExecution, null, false);
                        }
                    }
                }
            }
        }

        // Compensation helper methods

        ///<summary>
        /// <param name="executionEntity">The execution entity</param>
        /// <param name="executionEntityToIgnore"> The execution entity which we can ignore to be ended,
        /// as it's the execution currently being handled in this operation. </param>
        /// </summary>
        protected internal virtual IExecutionEntity FindNextParentScopeExecutionWithAllEndedChildExecutions(IExecutionEntity executionEntity, IExecutionEntity executionEntityToIgnore)
        {
            if (executionEntity.ParentId is object)
            {
                IExecutionEntity scopeExecutionEntity = executionEntity.Parent;

                // Find next scope
                while (!scopeExecutionEntity.IsScope || !scopeExecutionEntity.ProcessInstanceType)
                {
                    scopeExecutionEntity = scopeExecutionEntity.Parent;
                }

                // Return when all child executions for it are ended
                if (AllChildExecutionsEnded(scopeExecutionEntity, executionEntityToIgnore))
                {
                    return scopeExecutionEntity;
                }
            }
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parentExecutionEntity"></param>
        /// <param name="executionEntityToIgnore"></param>
        /// <returns></returns>
        protected internal virtual bool AllChildExecutionsEnded(IExecutionEntity parentExecutionEntity, IExecutionEntity executionEntityToIgnore)
        {
            foreach (IExecutionEntity childExecutionEntity in parentExecutionEntity.Executions)
            {
                if (executionEntityToIgnore == null || !executionEntityToIgnore.Id.Equals(childExecutionEntity.Id))
                {
                    if (!childExecutionEntity.Ended)
                    {
                        return false;
                    }
                    if (childExecutionEntity.Executions != null && childExecutionEntity.Executions.Count > 0)
                    {
                        if (!AllChildExecutionsEnded(childExecutionEntity, executionEntityToIgnore))
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }
    }
}