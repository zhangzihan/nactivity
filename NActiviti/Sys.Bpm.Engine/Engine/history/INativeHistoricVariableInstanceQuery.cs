namespace Sys.Workflow.Engine.History
{
    using Sys.Workflow.Engine.Query;

    /// <summary>
    /// Allows querying of <seealso cref="Sys.Workflow.Engine.History.IHistoricVariableInstance"/>s via native (SQL) queries
    /// 
    /// 
    /// </summary>
    public interface INativeHistoricVariableInstanceQuery : INativeQuery<INativeHistoricVariableInstanceQuery, IHistoricVariableInstance>
    {

    }
}