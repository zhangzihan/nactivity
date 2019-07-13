using System;
using System.Collections.Generic;

namespace Sys.Workflow.Engine.Impl.Cmd
{
    using Sys.Workflow.Engine.Delegate.Events;
    using Sys.Workflow.Engine.Delegate.Events.Impl;
    using Sys.Workflow.Engine.History;
    using Sys.Workflow.Engine.Impl.Cfg;
    using Sys.Workflow.Engine.Impl.Interceptor;
    using Sys.Workflow.Engine.Impl.Persistence.Entity;
    using Sys.Workflow.Engine.Impl.Util;
    using Sys.Workflow.Engine.Tasks;
    using System.Linq;

    /// 
    /// 
    [Serializable]
    public class ReturnToActivityCmd : ICommand<object>
    {
        private const long serialVersionUID = 1L;

        private readonly string currentTaskId;
        private readonly string returnToActivityId;
        private readonly string returnToReason;
        private readonly IDictionary<string, object> variables;
        private readonly object syncRoot = new object();

        public ReturnToActivityCmd(string currentTaskId, string returnToActivityId, string returnToReason, IDictionary<string, object> variables = null)
        {
            this.currentTaskId = currentTaskId;
            this.returnToActivityId = returnToActivityId;
            this.returnToReason = string.IsNullOrWhiteSpace(returnToReason) ? "已退回" : returnToReason;
            this.variables = variables ?? new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        }

        public virtual object Execute(ICommandContext commandContext)
        {
            lock (syncRoot)
            {
                ProcessEngineConfigurationImpl processEngineConfiguration = commandContext.ProcessEngineConfiguration;
                Interceptor.ICommandExecutor commandExecutor = processEngineConfiguration.CommandExecutor;

                ITaskEntity task = commandExecutor.Execute(new GetTaskByIdCmd(currentTaskId)) as ITaskEntity;
                if (task is null)
                {
                    throw new ActivitiObjectNotFoundException(string.Concat("No task found for id '", currentTaskId));
                }

                string currentExecutionId = task.ExecutionId;
                IExecutionEntity execution = task.Execution;
                if (execution is null)
                {
                    throw new ActivitiObjectNotFoundException(string.Concat("No execution found for id '", currentExecutionId));
                }

                var flowElement = ProcessDefinitionUtil.GetFlowElement(execution.ProcessDefinitionId, returnToActivityId);
                if (flowElement is null)
                {
                    throw new ActivitiObjectNotFoundException(string.Concat("No execution found for id '", currentExecutionId, "'"));
                }

                IHistoricActivityInstanceEntity hisInst = processEngineConfiguration.HistoryService.CreateHistoricActivityInstanceQuery()
                    .SetProcessInstanceId(execution.ProcessInstanceId)
                    .SetActivityId(returnToActivityId)
                    .OrderByHistoricActivityInstanceStartTime()
                    .Desc()
                    .List()
                    .FirstOrDefault() as IHistoricActivityInstanceEntity;

                IExecutionEntityManager executionEntityManager = commandContext.ExecutionEntityManager;

                IExecutionEntity returnToExec = executionEntityManager.CreateChildExecution(execution.ProcessInstance);
                returnToExec.CurrentFlowElement = flowElement;
                foreach (var key in variables.Keys)
                {
                    returnToExec.SetVariable(key, variables[key]);
                }

                executionEntityManager.Insert(returnToExec);

                commandContext.Agenda.PlanContinueProcessOperation(returnToExec);

                IExecutionEntity miRoot = commandExecutor.Execute(new GetMultiInstanceRootExecutionCmd(execution));

                List<ITask> tasks = new List<ITask>();
                if (miRoot != null)
                {
                    ITaskQuery query = commandContext.ProcessEngineConfiguration.TaskService.CreateTaskQuery();

                    IEnumerable<IExecutionEntity> childExecutions = commandExecutor.Execute(new GetChildExecutionsCmd(miRoot));

                    query.SetExecutionIdIn(childExecutions.Select(x => x.Id).ToArray());

                    tasks.AddRange(query.List());
                }
                else
                {
                    tasks.Add(task);
                }

                ReturnToTasks(commandContext, commandExecutor, miRoot != null ? miRoot : execution, executionEntityManager, tasks);

                return null;
            }
        }

        private void ReturnToTasks(ICommandContext commandContext, Interceptor.ICommandExecutor commandExecutor, IExecutionEntity execution, IExecutionEntityManager executionEntityManager, List<ITask> tasks)
        {
            foreach (ITaskEntity delTask in tasks)
            {
                IActivitiEventDispatcher eventDispatcher = commandContext.ProcessEngineConfiguration.EventDispatcher;
                if (eventDispatcher.Enabled)
                {
                    eventDispatcher.DispatchEvent(ActivitiEventBuilder.CreateTaskReturnEntityEvent(delTask, returnToActivityId));
                }

                commandExecutor.Execute(new AddCommentCmd(delTask.Id, delTask.ProcessInstanceId, returnToReason));

                commandContext.TaskEntityManager.DeleteTask(delTask, returnToReason, false, false);
            }

            DeleteExecutions(execution, true, returnToReason, commandContext);
        }

        // TODO: can the ExecutionManager.deleteChildExecution not be used?
        protected internal virtual void DeleteExecutions(IExecutionEntity execution, bool deleteExecution, string reason, ICommandContext commandContext)
        {
            // Delete all child executions
            IExecutionEntityManager executionEntityManager = commandContext.ExecutionEntityManager;
            ICollection<IExecutionEntity> childExecutions = executionEntityManager.FindChildExecutionsByParentExecutionId(execution.Id);
            if (CollectionUtil.IsNotEmpty(childExecutions))
            {
                foreach (IExecutionEntity childExecution in childExecutions)
                {
                    DeleteExecutions(childExecution, true, reason, commandContext);
                }
            }

            if (deleteExecution)
            {
                executionEntityManager.DeleteExecutionAndRelatedData(execution, reason, false);
            }
        }
    }
}