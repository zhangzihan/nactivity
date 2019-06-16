using System;

namespace Sys.Workflow.engine.impl.@event.logger.handler
{

    using Sys.Workflow.engine.@delegate.@event;
    using Sys.Workflow.engine.impl.interceptor;
    using Sys.Workflow.engine.impl.persistence.entity;
    using Sys.Bpm;

    /// 
    public interface IEventLoggerEventHandler
    {

        IEventLogEntryEntity GenerateEventLogEntry(CommandContext<IEventLogEntryEntity> commandContext);

        IActivitiEvent Event { set; }

        DateTime? TimeStamp { set; }

        ObjectMapper ObjectMapper { set; }

    }

}