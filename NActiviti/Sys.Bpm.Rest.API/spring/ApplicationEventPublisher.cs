using org.activiti.cloud.services.api.commands;
using org.activiti.services.connectors.model;

namespace org.springframework.context
{

    /// <summary>
    /// 
    /// </summary>
    public interface IApplicationEventPublisher
    {

        /// <summary>
        /// 
        /// </summary>
        void publishEvent(IntegrationRequestEvent @event);


        /// <summary>
        /// 
        /// </summary>
        void publishEvent(ICommand signalCmd);
    }
}
