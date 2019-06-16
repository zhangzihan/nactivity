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

namespace Sys.Workflow.Engine.Impl.Cmd
{

    using Sys.Workflow.Engine.Impl.Interceptor;
    using Sys.Workflow.Engine.Repository;
    using System.IO;

    /// <summary>
    /// Gives access to a deployed process model, e.g., a BPMN 2.0 XML file, through a stream of bytes.
    /// 
    /// 
    /// </summary>
    [Serializable]
    public class GetDeploymentProcessModelCmd : ICommand<Stream>
    {

        private const long serialVersionUID = 1L;
        protected internal string processDefinitionId;

        public GetDeploymentProcessModelCmd(string processDefinitionId)
        {
            if (string.IsNullOrWhiteSpace(processDefinitionId) || processDefinitionId.Length < 1)
            {
                throw new ActivitiIllegalArgumentException("The process definition id is mandatory, but '" + processDefinitionId + "' has been provided.");
            }
            this.processDefinitionId = processDefinitionId;
        }

        public  virtual Stream Execute(ICommandContext  commandContext)
        {
            IProcessDefinition processDefinition = commandContext.ProcessEngineConfiguration.DeploymentManager.FindDeployedProcessDefinitionById(processDefinitionId);
            string deploymentId = processDefinition.DeploymentId;
            string resourceName = processDefinition.ResourceName;
            Stream processModelStream = new GetDeploymentResourceCmd(deploymentId, resourceName).Execute(commandContext);
            return processModelStream;
        }

    }

}