using System.Collections.Generic;

namespace Sys.Workflow.Engine.Impl.Events.Logger
{

    using Sys.Workflow.Engine.Impl.Events.Logger.Handlers;
    using Sys.Workflow.Engine.Impl.Interceptor;

    /// 
    public abstract class AbstractEventFlusher : IEventFlusher
    {
        public abstract void CloseFailure(ICommandContext commandContext);
        public abstract void AfterSessionsFlush(ICommandContext commandContext);
        public abstract void Closing(ICommandContext commandContext);

        protected internal IList<IEventLoggerEventHandler> eventHandlers = new List<IEventLoggerEventHandler>();

        public virtual void Closed(ICommandContext commandContext)
        {
            // Not interested in closed
        }

        public virtual IList<IEventLoggerEventHandler> EventHandlers
        {
            get
            {
                return eventHandlers;
            }
            set
            {
                this.eventHandlers = value;
            }
        }


        public virtual void AddEventHandler(IEventLoggerEventHandler databaseEventLoggerEventHandler)
        {
            eventHandlers.Add(databaseEventLoggerEventHandler);
        }

    }

}