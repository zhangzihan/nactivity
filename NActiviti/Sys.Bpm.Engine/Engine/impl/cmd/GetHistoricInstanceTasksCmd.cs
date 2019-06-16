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
    using Sys.Workflow.engine.history;
    using Sys.Workflow.engine.impl.interceptor;
    using Sys.Workflow.engine.impl.persistence.entity;
    using Sys.Workflow.engine.query;
    using System.Linq;

    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class GetHistoricInstanceTasksCmd : PageQueryCmd<IHistoricTaskInstanceQuery, IHistoricTaskInstance>
    {
        private const long serialVersionUID = 1L;

        public GetHistoricInstanceTasksCmd(IQuery<IHistoricTaskInstanceQuery, IHistoricTaskInstance> query, int firstResult, int pageSize) : base(query, firstResult, pageSize)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="commandContext"></param>
        /// <returns></returns>
        public override IList<IHistoricTaskInstance> Execute(ICommandContext commandContext)
        {
            IEnumerable<IHistoricTaskInstance> tasks = base.Execute(commandContext) ?? new HistoricTaskInstanceEntityImpl[0];

            tasks = HistoricTaskInstanceEntityImpl.EnsureAssignerInitialized(tasks.Cast<HistoricTaskInstanceEntityImpl>());

            return tasks.ToList();
        }
    }
}