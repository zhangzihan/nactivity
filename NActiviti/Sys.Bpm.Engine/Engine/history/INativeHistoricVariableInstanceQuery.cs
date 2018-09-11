namespace org.activiti.engine.history
{
    using org.activiti.engine.query;

    /// <summary>
    /// Allows querying of <seealso cref="org.activiti.engine.history.IHistoricVariableInstance"/>s via native (SQL) queries
    /// 
    /// 
    /// </summary>
    public interface INativeHistoricVariableInstanceQuery : INativeQuery<INativeHistoricVariableInstanceQuery, IHistoricVariableInstance>
    {

    }
}