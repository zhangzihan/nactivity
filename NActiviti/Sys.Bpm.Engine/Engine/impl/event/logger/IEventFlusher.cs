using System.Collections.Generic;

namespace Sys.Workflow.engine.impl.@event.logger
{

    using Sys.Workflow.engine.impl.@event.logger.handler;
    using Sys.Workflow.engine.impl.interceptor;

    /// 
    public interface IEventFlusher : ICommandContextCloseListener
    {

        IList<IEventLoggerEventHandler> EventHandlers { get; set; }


        void AddEventHandler(IEventLoggerEventHandler databaseEventLoggerEventHandler);

    }

}