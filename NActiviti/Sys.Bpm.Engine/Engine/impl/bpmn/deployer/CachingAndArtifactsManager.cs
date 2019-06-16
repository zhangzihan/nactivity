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
namespace Sys.Workflow.Engine.Impl.Bpmn.Deployers
{
    using Newtonsoft.Json.Linq;
    using Sys.Workflow.Bpmn.Models;
    using Sys.Workflow.Engine.Impl.Cfg;
    using Sys.Workflow.Engine.Impl.Contexts;
    using Sys.Workflow.Engine.Impl.Interceptor;
    using Sys.Workflow.Engine.Impl.Persistence.Deploies;
    using Sys.Workflow.Engine.Impl.Persistence.Entity;
    using Sys.Workflow;
    using System;

    /// <summary>
    /// Updates caches and artifacts for a deployment, its process definitions, 
    /// and its process definition infos.
    /// </summary>
    public class CachingAndArtifactsManager
    {

        /// <summary>
        /// Ensures that the process definition is cached in the appropriate places, including the
        /// deployment's collection of deployed artifacts and the deployment manager's cache, as well
        /// as caching any ProcessDefinitionInfos.
        /// </summary>
        public virtual void UpdateCachingAndArtifacts(ParsedDeployment parsedDeployment)
        {
            ICommandContext commandContext = Context.CommandContext;

            ProcessEngineConfigurationImpl processEngineConfiguration = Context.ProcessEngineConfiguration;
            IDeploymentCache<ProcessDefinitionCacheEntry> processDefinitionCache = processEngineConfiguration.DeploymentManager.ProcessDefinitionCache;
            IDeploymentEntity deployment = parsedDeployment.Deployment;

            foreach (IProcessDefinitionEntity processDefinition in parsedDeployment.AllProcessDefinitions)
            {
                BpmnModel bpmnModel = parsedDeployment.GetBpmnModelForProcessDefinition(processDefinition);
                Process process = parsedDeployment.GetProcessModelForProcessDefinition(processDefinition);
                ProcessDefinitionCacheEntry cacheEntry = new ProcessDefinitionCacheEntry(processDefinition, bpmnModel, process);
                processDefinitionCache.Add(processDefinition.Id, cacheEntry);
                AddDefinitionInfoToCache(processDefinition, processEngineConfiguration, commandContext);

                // Add to deployment for further usage
                deployment.AddDeployedArtifact(processDefinition);
            }
        }

        protected internal virtual void AddDefinitionInfoToCache(IProcessDefinitionEntity processDefinition, ProcessEngineConfigurationImpl processEngineConfiguration, ICommandContext commandContext)
        {
            if (!processEngineConfiguration.EnableProcessDefinitionInfoCache)
            {
                return;
            }

            DeploymentManager deploymentManager = processEngineConfiguration.DeploymentManager;
            IProcessDefinitionInfoEntityManager definitionInfoEntityManager = commandContext.ProcessDefinitionInfoEntityManager;
            ObjectMapper objectMapper = commandContext.ProcessEngineConfiguration.ObjectMapper;
            IProcessDefinitionInfoEntity definitionInfoEntity = definitionInfoEntityManager.FindProcessDefinitionInfoByProcessDefinitionId(processDefinition.Id);

            JToken infoNode = null;
            if (definitionInfoEntity != null && !(definitionInfoEntity.InfoJsonId is null))
            {
                byte[] infoBytes = definitionInfoEntityManager.FindInfoJsonById(definitionInfoEntity.InfoJsonId);
                if (infoBytes != null)
                {
                    try
                    {
                        infoNode = objectMapper.ReadTree(infoBytes);
                    }
                    catch (Exception)
                    {
                        throw new ActivitiException("Error deserializing json info for process definition " + processDefinition.Id);
                    }
                }
            }

            ProcessDefinitionInfoCacheObject definitionCacheObject = new ProcessDefinitionInfoCacheObject();
            if (definitionInfoEntity == null)
            {
                definitionCacheObject.Revision = 0;
            }
            else
            {
                definitionCacheObject.Id = definitionInfoEntity.Id;
                definitionCacheObject.Revision = definitionInfoEntity.Revision;
            }

            if (infoNode == null)
            {
                infoNode = objectMapper.CreateObjectNode();
            }
            definitionCacheObject.InfoNode = infoNode;

            deploymentManager.ProcessDefinitionInfoCache.Add(processDefinition.Id, definitionCacheObject);
        }
    }


}