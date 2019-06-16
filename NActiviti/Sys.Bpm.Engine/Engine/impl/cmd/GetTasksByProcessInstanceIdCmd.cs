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
    using Newtonsoft.Json.Linq;
    using Sys.Workflow.engine.impl.cfg;
    using Sys.Workflow.engine.impl.interceptor;
    using Sys.Workflow.engine.impl.persistence.entity;
    using Sys.Workflow.engine.query;
    using Sys.Workflow.engine.runtime;
    using Sys.Workflow.engine.task;
    using Sys.Net.Http;
    using System.Linq;

    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class GetTasksByProcessInstanceIdCmd : ICommand<IList<ITask>>
    {
        private const long serialVersionUID = 1L;

        private readonly string[] processInstanceIds;

        public GetTasksByProcessInstanceIdCmd(string[] processInstanceIds)
        {
            this.processInstanceIds = processInstanceIds;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="commandContext"></param>
        /// <returns></returns>
        public virtual IList<ITask> Execute(ICommandContext commandContext)
        {
            ProcessEngineConfigurationImpl processEngineConfiguration = commandContext.ProcessEngineConfiguration;

            IEnumerable<ITask> tasks = processEngineConfiguration.TaskService
                .CreateTaskQuery()
                .SetProcessInstanceIdIn(processInstanceIds)
                .List();

            tasks = TaskEntityImpl.EnsureAssignerInitialized(tasks.Cast<TaskEntityImpl>());

            return tasks.ToList();
        }
    }
}