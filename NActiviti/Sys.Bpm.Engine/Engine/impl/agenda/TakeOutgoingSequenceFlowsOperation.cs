using System.Collections.Generic;

namespace org.activiti.engine.impl.agenda
{
    using Microsoft.Extensions.Logging;
    using org.activiti.bpmn.model;
    using org.activiti.engine.@delegate;
    using org.activiti.engine.@delegate.@event;
    using org.activiti.engine.@delegate.@event.impl;
    using org.activiti.engine.impl.bpmn.helper;
    using org.activiti.engine.impl.context;
    using org.activiti.engine.impl.el;
    using org.activiti.engine.impl.interceptor;
    using org.activiti.engine.impl.persistence.entity;
    using org.activiti.engine.impl.util;
    using org.activiti.engine.impl.util.condition;
    using Sys;

    /// <summary>
    /// Operation that leaves the <seealso cref="FlowElement"/> where the <seealso cref="IExecutionEntity"/> is currently at
    /// and leaves it following the sequence flow.
    /// </summary>
    public class TakeOutgoingSequenceFlowsOperation : AbstractOperation
    {
        private static readonly ILogger<TakeOutgoingSequenceFlowsOperation> log = ProcessEngineServiceProvider.LoggerService<TakeOutgoingSequenceFlowsOperation>();

        protected internal bool evaluateConditions;

        public TakeOutgoingSequenceFlowsOperation(ICommandContext commandContext, IExecutionEntity executionEntity, bool evaluateConditions) : base(commandContext, executionEntity)
        {
            this.evaluateConditions = evaluateConditions;
        }

        protected override void run()
        {
            FlowElement currentFlowElement = getCurrentFlowElement(execution);

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

                cleanupCompensation();

                return;
            }

            // When leaving the current activity, we need to delete any related execution (eg active boundary events)
            cleanupExecutions(currentFlowElement);

            if (currentFlowElement is FlowNode)
            {
                handleFlowNode((FlowNode)currentFlowElement);
            }
            else if (currentFlowElement is SequenceFlow)
            {
                handleSequenceFlow();
            }
        }

        protected internal virtual void handleFlowNode(FlowNode flowNode)
        {
            handleActivityEnd(flowNode);
            if (flowNode.ParentContainer != null && flowNode.ParentContainer is AdhocSubProcess)
            {
                handleAdhocSubProcess(flowNode);
            }
            else
            {
                leaveFlowNode(flowNode);
            }
        }

        protected internal virtual void handleActivityEnd(FlowNode flowNode)
        {
            // a process instance execution can never leave a flow node, but it can pass here whilst cleaning up
            // hence the check for NOT being a process instance
            if (!execution.ProcessInstanceType)
            {

                if (CollectionUtil.IsNotEmpty(flowNode.ExecutionListeners))
                {
                    executeExecutionListeners(flowNode, BaseExecutionListener_Fields.EVENTNAME_END);
                }

                commandContext.HistoryManager.recordActivityEnd(execution, null);

                if (!(execution.CurrentFlowElement is SubProcess))
                {
                    Context.ProcessEngineConfiguration.EventDispatcher.dispatchEvent(ActivitiEventBuilder.createActivityEvent(ActivitiEventType.ACTIVITY_COMPLETED, flowNode.Id, flowNode.Name, execution.Id, execution.ProcessInstanceId, execution.ProcessDefinitionId, flowNode));
                }
            }
        }

        protected internal virtual void leaveFlowNode(FlowNode flowNode)
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
                if (!SkipExpressionUtil.isSkipExpressionEnabled(execution, skipExpressionString))
                {

                    if (!evaluateConditions || (evaluateConditions && ConditionUtil.hasTrueCondition(sequenceFlow, execution) && (string.ReferenceEquals(defaultSequenceFlowId, null) || !defaultSequenceFlowId.Equals(sequenceFlow.Id))))
                    {
                        outgoingSequenceFlows.Add(sequenceFlow);
                    }
                }
                else if (flowNode.OutgoingFlows.Count == 1 || SkipExpressionUtil.shouldSkipFlowElement(commandContext, execution, skipExpressionString))
                {
                    // The 'skip' for a sequence flow means that we skip the condition, not the sequence flow.
                    outgoingSequenceFlows.Add(sequenceFlow);
                }
            }

            // Check if there is a default sequence flow
            if (outgoingSequenceFlows.Count == 0 && evaluateConditions)
            { // The elements that set this to false also have no support for default sequence flow
                if (!string.ReferenceEquals(defaultSequenceFlowId, null))
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
                    Context.Agenda.planEndExecutionOperation(execution);
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

                        IExecutionEntity parent = !string.ReferenceEquals(execution.ParentId, null) ? execution.Parent : execution;
                        IExecutionEntity outgoingExecutionEntity = commandContext.ExecutionEntityManager.createChildExecution(parent);

                        SequenceFlow outgoingSequenceFlow = outgoingSequenceFlows[i];
                        outgoingExecutionEntity.CurrentFlowElement = outgoingSequenceFlow;

                        executionEntityManager.insert(outgoingExecutionEntity);
                        outgoingExecutions.Add(outgoingExecutionEntity);
                    }
                }

                // Leave (only done when all executions have been made, since some queries depend on this)
                foreach (IExecutionEntity outgoingExecution in outgoingExecutions)
                {
                    Context.Agenda.planContinueProcessOperation(outgoingExecution);
                }
            }
        }

        protected internal virtual void handleAdhocSubProcess(FlowNode flowNode)
        {
            bool completeAdhocSubProcess = false;
            AdhocSubProcess adhocSubProcess = (AdhocSubProcess)flowNode.ParentContainer;
            if (!string.ReferenceEquals(adhocSubProcess.CompletionCondition, null))
            {
                IExpression expression = Context.ProcessEngineConfiguration.ExpressionManager.createExpression(adhocSubProcess.CompletionCondition);
                //ICondition condition = new UelExpressionCondition(expression);
                //if (condition.evaluate(adhocSubProcess.Id, execution))
                //{
                //    completeAdhocSubProcess = true;
                //}
            }

            if (flowNode.OutgoingFlows.Count > 0)
            {
                leaveFlowNode(flowNode);
            }
            else
            {
                commandContext.ExecutionEntityManager.deleteExecutionAndRelatedData(execution, null, false);
            }

            if (completeAdhocSubProcess)
            {
                bool endAdhocSubProcess = true;
                if (!adhocSubProcess.CancelRemainingInstances)
                {
                    IList<IExecutionEntity> childExecutions = commandContext.ExecutionEntityManager.findChildExecutionsByParentExecutionId(execution.ParentId);
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
                    Context.Agenda.planEndExecutionOperation(execution.Parent);
                }
            }
        }

        protected internal virtual void handleSequenceFlow()
        {
            commandContext.HistoryManager.recordActivityEnd(execution, null);
            Context.Agenda.planContinueProcessOperation(execution);
        }

        protected internal virtual void cleanupCompensation()
        {

            // The compensation is at the end here. Simply stop the execution.

            commandContext.HistoryManager.recordActivityEnd(execution, null);
            commandContext.ExecutionEntityManager.deleteExecutionAndRelatedData(execution, null, false);

            IExecutionEntity parentExecutionEntity = execution.Parent;
            if (parentExecutionEntity.IsScope && !parentExecutionEntity.ProcessInstanceType)
            {

                if (allChildExecutionsEnded(parentExecutionEntity, null))
                {

                    // Go up the hierarchy to check if the next scope is ended too.
                    // This could happen if only the compensation activity is still active, but the
                    // main process is already finished.

                    IExecutionEntity executionEntityToEnd = parentExecutionEntity;
                    IExecutionEntity scopeExecutionEntity = findNextParentScopeExecutionWithAllEndedChildExecutions(parentExecutionEntity, parentExecutionEntity);
                    while (scopeExecutionEntity != null)
                    {
                        executionEntityToEnd = scopeExecutionEntity;
                        scopeExecutionEntity = findNextParentScopeExecutionWithAllEndedChildExecutions(scopeExecutionEntity, parentExecutionEntity);
                    }

                    if (executionEntityToEnd.ProcessInstanceType)
                    {
                        Context.Agenda.planEndExecutionOperation(executionEntityToEnd);
                    }
                    else
                    {
                        Context.Agenda.planDestroyScopeOperation(executionEntityToEnd);
                    }
                }
            }
        }

        protected internal virtual void cleanupExecutions(FlowElement currentFlowElement)
        {
            if (!string.ReferenceEquals(execution.ParentId, null) && execution.IsScope)
            {

                // If the execution is a scope (and not a process instance), the scope must first be
                // destroyed before we can continue and follow the sequence flow

                Context.Agenda.planDestroyScopeOperation(execution);
            }
            else if (currentFlowElement is Activity)
            {

                // If the current activity is an activity, we need to remove any currently active boundary events

                Activity activity = (Activity)currentFlowElement;
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
                    ICollection<IExecutionEntity> childExecutions = commandContext.ExecutionEntityManager.findChildExecutionsByParentExecutionId(execution.Id);
                    foreach (IExecutionEntity childExecution in childExecutions)
                    {
                        if (childExecution.CurrentFlowElement == null || !notToDeleteEvents.Contains(childExecution.CurrentFlowElement.Id))
                        {
                            commandContext.ExecutionEntityManager.deleteExecutionAndRelatedData(childExecution, null, false);
                        }
                    }
                }
            }
        }

        // Compensation helper methods

        /// <param name="executionEntityToIgnore"> The execution entity which we can ignore to be ended,
        /// as it's the execution currently being handled in this operation. </param>
        protected internal virtual IExecutionEntity findNextParentScopeExecutionWithAllEndedChildExecutions(IExecutionEntity executionEntity, IExecutionEntity executionEntityToIgnore)
        {
            if (!string.ReferenceEquals(executionEntity.ParentId, null))
            {
                IExecutionEntity scopeExecutionEntity = executionEntity.Parent;

                // Find next scope
                while (!scopeExecutionEntity.IsScope || !scopeExecutionEntity.ProcessInstanceType)
                {
                    scopeExecutionEntity = scopeExecutionEntity.Parent;
                }

                // Return when all child executions for it are ended
                if (allChildExecutionsEnded(scopeExecutionEntity, executionEntityToIgnore))
                {
                    return scopeExecutionEntity;
                }
            }
            return null;
        }

        protected internal virtual bool allChildExecutionsEnded(IExecutionEntity parentExecutionEntity, IExecutionEntity executionEntityToIgnore)
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
                        if (!allChildExecutionsEnded(childExecutionEntity, executionEntityToIgnore))
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