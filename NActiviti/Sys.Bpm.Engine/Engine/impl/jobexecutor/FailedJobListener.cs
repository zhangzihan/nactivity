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

namespace org.activiti.engine.impl.jobexecutor
{
    using org.activiti.engine.@delegate.@event;
    using org.activiti.engine.@delegate.@event.impl;
    using org.activiti.engine.impl.interceptor;
    using org.activiti.engine.runtime;

    /// 
    /// 
    /// 
    public class FailedJobListener : ICommandContextCloseListener
    {
        protected internal ICommandExecutor commandExecutor;
        protected internal IJob job;

        public FailedJobListener(ICommandExecutor commandExecutor, IJob job)
        {
            this.commandExecutor = commandExecutor;
            this.job = job;
        }

        public virtual void closing(ICommandContext commandContext)
        {
        }

        public virtual void afterSessionsFlush(ICommandContext commandContext)
        {
        }

        public virtual void closed(ICommandContext context)
        {
            if (context.EventDispatcher.Enabled)
            {
                context.EventDispatcher.dispatchEvent(ActivitiEventBuilder.createEntityEvent(ActivitiEventType.JOB_EXECUTION_SUCCESS, job));
            }
        }

        public virtual void closeFailure(ICommandContext commandContext)
        {
            if (commandContext.EventDispatcher.Enabled)
            {
                commandContext.EventDispatcher.dispatchEvent(ActivitiEventBuilder.createEntityExceptionEvent(ActivitiEventType.JOB_EXECUTION_FAILURE, job, commandContext.Exception));
            }

            CommandConfig commandConfig = commandExecutor.DefaultConfig.transactionRequiresNew();
            IFailedJobCommandFactory failedJobCommandFactory = commandContext.FailedJobCommandFactory;
            ICommand<object> cmd = failedJobCommandFactory.getCommand(job.Id, commandContext.Exception);

            //log.trace("Using FailedJobCommandFactory '" + failedJobCommandFactory.GetType() + "' and command of type '" + cmd.GetType() + "'");
            commandExecutor.execute(commandConfig, cmd);
        }

    }

}