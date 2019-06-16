using Sys.Workflow.Engine.Impl.Persistence.Entity;
using Sys.Workflow.Engine.Query;

namespace Sys.Workflow.Engine.Impl
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