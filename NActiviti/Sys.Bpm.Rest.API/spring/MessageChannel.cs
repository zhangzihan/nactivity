namespace org.springframework.messaging
{
    /// <summary>
    /// 
    /// </summary>
    public interface IMessageChannel<T>
    {
        /// <summary>
        /// 
        /// </summary>
        void Send(IMessage<T> message);
    }
}
