using Sys.Workflow.Cloud.Services.Api.Commands;
using Sys.Workflow.Services.Connectors.Models;

namespace Sys.Workflow.Contexts
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
