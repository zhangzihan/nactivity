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

    using org.activiti.engine.impl.interceptor;

    /// 
    [Serializable]
    public class DeleteDeploymentCmd : ICommand<object>
    {

        private const long serialVersionUID = 1L;
        protected internal string deploymentId;
        protected internal bool cascade;

        public DeleteDeploymentCmd(string deploymentId, bool cascade)
        {
            this.deploymentId = deploymentId;
            this.cascade = cascade;
        }

        public virtual object execute(ICommandContext commandContext)
        {
            if (ReferenceEquals(deploymentId, null))
            {
                throw new ActivitiIllegalArgumentException("deploymentId is null");
            }

            // Remove process definitions from cache:
            commandContext.ProcessEngineConfiguration.DeploymentManager.removeDeployment(deploymentId, cascade);

            return null;
        }
    }

}