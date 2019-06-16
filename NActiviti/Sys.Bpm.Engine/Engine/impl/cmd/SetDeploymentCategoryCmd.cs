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
namespace Sys.Workflow.engine.impl.cmd
{
    using Sys.Workflow.engine.@delegate.@event;
    using Sys.Workflow.engine.@delegate.@event.impl;
    using Sys.Workflow.engine.impl.interceptor;
    using Sys.Workflow.engine.impl.persistence.entity;
    using Sys.Workflow.engine.repository;
    using System.Collections.Generic;

    /// 
    public class SetDeploymentCategoryCmd : ICommand<object>
    {

        protected internal string deploymentId;
        protected internal string category;

        public SetDeploymentCategoryCmd(string deploymentId, string category)
        {
            this.deploymentId = deploymentId;
            this.category = category;
        }

        public  virtual object  Execute(ICommandContext commandContext)
        {

            if (ReferenceEquals(deploymentId, null))
            {
                throw new ActivitiIllegalArgumentException("Deployment id is null");
            }

            IDeploymentEntity deployment = commandContext.DeploymentEntityManager.FindById<IDeploymentEntity>(deploymentId);

            if (deployment == null)
            {
                throw new ActivitiObjectNotFoundException("No deployment found for id = '" + deploymentId + "'", typeof(IDeployment));
            }

            // Update category
            deployment.Category = category;

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


        public virtual string Category
        {
            get
            {
                return category;
            }
            set
            {
                this.category = value;
            }
        }


    }

}