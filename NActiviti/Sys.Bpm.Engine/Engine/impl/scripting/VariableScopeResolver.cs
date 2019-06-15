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
namespace org.activiti.engine.impl.scripting
{

    using org.activiti.engine.@delegate;
    using org.activiti.engine.impl.cfg;
    using org.activiti.engine.impl.persistence.entity;


    public class VariableScopeResolver : IResolver
    {

        protected internal ProcessEngineConfigurationImpl processEngineConfiguration;
        protected internal IVariableScope variableScope;

        protected internal string variableScopeKey = "execution";

        protected internal const string processEngineConfigurationKey = "processEngineConfiguration";
        protected internal const string runtimeServiceKey = "runtimeService";
        protected internal const string taskServiceKey = "taskService";
        protected internal const string repositoryServiceKey = "repositoryService";
        protected internal const string managementServiceKey = "managementService";
        protected internal const string historyServiceKey = "historyService";
        protected internal const string formServiceKey = "formService";

        protected internal static readonly IList<string> KEYS = new List<string>(new string[]{
        processEngineConfigurationKey, runtimeServiceKey, taskServiceKey, repositoryServiceKey, managementServiceKey, historyServiceKey, formServiceKey });


        public VariableScopeResolver(ProcessEngineConfigurationImpl processEngineConfiguration, IVariableScope variableScope)
        {

            this.processEngineConfiguration = processEngineConfiguration;

            if (variableScope == null)
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

        public virtual bool ContainsKey(object key)
        {
            return variableScopeKey.Equals(key?.ToString()) || KEYS.Contains(key?.ToString()) || variableScope.HasVariable(key?.ToString());
        }

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