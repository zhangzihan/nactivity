namespace Sys.Workflow.Engine.Impl.Persistence.Entity
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