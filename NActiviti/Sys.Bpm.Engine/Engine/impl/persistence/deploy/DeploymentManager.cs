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

namespace org.activiti.engine.impl.persistence.deploy
{
    using org.activiti.engine.@delegate.@event;
    using org.activiti.engine.@delegate.@event.impl;
    using org.activiti.engine.impl.cfg;
    using org.activiti.engine.impl.context;
    using org.activiti.engine.impl.interceptor;
    using org.activiti.engine.impl.persistence.entity;
    using org.activiti.engine.repository;

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

        public virtual void deploy(IDeploymentEntity deployment)
        {
            deploy(deployment, null);
        }

        public virtual void deploy(IDeploymentEntity deployment, IDictionary<string, object> deploymentSettings)
        {
            foreach (IDeployer deployer in deployers)
            {
                deployer.deploy(deployment, deploymentSettings);
            }
        }

        public virtual IProcessDefinition findDeployedProcessDefinitionById(string processDefinitionId)
        {
            if (string.IsNullOrWhiteSpace(processDefinitionId))
            {
                throw new ActivitiIllegalArgumentException("Invalid process definition id : null");
            }

            // first try the cache
            ProcessDefinitionCacheEntry cacheEntry = processDefinitionCache.get(processDefinitionId);
            IProcessDefinition processDefinition = cacheEntry != null ? cacheEntry.ProcessDefinition : null;

            if (processDefinition == null)
            {
                processDefinition = processDefinitionEntityManager.findById<IProcessDefinitionEntity>(new KeyValuePair<string, object>("processDefinitionId", processDefinitionId));
                if (processDefinition == null)
                {
                    throw new ActivitiObjectNotFoundException("no deployed process definition found with id '" + processDefinitionId + "'", typeof(IProcessDefinition));
                }
                processDefinition = resolveProcessDefinition(processDefinition).ProcessDefinition;
            }
            return processDefinition;
        }

        public virtual IProcessDefinition findDeployedLatestProcessDefinitionByKey(string processDefinitionKey)
        {
            IProcessDefinition processDefinition = processDefinitionEntityManager.findLatestProcessDefinitionByKey(processDefinitionKey);

            if (processDefinition == null)
            {
                throw new ActivitiObjectNotFoundException("no processes deployed with key '" + processDefinitionKey + "'", typeof(IProcessDefinition));
            }
            processDefinition = resolveProcessDefinition(processDefinition).ProcessDefinition;
            return processDefinition;
        }

        public virtual IProcessDefinition findDeployedLatestProcessDefinitionByKeyAndTenantId(string processDefinitionKey, string tenantId)
        {
            IProcessDefinition processDefinition = processDefinitionEntityManager.findLatestProcessDefinitionByKeyAndTenantId(processDefinitionKey, tenantId);
            if (processDefinition == null)
            {
                throw new ActivitiObjectNotFoundException("no processes deployed with key '" + processDefinitionKey + "' for tenant identifier '" + tenantId + "'", typeof(IProcessDefinition));
            }
            processDefinition = resolveProcessDefinition(processDefinition).ProcessDefinition;
            return processDefinition;
        }

        public virtual IProcessDefinition findDeployedProcessDefinitionByKeyAndVersionAndTenantId(string processDefinitionKey, int? processDefinitionVersion, string tenantId)
        {
            IProcessDefinition processDefinition = (IProcessDefinitionEntity)processDefinitionEntityManager.findProcessDefinitionByKeyAndVersionAndTenantId(processDefinitionKey, processDefinitionVersion, tenantId);
            if (processDefinition == null)
            {
                throw new ActivitiObjectNotFoundException("no processes deployed with key = '" + processDefinitionKey + "' and version = '" + processDefinitionVersion + "'", typeof(IProcessDefinition));
            }
            processDefinition = resolveProcessDefinition(processDefinition).ProcessDefinition;
            return processDefinition;
        }

        /// <summary>
        /// Resolving the process definition will fetch the BPMN 2.0, parse it and store the <seealso cref="BpmnModel"/> in memory.
        /// </summary>
        public virtual ProcessDefinitionCacheEntry resolveProcessDefinition(IProcessDefinition processDefinition)
        {
            string processDefinitionId = processDefinition.Id;
            string deploymentId = processDefinition.DeploymentId;

            ProcessDefinitionCacheEntry cachedProcessDefinition = processDefinitionCache.get(processDefinitionId);

            if (cachedProcessDefinition == null)
            {
                ICommandContext commandContext = Context.CommandContext;
                IDeploymentEntity deployment = deploymentEntityManager.findById<IDeploymentEntity>(new KeyValuePair<string, object>("id", deploymentId));
                deployment.New = false;
                deploy(deployment, null);
                cachedProcessDefinition = processDefinitionCache.get(processDefinitionId);

                if (cachedProcessDefinition == null)
                {
                    throw new ActivitiException("deployment '" + deploymentId + "' didn't put process definition '" + processDefinitionId + "' in the cache");
                }
            }
            return cachedProcessDefinition;
        }

        public virtual void removeDeployment(string deploymentId, bool cascade)
        {

            IDeploymentEntity deployment = deploymentEntityManager.findById<IDeploymentEntity>(new KeyValuePair<string, object>("id", deploymentId));
            if (deployment == null)
            {
                throw new ActivitiObjectNotFoundException("Could not find a deployment with id '" + deploymentId + "'.", typeof(IDeploymentEntity));
            }

            // Remove any process definition from the cache
            IList<IProcessDefinition> processDefinitions = (new ProcessDefinitionQueryImpl()).deploymentId(deploymentId).list();
            IActivitiEventDispatcher eventDispatcher = Context.ProcessEngineConfiguration.EventDispatcher;

            foreach (IProcessDefinition processDefinition in processDefinitions)
            {

                // Since all process definitions are deleted by a single query, we should dispatch the events in this loop
                if (eventDispatcher.Enabled)
                {
                    eventDispatcher.dispatchEvent(ActivitiEventBuilder.createEntityEvent(ActivitiEventType.ENTITY_DELETED, processDefinition));
                }
            }

            // Delete data
            deploymentEntityManager.deleteDeployment(deploymentId, cascade);

            // Since we use a delete by query, delete-events are not automatically dispatched
            if (eventDispatcher.Enabled)
            {
                eventDispatcher.dispatchEvent(ActivitiEventBuilder.createEntityEvent(ActivitiEventType.ENTITY_DELETED, deployment));
            }

            foreach (IProcessDefinition processDefinition in processDefinitions)
            {
                processDefinitionCache.remove(processDefinition.Id);
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