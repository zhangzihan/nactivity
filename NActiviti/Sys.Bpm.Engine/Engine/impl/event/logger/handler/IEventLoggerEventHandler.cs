using System;

namespace org.activiti.engine.impl.@event.logger.handler
{

    using org.activiti.engine.@delegate.@event;
    using org.activiti.engine.impl.interceptor;
    using org.activiti.engine.impl.persistence.entity;
    using Sys.Bpm;

    /// 
    public interface IEventLoggerEventHandler
    {

        IEventLogEntryEntity generateEventLogEntry(CommandContext<IEventLogEntryEntity> commandContext);

        IActivitiEvent Event { set; }

        DateTime? TimeStamp { set; }

        ObjectMapper ObjectMapper { set; }

    }

}