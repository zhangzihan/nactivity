namespace org.activiti.engine.history
{
    using org.activiti.engine.task;

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
        IProcessInstanceHistoryLogQuery includeTasks();

        /// <summary>
        /// The <seealso cref="IProcessInstanceHistoryLog"/> will contain the <seealso cref="IHistoricActivityInstance"/> instances.
        /// </summary>
        IProcessInstanceHistoryLogQuery includeActivities();

        /// <summary>
        /// The <seealso cref="IProcessInstanceHistoryLog"/> will contain the <seealso cref="IHistoricVariableInstance"/> instances.
        /// </summary>
        IProcessInstanceHistoryLogQuery includeVariables();

        /// <summary>
        /// The <seealso cref="IProcessInstanceHistoryLog"/> will contain the <seealso cref="IComment"/> instances.
        /// </summary>
        IProcessInstanceHistoryLogQuery includeComments();

        /// <summary>
        /// The <seealso cref="IProcessInstanceHistoryLog"/> will contain the <seealso cref="IHistoricVariableUpdate"/> instances.
        /// </summary>
        IProcessInstanceHistoryLogQuery includeVariableUpdates();

        /// <summary>
        /// The <seealso cref="IProcessInstanceHistoryLog"/> will contain the <seealso cref="HistoricFormProperty"/> instances.
        /// </summary>
        IProcessInstanceHistoryLogQuery includeFormProperties();

        /// <summary>
        /// Executes the query. </summary>
        IProcessInstanceHistoryLog singleResult();

    }

}