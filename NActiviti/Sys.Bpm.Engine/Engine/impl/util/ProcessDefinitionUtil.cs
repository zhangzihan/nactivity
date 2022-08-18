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
namespace Sys.Workflow.Engine.Impl.Util
{
    using Sys.Workflow.Bpmn.Models;
    using Sys.Workflow.Engine.Impl.Cfg;
    using Sys.Workflow.Engine.Impl.Contexts;
    using Sys.Workflow.Engine.Impl.Persistence.Deploies;
    using Sys.Workflow.Engine.Impl.Persistence.Entity;
    using Sys.Workflow.Engine.Repository;
    using System.Collections.Generic;
    using Sys.Workflow.Engine;

    /// <summary>
    /// A utility class that hides the complexity of <seealso cref="IProcessDefinitionEntity"/> and <seealso cref="Process"/> lookup. Use this class rather than accessing the process definition cache or
    /// <seealso cref="DeploymentManager"/> directly.
    /// 
    /// 
    /// </summary>
    public class ProcessDefinitionUtil
    {
        public static IProcessDefinition GetProcessDefinition(string processDefinitionId)
        {
            return GetProcessDefinition(processDefinitionId, false);
        }

        public static IProcessDefinition GetProcessDefinition(string processDefinitionId, bool checkCacheOnly)
        {
            ProcessEngineConfigurationImpl processEngineConfiguration = Context.ProcessEngineConfiguration;
            if (checkCacheOnly)
            {
                ProcessDefinitionCacheEntry cacheEntry = processEngineConfiguration.ProcessDefinitionCache.Get(processDefinitionId);
                if (cacheEntry is not null)
                {
                    return cacheEntry.ProcessDefinition;
                }
                return null;

            }
            else
            {
                // This will check the cache in the findDeployedProcessDefinitionById method
                return processEngineConfiguration.DeploymentManager.FindDeployedProcessDefinitionById(processDefinitionId);
            }
        }

        public static Process GetProcess(string processDefinitionId)
        {
            if (Context.ProcessEngineConfiguration is not null)
            {
                DeploymentManager deploymentManager = Context.ProcessEngineConfiguration.DeploymentManager;

                // This will check the cache in the findDeployedProcessDefinitionById and resolveProcessDefinition method
                IProcessDefinition processDefinitionEntity = deploymentManager.FindDeployedProcessDefinitionById(processDefinitionId);
                return deploymentManager.ResolveProcessDefinition(processDefinitionEntity).Process;
            }
            else
            {
                return GetBpmnModel(processDefinitionId)?.MainProcess;
            }
        }

        public static BpmnModel GetBpmnModel(string processDefinitionId)
        {
            if (Context.ProcessEngineConfiguration is not null)
            {
                DeploymentManager deploymentManager = Context.ProcessEngineConfiguration.DeploymentManager;

                // This will check the cache in the findDeployedProcessDefinitionById and resolveProcessDefinition method
                IProcessDefinition processDefinitionEntity = deploymentManager.FindDeployedProcessDefinitionById(processDefinitionId);
                return deploymentManager.ResolveProcessDefinition(processDefinitionEntity).BpmnModel;
            }
            else
            {
                var processEngineConfig = Context.ProcessEngineConfiguration ?? ProcessEngineServiceProvider.Resolve<ProcessEngineConfiguration>() as ProcessEngineConfigurationImpl;
                return processEngineConfig.CommandExecutor.Execute(new Cmd.GetBpmnModelCmd(processDefinitionId));
            }
        }

        public static BpmnModel GetBpmnModelFromCache(string processDefinitionId)
        {
            ProcessDefinitionCacheEntry cacheEntry = Context.ProcessEngineConfiguration.ProcessDefinitionCache.Get(processDefinitionId);
            if (cacheEntry is not null)
            {
                return cacheEntry.BpmnModel;
            }
            return null;
        }

        public static bool IsProcessDefinitionSuspended(string processDefinitionId)
        {
            IProcessDefinitionEntity processDefinition = GetProcessDefinitionFromDatabase(processDefinitionId);
            return processDefinition.Suspended;
        }

        public static IProcessDefinitionEntity GetProcessDefinitionFromDatabase(string processDefinitionId)
        {
            IProcessDefinitionEntityManager processDefinitionEntityManager = Context.ProcessEngineConfiguration.ProcessDefinitionEntityManager;
            IProcessDefinitionEntity processDefinition = processDefinitionEntityManager.FindById<IProcessDefinitionEntity>(new KeyValuePair<string, object>("processDefinitionId", processDefinitionId));
            if (processDefinition is null)
            {
                throw new ActivitiException("No process definition found with id " + processDefinitionId);
            }

            return processDefinition;
        }

        public static FlowElement GetFlowElement(string processDefinitionId, string activityId, bool searchRecurive = true)
        {
            Process process = GetProcess(processDefinitionId);
            return process.GetFlowElement(activityId, searchRecurive);
        }
    }
}