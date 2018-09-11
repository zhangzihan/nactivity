using System.Collections.Generic;

namespace org.activiti.engine.impl.@event.logger.handler
{

    using org.activiti.engine.@delegate.@event;
    using org.activiti.engine.impl.interceptor;
    using org.activiti.engine.impl.persistence.entity;

    /// 
    public class ActivityErrorReceivedEventHandler : AbstractDatabaseEventLoggerEventHandler
    {

        public override IEventLogEntryEntity generateEventLogEntry(CommandContext<IEventLogEntryEntity> commandContext)
        {
            IActivitiActivityEvent activityEvent = (IActivitiActivityEvent)@event;

            IDictionary<string, object> data = new Dictionary<string, object>();
            putInMapIfNotNull(data, Fields_Fields.ACTIVITY_ID, activityEvent.ActivityId);
            putInMapIfNotNull(data, Fields_Fields.ACTIVITY_NAME, activityEvent.ActivityName);
            putInMapIfNotNull(data, Fields_Fields.PROCESS_DEFINITION_ID, activityEvent.ProcessDefinitionId);
            putInMapIfNotNull(data, Fields_Fields.PROCESS_INSTANCE_ID, activityEvent.ProcessInstanceId);
            putInMapIfNotNull(data, Fields_Fields.EXECUTION_ID, activityEvent.ExecutionId);
            putInMapIfNotNull(data, Fields_Fields.ACTIVITY_TYPE, activityEvent.ActivityType);

            return createEventLogEntry(activityEvent.ProcessDefinitionId, activityEvent.ProcessInstanceId, activityEvent.ExecutionId, null, data);
        }

    }

}