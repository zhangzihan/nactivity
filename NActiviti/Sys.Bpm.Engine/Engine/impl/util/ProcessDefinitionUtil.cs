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
namespace org.activiti.engine.impl.util
{
    using org.activiti.bpmn.model;
    using org.activiti.engine.impl.cfg;
    using org.activiti.engine.impl.context;
    using org.activiti.engine.impl.persistence.deploy;
    using org.activiti.engine.impl.persistence.entity;
    using org.activiti.engine.repository;
    using System.Collections.Generic;
    using org.activiti.engine;

    /// <summary>
    /// A utility class that hides the complexity of <seealso cref="IProcessDefinitionEntity"/> and <seealso cref="Process"/> lookup. Use this class rather than accessing the process definition cache or
    /// <seealso cref="DeploymentManager"/> directly.
    /// 
    /// 
    /// </summary>
    public class ProcessDefinitionUtil
    {

        public static IProcessDefinition getProcessDefinition(string processDefinitionId)
        {
            return getProcessDefinition(processDefinitionId, false);
        }

        public static IProcessDefinition getProcessDefinition(string processDefinitionId, bool checkCacheOnly)
        {
            ProcessEngineConfigurationImpl processEngineConfiguration = Context.ProcessEngineConfiguration;
            if (checkCacheOnly)
            {
                ProcessDefinitionCacheEntry cacheEntry = processEngineConfiguration.ProcessDefinitionCache.get(processDefinitionId);
                if (cacheEntry != null)
                {
                    return cacheEntry.ProcessDefinition;
                }
                return null;

            }
            else
            {
                // This will check the cache in the findDeployedProcessDefinitionById method
                return processEngineConfiguration.DeploymentManager.findDeployedProcessDefinitionById(processDefinitionId);
            }
        }

        public static Process getProcess(string processDefinitionId)
        {
            DeploymentManager deploymentManager = Context.ProcessEngineConfiguration.DeploymentManager;

            // This will check the cache in the findDeployedProcessDefinitionById and resolveProcessDefinition method
            IProcessDefinition processDefinitionEntity = deploymentManager.findDeployedProcessDefinitionById(processDefinitionId);
            return deploymentManager.resolveProcessDefinition(processDefinitionEntity).Process;
        }

        public static BpmnModel getBpmnModel(string processDefinitionId)
        {
            DeploymentManager deploymentManager = Context.ProcessEngineConfiguration.DeploymentManager;

            // This will check the cache in the findDeployedProcessDefinitionById and resolveProcessDefinition method
            IProcessDefinition processDefinitionEntity = deploymentManager.findDeployedProcessDefinitionById(processDefinitionId);
            return deploymentManager.resolveProcessDefinition(processDefinitionEntity).BpmnModel;
        }

        public static BpmnModel getBpmnModelFromCache(string processDefinitionId)
        {
            ProcessDefinitionCacheEntry cacheEntry = Context.ProcessEngineConfiguration.ProcessDefinitionCache.get(processDefinitionId);
            if (cacheEntry != null)
            {
                return cacheEntry.BpmnModel;
            }
            return null;
        }

        public static bool isProcessDefinitionSuspended(string processDefinitionId)
        {
            IProcessDefinitionEntity processDefinition = getProcessDefinitionFromDatabase(processDefinitionId);
            return processDefinition.Suspended;
        }

        public static IProcessDefinitionEntity getProcessDefinitionFromDatabase(string processDefinitionId)
        {
            IProcessDefinitionEntityManager processDefinitionEntityManager = Context.ProcessEngineConfiguration.ProcessDefinitionEntityManager;
            IProcessDefinitionEntity processDefinition = processDefinitionEntityManager.findById<IProcessDefinitionEntity>(new KeyValuePair<string, object>("id", processDefinitionId));
            if (processDefinition == null)
            {
                throw new ActivitiException("No process definition found with id " + processDefinitionId);
            }

            return processDefinition;
        }
    }

}