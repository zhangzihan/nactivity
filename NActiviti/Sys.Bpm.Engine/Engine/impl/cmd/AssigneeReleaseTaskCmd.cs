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

namespace Sys.Workflow.engine.impl.cmd
{

    using Sys.Workflow.engine.impl.interceptor;
    using Sys.Workflow.engine.impl.persistence.entity;
    using Sys.Workflow.engine.runtime;
    using Sys.Workflow.engine.task;
    using Sys.Workflow.services.api.commands;
    using Sys;
    using Sys.Workflow;
    using Sys.Workflow.Engine.Bpmn.Rules;
    using System.Linq;

    /// 
    /// 
    [Serializable]
    public class AssigneeReleaseTaskCmd : ICommand<ITask>
    {
        private const long serialVersionUID = 1L;

        private readonly string businessKey;
        private readonly string assignee;
        private readonly string taskId;
        private readonly string reason;

        public AssigneeReleaseTaskCmd(string taskId, string businessKey, string assignee, string reason)
        {
            this.businessKey = businessKey;
            this.assignee = assignee;
            this.taskId = taskId;
            this.reason = reason;
        }

        public virtual ITask Execute(ICommandContext commandContext)
        {
            var processConnfig = commandContext.ProcessEngineConfiguration;
            var commandExecutor = processConnfig.CommandExecutor;
            ITaskService taskService = processConnfig.TaskService;

            ITask task = null;
            if (string.IsNullOrWhiteSpace(taskId))
            {
                task = commandExecutor.Execute<ITask>(new GetTaskByIdCmd(taskId));

                if (task is null)
                {
                    throw new ActivitiObjectNotFoundException($"not found task with id:{taskId}");
                }
            }
            else if (string.IsNullOrWhiteSpace(businessKey) == false && string.IsNullOrWhiteSpace(assignee) == false)
            {
                task = taskService.CreateTaskQuery()
                    .SetProcessInstanceBusinessKey(businessKey)
                    .SetTaskInvolvedUser(assignee)
                    .List()
                    .FirstOrDefault();

                if (task is null)
                {
                    throw new ActivitiObjectNotFoundException($"not found task with businessKey:{businessKey}-- assignee:{assignee}");
                }
            }

            commandExecutor.Execute(new AddCommentCmd(taskId, task.ProcessInstanceId, reason));

            taskService.Unclaim(taskId);

            return taskService.CreateTaskQuery()
                .SetTaskId(task.Id)
                .SingleResult();
        }
    }
}