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
namespace Sys.Workflow.Engine.Impl.Cmd
{
	using Sys.Workflow.Bpmn.Constants;
	using Sys.Workflow.Bpmn.Models;
	using Sys.Workflow.Engine.Delegate.Events;
	using Sys.Workflow.Engine.Delegate.Events.Impl;
	using Sys.Workflow.Engine.Impl.Bpmn.Behavior;
	using Sys.Workflow.Engine.Impl.Bpmn.Listeners;
	using Sys.Workflow.Engine.Impl.Interceptor;
	using Sys.Workflow.Engine.Impl.Persistence.Entity;
	using Sys.Workflow.Engine.Impl.Repositories;
	using Sys.Workflow.Engine.Repository;
	using System.Collections;
	using System.IO;
	using System.Linq;
	using System.Text.RegularExpressions;
	using System.Xml;
	using System.Xml.Linq;
	using System.Xml.XPath;

	/// 
	/// 
	[Serializable]
	public class DeployCmd : ICommand<IDeployment>
	{
		private const long serialVersionUID = 1L;

		protected internal IDeploymentBuilder deploymentBuilder;

		private readonly object syncRoot = new object();

		public DeployCmd(IDeploymentBuilder deploymentBuilder)
		{
			this.deploymentBuilder = deploymentBuilder;
		}

		public virtual IDeployment Execute(ICommandContext commandContext)
		{
			return ExecuteDeploy(commandContext);
		}

		protected internal virtual IDeployment ExecuteDeploy(ICommandContext commandContext)
		{
			IDeploymentEntity deployment = deploymentBuilder.Deployment;

			deployment.Runable();

			deployment.DeploymentTime = commandContext.ProcessEngineConfiguration.Clock.CurrentTime;

			_ = this.deploymentBuilder.Deployment.GetResources();

			deployment.DeployExecutionBehavior();

			if (deploymentBuilder.DuplicateFilterEnabled)
			{
				List<IDeployment> existingDeployments = new();
				if (string.IsNullOrWhiteSpace(deployment.TenantId))
				{
					IDeploymentEntity existingDeployment = commandContext.DeploymentEntityManager.FindLatestDeploymentByName(deployment.Name);
					if (existingDeployment is object)
					{
						existingDeployments.Add(existingDeployment);
					}
				}
				else
				{
					var repoService = commandContext.ProcessEngineConfiguration.RepositoryService;

					//按部署时间倒序排序,始终与最后一次部署做比较
					IList<IDeployment> deploymentList = repoService.CreateDeploymentQuery()
						.SetDeploymentName(deployment.Name)
						.SetDeploymentTenantId(deployment.TenantId)
						.SetOrderByDeploymenTime()
						.Desc()
						.ListPage(0, 1);

					if (deploymentList.Count > 0)
					{
						existingDeployments.AddRange(deploymentList);
					}
				}

				{
					IDeploymentEntity existingDeployment = null;
					if (existingDeployments.Count > 0)
					{
						existingDeployment = (IDeploymentEntity)existingDeployments[0];
					}

					if ((existingDeployment is not null) && !DeploymentsDiffer(deployment, existingDeployment))
					{
						commandContext.ProcessEngineConfiguration.DeploymentEntityManager.RemoveDrafts(deployment.TenantId, deployment.Name);

						return existingDeployment;
					}
				}
			}

			deployment.New = true;

			// Save the data
			commandContext.DeploymentEntityManager.Insert(deployment);

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

			if (deploymentBuilder.ProcessDefinitionsActivationDate is not null)
			{
				ScheduleProcessDefinitionActivation(commandContext, deployment);
			}

			if (commandContext.ProcessEngineConfiguration.EventDispatcher.Enabled)
			{
				commandContext.ProcessEngineConfiguration.EventDispatcher.DispatchEvent(ActivitiEventBuilder.CreateEntityEvent(ActivitiEventType.ENTITY_INITIALIZED, deployment));
			}

			commandContext.ProcessEngineConfiguration.DeploymentEntityManager.RemoveDrafts(deployment.TenantId, deployment.Name);

			return deployment;
		}

		protected internal virtual bool DeploymentsDiffer(IDeploymentEntity deployment, IDeploymentEntity saved)
		{
			IDictionary<string, IResourceEntity> resources = deployment.GetResources();
			IDictionary<string, IResourceEntity> savedResources = saved.GetResources();

			if (resources is null || savedResources is null)
			{
				return true;
			}

			foreach (string resourceName in resources.Keys)
			{
				if (!savedResources.TryGetValue(resourceName, out var savedResource))
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