using System.Collections.Generic;

namespace org.activiti.engine.impl.@event.logger.handler
{

    using org.activiti.engine.@delegate.@event;
    using org.activiti.engine.impl.interceptor;
    using org.activiti.engine.impl.persistence.entity;

    /// 
    public class ActivitySignaledEventHandler : AbstractDatabaseEventLoggerEventHandler
    {
        public override IEventLogEntryEntity GenerateEventLogEntry(CommandContext<IEventLogEntryEntity> commandContext)
        {
            IActivitiSignalEvent signalEvent = (IActivitiSignalEvent)@event;

            IDictionary<string, object> data = new Dictionary<string, object>();
            PutInMapIfNotNull(data, FieldsFields.ACTIVITY_ID, signalEvent.ActivityId);
            PutInMapIfNotNull(data, FieldsFields.ACTIVITY_NAME, signalEvent.ActivityName);
            PutInMapIfNotNull(data, FieldsFields.PROCESS_DEFINITION_ID, signalEvent.ProcessDefinitionId);
            PutInMapIfNotNull(data, FieldsFields.PROCESS_INSTANCE_ID, signalEvent.ProcessInstanceId);
            PutInMapIfNotNull(data, FieldsFields.EXECUTION_ID, signalEvent.ExecutionId);
            PutInMapIfNotNull(data, FieldsFields.ACTIVITY_TYPE, signalEvent.ActivityType);

            PutInMapIfNotNull(data, FieldsFields.SIGNAL_NAME, signalEvent.SignalName);
            PutInMapIfNotNull(data, FieldsFields.SIGNAL_DATA, signalEvent.SignalData);

            return CreateEventLogEntry(signalEvent.ProcessDefinitionId, signalEvent.ProcessInstanceId, signalEvent.ExecutionId, null, data);
        }

    }

}