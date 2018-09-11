using System.Collections.Generic;

namespace org.activiti.engine.impl.@event.logger.handler
{

    using org.activiti.engine.@delegate.@event;
    using org.activiti.engine.impl.interceptor;
    using org.activiti.engine.impl.persistence.entity;

    /// 
    public class ActivitySignaledEventHandler : AbstractDatabaseEventLoggerEventHandler
    {

        public override IEventLogEntryEntity generateEventLogEntry(CommandContext<IEventLogEntryEntity> commandContext)
        {
            IActivitiSignalEvent signalEvent = (IActivitiSignalEvent)@event;

            IDictionary<string, object> data = new Dictionary<string, object>();
            putInMapIfNotNull(data, Fields_Fields.ACTIVITY_ID, signalEvent.ActivityId);
            putInMapIfNotNull(data, Fields_Fields.ACTIVITY_NAME, signalEvent.ActivityName);
            putInMapIfNotNull(data, Fields_Fields.PROCESS_DEFINITION_ID, signalEvent.ProcessDefinitionId);
            putInMapIfNotNull(data, Fields_Fields.PROCESS_INSTANCE_ID, signalEvent.ProcessInstanceId);
            putInMapIfNotNull(data, Fields_Fields.EXECUTION_ID, signalEvent.ExecutionId);
            putInMapIfNotNull(data, Fields_Fields.ACTIVITY_TYPE, signalEvent.ActivityType);

            putInMapIfNotNull(data, Fields_Fields.SIGNAL_NAME, signalEvent.SignalName);
            putInMapIfNotNull(data, Fields_Fields.SIGNAL_DATA, signalEvent.SignalData);

            return createEventLogEntry(signalEvent.ProcessDefinitionId, signalEvent.ProcessInstanceId, signalEvent.ExecutionId, null, data);
        }

    }

}