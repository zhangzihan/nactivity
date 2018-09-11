using System.Collections.Generic;

namespace org.activiti.engine.impl.@event.logger.handler
{

    using org.activiti.engine.@delegate.@event;
    using org.activiti.engine.impl.interceptor;
    using org.activiti.engine.impl.persistence.entity;

    /// 
    public class SequenceFlowTakenEventHandler : AbstractDatabaseEventLoggerEventHandler
    {

        public override IEventLogEntryEntity generateEventLogEntry(CommandContext<IEventLogEntryEntity> commandContext)
        {
            IActivitiSequenceFlowTakenEvent sequenceFlowTakenEvent = (IActivitiSequenceFlowTakenEvent)@event;

            IDictionary<string, object> data = new Dictionary<string, object>();
            putInMapIfNotNull(data, Fields_Fields.ID, sequenceFlowTakenEvent.Id);

            putInMapIfNotNull(data, Fields_Fields.SOURCE_ACTIVITY_ID, sequenceFlowTakenEvent.SourceActivityId);
            putInMapIfNotNull(data, Fields_Fields.SOURCE_ACTIVITY_NAME, sequenceFlowTakenEvent.SourceActivityName);
            putInMapIfNotNull(data, Fields_Fields.SOURCE_ACTIVITY_TYPE, sequenceFlowTakenEvent.SourceActivityType);
            putInMapIfNotNull(data, Fields_Fields.SOURCE_ACTIVITY_BEHAVIOR_CLASS, sequenceFlowTakenEvent.SourceActivityBehaviorClass);

            putInMapIfNotNull(data, Fields_Fields.TARGET_ACTIVITY_ID, sequenceFlowTakenEvent.TargetActivityId);
            putInMapIfNotNull(data, Fields_Fields.TARGET_ACTIVITY_NAME, sequenceFlowTakenEvent.TargetActivityName);
            putInMapIfNotNull(data, Fields_Fields.TARGET_ACTIVITY_TYPE, sequenceFlowTakenEvent.TargetActivityType);
            putInMapIfNotNull(data, Fields_Fields.TARGET_ACTIVITY_BEHAVIOR_CLASS, sequenceFlowTakenEvent.TargetActivityBehaviorClass);

            return createEventLogEntry(@event.ProcessDefinitionId, @event.ProcessInstanceId, @event.ExecutionId, null, data);
        }

    }

}