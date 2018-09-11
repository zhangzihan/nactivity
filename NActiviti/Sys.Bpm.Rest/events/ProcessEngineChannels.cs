using org.activiti.cloud.services.api.commands.results;
using org.activiti.cloud.services.api.events;
using org.springframework.messaging;
using System.Collections.Generic;

namespace org.activiti.cloud.services.events
{
    public interface ProcessEngineChannels
    {

        //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        //ORIGINAL LINE: @Input(ProcessEngineChannels_Fields.COMMAND_CONSUMER) org.springframework.messaging.SubscribableChannel commandConsumer();
        SubscribableChannel commandConsumer();

        //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        //ORIGINAL LINE: @Output(ProcessEngineChannels_Fields.COMMAND_RESULTS) org.springframework.messaging.MessageChannel commandResults();
        MessageChannel<IList<ProcessEngineEvent>> commandResults();

        //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        //ORIGINAL LINE: @Output(ProcessEngineChannels_Fields.AUDIT_PRODUCER) org.springframework.messaging.MessageChannel auditProducer();
        MessageChannel<IList<ProcessEngineEvent>> auditProducer();
    }

    public static class ProcessEngineChannels_Fields
    {
        public const string COMMAND_CONSUMER = "commandConsumer";
        public const string COMMAND_RESULTS = "commandResults";
        public const string AUDIT_PRODUCER = "auditProducer";
    }

}