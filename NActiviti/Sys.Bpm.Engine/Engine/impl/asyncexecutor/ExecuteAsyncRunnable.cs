using System;
using System.Threading;

/* Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *      http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
namespace Sys.Workflow.Engine.Impl.Asyncexecutor
{
    using Microsoft.Extensions.Logging;
    using Sys.Workflow.Engine.Delegate.Events;
    using Sys.Workflow.Engine.Delegate.Events.Impl;
    using Sys.Workflow.Engine.Impl.Cfg;
    using Sys.Workflow.Engine.Impl.Cmd;
    using Sys.Workflow.Engine.Impl.Contexts;
    using Sys.Workflow.Engine.Impl.Interceptor;
    using Sys.Workflow.Engine.Impl.JobExecutors;
    using Sys.Workflow.Engine.Impl.Persistence.Entity;
    using Sys.Workflow.Engine.Runtime;
    using Sys.Workflow;
    using System.Collections.Generic;

    /// 
    /// 
    public class ExecuteAsyncRunnable
    {
        /// <summary>
        /// 
        /// </summary>
        protected internal string jobId;

        /// <summary>
        /// 
        /// </summary>
        protected internal IJob job;

        /// <summary>
        /// 
        /// </summary>
        protected internal ProcessEngineConfigurationImpl processEngineConfiguration;

        /// <summary>
        /// 
        /// </summary>
        protected internal virtual ThreadStart Runable { get; private set; }

        private static readonly ILogger<ExecuteAsyncRunnable> log = ProcessEngineServiceProvider.LoggerService<ExecuteAsyncRunnable>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="jobId"></param>
        /// <param name="processEngineConfiguration"></param>
        public ExecuteAsyncRunnable(string jobId, ProcessEngineConfigurationImpl processEngineConfiguration)
        {
            this.jobId = jobId;
            this.processEngineConfiguration = processEngineConfiguration;

            this.Runable += new ThreadStart(Run);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="job"></param>
        /// <param name="processEngineConfiguration"></param>
        public ExecuteAsyncRunnable(IJob job, ProcessEngineConfigurationImpl processEngineConfiguration)
            : this(job.Id, processEngineConfiguration)
        {
            this.job = job;
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void Run()
        {
            if (job == null)
            {
                job = processEngineConfiguration.CommandExecutor.Execute(new CommandAnonymousInnerClass(this));
            }

            bool lockNotNeededOrSuccess = LockJobIfNeeded();

            if (lockNotNeededOrSuccess)
            {
                ExecuteJob();
                UnlockJobIfNeeded();
            }
        }

        private class CommandAnonymousInnerClass : ICommand<IJobEntity>
        {
            private readonly ExecuteAsyncRunnable outerInstance;

            public CommandAnonymousInnerClass(ExecuteAsyncRunnable outerInstance)
            {
                this.outerInstance = outerInstance;
            }

            public virtual IJobEntity Execute(ICommandContext commandContext)
            {
                return commandContext.JobEntityManager.FindById<IJobEntity>(outerInstance.jobId);
            }
        }

        private class CommandAnonymousInnerClass2 : ICommand<bool>
        {
            private readonly ExecuteAsyncRunnable outerInstance;

            public CommandAnonymousInnerClass2(ExecuteAsyncRunnable outerInstance)
            {
                this.outerInstance = outerInstance;
            }

            public virtual bool Execute(ICommandContext commandContext)
            {
                bool.TryParse(commandContext.GetResult()?.ToString(), out var res);

                return res;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected internal virtual void ExecuteJob()
        {
            try
            {
                processEngineConfiguration.CommandExecutor.Execute(new ExecuteAsyncJobCmd(jobId));
            }
            catch (InvalidCastException ex)
            {
                if (log.IsEnabled(LogLevel.Debug))
                {
                    log.LogDebug($"【无效CAST异常】:{ex.Message}");
                }
            }
            catch (ActivitiOptimisticLockingException e)
            {

                HandleFailedJob(e);

                if (log.IsEnabled(LogLevel.Debug))
                {
                    log.LogDebug($"Optimistic locking exception during job execution. If you have multiple async executors running against the same database, this exception means that this thread tried to acquire an exclusive job, which already was changed by another async executor thread. This is expected behavior in a clustered environment. You can ignore this message if you indeed have multiple job executor threads running against the same database. Exception message: {e.Message}");
                }

            }
            catch (Exception exception)
            {
                HandleFailedJob(exception);

                // Finally, Throw the exception to indicate the ExecuteAsyncJobCmd failed
                string message = "Job " + jobId + " failed";
                log.LogError(exception, message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected internal virtual void UnlockJobIfNeeded()
        {
            try
            {
                if (job.Exclusive)
                {
                    processEngineConfiguration.CommandExecutor.Execute(new UnlockExclusiveJobCmd(job));
                }

            }
            catch (ActivitiOptimisticLockingException optimisticLockingException)
            {
                if (log.IsEnabled(LogLevel.Debug))
                {
                    log.LogDebug($"Optimistic locking exception while unlocking the job. If you have multiple async executors running against the same database, this exception means that this thread tried to acquire an exclusive job, which already was changed by another async executor thread. This is expected behavior in a clustered environment. You can ignore this message if you indeed have multiple job executor acquisition threads running against the same database. Exception message: {optimisticLockingException.Message}");
                }

            }
            catch (Exception t)
            {
                log.LogError(t, $"Error while unlocking exclusive job {job.Id}");
                throw t;
            }
        }

        /// <summary>
        /// Returns true if lock succeeded, or no lock was needed.
        /// Returns false if locking was unsuccessfull. 
        /// </summary>
        protected internal virtual bool LockJobIfNeeded()
        {
            try
            {
                if (job.Exclusive)
                {
                    processEngineConfiguration.CommandExecutor.Execute(new LockExclusiveJobCmd(job));
                }

            }
            catch (Exception lockException)
            {
                if (log.IsEnabled(LogLevel.Debug))
                {
                    log.LogDebug($"Could not lock exclusive job. Unlocking job so it can be acquired again. Catched exception: {lockException.Message}");
                }

                // Release the job again so it can be acquired later or by another node
                UnacquireJob();

                return false;
            }

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        protected internal virtual void UnacquireJob()
        {
            ICommandContext commandContext = Context.CommandContext;
            if (commandContext != null)
            {
                commandContext.JobManager.Unacquire(job);
            }
            else
            {
                processEngineConfiguration.CommandExecutor.Execute(new CommandAnonymousInnerClass3(this));
            }
        }

        private class CommandAnonymousInnerClass3 : ICommand<object>
        {
            private readonly ExecuteAsyncRunnable outerInstance;

            public CommandAnonymousInnerClass3(ExecuteAsyncRunnable outerInstance)
            {
                this.outerInstance = outerInstance;
            }

            public virtual object Execute(ICommandContext commandContext)
            {
                commandContext.JobManager.Unacquire(outerInstance.job);

                return commandContext.GetResult();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="exception"></param>
        protected internal virtual void HandleFailedJob(Exception exception)
        {
            processEngineConfiguration.CommandExecutor.Execute(new CommandAnonymousInnerClass4(this, exception));
        }

        private class CommandAnonymousInnerClass4 : ICommand<object>
        {
            private readonly ExecuteAsyncRunnable outerInstance;

            private readonly Exception exception;

            public CommandAnonymousInnerClass4(ExecuteAsyncRunnable outerInstance, Exception exception)
            {
                this.outerInstance = outerInstance;
                this.exception = exception;
            }


            public virtual object Execute(ICommandContext commandContext)
            {
                CommandConfig commandConfig = outerInstance.processEngineConfiguration.CommandExecutor.DefaultConfig.TransactionRequiresNew();
                IFailedJobCommandFactory failedJobCommandFactory = commandContext.FailedJobCommandFactory;
                ICommand<object> cmd = failedJobCommandFactory.GetCommand(outerInstance.job.Id, exception);

                log.LogTrace($"Using FailedJobCommandFactory '{failedJobCommandFactory.GetType()}' and command of type '{cmd.GetType()}'");
                outerInstance.processEngineConfiguration.CommandExecutor.Execute(commandConfig, cmd);

                // Dispatch an event, indicating job execution failed in a
                // try-catch block, to prevent the original exception to be swallowed
                if (commandContext.EventDispatcher.Enabled)
                {
                    try
                    {
                        commandContext.EventDispatcher.DispatchEvent(ActivitiEventBuilder.CreateEntityExceptionEvent(ActivitiEventType.JOB_EXECUTION_FAILURE, outerInstance.job, exception));
                    }
                    catch (Exception ignore)
                    {
                        log.LogWarning(ignore, "Exception occurred while dispatching job failure event, ignoring.");
                    }
                }

                return commandContext.GetResult();
            }
        }
    }
}