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

        public virtual object Execute(ICommandContext commandContext)
        {

            if (processInstanceId is null)
            {
                throw new ActivitiIllegalArgumentException("ProcessInstanceId cannot be null.");
            }

            IExecutionEntity executionEntity = commandContext.ExecutionEntityManager.FindById<IExecutionEntity>(processInstanceId);

            if (executionEntity == null)
            {
                throw new ActivitiObjectNotFoundException("Cannot find processInstance for id '" + processInstanceId + "'.", typeof(IExecution));
            }
            if (!executionEntity.ProcessInstanceType)
            {
                throw new ActivitiException("Cannot set suspension state for execution '" + processInstanceId + "': not a process instance.");
            }

            SuspensionStateUtil.SetSuspensionState(executionEntity, NewState);
            commandContext.ExecutionEntityManager.Update(executionEntity, false);

            // All child executions are suspended
            ICollection<IExecutionEntity> childExecutions = commandContext.ExecutionEntityManager.FindChildExecutionsByProcessInstanceId(processInstanceId);
            foreach (IExecutionEntity childExecution in childExecutions)
            {
                if (!childExecution.Id.Equals(processInstanceId))
                {
                    SuspensionStateUtil.SetSuspensionState(childExecution, NewState);
                    commandContext.ExecutionEntityManager.Update(childExecution, false);
                }
            }

            // All tasks are suspended
            IList<ITaskEntity> tasks = commandContext.TaskEntityManager.FindTasksByProcessInstanceId(processInstanceId);
            foreach (ITaskEntity taskEntity in tasks)
            {
                SuspensionStateUtil.SetSuspensionState(taskEntity, NewState);
                commandContext.TaskEntityManager.Update(taskEntity, false);
            }

            // All jobs are suspended
            if (NewState == SuspensionStateProvider.ACTIVE)
            {
                IList<ISuspendedJobEntity> suspendedJobs = commandContext.SuspendedJobEntityManager.FindJobsByProcessInstanceId(processInstanceId);
                foreach (ISuspendedJobEntity suspendedJob in suspendedJobs)
                {
                    commandContext.JobManager.ActivateSuspendedJob(suspendedJob);
                }

            }
            else
            {
                IList<ITimerJobEntity> timerJobs = commandContext.TimerJobEntityManager.FindJobsByProcessInstanceId(processInstanceId);
                foreach (ITimerJobEntity timerJob in timerJobs)
                {
                    commandContext.JobManager.MoveJobToSuspendedJob(timerJob);
                }

                IList<IJobEntity> jobs = commandContext.JobEntityManager.FindJobsByProcessInstanceId(processInstanceId);
                foreach (IJobEntity job in jobs)
                {
                    commandContext.JobManager.MoveJobToSuspendedJob(job);
                }
            }

            return null;
        }

        protected internal abstract ISuspensionState NewState { get; }

    }

}