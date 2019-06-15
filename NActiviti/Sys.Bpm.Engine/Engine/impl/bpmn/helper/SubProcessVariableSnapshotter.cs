namespace org.activiti.engine.impl.bpmn.helper
{
    using org.activiti.engine.impl.persistence.entity;

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