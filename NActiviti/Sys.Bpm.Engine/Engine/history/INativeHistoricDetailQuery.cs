namespace org.activiti.engine.history
{
    using org.activiti.engine.query;

    /// <summary>
    /// Allows querying of <seealso cref="org.activiti.engine.history.IHistoricDetail"/>s via native (SQL) queries
    /// 
    /// 
    /// </summary>
    public interface INativeHistoricDetailQuery : INativeQuery<INativeHistoricDetailQuery, IHistoricDetail>
    {

    }
}