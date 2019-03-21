using System;
using System.Collections.Generic;

/* Licensed under the Apache License, Version 2.0 (the "License");
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

    using org.activiti.bpmn.model;
    using org.activiti.engine.impl.interceptor;
    using org.activiti.engine.impl.persistence.deploy;
    using org.activiti.engine.impl.runtime;
    using org.activiti.engine.impl.util;
    using org.activiti.engine.repository;
    using org.activiti.engine.runtime;
    using org.activiti.services.api.commands;

    /// 
    /// 
    [Serializable]
    public class StartProcessInstanceCmd : ICommand<IProcessInstance>
    {
        private const long serialVersionUID = 1L;
        protected internal string processDefinitionKey;
        protected internal string processDefinitionId;
        protected internal IDictionary<string, object> variables;
        protected internal IDictionary<string, object> transientVariables;
        protected internal string businessKey;
        protected internal string tenantId;
        protected internal string processInstanceName;
        protected internal ProcessInstanceHelper processInstanceHelper;
        protected internal string startForm;

        private IStartProcessInstanceCmd cmd;

        public StartProcessInstanceCmd(IStartProcessInstanceCmd cmd)
        {
            processDefinitionKey = cmd.ProcessDefinitionKey;
            processDefinitionId = cmd.ProcessDefinitionId;
            variables = cmd.Variables;
            businessKey = cmd.BusinessKey;
            tenantId = cmd.TenantId;
            processInstanceName = cmd.ProcessInstanceName;
            startForm = cmd.StartForm;

            this.cmd = cmd;
        }

        public StartProcessInstanceCmd(string processDefinitionKey, string processDefinitionId, string businessKey, IDictionary<string, object> variables, string startForm = null)
        {
            this.processDefinitionKey = processDefinitionKey;
            this.processDefinitionId = processDefinitionId;
            this.businessKey = businessKey;
            this.variables = variables;
        }

        public StartProcessInstanceCmd(string processDefinitionKey, string processDefinitionId, string businessKey, IDictionary<string, object> variables, string tenantId, string startForm = null) : this(processDefinitionKey, processDefinitionId, businessKey, variables)
        {
            this.tenantId = tenantId;
        }

        public StartProcessInstanceCmd(ProcessInstanceBuilderImpl processInstanceBuilder) : this(processInstanceBuilder.ProcessDefinitionKey, processInstanceBuilder.ProcessDefinitionId, processInstanceBuilder.BusinessKey, processInstanceBuilder.Variables, processInstanceBuilder.TenantId)
        {
            this.processInstanceName = processInstanceBuilder.ProcessInstanceName;
            this.transientVariables = processInstanceBuilder.TransientVariables;
        }

        public virtual IProcessInstance execute(ICommandContext commandContext)
        {
            if (this.cmd != null)
            {
                return this.startFromCommand(commandContext);
            }
            else
            {
                DeploymentManager deploymentCache = commandContext.ProcessEngineConfiguration.DeploymentManager;

                // Find the process definition
                IProcessDefinition processDefinition = null;
                if (!string.IsNullOrWhiteSpace(processDefinitionId))
                {
                    processDefinition = deploymentCache.findDeployedProcessDefinitionById(processDefinitionId);
                    if (processDefinition == null)
                    {
                        throw new ActivitiObjectNotFoundException("No process definition found for id = '" + processDefinitionId + "'", typeof(IProcessDefinition));
                    }

                }
                else if (!ReferenceEquals(processDefinitionKey, null) && (ReferenceEquals(tenantId, null) || ProcessEngineConfiguration.NO_TENANT_ID.Equals(tenantId)))
                {

                    processDefinition = deploymentCache.findDeployedLatestProcessDefinitionByKey(processDefinitionKey);
                    if (processDefinition == null)
                    {
                        throw new ActivitiObjectNotFoundException("No process definition found for key '" + processDefinitionKey + "'", typeof(IProcessDefinition));
                    }

                }
                else if (!ReferenceEquals(processDefinitionKey, null) && !ReferenceEquals(tenantId, null) && !ProcessEngineConfiguration.NO_TENANT_ID.Equals(tenantId))
                {

                    processDefinition = deploymentCache.findDeployedLatestProcessDefinitionByKeyAndTenantId(processDefinitionKey, tenantId);
                    if (processDefinition == null)
                    {
                        throw new ActivitiObjectNotFoundException("No process definition found for key '" + processDefinitionKey + "' for tenant identifier " + tenantId, typeof(IProcessDefinition));
                    }
                }
                else
                {
                    throw new ActivitiIllegalArgumentException("processDefinitionKey and processDefinitionId are null");
                }

                processInstanceHelper = commandContext.ProcessEngineConfiguration.ProcessInstanceHelper;
                IProcessInstance processInstance = createAndStartProcessInstance(processDefinition, businessKey, processInstanceName, variables, transientVariables);

                return processInstance;
            }
        }

        protected internal virtual IProcessInstance createAndStartProcessInstance(IProcessDefinition processDefinition, string businessKey, string processInstanceName, IDictionary<string, object> variables, IDictionary<string, object> transientVariables)
        {
            return processInstanceHelper.createAndStartProcessInstance(processDefinition, businessKey, processInstanceName, variables, transientVariables);
        }

        protected internal virtual IDictionary<string, object> processDataObjects(ICollection<ValuedDataObject> dataObjects)
        {
            IDictionary<string, object> variablesMap = new Dictionary<string, object>();
            // convert data objects to process variables
            if (dataObjects != null)
            {
                foreach (ValuedDataObject dataObject in dataObjects)
                {
                    variablesMap[dataObject.Name] = dataObject.Value;
                }
            }
            return variablesMap;
        }

        private IProcessInstance startFromCommand(ICommandContext commandContext)
        {
            IProcessDefinition definition = null;
            DeploymentManager deploymentCache = commandContext.ProcessEngineConfiguration.DeploymentManager;
            IRepositoryService repositoryService = commandContext.ProcessEngineConfiguration.RepositoryService;
            IRuntimeService runtimeService = commandContext.ProcessEngineConfiguration.RuntimeService;
            string id = cmd.ProcessDefinitionId;
            if (string.IsNullOrWhiteSpace(cmd.StartForm) == false)
            {
                definition = repositoryService.createProcessDefinitionQuery()
                    .processDefinitionStartForm(cmd.StartForm)
                    .processDefinitionTenantId(cmd.TenantId)
                    .latestVersion()
                    .singleResult();

                if (definition == null)
                {
                    throw new ActivitiObjectNotFoundException("Unable to find process definition for the given form key:'" + cmd.StartForm + "'");
                }
                id = definition.Id;
            }
            else if (string.IsNullOrWhiteSpace(processDefinitionKey) == false)
            {
                definition = repositoryService.createProcessDefinitionQuery()
                    .processDefinitionKey(processDefinitionKey)
                    .processDefinitionTenantId(cmd.TenantId)
                    .latestVersion()
                    .singleResult();

                if (definition == null)
                {
                    throw new ActivitiObjectNotFoundException("Unable to find process definition for the given key:'" + processDefinitionKey + "'");
                }
                id = definition.Id;
            }
            else
            {
                definition = repositoryService.getProcessDefinition(id);
                if (definition == null)
                {
                    throw new ActivitiObjectNotFoundException("Unable to find process definition for the given id:'" + id + "'");
                }
                id = definition.Id;
            }

            IProcessInstanceBuilder builder = runtimeService.createProcessInstanceBuilder();
            builder.processDefinitionId(definition.Id);
            if (string.IsNullOrWhiteSpace(cmd.ProcessInstanceName))
            {
                cmd.ProcessInstanceName = definition.Name;
            }
            builder.variables(cmd.Variables);
            builder.businessKey(cmd.BusinessKey);
            builder.name(cmd.ProcessInstanceName);
            builder.tenantId(cmd.TenantId);

            return builder.start();
        }
    }

}