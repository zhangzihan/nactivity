using System;
using System.Collections.Generic;

namespace org.activiti.engine.impl.agenda
{
    using Microsoft.Extensions.Logging;
    using org.activiti.bpmn.model;
    using org.activiti.engine.@delegate;
    using org.activiti.engine.@delegate.@event;
    using org.activiti.engine.@delegate.@event.impl;
    using org.activiti.engine.impl.context;
    using org.activiti.engine.impl.@delegate;
    using org.activiti.engine.impl.interceptor;
    using org.activiti.engine.impl.persistence.entity;
    using org.activiti.engine.impl.util;
    using org.activiti.engine.logging;
    using Sys;

    /// <summary>
    /// Operation that takes the current <seealso cref="FlowElement"/> set on the <seealso cref="IExecutionEntity"/>
    /// and executes the associated <seealso cref="IActivityBehavior"/>. In the case of async, schedules a <seealso cref="IJob"/>.
    /// <para>
    /// Also makes sure the <seealso cref="IExecutionListener"/> instances are called.
    /// </para>
    /// </summary>
    public class ContinueProcessOperation : AbstractOperation
    {
        private static readonly ILogger<ContinueProcessOperation> log = ProcessEngineServiceProvider.LoggerService<ContinueProcessOperation>();

        protected internal bool forceSynchronousOperation;
        protected internal bool inCompensation;

        public ContinueProcessOperation(ICommandContext commandContext, IExecutionEntity execution, bool forceSynchronousOperation, bool inCompensation) : base(commandContext, execution)
        {

            this.forceSynchronousOperation = forceSynchronousOperation;
            this.inCompensation = inCompensation;
        }

        public ContinueProcessOperation(ICommandContext commandContext, IExecutionEntity execution) : this(commandContext, execution, false, false)
        {
        }

        protected override void run()
        {
            FlowElement currentFlowElement = getCurrentFlowElement(execution);
            if (currentFlowElement is FlowNode)
            {
                continueThroughFlowNode((FlowNode)currentFlowElement);
            }
            else if (currentFlowElement is SequenceFlow)
            {
                continueThroughSequenceFlow((SequenceFlow)currentFlowElement);
            }
            else
            {
                throw new ActivitiException("Programmatic error: no current flow element found or invalid type: " + currentFlowElement + ". Halting.");
            }
        }

        protected internal virtual void executeProcessStartExecutionListeners()
        {
            Process process = ProcessDefinitionUtil.getProcess(execution.ProcessDefinitionId);
            executeExecutionListeners(process, execution.Parent, BaseExecutionListener_Fields.EVENTNAME_START);
        }

        protected internal virtual void continueThroughFlowNode(FlowNode flowNode)
        {

            // Check if it's the initial flow element. If so, we must fire the execution listeners for the process too
            if (flowNode.IncomingFlows != null && flowNode.IncomingFlows.Count == 0 && flowNode.SubProcess == null)
            {
                executeProcessStartExecutionListeners();
            }

            // For a subprocess, a new child execution is created that will visit the steps of the subprocess
            // The original execution that arrived here will wait until the subprocess is finished
            // and will then be used to continue the process instance.
            if (flowNode is SubProcess)
            {
                createChildExecutionForSubProcess((SubProcess)flowNode);
            }

            if (flowNode is Activity && ((Activity)flowNode).hasMultiInstanceLoopCharacteristics())
            {
                // the multi instance execution will look at async
                executeMultiInstanceSynchronous(flowNode);
            }
            else if (forceSynchronousOperation || !flowNode.Asynchronous)
            {
                executeSynchronous(flowNode);
            }
            else
            {
                executeAsynchronous(flowNode);
            }
        }

        protected internal virtual void createChildExecutionForSubProcess(SubProcess subProcess)
        {
            IExecutionEntity parentScopeExecution = findFirstParentScopeExecution(execution);

            // Create the sub process execution that can be used to set variables
            // We create a new execution and delete the incoming one to have a proper scope that
            // does not conflict anything with any existing scopes

            IExecutionEntity subProcessExecution = commandContext.ExecutionEntityManager.createChildExecution(parentScopeExecution);
            subProcessExecution.CurrentFlowElement = subProcess;
            subProcessExecution.IsScope = true;

            commandContext.ExecutionEntityManager.deleteExecutionAndRelatedData(execution, null, false);
            execution = subProcessExecution;
        }

        protected internal virtual void executeSynchronous(FlowNode flowNode)
        {
            commandContext.HistoryManager.recordActivityStart(execution);

            // Execution listener: event 'start'
            if (CollectionUtil.IsNotEmpty(flowNode.ExecutionListeners))
            {
                executeExecutionListeners(flowNode, BaseExecutionListener_Fields.EVENTNAME_START);
            }

            // Execute any boundary events, sub process boundary events will be executed from the activity behavior
            if (!inCompensation && flowNode is Activity)
            { // Only activities can have boundary events
                IList<BoundaryEvent> boundaryEvents = ((Activity)flowNode).BoundaryEvents;
                if (CollectionUtil.IsNotEmpty(boundaryEvents))
                {
                    executeBoundaryEvents(boundaryEvents, execution);
                }
            }

            // Execute actual behavior
            IActivityBehavior activityBehavior = (IActivityBehavior)flowNode.Behavior;

            if (activityBehavior != null)
            {
                executeActivityBehavior(activityBehavior, flowNode);
            }
            else
            {
                log.LogDebug($"No activityBehavior on activity '{flowNode.Id}' with execution {execution.Id}");
                Context.Agenda.planTakeOutgoingSequenceFlowsOperation(execution, true);
            }
        }

        protected internal virtual void executeAsynchronous(FlowNode flowNode)
        {
            IJobEntity job = commandContext.JobManager.createAsyncJob(execution, flowNode.Exclusive);
            commandContext.JobManager.scheduleAsyncJob(job);
        }

        protected internal virtual void executeMultiInstanceSynchronous(FlowNode flowNode)
        {

            // Execution listener: event 'start'
            if (CollectionUtil.IsNotEmpty(flowNode.ExecutionListeners))
            {
                executeExecutionListeners(flowNode, BaseExecutionListener_Fields.EVENTNAME_START);
            }

            // Execute any boundary events, sub process boundary events will be executed from the activity behavior
            if (!inCompensation && flowNode is Activity)
            { // Only activities can have boundary events
                IList<BoundaryEvent> boundaryEvents = ((Activity)flowNode).BoundaryEvents;
                if (CollectionUtil.IsNotEmpty(boundaryEvents))
                {
                    executeBoundaryEvents(boundaryEvents, execution);
                }
            }

            // Execute the multi instance behavior
            IActivityBehavior activityBehavior = (IActivityBehavior)flowNode.Behavior;

            if (activityBehavior != null)
            {
                executeActivityBehavior(activityBehavior, flowNode);
            }
            else
            {
                throw new ActivitiException("Expected an activity behavior in flow node " + flowNode.Id);
            }
        }

        protected internal virtual void executeActivityBehavior(IActivityBehavior activityBehavior, FlowNode flowNode)
        {
            log.LogDebug($"Executing activityBehavior {activityBehavior.GetType()} on activity '{flowNode.Id}' with execution {execution.Id}");

            if (Context.ProcessEngineConfiguration != null && Context.ProcessEngineConfiguration.EventDispatcher.Enabled)
            {
                Context.ProcessEngineConfiguration.EventDispatcher.dispatchEvent(ActivitiEventBuilder.createActivityEvent(ActivitiEventType.ACTIVITY_STARTED, flowNode.Id, flowNode.Name, execution.Id, execution.ProcessInstanceId, execution.ProcessDefinitionId, flowNode));
            }

            try
            {
                activityBehavior.execute(execution);
            }
            catch (Exception e)
            {
                if (LogMDC.MDCEnabled)
                {
                    LogMDC.putMDCExecution(execution);
                }
                throw e;
            }
        }

        protected internal virtual void continueThroughSequenceFlow(SequenceFlow sequenceFlow)
        {
            // Execution listener. Sequenceflow only 'take' makes sense ... but we've supported all three since the beginning
            if (CollectionUtil.IsNotEmpty(sequenceFlow.ExecutionListeners))
            {
                executeExecutionListeners(sequenceFlow, BaseExecutionListener_Fields.EVENTNAME_START);
                executeExecutionListeners(sequenceFlow, BaseExecutionListener_Fields.EVENTNAME_TAKE);
                executeExecutionListeners(sequenceFlow, BaseExecutionListener_Fields.EVENTNAME_END);
            }

            // Firing event that transition is being taken
            if (Context.ProcessEngineConfiguration != null && Context.ProcessEngineConfiguration.EventDispatcher.Enabled)
            {
                FlowElement sourceFlowElement = sequenceFlow.SourceFlowElement;
                FlowElement targetFlowElement = sequenceFlow.TargetFlowElement;

                IActivitiSequenceFlowTakenEvent asft = ActivitiEventBuilder.createSequenceFlowTakenEvent(execution, ActivitiEventType.SEQUENCEFLOW_TAKEN, sequenceFlow.Id,
                    sourceFlowElement != null ? sourceFlowElement.Id : null,
                    sourceFlowElement != null ? (string)sourceFlowElement.Name : null,
                    sourceFlowElement != null ? sourceFlowElement.GetType().FullName : null,
                    sourceFlowElement != null ? ((FlowNode)sourceFlowElement).Behavior : null,
                    targetFlowElement != null ? targetFlowElement.Id : null,
                    targetFlowElement != null ? targetFlowElement.Name : null,
                    targetFlowElement != null ? targetFlowElement.GetType().FullName : null,
                    targetFlowElement != null ? ((FlowNode)targetFlowElement).Behavior : null);
                Context.ProcessEngineConfiguration.EventDispatcher.dispatchEvent(asft);
            }

            {
                FlowElement targetFlowElement = sequenceFlow.TargetFlowElement;
                execution.CurrentFlowElement = targetFlowElement;

                log.LogDebug($"Sequence flow '{sequenceFlow.Id}' encountered. Continuing process by following it using execution {execution.Id}");
                Context.Agenda.planContinueProcessOperation(execution);
            }
        }

        protected internal virtual void executeBoundaryEvents(ICollection<BoundaryEvent> boundaryEvents, IExecutionEntity execution)
        {

            // The parent execution becomes a scope, and a child execution is created for each of the boundary events
            foreach (BoundaryEvent boundaryEvent in boundaryEvents)
            {

                if (CollectionUtil.IsEmpty(boundaryEvent.EventDefinitions) || (boundaryEvent.EventDefinitions[0] is CompensateEventDefinition))
                {
                    continue;
                }

                // A Child execution of the current execution is created to represent the boundary event being active
                IExecutionEntity childExecutionEntity = commandContext.ExecutionEntityManager.createChildExecution(execution);
                childExecutionEntity.ParentId = execution.Id;
                childExecutionEntity.CurrentFlowElement = boundaryEvent;
                childExecutionEntity.IsScope = false;

                IActivityBehavior boundaryEventBehavior = ((IActivityBehavior)boundaryEvent.Behavior);
                log.LogDebug($"Executing boundary event activityBehavior {boundaryEventBehavior.GetType()} with execution {childExecutionEntity.Id}");
                boundaryEventBehavior.execute(childExecutionEntity);
            }
        }
    }

}