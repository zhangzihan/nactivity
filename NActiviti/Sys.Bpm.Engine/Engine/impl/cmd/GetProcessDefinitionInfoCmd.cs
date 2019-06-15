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
    using Newtonsoft.Json.Linq;
    using org.activiti.engine.impl.interceptor;
    using org.activiti.engine.impl.persistence.deploy;
    using org.activiti.engine.repository;


    /// 
    [Serializable]
    public class GetProcessDefinitionInfoCmd : ICommand<JToken>
    {

        private const long serialVersionUID = 1L;

        protected internal string processDefinitionId;

        public GetProcessDefinitionInfoCmd(string processDefinitionId)
        {
            this.processDefinitionId = processDefinitionId;
        }

        public virtual JToken Execute(ICommandContext commandContext)
        {
            if (string.IsNullOrWhiteSpace(processDefinitionId))
            {
                throw new ActivitiIllegalArgumentException("process definition id is null");
            }

            JToken resultNode = null;
            DeploymentManager deploymentManager = commandContext.ProcessEngineConfiguration.DeploymentManager;
            // make sure the process definition is in the cache
            deploymentManager.FindDeployedProcessDefinitionById(processDefinitionId);

            ProcessDefinitionInfoCacheObject definitionInfoCacheObject = deploymentManager.ProcessDefinitionInfoCache.Get(processDefinitionId);
            if (definitionInfoCacheObject != null)
            {
                resultNode = definitionInfoCacheObject.InfoNode;
            }

            return resultNode;
        }

    }
}