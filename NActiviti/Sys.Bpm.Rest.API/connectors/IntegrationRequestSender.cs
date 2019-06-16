using Sys.Workflow.Cloud.Services.Api.Events;
using Sys.Workflow.Cloud.Services.Events.Configurations;
using Sys.Workflow.Cloud.Services.Events.Integration;
using Sys.Workflow.Services.Connectors.Models;

namespace Sys.Workflow.Services.Connectors
{
    using Sys.Workflow.Messaging;
    using Sys.Workflow.Cloud.Streams.Bindings;
    using Sys.Workflow.Messaging.Support;

    /// <summary>
    /// 
    /// </summary>
    public class IntegrationRequestSender
    {
        /// <summary>
        /// connectorType
        /// </summary>
        protected internal const string CONNECTOR_TYPE = "connectorType";
        private readonly RuntimeBundleProperties runtimeBundleProperties;
        private readonly IMessageChannel<IProcessEngineEvent[]> auditProducer;
        private readonly IBinderAwareChannelResolver resolver;


        /// <summary>
        /// 
        /// </summary>
        public IntegrationRequestSender(RuntimeBundleProperties runtimeBundleProperties, IMessageChannel<IProcessEngineEvent[]> auditProducer, IBinderAwareChannelResolver resolver)
        {
            this.runtimeBundleProperties = runtimeBundleProperties;
            this.auditProducer = auditProducer;
            this.resolver = resolver;
        }


        /// <summary>
        /// 
        /// </summary>
        public virtual void sendIntegrationRequest(IntegrationRequestEvent @event)
        {

            resolver.ResolveDestination(@event.ConnectorType).Send(buildIntegrationRequestMessage(@event));
            SendAuditEvent(@event);
        }


        /// <summary>
        /// 
        /// </summary>
        private void SendAuditEvent(IntegrationRequestEvent integrationRequestEvent)
        {
            if (runtimeBundleProperties.EventsProperties.IntegrationAuditEventsEnabled)
            {
                IntegrationRequestSentEventImpl @event = new IntegrationRequestSentEventImpl(runtimeBundleProperties.AppName, runtimeBundleProperties.AppVersion, runtimeBundleProperties.ServiceName, runtimeBundleProperties.ServiceFullName, runtimeBundleProperties.ServiceType, runtimeBundleProperties.ServiceVersion, integrationRequestEvent.ExecutionId, integrationRequestEvent.ProcessDefinitionId, integrationRequestEvent.ProcessInstanceId, integrationRequestEvent.IntegrationContextId, integrationRequestEvent.FlowNodeId);
                auditProducer.Send(MessageBuilder<IProcessEngineEvent[]>.WithPayload(new IProcessEngineEvent[] { @event }).Build());
            }
        }


        /// <summary>
        /// 
        /// </summary>
        private IMessage<IntegrationRequestEvent> buildIntegrationRequestMessage(IntegrationRequestEvent @event)
        {
            return MessageBuilder<IntegrationRequestEvent>.WithPayload(@event).SetHeader(CONNECTOR_TYPE, @event.ConnectorType).Build();
        }
    }

}