namespace org.springframework.messaging.support
{
    public class MessageBuilder<T>
    {
        public static MessageBuilder<T> withPayload(T value)
        {
            return new MessageBuilder<T>();
        }

        public IMessage<T> build()
        {
            return default(IMessage<T>);
        }

        public MessageBuilder<T> setHeader(string cONNECTOR_TYPE, string connectorType)
        {
            return this;
        }
    }
}
