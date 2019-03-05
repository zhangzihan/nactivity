namespace org.springframework.messaging
{
    public interface IMessageChannel<T>
    {
        void send(IMessage<T> message);
    }
}
