using Sys.Workflow.Bpmn.Models;
using Sys.Workflow.Cloud.Services.Api.Commands;
using Sys.Workflow.Engine.Impl.Bpmn.Behavior;
using Sys.Workflow.Engine.Impl.Contexts;
using Sys.Workflow.Engine.Impl.EL;
using Sys.Workflow.Engine.Impl.Interceptor;
using Sys.Workflow.Engine.Impl.Persistence.Entity;
using Sys.Workflow.Engine.Runtime;
using Sys.Workflow.Contexts;

namespace Sys.Workflow.Services.Subscriptions.Behavior
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
            if (signalEventName is object)
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

            eventPublisher.PublishEvent(new SignalCmd(eventSubscriptionName, execution.TenantId));
        }
    }
}