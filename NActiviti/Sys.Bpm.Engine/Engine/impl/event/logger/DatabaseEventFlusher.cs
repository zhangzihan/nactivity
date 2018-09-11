namespace org.activiti.engine.impl.@event.logger
{
    using org.activiti.engine.impl.@event.logger.handler;
    using org.activiti.engine.impl.interceptor;
    using org.activiti.engine.impl.persistence.entity;
    using System;

    /// 
    public class DatabaseEventFlusher : AbstractEventFlusher
    {
        public override void closing(ICommandContext commandContext)
        {

            if (commandContext.Exception != null)
            {
                return; // Not interested in events about exceptions
            }

            IEventLogEntryEntityManager eventLogEntryEntityManager = commandContext.EventLogEntryEntityManager;
            foreach (IEventLoggerEventHandler eventHandler in eventHandlers)
            {
                try
                {
                    eventLogEntryEntityManager.insert(eventHandler.generateEventLogEntry(null), false);
                }
                catch (Exception e)
                {
                    //logger.warn("Could not create event log", e);
                }
            }
        }

        public override void afterSessionsFlush(ICommandContext commandContext)
        {

        }

        public override void closeFailure(ICommandContext commandContext)
        {

        }

    }

}