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

namespace Sys.Workflow.Engine.Impl.Cmd
{
    using Newtonsoft.Json.Linq;
    using Sys.Workflow.Engine.Impl.Interceptor;
    using Sys.Workflow.Engine.Impl.Persistence.Entity;
    using Sys.Workflow.Engine.Tasks;
    using Sys.Net.Http;
    using Sys.Workflow;
    using System.Linq;

    /// 
    [Serializable]
    public class GetMyTasksCmd : ICommand<IList<ITask>>
    {

        private const long serialVersionUID = 1L;
        protected internal string assignee;

        public GetMyTasksCmd(string assignee)
        {
            this.assignee = assignee;
        }

        public virtual IList<ITask> Execute(ICommandContext commandContext)
        {
            var taskService = commandContext.ProcessEngineConfiguration.TaskService;

            List<ITask> tasks = taskService.CreateTaskQuery()
                .SetTaskAssignee(assignee)
                .OrderByTaskCreateTime()
                .Desc()
                .List()
                .ToList();

            if (tasks.Count > 0)
            {
                return TaskEntityImpl.EnsureAssignerInitialized(tasks.Cast<TaskEntityImpl>()).ToList();
            }

            return tasks;
        }
    }
}