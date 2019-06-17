using System.Collections.Generic;

namespace Sys.Workflow.Engine.Impl
{

    using Sys.Workflow.Engine.History;
    using Sys.Workflow.Engine.Impl.Interceptor;
    using Sys.Workflow.Engine.Impl.Persistence.Entity;
    using Sys.Workflow.Engine.Impl.Variable;

    /// 
    public class ProcessInstanceHistoryLogQueryImpl : IProcessInstanceHistoryLogQuery, ICommand<IProcessInstanceHistoryLog>
    {

        protected internal ICommandExecutor commandExecutor;

        protected internal string processInstanceId;
        protected internal bool _includeTasks;
        protected internal bool _includeActivities;
        protected internal bool _includeVariables;
        protected internal bool _includeComments;
        protected internal bool _includeVariableUpdates;
        protected internal bool _includeFormProperties;
        public ProcessInstanceHistoryLogQueryImpl(ICommandExecutor commandExecutor, string processInstanceId)
        {
            this.commandExecutor = commandExecutor;
            this.processInstanceId = processInstanceId;
        }

        public virtual IProcessInstanceHistoryLogQuery IncludeTasks()
        {
            this._includeTasks = true;
            return this;
        }

        public virtual IProcessInstanceHistoryLogQuery IncludeComments()
        {
            this._includeComments = true;
            return this;
        }

        public virtual IProcessInstanceHistoryLogQuery IncludeActivities()
        {
            this._includeActivities = true;
            return this;
        }

        public virtual IProcessInstanceHistoryLogQuery IncludeVariables()
        {
            this._includeVariables = true;
            return this;
        }

        public virtual IProcessInstanceHistoryLogQuery IncludeVariableUpdates()
        {
            this._includeVariableUpdates = true;
            return this;
        }

        public virtual IProcessInstanceHistoryLogQuery IncludeFormProperties()
        {
            this._includeFormProperties = true;
            return this;
        }

        public virtual IProcessInstanceHistoryLog SingleResult()
        {
            return commandExecutor.Execute(this);
        }

        public virtual IProcessInstanceHistoryLog Execute(ICommandContext commandContext)
        {

            // Fetch historic process instance
            IHistoricProcessInstanceEntity historicProcessInstance = commandContext.HistoricProcessInstanceEntityManager.FindById<IHistoricProcessInstanceEntity>(new KeyValuePair<string, object>("id", processInstanceId));

            if (historicProcessInstance == null)
            {
                return null;
            }

            // Create a log using this historic process instance
            ProcessInstanceHistoryLogImpl processInstanceHistoryLog = new ProcessInstanceHistoryLogImpl(historicProcessInstance);

            // Add events, based on query settings

            // Tasks
            if (_includeTasks)
            {
                IList<IHistoricData> tasks = commandContext.HistoricTaskInstanceEntityManager.FindHistoricTaskInstancesByQueryCriteria((new HistoricTaskInstanceQueryImpl(commandExecutor)).SetProcessInstanceId(processInstanceId)) as IList<IHistoricData>;
                processInstanceHistoryLog.AddHistoricData(tasks);
            }

            // Activities
            if (_includeActivities)
            {
                IList<IHistoricActivityInstance> activities = commandContext.HistoricActivityInstanceEntityManager.FindHistoricActivityInstancesByQueryCriteria((new HistoricActivityInstanceQueryImpl(commandExecutor)).SetProcessInstanceId(processInstanceId), null);
                processInstanceHistoryLog.AddHistoricData(activities);
            }

            // Variables
            if (_includeVariables)
            {
                IList<IHistoricVariableInstance> variables = commandContext.HistoricVariableInstanceEntityManager.FindHistoricVariableInstancesByQueryCriteria((new HistoricVariableInstanceQueryImpl(commandExecutor)).SetProcessInstanceId(processInstanceId), null);

                processInstanceHistoryLog.AddHistoricData(variables);
            }

            // Comment
            if (_includeComments)
            {
                IList<IHistoricData> comments = commandContext.CommentEntityManager.FindCommentsByProcessInstanceId(processInstanceId) as IList<IHistoricData>;
                processInstanceHistoryLog.AddHistoricData(comments);
            }

            // Details: variables
            if (_includeVariableUpdates)
            {
                IList<IHistoricData> variableUpdates = commandContext.HistoricDetailEntityManager.FindHistoricDetailsByQueryCriteria((new HistoricDetailQueryImpl(commandExecutor)).SetVariableUpdates(), null) as IList<IHistoricData>;

                // Make sure all variables values are fetched (similar to the HistoricVariableInstance query)
                foreach (IHistoricData historicData in variableUpdates)
                {
                    IHistoricVariableUpdate variableUpdate = (IHistoricVariableUpdate)historicData;
                    //variableUpdate.Value;
                }

                processInstanceHistoryLog.AddHistoricData(variableUpdates);
            }

            // Details: form properties
            if (_includeFormProperties)
            {
                IList<IHistoricData> formProperties = commandContext.HistoricDetailEntityManager.FindHistoricDetailsByQueryCriteria((new HistoricDetailQueryImpl(commandExecutor)).FormProperties(), null) as IList<IHistoricData>;
                processInstanceHistoryLog.AddHistoricData(formProperties);
            }

            // All events collected. Sort them by date.
            processInstanceHistoryLog.orderHistoricData();

            return processInstanceHistoryLog;
        }

    }

}