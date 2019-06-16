using System;

namespace Sys.Workflow.Engine.Impl.Events.Logger.Handlers
{

    using Sys.Workflow.Engine.Delegate.Events;
    using Sys.Workflow.Engine.Impl.Interceptor;
    using Sys.Workflow.Engine.Impl.Persistence.Entity;
    using Sys.Workflow;

    /// 
    public interface IEventLoggerEventHandler
    {

        IEventLogEntryEntity GenerateEventLogEntry(CommandContext<IEventLogEntryEntity> commandContext);

        IActivitiEvent Event { set; }

        DateTime? TimeStamp { set; }

        ObjectMapper ObjectMapper { set; }

    }

}