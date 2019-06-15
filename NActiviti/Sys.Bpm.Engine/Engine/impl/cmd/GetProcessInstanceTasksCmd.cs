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
    using Newtonsoft.Json.Linq;
    using org.activiti.engine.impl.interceptor;
    using org.activiti.engine.impl.persistence.entity;
    using org.activiti.engine.query;
    using org.activiti.engine.runtime;
    using org.activiti.engine.task;
    using Sys.Net.Http;
    using System.Linq;

    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class GetProcessInstanceTasksCmd : PageQueryCmd<ITaskQuery, ITask>
    {
        private const long serialVersionUID = 1L;

        public GetProcessInstanceTasksCmd(IQuery<ITaskQuery, ITask> query, int firstResult, int pageSize) : base(query, firstResult, pageSize)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="commandContext"></param>
        /// <returns></returns>
        public override IList<ITask> Execute(ICommandContext commandContext)
        {
            IEnumerable<ITask> tasks = base.Execute(commandContext) ?? new TaskEntityImpl[0];

            tasks = TaskEntityImpl.EnsureAssignerInitialized(tasks.Cast<TaskEntityImpl>());

            return tasks.ToList();
        }
    }
}