using System.Collections.Generic;

namespace Sys.Workflow.engine.impl.@event.logger
{

    using Sys.Workflow.engine.impl.@event.logger.handler;
    using Sys.Workflow.engine.impl.interceptor;

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