namespace Sys.Workflow.Engine.History
{
    using Sys.Workflow.Engine.Query;

    /// <summary>
    /// Allows querying of <seealso cref="IHistoricTaskInstanceQuery"/>s via native (SQL) queries
    /// 
    /// 
    /// </summary>
    public interface INativeHistoricTaskInstanceQuery : INativeQuery<INativeHistoricTaskInstanceQuery, IHistoricTaskInstance>
    {

    }

}