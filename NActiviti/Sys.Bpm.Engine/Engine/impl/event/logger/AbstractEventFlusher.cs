using System.Collections.Generic;

namespace org.activiti.engine.impl.@event.logger
{

    using org.activiti.engine.impl.@event.logger.handler;
    using org.activiti.engine.impl.interceptor;

    /// 
    public abstract class AbstractEventFlusher : IEventFlusher
    {
        public abstract void closeFailure(ICommandContext commandContext);
        public abstract void afterSessionsFlush(ICommandContext commandContext);
        public abstract void closing(ICommandContext commandContext);

        protected internal IList<IEventLoggerEventHandler> eventHandlers = new List<IEventLoggerEventHandler>();

        public virtual void closed(ICommandContext commandContext)
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


        public virtual void addEventHandler(IEventLoggerEventHandler databaseEventLoggerEventHandler)
        {
            eventHandlers.Add(databaseEventLoggerEventHandler);
        }

    }

}