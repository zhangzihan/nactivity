using System.Collections.Generic;

namespace org.activiti.engine.impl.@event.logger.handler
{

    using org.activiti.engine.@delegate.@event;
    using org.activiti.engine.impl.interceptor;
    using org.activiti.engine.impl.persistence.entity;

    /// 
    public class TaskAssignedEventHandler : AbstractTaskEventHandler
    {

        public override IEventLogEntryEntity generateEventLogEntry(CommandContext<IEventLogEntryEntity> commandContext)
        {
            ITaskEntity task = (ITaskEntity)((IActivitiEntityEvent)@event).Entity;
            IDictionary<string, object> data = handleCommonTaskFields(task);
            return createEventLogEntry(task.ProcessDefinitionId, task.ProcessInstanceId, task.ExecutionId, task.Id, data);
        }

    }

}