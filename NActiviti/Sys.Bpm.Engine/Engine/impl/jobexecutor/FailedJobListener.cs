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

namespace Sys.Workflow.Engine.Impl.JobExecutors
{
    using Microsoft.Extensions.Logging;
    using Sys.Workflow.Engine.Delegate.Events;
    using Sys.Workflow.Engine.Delegate.Events.Impl;
    using Sys.Workflow.Engine.Impl.Interceptor;
    using Sys.Workflow.Engine.Runtime;
    using Sys.Workflow;

    /// 
    /// 
    /// 
    public class FailedJobListener : ICommandContextCloseListener
    {
        private static readonly ILogger log = ProcessEngineServiceProvider.LoggerService<FailedJobListener>();

        protected internal ICommandExecutor commandExecutor;
        protected internal IJob job;

        public FailedJobListener(ICommandExecutor commandExecutor, IJob job)
        {
            this.commandExecutor = commandExecutor;
            this.job = job;
        }

        public virtual void Closing(ICommandContext commandContext)
        {
        }

        public virtual void AfterSessionsFlush(ICommandContext commandContext)
        {
        }

        public virtual void Closed(ICommandContext context)
        {
            if (context.EventDispatcher.Enabled)
            {
                context.EventDispatcher.DispatchEvent(ActivitiEventBuilder.CreateEntityEvent(ActivitiEventType.JOB_EXECUTION_SUCCESS, job));
            }
        }

        public virtual void CloseFailure(ICommandContext commandContext)
        {
            if (commandContext.EventDispatcher.Enabled)
            {
                commandContext.EventDispatcher.DispatchEvent(ActivitiEventBuilder.CreateEntityExceptionEvent(ActivitiEventType.JOB_EXECUTION_FAILURE, job, commandContext.Exception));
            }

            CommandConfig commandConfig = commandExecutor.DefaultConfig.TransactionRequiresNew();
            IFailedJobCommandFactory failedJobCommandFactory = commandContext.FailedJobCommandFactory;
            ICommand<object> cmd = failedJobCommandFactory.GetCommand(job.Id, commandContext.Exception);

            log.LogTrace("Using FailedJobCommandFactory '" + failedJobCommandFactory.GetType() + "' and command of type '" + cmd.GetType() + "'");
            commandExecutor.Execute(commandConfig, cmd);
        }

    }

}