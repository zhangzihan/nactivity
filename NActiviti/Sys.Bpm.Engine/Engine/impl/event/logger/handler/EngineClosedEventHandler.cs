using System.Collections.Generic;

namespace Sys.Workflow.engine.impl.@event.logger.handler
{

    using Sys.Workflow.engine.impl.interceptor;
    using Sys.Workflow.engine.impl.persistence.entity;
    using System;
    using System.Net;

    /// 
    public class EngineClosedEventHandler : AbstractDatabaseEventLoggerEventHandler
    {

        public override IEventLogEntryEntity GenerateEventLogEntry(CommandContext<IEventLogEntryEntity> commandContext)
        {
            IDictionary<string, object> data = new Dictionary<string, object>();
            try
            {
                data["ip"] = IPAddress.Loopback.ToString();//.LocalHost.HostAddress; // Note
                                                           // that
                                                           // this
                                                           // might
                                                           // give
                                                           // the
                                                           // wrong
                                                           // ip
                                                           // address
                                                           // in
                                                           // case
                                                           // of
                                                           // multiple
                                                           // network
                                                           // interfaces
                                                           // -
                                                           // but
                                                           // it's
                                                           // better
                                                           // than
                                                           // nothing.
            }
            catch (Exception)
            {
                // Best effort
            }
            return CreateEventLogEntry(data);
        }

    }

}