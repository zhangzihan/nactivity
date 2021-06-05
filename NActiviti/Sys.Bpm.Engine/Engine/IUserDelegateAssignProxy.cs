using Sys.Workflow.Engine.Impl.Persistence.Entity;

namespace Sys.Workflow.Engine
{
    public interface IUserDelegateAssignProxy
    {
        void Assign(IExecutionEntity execution);
    }
}
