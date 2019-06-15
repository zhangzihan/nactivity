using System;

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

    using org.activiti.bpmn.model;
    using org.activiti.engine.impl.interceptor;
    using org.activiti.engine.impl.util;

    /// 
    [Serializable]
    public class GetBpmnModelCmd : ICommand<BpmnModel>
    {

        private const long serialVersionUID = 8167762371289445046L;

        protected internal string processDefinitionId;

        public GetBpmnModelCmd(string processDefinitionId)
        {
            this.processDefinitionId = processDefinitionId;
        }

        public  virtual BpmnModel  Execute(ICommandContext  commandContext)
        {
            if (string.IsNullOrWhiteSpace(processDefinitionId))
            {
                throw new ActivitiIllegalArgumentException("processDefinitionId is null");
            }

            return ProcessDefinitionUtil.GetBpmnModel(processDefinitionId);
        }
    }
}