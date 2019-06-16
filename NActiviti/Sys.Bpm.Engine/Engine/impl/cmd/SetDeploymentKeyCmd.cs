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
    using Sys.Workflow.Engine.Delegate.Events;
    using Sys.Workflow.Engine.Delegate.Events.Impl;
    using Sys.Workflow.Engine.Impl.Interceptor;
    using Sys.Workflow.Engine.Impl.Persistence.Entity;
    using Sys.Workflow.Engine.Repository;
    using System.Collections.Generic;

    /// 
    public class SetDeploymentKeyCmd : ICommand<object>
    {

        protected internal string deploymentId;
        protected internal string key;

        public SetDeploymentKeyCmd(string deploymentId, string key)
        {
            this.deploymentId = deploymentId;
            this.key = key;
        }

        public  virtual object  Execute(ICommandContext commandContext)
        {

            if (deploymentId is null)
            {
                throw new ActivitiIllegalArgumentException("Deployment id is null");
            }

            IDeploymentEntity deployment = commandContext.DeploymentEntityManager.FindById<IDeploymentEntity>(deploymentId);

            if (deployment == null)
            {
                throw new ActivitiObjectNotFoundException("No deployment found for id = '" + deploymentId + "'", typeof(IDeployment));
            }

            // Update category
            deployment.Key = key;

            if (commandContext.ProcessEngineConfiguration.EventDispatcher.Enabled)
            {
                commandContext.ProcessEngineConfiguration.EventDispatcher.DispatchEvent(ActivitiEventBuilder.CreateEntityEvent(ActivitiEventType.ENTITY_UPDATED, deployment));
            }

            return null;
        }

        public virtual string DeploymentId
        {
            get
            {
                return deploymentId;
            }
            set
            {
                this.deploymentId = value;
            }
        }


        public virtual string Key
        {
            get
            {
                return key;
            }
            set
            {
                this.key = value;
            }
        }


    }

}