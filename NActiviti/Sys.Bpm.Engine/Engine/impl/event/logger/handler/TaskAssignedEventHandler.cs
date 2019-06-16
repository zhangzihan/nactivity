using System.Collections.Generic;

namespace Sys.Workflow.engine.impl.@event.logger.handler
{

    using Sys.Workflow.engine.@delegate.@event;
    using Sys.Workflow.engine.impl.interceptor;
    using Sys.Workflow.engine.impl.persistence.entity;

    /// 
    public class TaskAssignedEventHandler : AbstractTaskEventHandler
    {

        public override IEventLogEntryEntity GenerateEventLogEntry(CommandContext<IEventLogEntryEntity> commandContext)
        {
            ITaskEntity task = (ITaskEntity)((IActivitiEntityEvent)@event).Entity;
            IDictionary<string, object> data = handleCommonTaskFields(task);
            return CreateEventLogEntry(task.ProcessDefinitionId, task.ProcessInstanceId, task.ExecutionId, task.Id, data);
        }

    }

}