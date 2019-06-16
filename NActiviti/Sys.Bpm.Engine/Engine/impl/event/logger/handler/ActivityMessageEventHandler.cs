using System.Collections.Generic;

namespace Sys.Workflow.engine.impl.@event.logger.handler
{

    using Sys.Workflow.engine.@delegate.@event;
    using Sys.Workflow.engine.impl.interceptor;
    using Sys.Workflow.engine.impl.persistence.entity;

    /// 
    public class ActivityMessageEventHandler : AbstractDatabaseEventLoggerEventHandler
    {
        public override IEventLogEntryEntity GenerateEventLogEntry(CommandContext<IEventLogEntryEntity> commandContext)
        {
            IActivitiMessageEvent messageEvent = (IActivitiMessageEvent)@event;

            IDictionary<string, object> data = new Dictionary<string, object>();
            PutInMapIfNotNull(data, FieldsFields.ACTIVITY_ID, messageEvent.ActivityId);
            PutInMapIfNotNull(data, FieldsFields.ACTIVITY_NAME, messageEvent.ActivityName);
            PutInMapIfNotNull(data, FieldsFields.PROCESS_DEFINITION_ID, messageEvent.ProcessDefinitionId);
            PutInMapIfNotNull(data, FieldsFields.PROCESS_INSTANCE_ID, messageEvent.ProcessInstanceId);
            PutInMapIfNotNull(data, FieldsFields.EXECUTION_ID, messageEvent.ExecutionId);
            PutInMapIfNotNull(data, FieldsFields.ACTIVITY_TYPE, messageEvent.ActivityType);

            PutInMapIfNotNull(data, FieldsFields.MESSAGE_NAME, messageEvent.MessageName);
            PutInMapIfNotNull(data, FieldsFields.MESSAGE_DATA, messageEvent.MessageData);

            return CreateEventLogEntry(messageEvent.ProcessDefinitionId, messageEvent.ProcessInstanceId, messageEvent.ExecutionId, null, data);
        }

    }

}