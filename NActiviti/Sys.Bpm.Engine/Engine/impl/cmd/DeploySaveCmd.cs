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
namespace Sys.Workflow.engine.impl.cmd
{


    using Sys.Workflow.engine.@delegate.@event;
    using Sys.Workflow.engine.@delegate.@event.impl;
    using Sys.Workflow.engine.impl.interceptor;
    using Sys.Workflow.engine.impl.persistence.entity;
    using Sys.Workflow.engine.impl.repository;
    using Sys.Workflow.engine.repository;
    using System.Collections;

    /// 
    /// 
    [Serializable]
    public class DeploySaveCmd : ICommand<IDeployment>
    {

        private const long serialVersionUID = 1L;
        protected internal DeploymentBuilderImpl deploymentBuilder;

        public DeploySaveCmd(DeploymentBuilderImpl deploymentBuilder)
        {
            this.deploymentBuilder = deploymentBuilder;
            this.deploymentBuilder.DisableDuplicateStartForm();
        }

        public virtual IDeployment Execute(ICommandContext commandContext)
        {
            return ExecuteSave(commandContext);
        }

        protected internal virtual IDeployment ExecuteSave(ICommandContext commandContext)
        {
            IDeploymentEntity deployment = deploymentBuilder.Deployment;

            deployment.Unrunable();

            deployment.DeploymentTime = commandContext.ProcessEngineConfiguration.Clock.CurrentTime;

            commandContext.DeploymentEntityManager.SaveDraft(deployment);

            //if (deploymentBuilder.DuplicateFilterEnabled)
            //{
            //    IList<IDeployment> existingDeployments = new List<IDeployment>();
            //    if (ReferenceEquals(deployment.TenantId, null) || ProcessEngineConfiguration.NO_TENANT_ID.Equals(deployment.TenantId))
            //    {
            //        IDeploymentEntity existingDeployment = commandContext.DeploymentEntityManager.findLatestDeploymentByName(deployment.Name);
            //        if (existingDeployment != null)
            //        {
            //            existingDeployments.Add(existingDeployment);
            //        }
            //    }
            //    else
            //    {
            //        IList<IDeployment> deploymentList = commandContext.ProcessEngineConfiguration.RepositoryService.createDeploymentQuery().deploymentName(deployment.Name).deploymentTenantId(deployment.TenantId).orderByDeploymentId().desc().list();

            //        if (deploymentList.Count > 0)
            //        {
            //            ((List<IDeployment>)existingDeployments).AddRange(deploymentList);
            //        }
            //    }

            //    {
            //        IDeploymentEntity existingDeployment = null;
            //        if (existingDeployments.Count > 0)
            //        {
            //            existingDeployment = (IDeploymentEntity)existingDeployments[0];
            //        }

            //        if ((existingDeployment != null) && !deploymentsDiffer(deployment, existingDeployment))
            //        {
            //            return existingDeployment;
            //        }
            //    }
            //}

            //deployment.New = true;

            //// Save the data
            //commandContext.DeploymentEntityManager.insert(deployment);

            if (commandContext.ProcessEngineConfiguration.EventDispatcher.Enabled)
            {
                commandContext.ProcessEngineConfiguration.EventDispatcher.DispatchEvent(ActivitiEventBuilder.CreateEntityEvent(ActivitiEventType.ENTITY_CREATED, deployment));
            }

            // Deployment settings
            IDictionary<string, object> deploymentSettings = new Dictionary<string, object>
            {
                [DeploymentSettingsFields.IS_BPMN20_XSD_VALIDATION_ENABLED] = deploymentBuilder.Bpmn20XsdValidationEnabled,
                [DeploymentSettingsFields.IS_PROCESS_VALIDATION_ENABLED] = deploymentBuilder.ProcessValidationEnabled
            };

            // Actually deploy
            commandContext.ProcessEngineConfiguration.DeploymentManager.Deploy(deployment, deploymentSettings);

            if (deploymentBuilder.ProcessDefinitionsActivationDate != null)
            {
                ScheduleProcessDefinitionActivation(commandContext, deployment);
            }

            if (commandContext.ProcessEngineConfiguration.EventDispatcher.Enabled)
            {
                commandContext.ProcessEngineConfiguration.EventDispatcher.DispatchEvent(ActivitiEventBuilder.CreateEntityEvent(ActivitiEventType.ENTITY_INITIALIZED, deployment));
            }

            return deployment;
        }

        protected internal virtual bool DeploymentsDiffer(IDeploymentEntity deployment, IDeploymentEntity saved)
        {

            if (deployment.GetResources() == null || saved.GetResources() == null)
            {
                return true;
            }

            IDictionary<string, IResourceEntity> resources = deployment.GetResources();
            IDictionary<string, IResourceEntity> savedResources = saved.GetResources();

            foreach (string resourceName in resources.Keys)
            {
                IResourceEntity savedResource = savedResources[resourceName];

                if (savedResource == null)
                {
                    return true;
                }

                if (!savedResource.Generated)
                {
                    IResourceEntity resource = resources[resourceName];

                    byte[] bytes = resource.Bytes;
                    byte[] savedBytes = savedResource.Bytes;
                    if (!StructuralComparisons.StructuralEqualityComparer.Equals(bytes, savedBytes))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        protected internal virtual void ScheduleProcessDefinitionActivation(ICommandContext commandContext, IDeploymentEntity deployment)
        {
            foreach (IProcessDefinitionEntity processDefinitionEntity in deployment.GetDeployedArtifacts<IProcessDefinitionEntity>())
            {

                // If activation date is set, we first suspend all the process
                // definition
                SuspendProcessDefinitionCmd suspendProcessDefinitionCmd = new SuspendProcessDefinitionCmd(processDefinitionEntity, false, null, deployment.TenantId);
                suspendProcessDefinitionCmd.Execute(commandContext);

                // And we schedule an activation at the provided date
                ActivateProcessDefinitionCmd activateProcessDefinitionCmd = new ActivateProcessDefinitionCmd(processDefinitionEntity, false, deploymentBuilder.ProcessDefinitionsActivationDate, deployment.TenantId);
                activateProcessDefinitionCmd.Execute(commandContext);
            }
        }

    }

}