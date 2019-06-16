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
    using Sys.Workflow.Bpmn.Models;
    using Sys.Workflow.Engine.History;
    using Sys.Workflow.Engine.Impl.Interceptor;
    using Sys.Workflow.Engine.Impl.Persistence.Entity;
    using Sys.Workflow.Engine.Impl.Util;
    using Sys.Workflow.Engine.Tasks;
    using Sys.Net.Http;
    using Sys.Workflow;
    using System.Linq;

    /// 
    [Serializable]
    public class GetCompletedTaskModelsCmd : ICommand<IList<FlowElement>>
    {
        private const long serialVersionUID = 1L;
        private readonly string processInstanceId;
        private readonly bool? finished;

        public GetCompletedTaskModelsCmd(string processInstanceId, bool? finished)
        {
            this.processInstanceId = processInstanceId;
            this.finished = finished;
        }

        public virtual IList<FlowElement> Execute(ICommandContext commandContext)
        {
            Cfg.ProcessEngineConfigurationImpl processEngineConfiguration = commandContext.ProcessEngineConfiguration;
            Interceptor.ICommandExecutor commandExecutor = processEngineConfiguration.CommandExecutor;

            IHistoricActivityInstanceQuery query = processEngineConfiguration.HistoryService.CreateHistoricActivityInstanceQuery()
                .SetProcessInstanceId(processInstanceId);

            if (finished.HasValue)
            {
                if (finished.Value)
                {
                    query.SetFinished();
                }
                else
                {
                    query.SetUnfinished();
                }
            }

            IList<IHistoricActivityInstance> list = query.OrderByHistoricActivityInstanceStartTime().Desc().List();

            string processDefinitionId = list[0].ProcessDefinitionId;
            Process process = ProcessDefinitionUtil.GetProcess(processDefinitionId);
            IList<FlowElement> elements = new List<FlowElement>();
            foreach (var actInst in list)
            {
                elements.Add(process.GetFlowElement(actInst.ActivityId, true));
            }

            return elements.Distinct().ToList();
        }
    }
}