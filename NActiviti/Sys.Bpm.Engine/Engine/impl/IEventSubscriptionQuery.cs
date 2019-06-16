using Sys.Workflow.engine.impl.persistence.entity;
using Sys.Workflow.engine.query;

namespace Sys.Workflow.engine.impl
{
    public interface IEventSubscriptionQuery : IQuery<IEventSubscriptionQuery, IEventSubscriptionEntity>
    {
        IEventSubscriptionQuery SetActivityId(string activityId);

        IEventSubscriptionQuery SetEventName(string eventName);

        IEventSubscriptionQuery SetEventSubscriptionId(string eventSubscriptionId);

        IEventSubscriptionQuery SetEventType(string eventType);

        IEventSubscriptionQuery SetExecutionId(string executionId);

        IEventSubscriptionQuery SetOrderByCreated();

        IEventSubscriptionQuery SetProcessInstanceId(string processInstanceId);

        IEventSubscriptionQuery SetTenantId(string tenantId);
    }
}