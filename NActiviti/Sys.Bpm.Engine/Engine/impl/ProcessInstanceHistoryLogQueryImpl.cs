using System.Collections.Generic;

namespace org.activiti.engine.impl
{

    using org.activiti.engine.history;
    using org.activiti.engine.impl.interceptor;
    using org.activiti.engine.impl.persistence.entity;
    using org.activiti.engine.impl.variable;

    /// 
    public class ProcessInstanceHistoryLogQueryImpl : IProcessInstanceHistoryLogQuery, ICommand<IProcessInstanceHistoryLog>
    {

        protected internal ICommandExecutor commandExecutor;

        protected internal string processInstanceId;
        protected internal bool includeTasks_Renamed;
        protected internal bool includeActivities_Renamed;
        protected internal bool includeVariables_Renamed;
        protected internal bool includeComments_Renamed;
        protected internal bool includeVariableUpdates_Renamed;
        protected internal bool includeFormProperties_Renamed;
        public ProcessInstanceHistoryLogQueryImpl(ICommandExecutor commandExecutor, string processInstanceId)
        {
            this.commandExecutor = commandExecutor;
            this.processInstanceId = processInstanceId;
        }

        public virtual IProcessInstanceHistoryLogQuery includeTasks()
        {
            this.includeTasks_Renamed = true;
            return this;
        }

        public virtual IProcessInstanceHistoryLogQuery includeComments()
        {
            this.includeComments_Renamed = true;
            return this;
        }

        public virtual IProcessInstanceHistoryLogQuery includeActivities()
        {
            this.includeActivities_Renamed = true;
            return this;
        }

        public virtual IProcessInstanceHistoryLogQuery includeVariables()
        {
            this.includeVariables_Renamed = true;
            return this;
        }

        public virtual IProcessInstanceHistoryLogQuery includeVariableUpdates()
        {
            this.includeVariableUpdates_Renamed = true;
            return this;
        }

        public virtual IProcessInstanceHistoryLogQuery includeFormProperties()
        {
            this.includeFormProperties_Renamed = true;
            return this;
        }

        public virtual IProcessInstanceHistoryLog singleResult()
        {
            return commandExecutor.execute(this);
        }

        public virtual IProcessInstanceHistoryLog execute(ICommandContext commandContext)
        {

            // Fetch historic process instance
            IHistoricProcessInstanceEntity historicProcessInstance = commandContext.HistoricProcessInstanceEntityManager.findById<IHistoricProcessInstanceEntity>(new KeyValuePair<string, object>("id", processInstanceId));

            if (historicProcessInstance == null)
            {
                return null;
            }

            // Create a log using this historic process instance
            ProcessInstanceHistoryLogImpl processInstanceHistoryLog = new ProcessInstanceHistoryLogImpl(historicProcessInstance);

            // Add events, based on query settings

            // Tasks
            if (includeTasks_Renamed)
            {
                IList<IHistoricData> tasks = commandContext.HistoricTaskInstanceEntityManager.findHistoricTaskInstancesByQueryCriteria((new HistoricTaskInstanceQueryImpl(commandExecutor)).processInstanceId(processInstanceId)) as IList<IHistoricData>;
                processInstanceHistoryLog.addHistoricData(tasks);
            }

            // Activities
            if (includeActivities_Renamed)
            {
                IList<IHistoricActivityInstance> activities = commandContext.HistoricActivityInstanceEntityManager.findHistoricActivityInstancesByQueryCriteria((new HistoricActivityInstanceQueryImpl(commandExecutor)).processInstanceId(processInstanceId), null);
                processInstanceHistoryLog.addHistoricData(activities);
            }

            // Variables
            if (includeVariables_Renamed)
            {
                IList<IHistoricVariableInstance> variables = commandContext.HistoricVariableInstanceEntityManager.findHistoricVariableInstancesByQueryCriteria((new HistoricVariableInstanceQueryImpl(commandExecutor)).processInstanceId(processInstanceId), null);

                // Make sure all variables values are fetched (similar to the HistoricVariableInstance query)
                foreach (IHistoricVariableInstance historicVariableInstance in variables)
                {
                    //historicVariableInstance.Value;

                    // make sure JPA entities are cached for later retrieval
                    IHistoricVariableInstanceEntity variableEntity = (IHistoricVariableInstanceEntity)historicVariableInstance;
                    if (JPAEntityVariableType.TYPE_NAME.Equals(variableEntity.VariableType.TypeName) || JPAEntityListVariableType.TYPE_NAME.Equals(variableEntity.VariableType.TypeName))
                    {
                        ((ICacheableVariable)variableEntity.VariableType).ForceCacheable = true;
                    }
                }

                processInstanceHistoryLog.addHistoricData(variables);
            }

            // Comment
            if (includeComments_Renamed)
            {
                IList<IHistoricData> comments = commandContext.CommentEntityManager.findCommentsByProcessInstanceId(processInstanceId) as IList<IHistoricData>;
                processInstanceHistoryLog.addHistoricData(comments);
            }

            // Details: variables
            if (includeVariableUpdates_Renamed)
            {
                IList<IHistoricData> variableUpdates = commandContext.HistoricDetailEntityManager.findHistoricDetailsByQueryCriteria((new HistoricDetailQueryImpl(commandExecutor)).variableUpdates(), null) as IList<IHistoricData>;

                // Make sure all variables values are fetched (similar to the HistoricVariableInstance query)
                foreach (IHistoricData historicData in variableUpdates)
                {
                    IHistoricVariableUpdate variableUpdate = (IHistoricVariableUpdate)historicData;
                    //variableUpdate.Value;
                }

                processInstanceHistoryLog.addHistoricData(variableUpdates);
            }

            // Details: form properties
            if (includeFormProperties_Renamed)
            {
                IList<IHistoricData> formProperties = commandContext.HistoricDetailEntityManager.findHistoricDetailsByQueryCriteria((new HistoricDetailQueryImpl(commandExecutor)).formProperties(), null) as IList<IHistoricData>;
                processInstanceHistoryLog.addHistoricData(formProperties);
            }

            // All events collected. Sort them by date.
            processInstanceHistoryLog.orderHistoricData();

            return processInstanceHistoryLog;
        }

    }

}