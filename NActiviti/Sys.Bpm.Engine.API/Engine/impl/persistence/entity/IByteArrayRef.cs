namespace org.activiti.engine.impl.persistence.entity
{
    public interface IByteArrayRef
    {
        byte[] Bytes { get; set; }
        bool Deleted { get; }
        IByteArrayEntity Entity { get; }
        string Id { get; }
        string Name { get; }

        void Delete();
    }
}