using org.activiti.cloud.services.api.commands;
using org.springframework.messaging;

namespace org.activiti.services.subscription.channel
{
    public interface ProcessEngineSignalChannels
    {
        SubscribableChannel signalConsumer();

        MessageChannel<SignalCmd> signalProducer();
    }

    public static class ProcessEngineSignalChannels_Fields
    {
        public const string SIGNAL_CONSUMER = "signalConsumer";
        public const string SIGNAL_PRODUCER = "signalProducer";
    }

}