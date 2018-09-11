namespace org.activiti.engine.impl.agenda
{
    using Microsoft.Extensions.Logging;
    using org.activiti.bpmn.model;
    using org.activiti.engine.impl.@delegate;
    using org.activiti.engine.impl.interceptor;
    using org.activiti.engine.impl.persistence.entity;
    using Sys;

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
        public TriggerExecutionOperation(ICommandContext commandContext, IExecutionEntity execution) : base(commandContext, execution)
        {
        }

        protected override void run()
        {
            FlowElement currentFlowElement = getCurrentFlowElement(execution);
            if (currentFlowElement is FlowNode)
            {

                IActivityBehavior activityBehavior = (IActivityBehavior)((FlowNode)currentFlowElement).Behavior;
                if (activityBehavior is ITriggerableActivityBehavior)
                {

                    if (currentFlowElement is BoundaryEvent)
                    {
                        commandContext.HistoryManager.recordActivityStart(execution);
                    }

                  ((ITriggerableActivityBehavior)activityBehavior).trigger(execution, null, null);

                    if (currentFlowElement is BoundaryEvent)
                    {
                        commandContext.HistoryManager.recordActivityEnd(execution, null);
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

    }

}