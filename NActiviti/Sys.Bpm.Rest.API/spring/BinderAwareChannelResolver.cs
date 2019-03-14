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
        IBinderAwareChannelResolver resolveDestination(string connectorType);

        /// <summary>
        /// 
        /// </summary>
        void send<T>(IMessage<T> message);
    }
}
