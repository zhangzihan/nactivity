using org.springframework.messaging;

namespace org.springframework.cloud.stream.binding
{
    public interface IBinderAwareChannelResolver
    {
        IBinderAwareChannelResolver resolveDestination(string connectorType);
        void send<T>(IMessage<T> message);
    }
}
