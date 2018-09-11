namespace org.activiti.engine.history
{
    using org.activiti.engine.query;

    /// <summary>
    /// Allows querying of <seealso cref="IHistoricTaskInstanceQuery"/>s via native (SQL) queries
    /// 
    /// 
    /// </summary>
    public interface INativeHistoricProcessInstanceQuery : INativeQuery<INativeHistoricProcessInstanceQuery, IHistoricProcessInstance>
    {

    }

}