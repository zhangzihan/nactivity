using org.activiti.cloud.services.api.commands;
using org.springframework.messaging;

namespace org.activiti.services.subscription.channel
{
    /// <summary>
    /// 
    /// </summary>
    public interface IProcessEngineSignalChannels
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        ISubscribableChannel SignalConsumer();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IMessageChannel<SignalCmd> SignalProducer();
    }

    /// <summary>
    /// 
    /// </summary>
    public static class ProcessEngineSignalChannelsFields
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