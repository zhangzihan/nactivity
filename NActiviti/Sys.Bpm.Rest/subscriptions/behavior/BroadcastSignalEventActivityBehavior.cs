using org.activiti.bpmn.model;
using org.activiti.cloud.services.api.commands;
using org.activiti.engine.impl.bpmn.behavior;
using org.activiti.engine.impl.context;
using org.activiti.engine.impl.el;
using org.activiti.engine.impl.interceptor;
using org.activiti.engine.impl.persistence.entity;
using org.activiti.engine.runtime;
using org.springframework.context;

namespace org.activiti.services.subscriptions.behavior
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