namespace Sys.Workflow.Engine.Impl.Bpmn.Helper
{
    using Sys.Workflow.Engine.Impl.Persistence.Entity;

    /// 
    public class SubProcessVariableSnapshotter
    {
        public virtual void SetVariablesSnapshots(IExecutionEntity sourceExecution, IExecutionEntity snapshotHolder)
        {
            snapshotHolder.VariablesLocal = sourceExecution.VariablesLocal;

            IExecutionEntity parentExecution = sourceExecution.Parent;
            if (parentExecution != null && parentExecution.IsMultiInstanceRoot)
            {
                snapshotHolder.VariablesLocal = parentExecution.VariablesLocal;
            }
        }
    }
}