namespace Sys.Workflow.engine.impl.@event.logger
{
    using Microsoft.Extensions.Logging;
    using Sys.Workflow.engine.impl.@event.logger.handler;
    using Sys.Workflow.engine.impl.interceptor;
    using Sys.Workflow.engine.impl.persistence.entity;
    using Sys.Workflow;
    using System;

    /// 
    public class DatabaseEventFlusher : AbstractEventFlusher
    {
        private static readonly ILogger log = ProcessEngineServiceProvider.LoggerService<DatabaseEventFlusher>();
        
        public override void Closing(ICommandContext commandContext)
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
                    eventLogEntryEntityManager.Insert(eventHandler.GenerateEventLogEntry(null), false);
                }
                catch (Exception e)
                {
                    log.LogWarning("Could not create event log", e);
                }
            }
        }

        public override void AfterSessionsFlush(ICommandContext commandContext)
        {

        }

        public override void CloseFailure(ICommandContext commandContext)
        {

        }
    }
}