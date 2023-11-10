using System;
using System.Collections.Generic;

namespace Sys.Workflow.Engine.Impl.Cmd
{
	using Sys.Workflow.Engine.Delegate;
	using Sys.Workflow.Engine.Delegate.Events;
	using Sys.Workflow.Engine.Delegate.Events.Impl;
	using Sys.Workflow.Engine.History;
	using Sys.Workflow.Engine.Impl.Cfg;
	using Sys.Workflow.Engine.Impl.Interceptor;
	using Sys.Workflow.Engine.Impl.Persistence.Entity;
	using Sys.Workflow.Engine.Impl.Util;
	using Sys.Workflow.Engine.Impl.Variable;
	using Sys.Workflow.Engine.Runtime;
	using Sys.Workflow.Engine.Tasks;
	using Sys.Workflow.Services.Api.Commands;
	using System.Linq;

	/// <summary>
	/// 
	/// </summary>
	[Serializable]
	public class RunTaskCmd : ICommand<IList<ITask>>
	{
		private const long serialVersionUID = 1L;

		private readonly _RunTaskCmd cmd;

		public RunTaskCmd(string taskId, string runActivityId, string runReason, IDictionary<string, object> variables = null, IDictionary<string, object> transientVariables = null, string businessKey = null)
		{
			if (string.IsNullOrWhiteSpace(taskId) && string.IsNullOrWhiteSpace(businessKey))
			{
				throw new ArgumentNullException($"构造函数参数taskId和businessKey不能同时为空");
			}

			if (!string.IsNullOrWhiteSpace(taskId) && !string.IsNullOrWhiteSpace(businessKey))
			{
				throw new ArgumentNullException($"构造函数参数taskId和businessKey不能同时有值");
			}

			cmd = new _RunTaskCmd(taskId, runActivityId, runReason, variables, transientVariables, businessKey);
		}

		class _RunTaskCmd : ICommand<IExecutionEntity>
		{
			private readonly string runActivityId;
			private readonly string runReason;
			private readonly IDictionary<string, object> variables;
			private readonly IDictionary<string, object> transientVariables;
			private readonly string taskId;
			private readonly string businessKey;

			public _RunTaskCmd(string taskId, string runActivityId, string runReason, IDictionary<string, object> variables = null, IDictionary<string, object> transientVariables = null, string businessKey = null)
			{
				this.taskId = taskId;
				this.runActivityId = runActivityId;
				this.runReason = string.IsNullOrWhiteSpace(runReason) ? "已退回" : runReason;
				this.variables = variables ?? new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
				this.transientVariables = transientVariables;
				this.businessKey = businessKey;
			}

			private void DeleteTasks(ICommandContext commandContext, bool deleteExecution, IExecutionEntity execution, List<ITask> tasks)
			{
				if (deleteExecution)
				{
					foreach (ITaskEntity delTask in tasks.Cast<ITaskEntity>())
					{
						IActivitiEventDispatcher eventDispatcher = commandContext.ProcessEngineConfiguration.EventDispatcher;
						if (eventDispatcher.Enabled)
						{
							eventDispatcher.DispatchEvent(ActivitiEventBuilder.CreateTaskReturnEntityEvent(delTask, runActivityId));
						}

						commandContext.TaskEntityManager.DeleteTask(delTask, runReason, false, false);
					}

					DeleteExecutions(execution, deleteExecution, runReason, commandContext);
				}
			}

			/// <summary>
			/// 
			/// </summary>
			/// <param name="execution"></param>
			/// <param name="deleteExecution"></param>
			/// <param name="reason"></param>
			/// <param name="commandContext"></param>
			// TODO: can the ExecutionManager.deleteChildExecution not be used?
			protected internal virtual void DeleteExecutions(IExecutionEntity execution, bool deleteExecution, string reason, ICommandContext commandContext)
			{
				if (deleteExecution)
				{
					// Delete all child executions
					IExecutionEntityManager executionEntityManager = commandContext.ExecutionEntityManager;
					ICollection<IExecutionEntity> childExecutions = executionEntityManager.FindChildExecutionsByParentExecutionId(execution.Id);
					if (CollectionUtil.IsNotEmpty(childExecutions))
					{
						foreach (IExecutionEntity childExecution in childExecutions)
						{
							DeleteExecutions(childExecution, deleteExecution, reason, commandContext);
						}
					}
					executionEntityManager.DeleteExecutionAndRelatedData(execution, reason, false);
				}
			}

			private IExecutionEntity Execute(ICommandContext commandContext, ITaskEntity task)
			{
				if (variables is object && task is not null)
				{
					if (task.ExecutionId is not null)
					{
						task.ExecutionVariables = variables;
					}
					else
					{
						task.Variables = variables;
					}
				}

				if (transientVariables is object && task is not null)
				{
					task.TransientVariables = transientVariables;
				}

				ProcessEngineConfigurationImpl processEngineConfiguration = commandContext.ProcessEngineConfiguration;
				var commandExecutor = processEngineConfiguration.CommandExecutor;

				task?.SetVariable(WorkflowVariable.GLOBAL_APPROVALED_VARIABLE, true);
				if (task is not null)
				{
					processEngineConfiguration.HistoricVariableInstanceEntityManager
						.RecordHistoricTaskVariableInstance(task, WorkflowVariable.GLOBAL_OPERATOR_STATE, 2);
				}

				if (!string.IsNullOrWhiteSpace(runReason) && task is not null)
				{
					commandExecutor.Execute(new AddCommentCmd(task.Id, task.ProcessInstanceId, runReason));
				}

				string currentExecutionId = task?.ExecutionId;
				IExecutionEntity execution = null;
				if (task is not null)
				{
					execution = task.GetExecution();
				}
				else
				{
					var tq = processEngineConfiguration.TaskService.CreateTaskQuery();
					tq.SetProcessInstanceBusinessKey(businessKey);

					execution = (commandExecutor.Execute(new GetTasksCmd(tq, 0, 1)).FirstOrDefault() as ITaskEntity).GetExecution();
					//commandExecutor.Execute(new GetProcessInstanceByBusinessKeyCmd(businessKey)) as IExecutionEntity;
				}

				var flowElement = ProcessDefinitionUtil.GetFlowElement(execution.ProcessDefinitionId, runActivityId);
				if (flowElement is null)
				{
					throw new ActivitiObjectNotFoundException(string.Concat("No execution found for id '", currentExecutionId, "'"));
				}

				IHistoricActivityInstanceEntity hisInst = processEngineConfiguration.HistoryService.CreateHistoricActivityInstanceQuery()
					.SetProcessInstanceId(execution.Id)
					.SetActivityId(runActivityId)
					.OrderByHistoricActivityInstanceStartTime()
					.Desc()
					.List()
					.FirstOrDefault() as IHistoricActivityInstanceEntity;

				IExecutionEntityManager executionEntityManager = commandContext.ExecutionEntityManager;

				IExecutionEntity runExec = executionEntityManager.CreateChildExecution(execution.GetProcessInstance());
				runExec.CurrentFlowElement = flowElement;
				foreach (var key in variables.Keys)
				{
					runExec.SetVariable(key, variables[key]);
				}

				executionEntityManager.Insert(runExec);

				commandContext.Agenda.PlanContinueProcessOperation(runExec);

				IExecutionEntity miRoot = commandExecutor.Execute(new GetMultiInstanceRootExecutionCmd(execution));

				List<ITask> tasks = new();
				if (miRoot is not null)
				{
					ITaskQuery query = commandContext.ProcessEngineConfiguration.TaskService.CreateTaskQuery();

					IEnumerable<IExecutionEntity> childExecutions = commandExecutor.Execute(new GetChildExecutionsCmd(miRoot));

					query.SetExecutionIdIn(childExecutions.Select(x => x.Id).ToArray());

					tasks.AddRange(query.List());
				}
				else if (task is not null)
				{
					tasks.Add(task);
				}

				DeleteTasks(commandContext, task is not null, miRoot ?? execution, tasks);

				return runExec;
			}

			public IExecutionEntity Execute(ICommandContext commandContext)
			{
				ITaskEntity task = null;
				if (!string.IsNullOrWhiteSpace(taskId))
				{
					task = commandContext.TaskEntityManager.FindById<ITaskEntity>(taskId);
				}

				return Execute(commandContext, task);
			}
		}

		public IList<ITask> Execute(ICommandContext commandContext)
		{
			ProcessEngineConfigurationImpl processEngineConfiguration = commandContext.ProcessEngineConfiguration;
			var commandExecutor = processEngineConfiguration.CommandExecutor;

			var execution = commandExecutor.Execute(cmd);

			if (execution.IsMultiInstanceRoot)
			{
				return execution.Executions.SelectMany(x => x.GetTasks().Cast<ITask>()).ToList();
			}

			return execution.GetTasks().Cast<ITask>().ToList();
		}
	}
}