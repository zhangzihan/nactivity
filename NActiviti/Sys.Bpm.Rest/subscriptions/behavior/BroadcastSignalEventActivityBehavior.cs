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
    public class BroadcastSignalEventActivityBehavior : IntermediateThrowSignalEventActivityBehavior
    {
        private const long serialVersionUID = 1L;

        private readonly IApplicationEventPublisher eventPublisher;

        public BroadcastSignalEventActivityBehavior(IApplicationEventPublisher eventPublisher, SignalEventDefinition signalEventDefinition, Signal signal) : base(signalEventDefinition, signal)
        {
            this.eventPublisher = eventPublisher;
        }

        public override void execute(IExecutionEntity execution)
        {
            base.execute(execution);

            ICommandContext commandContext = Context.CommandContext;
            string eventSubscriptionName = null;
            if (signalEventName != null)
            {
                eventSubscriptionName = signalEventName;
            }
            else
            {
                eventSubscriptionName = new ExpressionManager().createExpression(signalExpression)
                    .getValue(execution).ToString();

                //Expression expressionObject = commandContext.ProcessEngineConfiguration.ExpressionManager.createExpression(signalExpression);
                //eventSubscriptionName = expressionObject.getValue(execution).ToString();
            }

            eventPublisher.publishEvent(new SignalCmd(eventSubscriptionName, null));
        }
    }
}