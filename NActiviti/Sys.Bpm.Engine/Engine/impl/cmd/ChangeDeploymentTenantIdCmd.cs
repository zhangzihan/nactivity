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
namespace org.activiti.engine.impl.cmd
{
    using org.activiti.engine.impl.interceptor;
    using org.activiti.engine.impl.persistence.entity;
    using org.activiti.engine.repository;

    /// 
    [Serializable]
    public class ChangeDeploymentTenantIdCmd : ICommand<object>
    {

        private const long serialVersionUID = 1L;

        protected internal string deploymentId;
        protected internal string newTenantId;

        public ChangeDeploymentTenantIdCmd(string deploymentId, string newTenantId)
        {
            this.deploymentId = deploymentId;
            this.newTenantId = newTenantId;
        }

        public virtual object execute(ICommandContext commandContext)
        {
            if (string.ReferenceEquals(deploymentId, null))
            {
                throw new ActivitiIllegalArgumentException("deploymentId is null");
            }

            // Update all entities

            IDeploymentEntity deployment = commandContext.DeploymentEntityManager.findById<IDeploymentEntity>(new KeyValuePair<string, object>("id", deploymentId));
            if (deployment == null)
            {
                throw new ActivitiObjectNotFoundException("Could not find deployment with id " + deploymentId, typeof(IDeployment));
            }

            string oldTenantId = deployment.TenantId;
            deployment.TenantId = newTenantId;

            // Doing process instances, executions and tasks with direct SQL updates
            // (otherwise would not be performant)
            commandContext.ProcessDefinitionEntityManager.updateProcessDefinitionTenantIdForDeployment(deploymentId, newTenantId);
            commandContext.ExecutionEntityManager.updateExecutionTenantIdForDeployment(deploymentId, newTenantId);
            commandContext.TaskEntityManager.updateTaskTenantIdForDeployment(deploymentId, newTenantId);
            commandContext.JobEntityManager.updateJobTenantIdForDeployment(deploymentId, newTenantId);
            commandContext.TimerJobEntityManager.updateJobTenantIdForDeployment(deploymentId, newTenantId);
            commandContext.SuspendedJobEntityManager.updateJobTenantIdForDeployment(deploymentId, newTenantId);
            commandContext.DeadLetterJobEntityManager.updateJobTenantIdForDeployment(deploymentId, newTenantId);
            commandContext.EventSubscriptionEntityManager.updateEventSubscriptionTenantId(oldTenantId, newTenantId);

            // Doing process definitions in memory, cause we need to clear the process definition cache
            IList<IProcessDefinition> processDefinitions = (new ProcessDefinitionQueryImpl()).deploymentId(deploymentId).list();
            foreach (IProcessDefinition processDefinition in processDefinitions)
            {
                commandContext.ProcessEngineConfiguration.ProcessDefinitionCache.remove(processDefinition.Id);
            }

            // Clear process definition cache
            commandContext.ProcessEngineConfiguration.ProcessDefinitionCache.clear();

            return null;

        }

    }

}