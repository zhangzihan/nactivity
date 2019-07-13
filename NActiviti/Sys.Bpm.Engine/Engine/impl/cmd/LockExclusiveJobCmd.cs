using System;

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
namespace Sys.Workflow.Engine.Impl.Cmd
{
    using Microsoft.Extensions.Logging;
    using Sys.Workflow.Engine.Impl.Interceptor;
    using Sys.Workflow.Engine.Impl.Persistence.Entity;
    using Sys.Workflow.Engine.Runtime;
    using Sys.Workflow;

    /// 
    [Serializable]
    public class LockExclusiveJobCmd : ICommand<object>
    {
        private static readonly ILogger<LockExclusiveJobCmd> log = ProcessEngineServiceProvider.LoggerService<LockExclusiveJobCmd>();

        private const long serialVersionUID = 1L;

        protected internal IJob job;

        public LockExclusiveJobCmd(IJob job)
        {
            this.job = job;
        }

        public  virtual object  Execute(ICommandContext commandContext)
        {

            if (job == null)
            {
                throw new ActivitiIllegalArgumentException("job is null");
            }

            if (log.IsEnabled(LogLevel.Debug))
            {
                log.LogDebug($"Executing lock exclusive job {job.Id} {job.ExecutionId}");
            }

            if (job.Exclusive)
            {
                if (job.ExecutionId is object)
                {
                    IExecutionEntity execution = commandContext.ExecutionEntityManager.FindById<IExecutionEntity>(job.ExecutionId);
                    if (execution != null)
                    {
                        commandContext.ExecutionEntityManager.UpdateProcessInstanceLockTime(execution.ProcessInstanceId);
                    }
                }
            }

            return null;
        }
    }
}