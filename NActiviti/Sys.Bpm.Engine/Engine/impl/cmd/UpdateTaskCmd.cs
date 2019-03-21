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


    using org.activiti.engine.@delegate.@event;
    using org.activiti.engine.@delegate.@event.impl;
    using org.activiti.engine.impl.identity;
    using org.activiti.engine.impl.interceptor;
    using org.activiti.engine.impl.persistence.entity;
    using org.activiti.engine.impl.util;
    using org.activiti.engine.runtime;
    using org.activiti.engine.task;
    using org.activiti.services.api.commands;
    using System;
    using System.Collections.Generic;

    /// 
    /// 
    // Not Serializable
    public class UpdateTaskCmd : ICommand<ITask>
    {
        protected internal IUpdateTaskCmd updateTaskCmd;
        
        public UpdateTaskCmd(IUpdateTaskCmd cmd)
        {
            this.updateTaskCmd = cmd;
        }

        public virtual ITask execute(ICommandContext commandContext)
        {
            ITaskService taskService = commandContext.ProcessEngineConfiguration.TaskService;
            ITask task = taskService.createTaskQuery().taskId(updateTaskCmd.TaskId).singleResult();
            if (task == null)
            {
                throw new ActivitiObjectNotFoundException("Unable to find task for the given id: " + updateTaskCmd.TaskId);
            }
            task.Assignee = updateTaskCmd.Assignee;
            task.Name = updateTaskCmd.Name;
            task.Description = updateTaskCmd.Description;
            task.DueDate = updateTaskCmd.DueDate;
            task.Priority = updateTaskCmd.Priority;
            task.ParentTaskId = updateTaskCmd.ParentTaskId;
            taskService.saveTask(task);

            return task;
        }
    }
}