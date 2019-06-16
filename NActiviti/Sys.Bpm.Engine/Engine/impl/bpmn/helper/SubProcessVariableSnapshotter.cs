namespace Sys.Workflow.engine.impl.bpmn.helper
{
    using Sys.Workflow.engine.impl.persistence.entity;

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