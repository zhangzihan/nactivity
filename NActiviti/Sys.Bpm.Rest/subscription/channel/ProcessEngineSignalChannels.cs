using org.activiti.cloud.services.api.commands;
using org.springframework.messaging;

namespace org.activiti.services.subscription.channel
{
    /// <summary>
    /// 
    /// </summary>
    public interface ProcessEngineSignalChannels
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        ISubscribableChannel signalConsumer();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IMessageChannel<SignalCmd> signalProducer();
    }

    /// <summary>
    /// 
    /// </summary>
    public static class ProcessEngineSignalChannels_Fields
    {
        /// <summary>
        /// 
        /// </summary>
        public const string SIGNAL_CONSUMER = "signalConsumer";

        /// <summary>
        /// 
        /// </summary>
        public const string SIGNAL_PRODUCER = "signalProducer";
    }

}