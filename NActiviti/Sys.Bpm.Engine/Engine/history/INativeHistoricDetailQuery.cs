namespace Sys.Workflow.Engine.History
{
    using Sys.Workflow.Engine.Query;

    /// <summary>
    /// Allows querying of <seealso cref="Sys.Workflow.Engine.History.IHistoricDetail"/>s via native (SQL) queries
    /// 
    /// 
    /// </summary>
    public interface INativeHistoricDetailQuery : INativeQuery<INativeHistoricDetailQuery, IHistoricDetail>
    {

    }
}