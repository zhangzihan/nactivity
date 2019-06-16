using System.Collections.Generic;

namespace Sys.Workflow.Engine.Impl.Events.Logger.Handlers
{

    using Sys.Workflow.Engine.Impl.Interceptor;
    using Sys.Workflow.Engine.Impl.Persistence.Entity;
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