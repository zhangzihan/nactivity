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
    using Sys.Workflow.Engine.History;
    using Sys.Workflow.Engine.Impl.Interceptor;
    using Sys.Workflow.Engine.Impl.Persistence.Entity;
    using Sys.Workflow.Engine.Query;
    using Sys.Workflow.Engine.Runtime;
    using System.Linq;

    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class GetHistoricProcessInstancesCmd : PageQueryCmd<IHistoricProcessInstanceQuery, IHistoricProcessInstance>
    {
        private const long serialVersionUID = 1L;

        public GetHistoricProcessInstancesCmd(IQuery<IHistoricProcessInstanceQuery, IHistoricProcessInstance> query, int firstResult, int pageSize) : base(query, firstResult, pageSize)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="commandContext"></param>
        /// <returns></returns>
        public override IList<IHistoricProcessInstance> Execute(ICommandContext commandContext)
        {
            IEnumerable<IHistoricProcessInstance> insts = base.Execute(commandContext) ?? new HistoricProcessInstanceEntityImpl[0];

            insts = HistoricProcessInstanceEntityImpl.EnsureStarterInitialized(insts.Cast<HistoricProcessInstanceEntityImpl>());

            return insts.ToList();
        }
    }
}