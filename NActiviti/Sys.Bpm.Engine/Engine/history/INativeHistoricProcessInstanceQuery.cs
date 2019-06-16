namespace Sys.Workflow.engine.history
{
    using Sys.Workflow.engine.query;

    /// <summary>
    /// Allows querying of <seealso cref="IHistoricTaskInstanceQuery"/>s via native (SQL) queries
    /// 
    /// 
    /// </summary>
    public interface INativeHistoricProcessInstanceQuery : INativeQuery<INativeHistoricProcessInstanceQuery, IHistoricProcessInstance>
    {

    }

}