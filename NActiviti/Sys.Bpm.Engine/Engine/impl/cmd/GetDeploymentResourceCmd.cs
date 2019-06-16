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
    using Sys.Workflow.Engine.Impl.Persistence.Entity;
    using Sys.Workflow.Engine.Repository;
    using System.Collections.Generic;
    using System.IO;

    /// 
    [Serializable]
    public class GetDeploymentResourceCmd : ICommand<Stream>
    {

        private const long serialVersionUID = 1L;
        protected internal string deploymentId;
        protected internal string resourceName;

        public GetDeploymentResourceCmd(string deploymentId, string resourceName)
        {
            this.deploymentId = deploymentId;
            this.resourceName = resourceName;
        }

        public virtual Stream Execute(ICommandContext commandContext)
        {
            if (ReferenceEquals(deploymentId, null))
            {
                throw new ActivitiIllegalArgumentException("deploymentId is null");
            }
            if (ReferenceEquals(resourceName, null))
            {
                throw new ActivitiIllegalArgumentException("resourceName is null");
            }

            IResourceEntity resource = commandContext.ResourceEntityManager.FindResourceByDeploymentIdAndResourceName(deploymentId, resourceName);
            if (resource == null)
            {
                if (commandContext.DeploymentEntityManager.FindById<IDeployment>(new KeyValuePair<string, object>("id", deploymentId)) == null)
                {
                    throw new ActivitiObjectNotFoundException("deployment does not exist: " + deploymentId, typeof(IDeployment));
                }
                else
                {
                    throw new ActivitiObjectNotFoundException("no resource found with name '" + resourceName + "' in deployment '" + deploymentId + "'", typeof(System.IO.Stream));
                }
            }
            return new MemoryStream(resource.Bytes);
        }

    }

}