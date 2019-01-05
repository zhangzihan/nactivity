using org.activiti.cloud.services.api.commands.results;
using org.activiti.cloud.services.api.events;
using org.springframework.messaging;
using System.Collections.Generic;

namespace org.activiti.cloud.services.events
{
    public interface ProcessEngineChannels
    {
        SubscribableChannel commandConsumer();

        MessageChannel<IList<ProcessEngineEvent>> commandResults();

        MessageChannel<IList<ProcessEngineEvent>> auditProducer();
    }

    public static class ProcessEngineChannels_Fields
    {
        public const string COMMAND_CONSUMER = "commandConsumer";
        public const string COMMAND_RESULTS = "commandResults";
        public const string AUDIT_PRODUCER = "auditProducer";
    }

}