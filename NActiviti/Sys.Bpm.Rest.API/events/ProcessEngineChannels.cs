using Sys.Workflow.cloud.services.api.commands.results;
using Sys.Workflow.cloud.services.api.events;
using org.springframework.messaging;
using System.Collections.Generic;

namespace Sys.Workflow.cloud.services.events
{

    /// <summary>
    /// 
    /// </summary>
    public interface IProcessEngineChannels
    {

        /// <summary>
        /// 
        /// </summary>
        ISubscribableChannel CommandConsumer();


        /// <summary>
        /// 
        /// </summary>
        IMessageChannel<IList<IProcessEngineEvent>> CommandResults();


        /// <summary>
        /// 
        /// </summary>
        IMessageChannel<IList<IProcessEngineEvent>> AuditProducer();
    }


    /// <summary>
    /// 
    /// </summary>
    public static class ProcessEngineChannelsFields
    {

        /// <summary>
        /// 
        /// </summary>
        public const string COMMAND_CONSUMER = "commandConsumer";

        /// <summary>
        /// 
        /// </summary>
        public const string COMMAND_RESULTS = "commandResults";

        /// <summary>
        /// 
        /// </summary>
        public const string AUDIT_PRODUCER = "auditProducer";
    }

}