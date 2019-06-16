namespace Sys.Workflow.engine.history
{
    using Sys.Workflow.engine.query;

    /// <summary>
    /// Allows querying of <seealso cref="Sys.Workflow.engine.history.IHistoricVariableInstance"/>s via native (SQL) queries
    /// 
    /// 
    /// </summary>
    public interface INativeHistoricVariableInstanceQuery : INativeQuery<INativeHistoricVariableInstanceQuery, IHistoricVariableInstance>
    {

    }
}