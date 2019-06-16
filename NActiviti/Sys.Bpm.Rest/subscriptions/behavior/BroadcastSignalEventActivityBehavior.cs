using Sys.Workflow.bpmn.model;
using Sys.Workflow.cloud.services.api.commands;
using Sys.Workflow.engine.impl.bpmn.behavior;
using Sys.Workflow.engine.impl.context;
using Sys.Workflow.engine.impl.el;
using Sys.Workflow.engine.impl.interceptor;
using Sys.Workflow.engine.impl.persistence.entity;
using Sys.Workflow.engine.runtime;
using org.springframework.context;

namespace Sys.Workflow.services.subscriptions.behavior
{
    /// <summary>
    /// 
    /// </summary>
    public class BroadcastSignalEventActivityBehavior : IntermediateThrowSignalEventActivityBehavior
    {
        private const long serialVersionUID = 1L;

        private readonly IApplicationEventPublisher eventPublisher;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventPublisher"></param>
        /// <param name="signalEventDefinition"></param>
        /// <param name="signal"></param>
        public BroadcastSignalEventActivityBehavior(IApplicationEventPublisher eventPublisher, SignalEventDefinition signalEventDefinition, Signal signal) : base(signalEventDefinition, signal)
        {
            this.eventPublisher = eventPublisher;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="execution"></param>
        public override void Execute(IExecutionEntity execution)
        {
            base.Execute(execution);
            _ = Context.CommandContext;
            string eventSubscriptionName;
            if (signalEventName != null)
            {
                eventSubscriptionName = signalEventName;
            }
            else
            {
                eventSubscriptionName = new ExpressionManager().CreateExpression(signalExpression)
                    .GetValue(execution).ToString();

                //Expression expressionObject = commandContext.ProcessEngineConfiguration.ExpressionManager.createExpression(signalExpression);
                //eventSubscriptionName = expressionObject.getValue(execution).ToString();
            }

            eventPublisher.PublishEvent(new SignalCmd(eventSubscriptionName, null));
        }
    }
}