using org.activiti.cloud.services.api.commands;
using org.activiti.services.connectors.model;

namespace org.springframework.context
{
    public interface IApplicationEventPublisher
    {
        void publishEvent(IntegrationRequestEvent @event);

        void publishEvent(ICommand signalCmd);
    }
}
