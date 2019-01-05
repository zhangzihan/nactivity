namespace org.activiti.services.connectors
{
    using ProcessEngineEvent = org.activiti.cloud.services.api.events.ProcessEngineEvent;
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
        private readonly MessageChannel<ProcessEngineEvent[]> auditProducer;
        private readonly BinderAwareChannelResolver resolver;

        public IntegrationRequestSender(RuntimeBundleProperties runtimeBundleProperties, MessageChannel<ProcessEngineEvent[]> auditProducer, BinderAwareChannelResolver resolver)
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
                auditProducer.send(MessageBuilder<ProcessEngineEvent[]>.withPayload(new ProcessEngineEvent[] { @event }).build());
            }
        }

        private Message<IntegrationRequestEvent> buildIntegrationRequestMessage(IntegrationRequestEvent @event)
        {
            return MessageBuilder<IntegrationRequestEvent>.withPayload(@event).setHeader(CONNECTOR_TYPE, @event.ConnectorType).build();
        }
    }

}