using System.Collections.Generic;

namespace Sys.Workflow.Engine.Impl.Agenda
{
    using Microsoft.Extensions.Logging;
    using Sys.Workflow.Engine.Impl.Interceptor;
    using Sys.Workflow.Engine.Impl.Persistence.Entity;
    using Sys.Workflow;
    using System;

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
        private static readonly ILogger<DestroyScopeOperation> logger = ProcessEngineServiceProvider.LoggerService<DestroyScopeOperation>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="commandContext"></param>
        /// <param name="execution"></param>
        public DestroyScopeOperation(ICommandContext commandContext, IExecutionEntity execution) : base(commandContext, execution)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void RunOperation()
        {
            try
            {
                // Find the actual scope that needs to be destroyed.
                // This could be the incoming execution, or the first parent execution where isScope = true

                // Find parent scope execution
                IExecutionEntity scopeExecution = execution.IsScope ? execution : FindFirstParentScopeExecution(execution);

                if (scopeExecution == null)
                {
                    throw new ActivitiException("Programmatic error: no parent scope execution found for boundary event");
                }

                IExecutionEntityManager executionEntityManager = commandContext.ExecutionEntityManager;
                DeleteAllChildExecutions(executionEntityManager, scopeExecution);

                // Delete all scope tasks
                ITaskEntityManager taskEntityManager = commandContext.TaskEntityManager;
                DeleteAllScopeTasks(scopeExecution, taskEntityManager);

                // Delete all scope jobs
                ITimerJobEntityManager timerJobEntityManager = commandContext.TimerJobEntityManager;
                DeleteAllScopeJobs(scopeExecution, timerJobEntityManager);

                // Remove variables associated with this scope
                IVariableInstanceEntityManager variableInstanceEntityManager = commandContext.VariableInstanceEntityManager;
                RemoveAllVariablesFromScope(scopeExecution, variableInstanceEntityManager);

                commandContext.HistoryManager.RecordActivityEnd(scopeExecution, scopeExecution.DeleteReason);
                executionEntityManager.Delete(scopeExecution);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                throw;
            }
        }

        private void RemoveAllVariablesFromScope(IExecutionEntity scopeExecution, IVariableInstanceEntityManager variableInstanceEntityManager)
        {
            ICollection<IVariableInstanceEntity> variablesForExecution = variableInstanceEntityManager.FindVariableInstancesByExecutionId(scopeExecution.Id);
            foreach (IVariableInstanceEntity variable in variablesForExecution)
            {
                variableInstanceEntityManager.Delete(variable);
            }
        }

        private void DeleteAllScopeJobs(IExecutionEntity scopeExecution, ITimerJobEntityManager timerJobEntityManager)
        {
            ICollection<ITimerJobEntity> timerJobsForExecution = timerJobEntityManager.FindJobsByExecutionId(scopeExecution.Id);
            foreach (ITimerJobEntity job in timerJobsForExecution)
            {
                timerJobEntityManager.Delete(job);
            }

            IJobEntityManager jobEntityManager = commandContext.JobEntityManager;
            ICollection<IJobEntity> jobsForExecution = jobEntityManager.FindJobsByExecutionId(scopeExecution.Id);
            foreach (IJobEntity job in jobsForExecution)
            {
                jobEntityManager.Delete(job);
            }

            ISuspendedJobEntityManager suspendedJobEntityManager = commandContext.SuspendedJobEntityManager;
            ICollection<ISuspendedJobEntity> suspendedJobsForExecution = suspendedJobEntityManager.FindJobsByExecutionId(scopeExecution.Id);
            foreach (ISuspendedJobEntity job in suspendedJobsForExecution)
            {
                suspendedJobEntityManager.Delete(job);
            }

            IDeadLetterJobEntityManager deadLetterJobEntityManager = commandContext.DeadLetterJobEntityManager;
            ICollection<IDeadLetterJobEntity> deadLetterJobsForExecution = deadLetterJobEntityManager.FindJobsByExecutionId(scopeExecution.Id);
            foreach (IDeadLetterJobEntity job in deadLetterJobsForExecution)
            {
                deadLetterJobEntityManager.Delete(job);
            }
        }

        private void DeleteAllScopeTasks(IExecutionEntity scopeExecution, ITaskEntityManager taskEntityManager)
        {
            ICollection<ITaskEntity> tasksForExecution = taskEntityManager.FindTasksByExecutionId(scopeExecution.Id);
            foreach (ITaskEntity taskEntity in tasksForExecution)
            {
                taskEntityManager.DeleteTask(taskEntity, execution.DeleteReason, false, false);
            }
        }

        private IExecutionEntityManager DeleteAllChildExecutions(IExecutionEntityManager executionEntityManager, IExecutionEntity scopeExecution)
        {
            // Delete all child executions
            ICollection<IExecutionEntity> childExecutions = executionEntityManager.FindChildExecutionsByParentExecutionId(scopeExecution.Id);
            foreach (IExecutionEntity childExecution in childExecutions)
            {
                executionEntityManager.DeleteExecutionAndRelatedData(childExecution, execution.DeleteReason, false);
            }
            return executionEntityManager;
        }
    }
}