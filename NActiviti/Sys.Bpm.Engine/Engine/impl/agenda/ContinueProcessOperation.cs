using System;
using System.Collections.Generic;

namespace Sys.Workflow.engine.impl.agenda
{
    using Microsoft.Extensions.Logging;
    using Sys.Workflow.bpmn.model;
    using Sys.Workflow.engine.@delegate;
    using Sys.Workflow.engine.@delegate.@event;
    using Sys.Workflow.engine.@delegate.@event.impl;
    using Sys.Workflow.engine.impl.cfg;
    using Sys.Workflow.engine.impl.context;
    using Sys.Workflow.engine.impl.@delegate;
    using Sys.Workflow.engine.impl.interceptor;
    using Sys.Workflow.engine.impl.persistence.entity;
    using Sys.Workflow.engine.impl.util;
    using Sys.Workflow.engine.logging;
    using Sys.Workflow;

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

        /// <summary>
        /// 
        /// </summary>
        protected internal bool forceSynchronousOperation;

        /// <summary>
        /// 
        /// </summary>
        protected internal bool inCompensation;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="commandContext"></param>
        /// <param name="execution"></param>
        /// <param name="forceSynchronousOperation"></param>
        /// <param name="inCompensation"></param>
        public ContinueProcessOperation(ICommandContext commandContext, IExecutionEntity execution, bool forceSynchronousOperation, bool inCompensation) : base(commandContext, execution)
        {

            this.forceSynchronousOperation = forceSynchronousOperation;
            this.inCompensation = inCompensation;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="commandContext"></param>
        /// <param name="execution"></param>
        public ContinueProcessOperation(ICommandContext commandContext, IExecutionEntity execution) : this(commandContext, execution, false, false)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void RunOperation()
        {
            try
            {
                FlowElement currentFlowElement = GetCurrentFlowElement(execution);
                if (currentFlowElement is FlowNode)
                {
                    ContinueThroughFlowNode((FlowNode)currentFlowElement);
                }
                else if (currentFlowElement is SequenceFlow)
                {
                    ContinueThroughSequenceFlow((SequenceFlow)currentFlowElement);
                }
                else
                {
                    throw new ActivitiException("Programmatic error: no current flow element found or invalid type: " + currentFlowElement + ". Halting.");
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
        protected internal virtual void ExecuteProcessStartExecutionListeners()
        {
            Process process = ProcessDefinitionUtil.GetProcess(execution.ProcessDefinitionId);
            ExecuteExecutionListeners(process, execution.Parent, BaseExecutionListenerFields.EVENTNAME_START);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="flowNode"></param>
        protected internal virtual void ContinueThroughFlowNode(FlowNode flowNode)
        {

            // Check if it's the initial flow element. If so, we must fire the execution listeners for the process too
            if (flowNode.IncomingFlows != null && flowNode.IncomingFlows.Count == 0 && flowNode.SubProcess == null)
            {
                ExecuteProcessStartExecutionListeners();
            }

            // For a subprocess, a new child execution is created that will visit the steps of the subprocess
            // The original execution that arrived here will wait until the subprocess is finished
            // and will then be used to continue the process instance.
            if (flowNode is SubProcess)
            {
                CreateChildExecutionForSubProcess((SubProcess)flowNode);
            }

            if (flowNode is Activity && ((Activity)flowNode).HasMultiInstanceLoopCharacteristics())
            {
                // the multi instance execution will look at async
                ExecuteMultiInstanceSynchronous(flowNode);
            }
            else if (forceSynchronousOperation || !flowNode.Asynchronous)
            {
                ExecuteSynchronous(flowNode);
            }
            else
            {
                ExecuteAsynchronous(flowNode);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="subProcess"></param>
        protected internal virtual void CreateChildExecutionForSubProcess(SubProcess subProcess)
        {
            IExecutionEntity parentScopeExecution = FindFirstParentScopeExecution(execution);

            // Create the sub process execution that can be used to set variables
            // We create a new execution and delete the incoming one to have a proper scope that
            // does not conflict anything with any existing scopes

            IExecutionEntity subProcessExecution = commandContext.ExecutionEntityManager.CreateChildExecution(parentScopeExecution);
            subProcessExecution.CurrentFlowElement = subProcess;
            subProcessExecution.IsScope = true;

            commandContext.ExecutionEntityManager.DeleteExecutionAndRelatedData(execution, null, false);
            execution = subProcessExecution;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="flowNode"></param>
        protected internal virtual void ExecuteSynchronous(FlowNode flowNode)
        {
            commandContext.HistoryManager.RecordActivityStart(execution);

            // Execution listener: event 'start'
            if (CollectionUtil.IsNotEmpty(flowNode.ExecutionListeners))
            {
                ExecuteExecutionListeners(flowNode, BaseExecutionListenerFields.EVENTNAME_START);
            }

            // Execute any boundary events, sub process boundary events will be executed from the activity behavior
            if (!inCompensation && flowNode is Activity)
            { // Only activities can have boundary events
                IList<BoundaryEvent> boundaryEvents = ((Activity)flowNode).BoundaryEvents;
                if (CollectionUtil.IsNotEmpty(boundaryEvents))
                {
                    if (string.IsNullOrWhiteSpace(execution.Name))
                    {
                        execution.Name = flowNode.Name;
                    }
                    ExecuteBoundaryEvents(boundaryEvents, execution);
                }
            }

            // Execute actual behavior
            IActivityBehavior activityBehavior = (IActivityBehavior)flowNode.Behavior;

            if (activityBehavior != null)
            {
                ExecuteActivityBehavior(activityBehavior, flowNode);
            }
            else
            {
                log.LogDebug($"No activityBehavior on activity '{flowNode.Id}' with execution {execution.Id}");
                Context.Agenda.PlanTakeOutgoingSequenceFlowsOperation(execution, true);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="flowNode"></param>
        protected internal virtual void ExecuteAsynchronous(FlowNode flowNode)
        {
            IJobEntity job = commandContext.JobManager.CreateAsyncJob(execution, flowNode.Exclusive);
            commandContext.JobManager.ScheduleAsyncJob(job);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="flowNode"></param>
        protected internal virtual void ExecuteMultiInstanceSynchronous(FlowNode flowNode)
        {
            // Execution listener: event 'start'
            if (CollectionUtil.IsNotEmpty(flowNode.ExecutionListeners))
            {
                ExecuteExecutionListeners(flowNode, BaseExecutionListenerFields.EVENTNAME_START);
            }

            // Execute any boundary events, sub process boundary events will be executed from the activity behavior
            if (!inCompensation && flowNode is Activity)
            { // Only activities can have boundary events
                IList<BoundaryEvent> boundaryEvents = ((Activity)flowNode).BoundaryEvents;
                if (CollectionUtil.IsNotEmpty(boundaryEvents))
                {
                    ExecuteBoundaryEvents(boundaryEvents, execution);
                }
            }

            // Execute the multi instance behavior
            IActivityBehavior activityBehavior = (IActivityBehavior)flowNode.Behavior;

            if (activityBehavior != null)
            {
                ExecuteActivityBehavior(activityBehavior, flowNode);
            }
            else
            {
                throw new ActivitiException("Expected an activity behavior in flow node " + flowNode.Id);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="activityBehavior"></param>
        /// <param name="flowNode"></param>
        protected internal virtual void ExecuteActivityBehavior(IActivityBehavior activityBehavior, FlowNode flowNode)
        {
            log.LogDebug($"Executing activityBehavior {activityBehavior.GetType()} on activity '{flowNode.Id}' with execution {execution.Id}");

            ProcessEngineConfigurationImpl processEngineConfiguration = Context.ProcessEngineConfiguration;
            if (processEngineConfiguration != null && processEngineConfiguration.EventDispatcher.Enabled)
            {
                processEngineConfiguration.EventDispatcher.DispatchEvent(ActivitiEventBuilder.CreateActivityEvent(ActivitiEventType.ACTIVITY_STARTED, flowNode.Id, flowNode.Name, execution.Id, execution.ProcessInstanceId, execution.ProcessDefinitionId, flowNode));
            }

            try
            {
                activityBehavior.Execute(execution);
            }
            catch (Exception e)
            {
                throw new Exception($"{e.Message}", e);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sequenceFlow"></param>
        protected internal virtual void ContinueThroughSequenceFlow(SequenceFlow sequenceFlow)
        {
            // Execution listener. Sequenceflow only 'take' makes sense ... but we've supported all three since the beginning
            if (CollectionUtil.IsNotEmpty(sequenceFlow.ExecutionListeners))
            {
                ExecuteExecutionListeners(sequenceFlow, BaseExecutionListenerFields.EVENTNAME_START);
                ExecuteExecutionListeners(sequenceFlow, BaseExecutionListenerFields.EVENTNAME_TAKE);
                ExecuteExecutionListeners(sequenceFlow, BaseExecutionListenerFields.EVENTNAME_END);
            }

            // Firing event that transition is being taken
            ProcessEngineConfigurationImpl processEngineConfiguration = Context.ProcessEngineConfiguration;
            if (processEngineConfiguration != null && processEngineConfiguration.EventDispatcher.Enabled)
            {
                FlowElement sourceFlowElement = sequenceFlow.SourceFlowElement;
                FlowElement targetFlowElement = sequenceFlow.TargetFlowElement;

                IActivitiSequenceFlowTakenEvent asft = ActivitiEventBuilder.CreateSequenceFlowTakenEvent(execution, ActivitiEventType.SEQUENCEFLOW_TAKEN, sequenceFlow.Id,
                    sourceFlowElement?.Id,
                    sourceFlowElement?.Name,
                    sourceFlowElement?.GetType().FullName,
                    sourceFlowElement == null ? null : ((FlowNode)sourceFlowElement).Behavior,
                    targetFlowElement?.Id,
                    targetFlowElement?.Name,
                    targetFlowElement?.GetType().FullName,
                    targetFlowElement == null ? null : ((FlowNode)targetFlowElement).Behavior);
                processEngineConfiguration.EventDispatcher.DispatchEvent(asft);
            }

            {
                FlowElement targetFlowElement = sequenceFlow.TargetFlowElement;
                execution.CurrentFlowElement = targetFlowElement;

                log.LogDebug($"Sequence flow '{sequenceFlow.Id}' encountered. Continuing process by following it using execution {execution.Id}");
                Context.Agenda.PlanContinueProcessOperation(execution);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="boundaryEvents"></param>
        /// <param name="execution"></param>
        protected internal virtual void ExecuteBoundaryEvents(ICollection<BoundaryEvent> boundaryEvents, IExecutionEntity execution)
        {

            // The parent execution becomes a scope, and a child execution is created for each of the boundary events
            foreach (BoundaryEvent boundaryEvent in boundaryEvents)
            {

                if (CollectionUtil.IsEmpty(boundaryEvent.EventDefinitions) || (boundaryEvent.EventDefinitions[0] is CompensateEventDefinition))
                {
                    continue;
                }

                // A Child execution of the current execution is created to represent the boundary event being active
                IExecutionEntity childExecutionEntity = commandContext.ExecutionEntityManager.CreateChildExecution(execution);
                childExecutionEntity.ParentId = execution.Id;
                childExecutionEntity.CurrentFlowElement = boundaryEvent;
                childExecutionEntity.IsScope = false;

                IActivityBehavior boundaryEventBehavior = ((IActivityBehavior)boundaryEvent.Behavior);
                log.LogDebug($"Executing boundary event activityBehavior {boundaryEventBehavior.GetType()} with execution {childExecutionEntity.Id}");
                boundaryEventBehavior.Execute(childExecutionEntity);
            }
        }
    }
}