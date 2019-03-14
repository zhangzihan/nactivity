using org.activiti.cloud.services.api.commands.results;
using org.activiti.cloud.services.api.events;
using org.springframework.messaging;
using System.Collections.Generic;

namespace org.activiti.cloud.services.events
{

    /// <summary>
    /// 
    /// </summary>
    public interface IProcessEngineChannels
    {

        /// <summary>
        /// 
        /// </summary>
        ISubscribableChannel commandConsumer();


        /// <summary>
        /// 
        /// </summary>
        IMessageChannel<IList<IProcessEngineEvent>> commandResults();


        /// <summary>
        /// 
        /// </summary>
        IMessageChannel<IList<IProcessEngineEvent>> auditProducer();
    }


    /// <summary>
    /// 
    /// </summary>
    public static class ProcessEngineChannels_Fields
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