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
    public class ReassignTaskUsersCmd : ICommand<ITask[]>
    {
        private const long serialVersionUID = 1L;

        private readonly IUserServiceProxy userService = ProcessEngineServiceProvider.Resolve<IUserServiceProxy>();

        private readonly ReassignUser[] users = null;

        public ReassignTaskUsersCmd(ReassignUser[] users)
        {
            this.users = users;
        }

        public virtual ITask[] Execute(ICommandContext commandContext)
        {
            if (users == null)
            {
                throw new ActivitiException("There are no personnel to be assigned.");
            }

            var taskService = commandContext.ProcessEngineConfiguration.TaskService;
            var processConfig = commandContext.ProcessEngineConfiguration;
            IList<ITask> tasks = new List<ITask>();
            foreach (var user in users)
            {
                ITaskEntity task = commandContext.TaskEntityManager.FindById<ITaskEntity>(user.TaskId);
                taskService.Unclaim(user.TaskId);
                taskService.Claim(user.TaskId, user.To);
                tasks.Add(task);
                //if (string.IsNullOrWhiteSpace(user.BusinessKey) == false)
                //{
                //    IList<IExecution> executions = processConfig.RuntimeService.CreateExecutionQuery()
                //        .SetProcessInstanceBusinessKey(user.BusinessKey)
                //        .List();

                //}
                //else if (string.IsNullOrWhiteSpace(user.TaskId) == false)
                //{
                //}
                //else
                //{
                //    throw new ActivitiException("must input businessKey or taskId.");
                //}
            }

            return tasks.ToArray();
        }
    }

}