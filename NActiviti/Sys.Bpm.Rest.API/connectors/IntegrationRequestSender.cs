namespace org.activiti.services.connectors
{
    using IProcessEngineEvent = org.activiti.cloud.services.api.events.IProcessEngineEvent;
    using RuntimeBundleProperties = org.activiti.cloud.services.events.configuration.RuntimeBundleProperties;
    using IntegrationRequestSentEventImpl = org.activiti.cloud.services.events.integration.IntegrationRequestSentEventImpl;
    using IntegrationRequestEvent = org.activiti.services.connectors.model.IntegrationRequestEvent;
    using org.springframework.messaging;
    using org.springframework.cloud.stream.binding;
    using org.springframework.messaging.support;

    public class IntegrationRequestSender
    {
        protected internal const string CONNECTOR_TYPE = "connectorType";
        private readonly RuntimeBundleProperties runtimeBundleProperties;
        private readonly IMessageChannel<IProcessEngineEvent[]> auditProducer;
        private readonly IBinderAwareChannelResolver resolver;

        public IntegrationRequestSender(RuntimeBundleProperties runtimeBundleProperties, IMessageChannel<IProcessEngineEvent[]> auditProducer, IBinderAwareChannelResolver resolver)
        {
            this.runtimeBundleProperties = runtimeBundleProperties;
            this.auditProducer = auditProducer;
            this.resolver = resolver;
        }

        public virtual void sendIntegrationRequest(IntegrationRequestEvent @event)
        {

            resolver.resolveDestination(@event.ConnectorType).send(buildIntegrationRequestMessage(@event));
            sendAuditEvent(@event);
        }

        private void sendAuditEvent(IntegrationRequestEvent integrationRequestEvent)
        {
            if (runtimeBundleProperties.EventsProperties.IntegrationAuditEventsEnabled)
            {
                IntegrationRequestSentEventImpl @event = new IntegrationRequestSentEventImpl(runtimeBundleProperties.AppName, runtimeBundleProperties.AppVersion, runtimeBundleProperties.ServiceName, runtimeBundleProperties.ServiceFullName, runtimeBundleProperties.ServiceType, runtimeBundleProperties.ServiceVersion, integrationRequestEvent.ExecutionId, integrationRequestEvent.ProcessDefinitionId, integrationRequestEvent.ProcessInstanceId, integrationRequestEvent.IntegrationContextId, integrationRequestEvent.FlowNodeId);
                auditProducer.send(MessageBuilder<IProcessEngineEvent[]>.withPayload(new IProcessEngineEvent[] { @event }).build());
            }
        }

        private IMessage<IntegrationRequestEvent> buildIntegrationRequestMessage(IntegrationRequestEvent @event)
        {
            return MessageBuilder<IntegrationRequestEvent>.withPayload(@event).setHeader(CONNECTOR_TYPE, @event.ConnectorType).build();
        }
    }

}