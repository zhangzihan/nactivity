using System.Collections.Generic;

namespace Sys.Workflow.engine.impl.@event.logger.handler
{

    using Sys.Workflow.engine.@delegate.@event;
    using Sys.Workflow.engine.impl.interceptor;
    using Sys.Workflow.engine.impl.persistence.entity;

    /// 
    public class SequenceFlowTakenEventHandler : AbstractDatabaseEventLoggerEventHandler
    {

        public override IEventLogEntryEntity GenerateEventLogEntry(CommandContext<IEventLogEntryEntity> commandContext)
        {
            IActivitiSequenceFlowTakenEvent sequenceFlowTakenEvent = (IActivitiSequenceFlowTakenEvent)@event;

            IDictionary<string, object> data = new Dictionary<string, object>();
            PutInMapIfNotNull(data, FieldsFields.ID, sequenceFlowTakenEvent.Id);

            PutInMapIfNotNull(data, FieldsFields.SOURCE_ACTIVITY_ID, sequenceFlowTakenEvent.SourceActivityId);
            PutInMapIfNotNull(data, FieldsFields.SOURCE_ACTIVITY_NAME, sequenceFlowTakenEvent.SourceActivityName);
            PutInMapIfNotNull(data, FieldsFields.SOURCE_ACTIVITY_TYPE, sequenceFlowTakenEvent.SourceActivityType);
            PutInMapIfNotNull(data, FieldsFields.SOURCE_ACTIVITY_BEHAVIOR_CLASS, sequenceFlowTakenEvent.SourceActivityBehaviorClass);

            PutInMapIfNotNull(data, FieldsFields.TARGET_ACTIVITY_ID, sequenceFlowTakenEvent.TargetActivityId);
            PutInMapIfNotNull(data, FieldsFields.TARGET_ACTIVITY_NAME, sequenceFlowTakenEvent.TargetActivityName);
            PutInMapIfNotNull(data, FieldsFields.TARGET_ACTIVITY_TYPE, sequenceFlowTakenEvent.TargetActivityType);
            PutInMapIfNotNull(data, FieldsFields.TARGET_ACTIVITY_BEHAVIOR_CLASS, sequenceFlowTakenEvent.TargetActivityBehaviorClass);

            return CreateEventLogEntry(@event.ProcessDefinitionId, @event.ProcessInstanceId, @event.ExecutionId, null, data);
        }

    }

}