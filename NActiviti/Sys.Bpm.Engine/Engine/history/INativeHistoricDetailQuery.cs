namespace Sys.Workflow.engine.history
{
    using Sys.Workflow.engine.query;

    /// <summary>
    /// Allows querying of <seealso cref="Sys.Workflow.engine.history.IHistoricDetail"/>s via native (SQL) queries
    /// 
    /// 
    /// </summary>
    public interface INativeHistoricDetailQuery : INativeQuery<INativeHistoricDetailQuery, IHistoricDetail>
    {

    }
}