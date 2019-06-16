namespace Sys.Workflow.Engine.History
{
    using Sys.Workflow.Engine.Query;

    /// <summary>
    /// Allows querying of <seealso cref="IHistoricActivityInstanceQuery"/>s via native (SQL) queries
    /// 
    /// 
    /// </summary>
    public interface INativeHistoricActivityInstanceQuery : INativeQuery<INativeHistoricActivityInstanceQuery, IHistoricActivityInstance>
    {

    }

}