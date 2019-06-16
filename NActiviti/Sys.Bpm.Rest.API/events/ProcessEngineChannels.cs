using Sys.Workflow.Cloud.Services.Api.Commands.Results;
using Sys.Workflow.Cloud.Services.Api.Events;
using Sys.Workflow.Messaging;
using System.Collections.Generic;

namespace Sys.Workflow.Cloud.Services.Events
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