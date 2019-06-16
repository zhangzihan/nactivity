namespace Sys.Workflow.Messaging.Support
{

    /// <summary>
    /// 
    /// </summary>
    public class MessageBuilder<T>
    {

        /// <summary>
        /// 
        /// </summary>
        public static MessageBuilder<T> WithPayload(T value)
        {
            return new MessageBuilder<T>();
        }

        /// <summary>
        /// 
        /// </summary>

        public IMessage<T> Build()
        {
            return default;
        }

        /// <summary>
        /// 
        /// </summary>

        public MessageBuilder<T> SetHeader(string cONNECTOR_TYPE, string connectorType)
        {
            return this;
        }
    }
}
