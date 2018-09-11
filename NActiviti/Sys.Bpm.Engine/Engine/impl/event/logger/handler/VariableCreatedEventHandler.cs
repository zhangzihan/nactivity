using System.Collections.Generic;

namespace org.activiti.engine.impl.@event.logger.handler
{

    using org.activiti.engine.@delegate.@event;
    using org.activiti.engine.impl.interceptor;
    using org.activiti.engine.impl.persistence.entity;

    /// 
    public class VariableCreatedEventHandler : VariableEventHandler
    {

        public override IEventLogEntryEntity generateEventLogEntry(CommandContext<IEventLogEntryEntity> commandContext)
        {
            IActivitiVariableEvent variableEvent = (IActivitiVariableEvent)@event;
            IDictionary<string, object> data = createData(variableEvent);

            data[Fields_Fields.CREATE_TIME] = timeStamp;

            return createEventLogEntry(variableEvent.ProcessDefinitionId, variableEvent.ProcessInstanceId, variableEvent.ExecutionId, variableEvent.TaskId, data);
        }

    }

}