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

namespace Sys.Workflow.engine.impl.persistence.deploy
{
    using Microsoft.Extensions.Logging;
    using Sys.Workflow.engine.@delegate.@event;
    using Sys.Workflow.engine.@delegate.@event.impl;
    using Sys.Workflow.engine.impl.cfg;
    using Sys.Workflow.engine.impl.context;
    using Sys.Workflow.engine.impl.interceptor;
    using Sys.Workflow.engine.impl.persistence.entity;
    using Sys.Workflow.engine.repository;
    using Sys.Workflow;

    /// 
    /// 
    /// 
    public class DeploymentManager
    {

        protected internal IDeploymentCache<ProcessDefinitionCacheEntry> processDefinitionCache;
        protected internal ProcessDefinitionInfoCache processDefinitionInfoCache;
        protected internal IDeploymentCache<object> knowledgeBaseCache; // Needs to be object to avoid an import to Drools in this core class
        protected internal IList<IDeployer> deployers;

        protected internal ProcessEngineConfigurationImpl processEngineConfiguration;
        protected internal IProcessDefinitionEntityManager processDefinitionEntityManager;
        protected internal IDeploymentEntityManager deploymentEntityManager;
        
        public virtual void Deploy(IDeploymentEntity deployment)
        {
            Deploy(deployment, null);
        }

        public virtual void Deploy(IDeploymentEntity deployment, IDictionary<string, object> deploymentSettings)
        {
            foreach (IDeployer deployer in deployers)
            {
                deployer.Deploy(deployment, deploymentSettings);
            }
        }

        public virtual IProcessDefinition FindDeployedProcessDefinitionById(string processDefinitionId)
        {
            if (string.IsNullOrWhiteSpace(processDefinitionId))
            {
                throw new ActivitiIllegalArgumentException("Invalid process definition id : null");
            }

            // first try the cache
            ProcessDefinitionCacheEntry cacheEntry = processDefinitionCache.Get(processDefinitionId);
            IProcessDefinition processDefinition = cacheEntry?.ProcessDefinition;

            if (processDefinition == null)
            {
                processDefinition = processDefinitionEntityManager.FindById<IProcessDefinitionEntity>(new KeyValuePair<string, object>("processDefinitionId", processDefinitionId));
                if (processDefinition == null)
                {
                    throw new ActivitiObjectNotFoundException("no deployed process definition found with id '" + processDefinitionId + "'", typeof(IProcessDefinition));
                }
                processDefinition = ResolveProcessDefinition(processDefinition).ProcessDefinition;
            }
            return processDefinition;
        }

        public virtual IProcessDefinition FindDeployedLatestProcessDefinitionByKey(string processDefinitionKey)
        {
            IProcessDefinition processDefinition = processDefinitionEntityManager.FindLatestProcessDefinitionByKey(processDefinitionKey);

            if (processDefinition == null)
            {
                throw new ActivitiObjectNotFoundException("no processes deployed with key '" + processDefinitionKey + "'", typeof(IProcessDefinition));
            }
            processDefinition = ResolveProcessDefinition(processDefinition).ProcessDefinition;
            return processDefinition;
        }

        public virtual IProcessDefinition FindDeployedLatestProcessDefinitionByKeyAndTenantId(string processDefinitionKey, string tenantId)
        {
            IProcessDefinition processDefinition = processDefinitionEntityManager.FindLatestProcessDefinitionByKeyAndTenantId(processDefinitionKey, tenantId);
            if (processDefinition == null)
            {
                throw new ActivitiObjectNotFoundException("no processes deployed with key '" + processDefinitionKey + "' for tenant identifier '" + tenantId + "'", typeof(IProcessDefinition));
            }
            processDefinition = ResolveProcessDefinition(processDefinition).ProcessDefinition;
            return processDefinition;
        }

        public virtual IProcessDefinition FindDeployedProcessDefinitionByKeyAndVersionAndTenantId(string processDefinitionKey, int? processDefinitionVersion, string tenantId)
        {
            IProcessDefinition processDefinition = (IProcessDefinitionEntity)processDefinitionEntityManager.FindProcessDefinitionByKeyAndVersionAndTenantId(processDefinitionKey, processDefinitionVersion, tenantId);
            if (processDefinition == null)
            {
                throw new ActivitiObjectNotFoundException("no processes deployed with key = '" + processDefinitionKey + "' and version = '" + processDefinitionVersion + "'", typeof(IProcessDefinition));
            }
            processDefinition = ResolveProcessDefinition(processDefinition).ProcessDefinition;
            return processDefinition;
        }

        /// <summary>
        /// Resolving the process definition will fetch the BPMN 2.0, parse it and store the <seealso cref="BpmnModel"/> in memory.
        /// </summary>
        public virtual ProcessDefinitionCacheEntry ResolveProcessDefinition(IProcessDefinition processDefinition)
        {
            string processDefinitionId = processDefinition.Id;
            string deploymentId = processDefinition.DeploymentId;

            ProcessDefinitionCacheEntry cachedProcessDefinition = processDefinitionCache.Get(processDefinitionId);

            if (cachedProcessDefinition == null)
            {
                IDeploymentEntity deployment = deploymentEntityManager.FindById<IDeploymentEntity>(new KeyValuePair<string, object>("id", deploymentId));
                deployment.New = false;
                Deploy(deployment, null);
                cachedProcessDefinition = processDefinitionCache.Get(processDefinitionId);

                if (cachedProcessDefinition == null)
                {
                    throw new ActivitiException("deployment '" + deploymentId + "' didn't put process definition '" + processDefinitionId + "' in the cache");
                }
            }
            return cachedProcessDefinition;
        }

        public virtual void RemoveDeployment(string deploymentId, bool cascade)
        {

            IDeploymentEntity deployment = deploymentEntityManager.FindById<IDeploymentEntity>(new KeyValuePair<string, object>("id", deploymentId));
            if (deployment == null)
            {
                throw new ActivitiObjectNotFoundException("Could not find a deployment with id '" + deploymentId + "'.", typeof(IDeploymentEntity));
            }

            // Remove any process definition from the cache
            IList<IProcessDefinition> processDefinitions = (new ProcessDefinitionQueryImpl()).SetDeploymentId(deploymentId).List();
            IActivitiEventDispatcher eventDispatcher = Context.ProcessEngineConfiguration.EventDispatcher;

            foreach (IProcessDefinition processDefinition in processDefinitions)
            {
                // Since all process definitions are deleted by a single query, we should dispatch the events in this loop
                if (eventDispatcher.Enabled)
                {
                    eventDispatcher.DispatchEvent(ActivitiEventBuilder.CreateEntityEvent(ActivitiEventType.ENTITY_DELETED, processDefinition));
                }
            }

            // Delete data
            deploymentEntityManager.DeleteDeployment(deploymentId, cascade);

            // Since we use a delete by query, delete-events are not automatically dispatched
            if (eventDispatcher.Enabled)
            {
                eventDispatcher.DispatchEvent(ActivitiEventBuilder.CreateEntityEvent(ActivitiEventType.ENTITY_DELETED, deployment));
            }

            foreach (IProcessDefinition processDefinition in processDefinitions)
            {
                processDefinitionCache.Remove(processDefinition.Id);
            }
        }

        // getters and setters
        // //////////////////////////////////////////////////////

        public virtual IList<IDeployer> Deployers
        {
            get
            {
                return deployers;
            }
            set
            {
                this.deployers = value;
            }
        }


        public virtual IDeploymentCache<ProcessDefinitionCacheEntry> ProcessDefinitionCache
        {
            get
            {
                return processDefinitionCache;
            }
            set
            {
                this.processDefinitionCache = value;
            }
        }


        public virtual ProcessDefinitionInfoCache ProcessDefinitionInfoCache
        {
            get
            {
                return processDefinitionInfoCache;
            }
            set
            {
                this.processDefinitionInfoCache = value;
            }
        }


        public virtual IDeploymentCache<object> KnowledgeBaseCache
        {
            get
            {
                return knowledgeBaseCache;
            }
            set
            {
                this.knowledgeBaseCache = value;
            }
        }


        public virtual ProcessEngineConfigurationImpl ProcessEngineConfiguration
        {
            get
            {
                return processEngineConfiguration;
            }
            set
            {
                this.processEngineConfiguration = value;
            }
        }


        public virtual IProcessDefinitionEntityManager ProcessDefinitionEntityManager
        {
            get
            {
                return processDefinitionEntityManager;
            }
            set
            {
                this.processDefinitionEntityManager = value;
            }
        }


        public virtual IDeploymentEntityManager DeploymentEntityManager
        {
            get
            {
                return deploymentEntityManager;
            }
            set
            {
                this.deploymentEntityManager = value;
            }
        }
    }
}