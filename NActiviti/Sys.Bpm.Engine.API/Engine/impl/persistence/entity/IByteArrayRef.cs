namespace Sys.Workflow.Engine.Impl.Persistence.Entity
{
    public interface IByteArrayRef
    {
		byte[] GetBytes();
		void SetBytes(byte[] value);

		bool Deleted { get; }
        IByteArrayEntity Entity { get; }
        string Id { get; }
        string Name { get; }

        void Delete();
    }
}