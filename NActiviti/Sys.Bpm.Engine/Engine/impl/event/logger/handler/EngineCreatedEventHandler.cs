using System.Collections.Generic;

namespace org.activiti.engine.impl.@event.logger.handler
{

    using org.activiti.engine.impl.interceptor;
    using org.activiti.engine.impl.persistence.entity;
    using System;
    using System.Net;

    /// 
    public class EngineCreatedEventHandler : AbstractDatabaseEventLoggerEventHandler
    {

        public override IEventLogEntryEntity generateEventLogEntry(CommandContext<IEventLogEntryEntity> commandContext)
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
            return createEventLogEntry(data);
        }

    }

}