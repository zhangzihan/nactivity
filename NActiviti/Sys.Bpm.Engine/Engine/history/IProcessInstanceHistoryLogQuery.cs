namespace Sys.Workflow.engine.history
{
    using Sys.Workflow.engine.task;

    /// <summary>
    /// Allows to fetch the <seealso cref="IProcessInstanceHistoryLog"/> for a process instance.
    /// 
    /// Note that every includeXXX() method below will lead to an additional query.
    /// 
    /// This class is actually a convenience on top of the other specific queries such as <seealso cref="IHistoricTaskInstanceQuery"/>, <seealso cref="IHistoricActivityInstanceQuery"/>, ... It will execute separate queries for
    /// each included type, order the data according to the date (ascending) and wrap the results in the <seealso cref="IProcessInstanceHistoryLog"/>.
    /// 
    /// 
    /// </summary>
    public interface IProcessInstanceHistoryLogQuery
    {

        /// <summary>
        /// The <seealso cref="IProcessInstanceHistoryLog"/> will contain the <seealso cref="IHistoricTaskInstance"/> instances.
        /// </summary>
        IProcessInstanceHistoryLogQuery IncludeTasks();

        /// <summary>
        /// The <seealso cref="IProcessInstanceHistoryLog"/> will contain the <seealso cref="IHistoricActivityInstance"/> instances.
        /// </summary>
        IProcessInstanceHistoryLogQuery IncludeActivities();

        /// <summary>
        /// The <seealso cref="IProcessInstanceHistoryLog"/> will contain the <seealso cref="IHistoricVariableInstance"/> instances.
        /// </summary>
        IProcessInstanceHistoryLogQuery IncludeVariables();

        /// <summary>
        /// The <seealso cref="IProcessInstanceHistoryLog"/> will contain the <seealso cref="IComment"/> instances.
        /// </summary>
        IProcessInstanceHistoryLogQuery IncludeComments();

        /// <summary>
        /// The <seealso cref="IProcessInstanceHistoryLog"/> will contain the <seealso cref="IHistoricVariableUpdate"/> instances.
        /// </summary>
        IProcessInstanceHistoryLogQuery IncludeVariableUpdates();

        /// <summary>
        /// The <seealso cref="IProcessInstanceHistoryLog"/> will contain the <seealso cref="HistoricFormProperty"/> instances.
        /// </summary>
        IProcessInstanceHistoryLogQuery IncludeFormProperties();

        /// <summary>
        /// Executes the query. </summary>
        IProcessInstanceHistoryLog SingleResult();
    }
}