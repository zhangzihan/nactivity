namespace Sys.Workflow.engine.history
{
    using Sys.Workflow.engine.query;

    /// <summary>
    /// Allows querying of <seealso cref="IHistoricActivityInstanceQuery"/>s via native (SQL) queries
    /// 
    /// 
    /// </summary>
    public interface INativeHistoricActivityInstanceQuery : INativeQuery<INativeHistoricActivityInstanceQuery, IHistoricActivityInstance>
    {

    }

}