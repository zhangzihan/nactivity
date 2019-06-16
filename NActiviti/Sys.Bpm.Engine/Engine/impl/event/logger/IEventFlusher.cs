using System.Collections.Generic;

namespace Sys.Workflow.Engine.Impl.Events.Logger
{

    using Sys.Workflow.Engine.Impl.Events.Logger.Handlers;
    using Sys.Workflow.Engine.Impl.Interceptor;

    /// 
    public interface IEventFlusher : ICommandContextCloseListener
    {

        IList<IEventLoggerEventHandler> EventHandlers { get; set; }


        void AddEventHandler(IEventLoggerEventHandler databaseEventLoggerEventHandler);

    }

}