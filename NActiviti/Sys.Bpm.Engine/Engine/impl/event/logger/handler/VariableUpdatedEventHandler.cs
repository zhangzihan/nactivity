using System.Collections.Generic;

namespace org.activiti.engine.impl.@event.logger.handler
{

    using org.activiti.engine.@delegate.@event;
    using org.activiti.engine.impl.interceptor;
    using org.activiti.engine.impl.persistence.entity;

    /// 
    public class VariableUpdatedEventHandler : VariableEventHandler
    {

        public override IEventLogEntryEntity GenerateEventLogEntry(CommandContext<IEventLogEntryEntity> commandContext)
        {
            IActivitiVariableEvent variableEvent = (IActivitiVariableEvent)@event;
            IDictionary<string, object> data = createData(variableEvent);

            return CreateEventLogEntry(variableEvent.ProcessDefinitionId, variableEvent.ProcessInstanceId, variableEvent.ExecutionId, variableEvent.TaskId, data);
        }

    }

}