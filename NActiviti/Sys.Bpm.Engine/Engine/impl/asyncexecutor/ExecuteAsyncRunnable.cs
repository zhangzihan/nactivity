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
namespace org.activiti.engine.impl.asyncexecutor
{
    using Microsoft.Extensions.Logging;
    using org.activiti.engine.@delegate.@event;
    using org.activiti.engine.@delegate.@event.impl;
    using org.activiti.engine.impl.cfg;
    using org.activiti.engine.impl.cmd;
    using org.activiti.engine.impl.context;
    using org.activiti.engine.impl.interceptor;
    using org.activiti.engine.impl.jobexecutor;
    using org.activiti.engine.impl.persistence.entity;
    using org.activiti.engine.runtime;
    using Sys;
    using System.Collections.Generic;

    /// 
    /// 
    public class ExecuteAsyncRunnable
    {
        protected internal string jobId;
        protected internal IJob job;
        protected internal ProcessEngineConfigurationImpl processEngineConfiguration;
        protected internal virtual ThreadStart Runable { get; private set; }

        private static readonly ILogger<ExecuteAsyncRunnable> log = ProcessEngineServiceProvider.LoggerService<ExecuteAsyncRunnable>();

        public ExecuteAsyncRunnable(string jobId, ProcessEngineConfigurationImpl processEngineConfiguration)
        {
            this.jobId = jobId;
            this.processEngineConfiguration = processEngineConfiguration;

            this.Runable += new ThreadStart(run);
        }

        public ExecuteAsyncRunnable(IJob job, ProcessEngineConfigurationImpl processEngineConfiguration)
        {
            this.job = job;
            this.jobId = job.Id;
            this.processEngineConfiguration = processEngineConfiguration;
        }


        public virtual void run()
        {
            if (job == null)
            {
                job = processEngineConfiguration.CommandExecutor.execute(new CommandAnonymousInnerClass(this));
            }

            bool lockNotNeededOrSuccess = lockJobIfNeeded();

            if (lockNotNeededOrSuccess)
            {
                executeJob();
                unlockJobIfNeeded();
            }
        }

        private class CommandAnonymousInnerClass : ICommand<IJobEntity>
        {
            private readonly ExecuteAsyncRunnable outerInstance;

            public CommandAnonymousInnerClass(ExecuteAsyncRunnable outerInstance)
            {
                this.outerInstance = outerInstance;
            }

            public virtual IJobEntity execute(ICommandContext commandContext)
            {
                return commandContext.JobEntityManager.findById<JobEntityImpl>(new KeyValuePair<string, object>("id", outerInstance.jobId));
            }
        }

        private class CommandAnonymousInnerClass2 : ICommand<bool>
        {
            private readonly ExecuteAsyncRunnable outerInstance;

            public CommandAnonymousInnerClass2(ExecuteAsyncRunnable outerInstance)
            {
                this.outerInstance = outerInstance;
            }

            public virtual bool execute(ICommandContext commandContext)
            {
                bool.TryParse(commandContext.GetResult()?.ToString(), out var res);

                return res;
            }
        }

        protected internal virtual void executeJob()
        {
            try
            {
                processEngineConfiguration.CommandExecutor.execute(new ExecuteAsyncJobCmd(jobId));
            }

            catch (ActivitiOptimisticLockingException e)
            {

                handleFailedJob(e);

                if (log.IsEnabled(LogLevel.Debug))
                {
                    log.LogDebug($"Optimistic locking exception during job execution. If you have multiple async executors running against the same database, this exception means that this thread tried to acquire an exclusive job, which already was changed by another async executor thread. This is expected behavior in a clustered environment. You can ignore this message if you indeed have multiple job executor threads running against the same database. Exception message: {e.Message}");
                }

            }
            catch (Exception exception)
            {
                handleFailedJob(exception);

                // Finally, Throw the exception to indicate the ExecuteAsyncJobCmd failed
                string message = "Job " + jobId + " failed";
                log.LogError(exception, message);
            }
        }

        protected internal virtual void unlockJobIfNeeded()
        {
            try
            {
                if (job.Exclusive)
                {
                    processEngineConfiguration.CommandExecutor.execute(new UnlockExclusiveJobCmd(job));
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
        protected internal virtual bool lockJobIfNeeded()
        {
            try
            {
                if (job.Exclusive)
                {
                    processEngineConfiguration.CommandExecutor.execute(new LockExclusiveJobCmd(job));
                }

            }
            catch (Exception lockException)
            {
                if (log.IsEnabled(LogLevel.Debug))
                {
                    log.LogDebug($"Could not lock exclusive job. Unlocking job so it can be acquired again. Catched exception: {lockException.Message}");
                }

                // Release the job again so it can be acquired later or by another node
                unacquireJob();

                return false;
            }

            return true;
        }

        protected internal virtual void unacquireJob()
        {
            ICommandContext commandContext = Context.CommandContext;
            if (commandContext != null)
            {
                commandContext.JobManager.unacquire(job);
            }
            else
            {
                processEngineConfiguration.CommandExecutor.execute(new CommandAnonymousInnerClass3(this, commandContext));
            }
        }

        private class CommandAnonymousInnerClass3 : ICommand<object>
        {
            private readonly ExecuteAsyncRunnable outerInstance;

            private ICommandContext commandContext;

            public CommandAnonymousInnerClass3(ExecuteAsyncRunnable outerInstance, ICommandContext commandContext)
            {
                this.outerInstance = outerInstance;
                this.commandContext = commandContext;
            }

            public virtual object execute(ICommandContext commandContext)
            {
                commandContext.JobManager.unacquire(outerInstance.job);

                return commandContext.GetResult();
            }
        }

        protected internal virtual void handleFailedJob(Exception exception)
        {
            processEngineConfiguration.CommandExecutor.execute(new CommandAnonymousInnerClass4(this, exception));
        }

        private class CommandAnonymousInnerClass4 : ICommand<object>
        {
            private readonly ExecuteAsyncRunnable outerInstance;

            private Exception exception;

            public CommandAnonymousInnerClass4(ExecuteAsyncRunnable outerInstance, Exception exception)
            {
                this.outerInstance = outerInstance;
                this.exception = exception;
            }


            public virtual object execute(ICommandContext commandContext)
            {
                CommandConfig commandConfig = outerInstance.processEngineConfiguration.CommandExecutor.DefaultConfig.transactionRequiresNew();
                IFailedJobCommandFactory failedJobCommandFactory = commandContext.FailedJobCommandFactory;
                ICommand<object> cmd = failedJobCommandFactory.getCommand(outerInstance.job.Id, exception);

                log.LogTrace($"Using FailedJobCommandFactory '{failedJobCommandFactory.GetType()}' and command of type '{cmd.GetType()}'");
                outerInstance.processEngineConfiguration.CommandExecutor.execute(commandConfig, cmd);

                // Dispatch an event, indicating job execution failed in a
                // try-catch block, to prevent the original exception to be swallowed
                if (commandContext.EventDispatcher.Enabled)
                {
                    try
                    {
                        commandContext.EventDispatcher.dispatchEvent(ActivitiEventBuilder.createEntityExceptionEvent(ActivitiEventType.JOB_EXECUTION_FAILURE, outerInstance.job, exception));
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