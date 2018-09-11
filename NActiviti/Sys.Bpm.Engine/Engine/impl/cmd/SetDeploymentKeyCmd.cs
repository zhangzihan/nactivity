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
    using org.activiti.engine.@delegate.@event;
    using org.activiti.engine.@delegate.@event.impl;
    using org.activiti.engine.impl.interceptor;
    using org.activiti.engine.impl.persistence.entity;
    using org.activiti.engine.repository;
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

        public  virtual object  execute(ICommandContext commandContext)
        {

            if (string.ReferenceEquals(deploymentId, null))
            {
                throw new ActivitiIllegalArgumentException("Deployment id is null");
            }

            IDeploymentEntity deployment = commandContext.DeploymentEntityManager.findById<IDeploymentEntity>(new KeyValuePair<string, object>("id", deploymentId));

            if (deployment == null)
            {
                throw new ActivitiObjectNotFoundException("No deployment found for id = '" + deploymentId + "'", typeof(IDeployment));
            }

            // Update category
            deployment.Key = key;

            if (commandContext.ProcessEngineConfiguration.EventDispatcher.Enabled)
            {
                commandContext.ProcessEngineConfiguration.EventDispatcher.dispatchEvent(ActivitiEventBuilder.createEntityEvent(ActivitiEventType.ENTITY_UPDATED, deployment));
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