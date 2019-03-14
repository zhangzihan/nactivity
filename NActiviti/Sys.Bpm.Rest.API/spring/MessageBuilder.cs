namespace org.springframework.messaging.support
{

    /// <summary>
    /// 
    /// </summary>
    public class MessageBuilder<T>
    {

        /// <summary>
        /// 
        /// </summary>
        public static MessageBuilder<T> withPayload(T value)
        {
            return new MessageBuilder<T>();
        }

        /// <summary>
        /// 
        /// </summary>

        public IMessage<T> build()
        {
            return default(IMessage<T>);
        }

        /// <summary>
        /// 
        /// </summary>

        public MessageBuilder<T> setHeader(string cONNECTOR_TYPE, string connectorType)
        {
            return this;
        }
    }
}
