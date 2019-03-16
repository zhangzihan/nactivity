using System.Collections.Generic;

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
namespace org.activiti.engine.impl.cmd
{

    using org.activiti.engine.impl.interceptor;
    using org.activiti.engine.impl.persistence.entity;
    using org.activiti.engine.impl.util;
    using org.activiti.engine.runtime;

    /// 
    /// 
    public abstract class AbstractSetProcessInstanceStateCmd : ICommand<object>
    {

        protected internal readonly string processInstanceId;

        public AbstractSetProcessInstanceStateCmd(string processInstanceId)
        {
            this.processInstanceId = processInstanceId;
        }

        public virtual object execute(ICommandContext commandContext)
        {

            if (ReferenceEquals(processInstanceId, null))
            {
                throw new ActivitiIllegalArgumentException("ProcessInstanceId cannot be null.");
            }

            IExecutionEntity executionEntity = commandContext.ExecutionEntityManager.findById<IExecutionEntity>(processInstanceId);

            if (executionEntity == null)
            {
                throw new ActivitiObjectNotFoundException("Cannot find processInstance for id '" + processInstanceId + "'.", typeof(IExecution));
            }
            if (!executionEntity.ProcessInstanceType)
            {
                throw new ActivitiException("Cannot set suspension state for execution '" + processInstanceId + "': not a process instance.");
            }

            SuspensionStateUtil.setSuspensionState(executionEntity, NewState);
            commandContext.ExecutionEntityManager.update(executionEntity, false);

            // All child executions are suspended
            ICollection<IExecutionEntity> childExecutions = commandContext.ExecutionEntityManager.findChildExecutionsByProcessInstanceId(processInstanceId);
            foreach (IExecutionEntity childExecution in childExecutions)
            {
                if (!childExecution.Id.Equals(processInstanceId))
                {
                    SuspensionStateUtil.setSuspensionState(childExecution, NewState);
                    commandContext.ExecutionEntityManager.update(childExecution, false);
                }
            }

            // All tasks are suspended
            IList<ITaskEntity> tasks = commandContext.TaskEntityManager.findTasksByProcessInstanceId(processInstanceId);
            foreach (ITaskEntity taskEntity in tasks)
            {
                SuspensionStateUtil.setSuspensionState(taskEntity, NewState);
                commandContext.TaskEntityManager.update(taskEntity, false);
            }

            // All jobs are suspended
            if (NewState == SuspensionStateProvider.ACTIVE)
            {
                IList<ISuspendedJobEntity> suspendedJobs = commandContext.SuspendedJobEntityManager.findJobsByProcessInstanceId(processInstanceId);
                foreach (ISuspendedJobEntity suspendedJob in suspendedJobs)
                {
                    commandContext.JobManager.activateSuspendedJob(suspendedJob);
                }

            }
            else
            {
                IList<ITimerJobEntity> timerJobs = commandContext.TimerJobEntityManager.findJobsByProcessInstanceId(processInstanceId);
                foreach (ITimerJobEntity timerJob in timerJobs)
                {
                    commandContext.JobManager.moveJobToSuspendedJob(timerJob);
                }

                IList<IJobEntity> jobs = commandContext.JobEntityManager.findJobsByProcessInstanceId(processInstanceId);
                foreach (IJobEntity job in jobs)
                {
                    commandContext.JobManager.moveJobToSuspendedJob(job);
                }
            }

            return null;
        }

        protected internal abstract ISuspensionState NewState { get; }

    }

}