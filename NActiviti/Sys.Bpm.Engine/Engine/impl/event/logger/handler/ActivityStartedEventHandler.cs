using System.Collections.Generic;

namespace org.activiti.engine.impl.@event.logger.handler
{

    using org.activiti.engine.@delegate.@event;
    using org.activiti.engine.impl.interceptor;
    using org.activiti.engine.impl.persistence.entity;

    /// 
    public class ActivityStartedEventHandler : AbstractDatabaseEventLoggerEventHandler
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
            PutInMapIfNotNull(data, FieldsFields.BEHAVIOR_CLASS, activityEvent.BehaviorClass);

            return CreateEventLogEntry(activityEvent.ProcessDefinitionId, activityEvent.ProcessInstanceId, activityEvent.ExecutionId, null, data);
        }

    }

}