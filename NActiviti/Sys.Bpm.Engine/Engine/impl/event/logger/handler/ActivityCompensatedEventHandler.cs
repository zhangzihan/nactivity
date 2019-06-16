using System.Collections.Generic;

namespace Sys.Workflow.engine.impl.@event.logger.handler
{

    using Sys.Workflow.engine.@delegate.@event;
    using Sys.Workflow.engine.impl.interceptor;
    using Sys.Workflow.engine.impl.persistence.entity;

    /// 
    public class ActivityCompensatedEventHandler : AbstractDatabaseEventLoggerEventHandler
    {
        public override IEventLogEntryEntity GenerateEventLogEntry(CommandContext<IEventLogEntryEntity> commandContext)
        {
            IActivitiActivityEvent activityEvent = (IActivitiActivityEvent)@event;

            IDictionary<string, object> data = new Dictionary<string, object>();
            PutInMapIfNotNull(data, FieldsFields.ACTIVITY_ID, activityEvent.ActivityId);
            PutInMapIfNotNull(data, FieldsFields.ACTIVITY_NAME, activityEvent.ActivityName);
            PutInMapIfNotNull(data, FieldsFields.PROCESS_DEFINITION_ID, activityEvent.ProcessDefinitionId);
            PutInMapIfNotNull(data, FieldsFields.PROCESS_INSTANCE_ID, activityEvent.ProcessInstanceId);
            PutInMapIfNotNull(data, FieldsFields.EXECUTION_ID, activityEvent.ExecutionId);
            PutInMapIfNotNull(data, FieldsFields.ACTIVITY_TYPE, activityEvent.ActivityType);

            return CreateEventLogEntry(activityEvent.ProcessDefinitionId, activityEvent.ProcessInstanceId, activityEvent.ExecutionId, null, data);
        }

    }

}