using System.Collections.Generic;

namespace org.activiti.engine.impl.@event.logger.handler
{

    using org.activiti.engine.@delegate.@event;
    using org.activiti.engine.impl.interceptor;
    using org.activiti.engine.impl.persistence.entity;

    /// 
    public class ActivityMessageEventHandler : AbstractDatabaseEventLoggerEventHandler
    {

        public override IEventLogEntryEntity generateEventLogEntry(CommandContext<IEventLogEntryEntity> commandContext)
        {
            IActivitiMessageEvent messageEvent = (IActivitiMessageEvent)@event;

            IDictionary<string, object> data = new Dictionary<string, object>();
            putInMapIfNotNull(data, Fields_Fields.ACTIVITY_ID, messageEvent.ActivityId);
            putInMapIfNotNull(data, Fields_Fields.ACTIVITY_NAME, messageEvent.ActivityName);
            putInMapIfNotNull(data, Fields_Fields.PROCESS_DEFINITION_ID, messageEvent.ProcessDefinitionId);
            putInMapIfNotNull(data, Fields_Fields.PROCESS_INSTANCE_ID, messageEvent.ProcessInstanceId);
            putInMapIfNotNull(data, Fields_Fields.EXECUTION_ID, messageEvent.ExecutionId);
            putInMapIfNotNull(data, Fields_Fields.ACTIVITY_TYPE, messageEvent.ActivityType);

            putInMapIfNotNull(data, Fields_Fields.MESSAGE_NAME, messageEvent.MessageName);
            putInMapIfNotNull(data, Fields_Fields.MESSAGE_DATA, messageEvent.MessageData);

            return createEventLogEntry(messageEvent.ProcessDefinitionId, messageEvent.ProcessInstanceId, messageEvent.ExecutionId, null, data);
        }

    }

}