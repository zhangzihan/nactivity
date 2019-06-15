namespace org.activiti.engine.impl.agenda
{
    using Microsoft.Extensions.Logging;
    using org.activiti.bpmn.model;
    using org.activiti.engine.impl.@delegate;
    using org.activiti.engine.impl.interceptor;
    using org.activiti.engine.impl.persistence.entity;
    using Sys;
    using Sys.Workflow;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Operation that triggers a wait state and continues the process, leaving that activity.
    /// 
    /// The <seealso cref="IExecutionEntity"/> for this operations should be in a wait state (receive task for example)
    /// and have a <seealso cref="FlowElement"/> that has a behaviour that implements the <seealso cref="ITriggerableActivityBehavior"/>.
    /// 
    /// 
    /// </summary>
    public class TriggerExecutionOperation : AbstractOperation
    {
        private readonly ILogger<TriggerExecutionOperation> logger = ProcessEngineServiceProvider.LoggerService<TriggerExecutionOperation>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="commandContext"></param>
        /// <param name="execution"></param>
        public TriggerExecutionOperation(ICommandContext commandContext, IExecutionEntity execution) : this(commandContext, execution, null)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="commandContext"></param>
        /// <param name="execution"></param>
        public TriggerExecutionOperation(ICommandContext commandContext, IExecutionEntity execution, object signalData) : base(commandContext, execution, signalData)
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
                if (currentFlowElement is FlowNode node)
                {
                    IActivityBehavior activityBehavior = (IActivityBehavior)node.Behavior;
                    if (activityBehavior is ITriggerableActivityBehavior behavior)
                    {
                        if (currentFlowElement is BoundaryEvent)
                        {
                            commandContext.HistoryManager.RecordActivityStart(execution);
                        }

                        behavior.Trigger(execution, null, SignalData, false);

                        if (currentFlowElement is BoundaryEvent)
                        {
                            commandContext.HistoryManager.RecordActivityEnd(execution, null);
                        }

                    }
                    else
                    {
                        throw new ActivitiException("Invalid behavior: " + activityBehavior + " should implement " + typeof(ITriggerableActivityBehavior).FullName);
                    }

                }
                else
                {
                    throw new ActivitiException("Programmatic error: no current flow element found or invalid type: " + currentFlowElement + ". Halting.");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                throw;
            }
        }
    }
}