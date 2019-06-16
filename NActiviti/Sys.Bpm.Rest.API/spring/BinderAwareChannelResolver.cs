using Sys.Workflow.Messaging;

namespace Sys.Workflow.Cloud.Streams.Bindings
{

    /// <summary>
    /// 
    /// </summary>
    public interface IBinderAwareChannelResolver
    {

        /// <summary>
        /// 
        /// </summary>
        IBinderAwareChannelResolver ResolveDestination(string connectorType);

        /// <summary>
        /// 
        /// </summary>
        void Send<T>(IMessage<T> message);
    }
}
