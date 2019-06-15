using System.Collections.Generic;

namespace org.activiti.engine.impl.@event.logger.handler
{

    using org.activiti.engine.@delegate.@event;
    using org.activiti.engine.impl.interceptor;
    using org.activiti.engine.impl.persistence.entity;

    /// 
    public class VariableDeletedEventHandler : VariableEventHandler
    {

        public override IEventLogEntryEntity GenerateEventLogEntry(CommandContext<IEventLogEntryEntity> commandContext)
        {
            IActivitiVariableEvent variableEvent = (IActivitiVariableEvent)@event;
            IDictionary<string, object> data = createData(variableEvent);

            data[FieldsFields.END_TIME] = timeStamp;

            return CreateEventLogEntry(variableEvent.ProcessDefinitionId, variableEvent.ProcessInstanceId, variableEvent.ExecutionId, variableEvent.TaskId, data);
        }

    }

}