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
namespace Sys.Workflow.Engine.Impl.Scripting
{

    using Sys.Workflow.Engine.Delegate;
    using Sys.Workflow.Engine.Impl.Cfg;
    using Sys.Workflow.Engine.Impl.Persistence.Entity;

    /// <summary>
    /// 
    /// </summary>
    public class VariableScopeResolver : IResolver
    {
        /// <summary>
        /// 
        /// </summary>
        protected internal ProcessEngineConfigurationImpl processEngineConfiguration;
        /// <summary>
        /// 
        /// </summary>
        protected internal IVariableScope variableScope;
        /// <summary>
        /// 
        /// </summary>
        protected internal string variableScopeKey = "execution";
        /// <summary>
        /// 
        /// </summary>
        protected internal const string processEngineConfigurationKey = "processEngineConfiguration";
        /// <summary>
        /// 
        /// </summary>
        protected internal const string runtimeServiceKey = "runtimeService";
        /// <summary>
        /// 
        /// </summary>
        protected internal const string taskServiceKey = "taskService";
        /// <summary>
        /// 
        /// </summary>
        protected internal const string repositoryServiceKey = "repositoryService";
        /// <summary>
        /// 
        /// </summary>
        protected internal const string managementServiceKey = "managementService";
        /// <summary>
        /// 
        /// </summary>
        protected internal const string historyServiceKey = "historyService";
        /// <summary>
        /// 
        /// </summary>
        protected internal const string formServiceKey = "formService";
        /// <summary>
        /// 
        /// </summary>
        protected internal static readonly IList<string> KEYS = new List<string>(new string[]{
        processEngineConfigurationKey, runtimeServiceKey, taskServiceKey, repositoryServiceKey, managementServiceKey, historyServiceKey, formServiceKey });

        /// <summary>
        /// 
        /// </summary>
        /// <param name="processEngineConfiguration"></param>
        /// <param name="variableScope"></param>
        public VariableScopeResolver(ProcessEngineConfigurationImpl processEngineConfiguration, IVariableScope variableScope)
        {

            this.processEngineConfiguration = processEngineConfiguration;

            if (variableScope is null)
            {
                throw new ActivitiIllegalArgumentException("variableScope cannot be null");
            }
            if (variableScope is IExecutionEntity)
            {
                variableScopeKey = "execution";
            }
            else if (variableScope is ITaskEntity)
            {
                variableScopeKey = "task";
            }
            else
            {
                throw new ActivitiException("unsupported variable scope type: " + variableScope.GetType().FullName);
            }
            this.variableScope = variableScope;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public virtual bool ContainsKey(object key)
        {
            return variableScopeKey.Equals(key?.ToString()) || KEYS.Contains(key?.ToString()) || variableScope.HasVariable(key?.ToString());
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public virtual object Get(object key)
        {
            if (variableScopeKey.Equals(key))
            {
                return variableScope;
            }
            else if (processEngineConfigurationKey.Equals(key))
            {
                return processEngineConfiguration;
            }
            else if (runtimeServiceKey.Equals(key))
            {
                return processEngineConfiguration.RuntimeService;
            }
            else if (taskServiceKey.Equals(key))
            {
                return processEngineConfiguration.TaskService;
            }
            else if (repositoryServiceKey.Equals(key))
            {
                return processEngineConfiguration.RepositoryService;
            }
            else if (managementServiceKey.Equals(key))
            {
                return processEngineConfiguration.ManagementService;
            }

            return variableScope.GetVariable((string)key);
        }
    }

}