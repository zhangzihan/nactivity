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
    using Sys.Workflow.Engine.Runtime;
    using Sys.Workflow.Engine.Tasks;
    using System.Linq;

    /// 
    [Serializable]
    public class GetHistoricProcessInstanceByIdCmd : ICommand<IHistoricProcessInstance>
    {
        private const long serialVersionUID = 1L;
        protected internal string instanceId;

        public GetHistoricProcessInstanceByIdCmd(string instanceId)
        {
            this.instanceId = instanceId;
        }

        public virtual IHistoricProcessInstance Execute(ICommandContext commandContext)
        {
            var historyService = commandContext.ProcessEngineConfiguration.HistoryService;

            IHistoricProcessInstanceQuery query = historyService.CreateHistoricProcessInstanceQuery().SetProcessInstanceId(instanceId);

            IHistoricProcessInstance processInstance = query.SingleResult();

            if (processInstance != null)
            {
                HistoricProcessInstanceEntityImpl.EnsureStarterInitialized(new HistoricProcessInstanceEntityImpl[]
                {
                    processInstance as HistoricProcessInstanceEntityImpl
                });
            }

            return processInstance;
        }
    }
}