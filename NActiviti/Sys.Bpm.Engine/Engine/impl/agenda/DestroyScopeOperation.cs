using System.Collections.Generic;

namespace org.activiti.engine.impl.agenda
{

    using org.activiti.engine.impl.interceptor;
    using org.activiti.engine.impl.persistence.entity;

    /// <summary>
    /// Destroys a scope (for example a subprocess): this means that all child executions,
    /// tasks, jobs, variables, etc within that scope are deleted.
    /// <para>
    /// The typical example is an interrupting boundary event that is on the boundary
    /// of a subprocess and is triggered. At that point, everything within the subprocess would
    /// need to be destroyed.
    /// </para>
    /// </summary>
    public class DestroyScopeOperation : AbstractOperation
	{

		public DestroyScopeOperation(ICommandContext commandContext, IExecutionEntity execution) : base(commandContext, execution)
		{
		}

        protected override void run()
		{

			// Find the actual scope that needs to be destroyed.
			// This could be the incoming execution, or the first parent execution where isScope = true

			// Find parent scope execution
			IExecutionEntity scopeExecution = execution.IsScope ? execution : findFirstParentScopeExecution(execution);

			if (scopeExecution == null)
			{
				throw new ActivitiException("Programmatic error: no parent scope execution found for boundary event");
			}

			IExecutionEntityManager executionEntityManager = commandContext.ExecutionEntityManager;
			deleteAllChildExecutions(executionEntityManager, scopeExecution);

			// Delete all scope tasks
			ITaskEntityManager taskEntityManager = commandContext.TaskEntityManager;
			deleteAllScopeTasks(scopeExecution, taskEntityManager);

			// Delete all scope jobs
			ITimerJobEntityManager timerJobEntityManager = commandContext.TimerJobEntityManager;
			deleteAllScopeJobs(scopeExecution, timerJobEntityManager);


			// Remove variables associated with this scope
			IVariableInstanceEntityManager variableInstanceEntityManager = commandContext.VariableInstanceEntityManager;
			removeAllVariablesFromScope(scopeExecution, variableInstanceEntityManager);

			commandContext.HistoryManager.recordActivityEnd(scopeExecution, scopeExecution.DeleteReason);
			executionEntityManager.delete(scopeExecution);
		}

		private void removeAllVariablesFromScope(IExecutionEntity scopeExecution, IVariableInstanceEntityManager variableInstanceEntityManager)
		{
			ICollection<IVariableInstanceEntity> variablesForExecution = variableInstanceEntityManager.findVariableInstancesByExecutionId(scopeExecution.Id);
			foreach (IVariableInstanceEntity variable in variablesForExecution)
			{
				variableInstanceEntityManager.delete(variable);
			}
		}

		private void deleteAllScopeJobs(IExecutionEntity scopeExecution, ITimerJobEntityManager timerJobEntityManager)
		{
			ICollection<ITimerJobEntity> timerJobsForExecution = timerJobEntityManager.findJobsByExecutionId(scopeExecution.Id);
			foreach (ITimerJobEntity job in timerJobsForExecution)
			{
				timerJobEntityManager.delete(job);
			}

			IJobEntityManager jobEntityManager = commandContext.JobEntityManager;
			ICollection<IJobEntity> jobsForExecution = jobEntityManager.findJobsByExecutionId(scopeExecution.Id);
			foreach (IJobEntity job in jobsForExecution)
			{
				jobEntityManager.delete(job);
			}

			ISuspendedJobEntityManager suspendedJobEntityManager = commandContext.SuspendedJobEntityManager;
			ICollection<ISuspendedJobEntity> suspendedJobsForExecution = suspendedJobEntityManager.findJobsByExecutionId(scopeExecution.Id);
			foreach (ISuspendedJobEntity job in suspendedJobsForExecution)
			{
				suspendedJobEntityManager.delete(job);
			}

			IDeadLetterJobEntityManager deadLetterJobEntityManager = commandContext.DeadLetterJobEntityManager;
			ICollection<IDeadLetterJobEntity> deadLetterJobsForExecution = deadLetterJobEntityManager.findJobsByExecutionId(scopeExecution.Id);
			foreach (IDeadLetterJobEntity job in deadLetterJobsForExecution)
			{
				deadLetterJobEntityManager.delete(job);
			}
		}

		private void deleteAllScopeTasks(IExecutionEntity scopeExecution, ITaskEntityManager taskEntityManager)
		{
			ICollection<ITaskEntity> tasksForExecution = taskEntityManager.findTasksByExecutionId(scopeExecution.Id);
			foreach (ITaskEntity taskEntity in tasksForExecution)
			{
				taskEntityManager.deleteTask(taskEntity, execution.DeleteReason, false, false);
			}
		}

		private IExecutionEntityManager deleteAllChildExecutions(IExecutionEntityManager executionEntityManager, IExecutionEntity scopeExecution)
		{
			// Delete all child executions
			ICollection<IExecutionEntity> childExecutions = executionEntityManager.findChildExecutionsByParentExecutionId(scopeExecution.Id);
			foreach (IExecutionEntity childExecution in childExecutions)
			{
				executionEntityManager.deleteExecutionAndRelatedData(childExecution, execution.DeleteReason, false);
			}
			return executionEntityManager;
		}
	}

}