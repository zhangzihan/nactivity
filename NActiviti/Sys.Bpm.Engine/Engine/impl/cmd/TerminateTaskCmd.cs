using System;
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
    using org.activiti.engine.impl.context;
    using org.activiti.engine.impl.interceptor;
    using org.activiti.engine.impl.persistence.entity;
    using org.activiti.engine.task;

    /// 
    /// 
    [Serializable]
    public class TerminateTaskCmd : ICommand<object>
    {
        private const long serialVersionUID = 1L;

        private readonly string taskId;
        private readonly string terminateReason;
        private readonly bool isTerminateExecution;

        public TerminateTaskCmd(string taskId, string terminateReason, bool isTerminateExecution)
        {
            this.taskId = taskId;
            this.terminateReason = terminateReason;
            this.isTerminateExecution = isTerminateExecution;
        }

        public virtual object execute(ICommandContext commandContext)
        {
            if (string.IsNullOrWhiteSpace(taskId))
            {
                throw new ActivitiIllegalArgumentException("taskId is null");
            }

            ITaskEntity task = commandContext.TaskEntityManager.findById<ITaskEntity>(taskId);

            if (task == null)
            {
                throw new ActivitiObjectNotFoundException("Cannot find task with id " + taskId, typeof(ITask));
            }

            string executionId = task.ExecutionId;

            if (isTerminateExecution && string.IsNullOrWhiteSpace(executionId) == false)
            {
                IExecutionEntity execution = commandContext.ExecutionEntityManager.findById<IExecutionEntity>(executionId);

                if (execution != null)
                {
                    var query = new TaskQueryImpl();
                    query.executionId(executionId);
                    query.taskIdNotIn(new string[] { taskId });

                    long count = commandContext.TaskEntityManager.findTaskCountByQueryCriteria(query);
                    if (count == 0)
                    {
                        Context.ProcessEngineConfiguration.CommandExecutor.execute(new DeleteProcessInstanceCmd(execution.ProcessInstanceId, "强制终止任务导致当前流程已不存在可执行的任务,流程已强制终止."));
                    }
                    else
                    {
                        deleteTask(commandContext, task);
                    }
                }
                else
                {
                    deleteTask(commandContext, task);
                }
            }
            else
            {
                deleteTask(commandContext, task);
            }

            return null;
        }

        private void deleteTask(ICommandContext commandContext, ITaskEntity task)
        {
            task.ExecutionId = null;
            commandContext.TaskEntityManager.update(task);

            Context.ProcessEngineConfiguration.CommandExecutor.execute(new DeleteTaskCmd(taskId, terminateReason, false));
        }
    }
}