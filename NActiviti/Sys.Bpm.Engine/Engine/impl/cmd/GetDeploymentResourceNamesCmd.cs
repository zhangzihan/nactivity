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
    using System.Collections.Generic;

    /// 
    [Serializable]
    public class GetDeploymentResourceNamesCmd : ICommand<IList<string>>
    {

        private const long serialVersionUID = 1L;
        protected internal string deploymentId;

        public GetDeploymentResourceNamesCmd(string deploymentId)
        {
            this.deploymentId = deploymentId;
        }

        public virtual IList<string> Execute(ICommandContext commandContext)
        {
            if (ReferenceEquals(deploymentId, null))
            {
                throw new ActivitiIllegalArgumentException("deploymentId is null");
            }

            return commandContext.DeploymentEntityManager.GetDeploymentResourceNames(deploymentId);
        }

    }

}