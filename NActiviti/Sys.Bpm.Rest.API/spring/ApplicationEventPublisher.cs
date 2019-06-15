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
        void PublishEvent(IntegrationRequestEvent @event);


        /// <summary>
        /// 
        /// </summary>
        void PublishEvent(ICommand signalCmd);
    }
}
