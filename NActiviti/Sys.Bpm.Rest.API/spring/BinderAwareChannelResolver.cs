using org.springframework.messaging;

namespace org.springframework.cloud.stream.binding
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
