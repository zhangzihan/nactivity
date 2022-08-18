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
namespace Sys.Workflow.Engine.Impl.Cmd
{
    using Microsoft.Extensions.Logging;
    using Sys.Workflow.Bpmn.Models;
    using Sys.Workflow.Engine.Impl.Interceptor;
    using Sys.Workflow.Engine.Impl.Persistence.Deploies;
    using Sys.Workflow.Engine.Impl.Runtimes;
    using Sys.Workflow.Engine.Impl.Util;
    using Sys.Workflow.Engine.Repository;
    using Sys.Workflow.Engine.Runtime;
    using Sys.Workflow.Services.Api.Commands;
    using Sys.Workflow;

    /// 
    /// 
    [Serializable]
    public class StartProcessInstanceCmd : ICommand<IProcessInstance>
    {
        private const long serialVersionUID = 1L;
        private string processDefinitionKey;
        private string processDefinitionId;
        private string processDefinitionBusinessKey;
        private IDictionary<string, object> variables;
        private IDictionary<string, object> transientVariables;
        private string businessKey;
        private string tenantId;
        private string processInstanceName;
        private ProcessInstanceHelper processInstanceHelper;
        private string startForm;
        private string processName;
        private readonly string initialFlowElementId;

        private readonly IStartProcessInstanceCmd startCmd;

        private readonly ILogger logger = ProcessEngineServiceProvider.LoggerService<StartProcessInstanceCmd>();

        /// <inheritdoc />
        public StartProcessInstanceCmd(IStartProcessInstanceCmd cmd)
        {
            processDefinitionKey = cmd.ProcessDefinitionKey;
            processDefinitionId = cmd.ProcessDefinitionId;
            variables = cmd.Variables;
            businessKey = cmd.BusinessKey;
            tenantId = cmd.TenantId;
            processInstanceName = cmd.ProcessInstanceName;
            startForm = cmd.StartForm;
            processName = cmd.ProcessName;
            processDefinitionBusinessKey = cmd.ProcessDefinitionBusinessKey;
            initialFlowElementId = cmd.InitialFlowElementId;

            this.startCmd = cmd;
        }

        /// <inheritdoc />
        public StartProcessInstanceCmd(string processDefinitionKey, string processDefinitionId, string businessKey, IDictionary<string, object> variables, string startForm = null)
        {
            this.processDefinitionKey = processDefinitionKey;
            this.processDefinitionId = processDefinitionId;
            this.businessKey = businessKey;
            this.variables = variables;
            this.startForm = startForm;
        }

        /// <inheritdoc />
        public StartProcessInstanceCmd(string processDefinitionKey, string processDefinitionId, string businessKey, IDictionary<string, object> variables, string tenantId, string startForm = null) : this(processDefinitionKey, processDefinitionId, businessKey, variables, startForm)
        {
            this.tenantId = tenantId;
        }

        /// <inheritdoc />
        public StartProcessInstanceCmd(string processDefinitionId, IDictionary<string, object> variables)
        {
            this.processDefinitionId = processDefinitionId;
            this.variables = variables;
        }

        /// <inheritdoc />
        public StartProcessInstanceCmd(ProcessInstanceBuilderImpl processInstanceBuilder) : this(processInstanceBuilder.ProcessDefinitionKey, processInstanceBuilder.ProcessDefinitionId, processInstanceBuilder.BusinessKey, processInstanceBuilder.Variables, processInstanceBuilder.TenantId)
        {
            this.processInstanceName = processInstanceBuilder.ProcessInstanceName;
            this.transientVariables = processInstanceBuilder.TransientVariables;
            this.initialFlowElementId = processInstanceBuilder.InitialFlowElementId;
        }

        /// <inheritdoc />
        public virtual IProcessInstance Execute(ICommandContext commandContext)
        {
            if (this.startCmd is object)
            {
                return this.StartFromCommand(commandContext);
            }
            else
            {
                DeploymentManager deploymentCache = commandContext.ProcessEngineConfiguration.DeploymentManager;

                // Find the process definition
                IProcessDefinition processDefinition;
                if (!string.IsNullOrWhiteSpace(processDefinitionId))
                {
                    processDefinition = deploymentCache.FindDeployedProcessDefinitionById(processDefinitionId);
                    if (processDefinition is null)
                    {
                        throw new ActivitiObjectNotFoundException("No process definition found for id = '" + processDefinitionId + "'", typeof(IProcessDefinition));
                    }

                }
                else if (processDefinitionKey is not null && (tenantId is null || ProcessEngineConfiguration.NO_TENANT_ID.Equals(tenantId)))
                {
                    processDefinition = deploymentCache.FindDeployedLatestProcessDefinitionByKey(processDefinitionKey);
                    if (processDefinition is null)
                    {
                        throw new ActivitiObjectNotFoundException("No process definition found for key '" + processDefinitionKey + "'", typeof(IProcessDefinition));
                    }

                }
                else if (processDefinitionKey is not null && tenantId is not null && !ProcessEngineConfiguration.NO_TENANT_ID.Equals(tenantId))
                {
                    processDefinition = deploymentCache.FindDeployedLatestProcessDefinitionByKeyAndTenantId(processDefinitionKey, tenantId);
                    if (processDefinition is null)
                    {
                        throw new ActivitiObjectNotFoundException("No process definition found for key '" + processDefinitionKey + "' for tenant identifier " + tenantId, typeof(IProcessDefinition));
                    }
                }
                else
                {
                    throw new ActivitiIllegalArgumentException("processDefinitionKey and processDefinitionId are null");
                }

                processInstanceHelper = commandContext.ProcessEngineConfiguration.ProcessInstanceHelper;

                IProcessInstance processInstance = processInstanceHelper.CreateAndStartProcessInstance(processDefinition, businessKey, processInstanceName, variables, transientVariables, initialFlowElementId);

                return processInstance;
            }
        }

        /// <inheritdoc />
        protected internal virtual IDictionary<string, object> ProcessDataObjects(ICollection<ValuedDataObject> dataObjects)
        {
            IDictionary<string, object> variablesMap = new Dictionary<string, object>();
            // convert data objects to process variables
            if (dataObjects is object)
            {
                foreach (ValuedDataObject dataObject in dataObjects)
                {
                    variablesMap[dataObject.Name] = dataObject.Value;
                }
            }
            return variablesMap;
        }

        private IProcessInstance StartFromCommand(ICommandContext commandContext)
        {
            IRepositoryService repositoryService = commandContext.ProcessEngineConfiguration.RepositoryService;
            IRuntimeService runtimeService = commandContext.ProcessEngineConfiguration.RuntimeService;
            string id = startCmd.ProcessDefinitionId;
            IProcessDefinition definition = null;
            IProcessInstanceBuilder builder = runtimeService.CreateProcessInstanceBuilder();
            if (string.IsNullOrWhiteSpace(startCmd.StartByMessage) == false)
            {
                builder.SetMessageName(startCmd.StartByMessage);
            }
            else if (string.IsNullOrWhiteSpace(startCmd.StartForm) == false)
            {
                definition = repositoryService.CreateProcessDefinitionQuery()
                    .SetProcessDefinitionStartForm(startCmd.StartForm)
                    .SetProcessDefinitionTenantId(startCmd.TenantId)
                    .SetLatestVersion()
                    .SingleResult();

                if (definition is null)
                {
                    logger.LogError($"Unable to find process definition for the given start form key:'{startCmd.StartForm}' with tenantId:'{startCmd.TenantId}'");

                    throw new ActivitiObjectNotFoundException("Unable to find process definition for the given start form key:'" + startCmd.StartForm + "'");
                }
                processDefinitionId = definition.Id;
            }
            else if (string.IsNullOrWhiteSpace(processName) == false)
            {
                definition = repositoryService.CreateProcessDefinitionQuery()
                    .SetProcessDefinitionName(startCmd.ProcessName)
                    .SetProcessDefinitionTenantId(startCmd.TenantId)
                    .SetLatestVersion()
                    .SingleResult();

                if (definition is null)
                {
                    logger.LogError($"Unable to find process definition for the given process name:'{startCmd.ProcessName}' with tenantid:'{tenantId}'");

                    throw new ActivitiObjectNotFoundException($"Unable to find process definition for the given process name:'{startCmd.ProcessName}' with tenantid:'{tenantId}'");
                }
                processDefinitionId = definition.Id;
            }
            else if (string.IsNullOrWhiteSpace(processDefinitionKey) == false)
            {
                definition = repositoryService.CreateProcessDefinitionQuery()
                    .SetProcessDefinitionKey(processDefinitionKey)
                    .SetProcessDefinitionTenantId(startCmd.TenantId)
                    .SetLatestVersion()
                    .SingleResult();

                if (definition is null)
                {
                    logger.LogError($"Unable to find process definition for the given key:'{processDefinitionKey}'");

                    throw new ActivitiObjectNotFoundException($"Unable to find process definition for the given key:'{processDefinitionKey}'");
                }
                processDefinitionId = definition.Id;
            }
            else if (string.IsNullOrWhiteSpace(processDefinitionBusinessKey) == false)
            {
                definition = repositoryService.CreateProcessDefinitionQuery()
                    .SetProcessDefinitionBusinessKey(processDefinitionBusinessKey)
                    .SetProcessDefinitionTenantId(startCmd.TenantId)
                    .SetLatestVersion()
                    .SingleResult();

                if (definition is null)
                {
                    logger.LogError($"Unable to find process definition for the given key:'{processDefinitionKey}'");

                    throw new ActivitiObjectNotFoundException($"Unable to find process definition for the given key:'{processDefinitionKey}'");
                }
                processDefinitionId = definition.Id;
            }
            else
            {
                definition = repositoryService.GetProcessDefinition(id);
                if (definition is null)
                {
                    logger.LogError($"Unable to find process definition for the given id:'{id}'");

                    throw new ActivitiObjectNotFoundException($"Unable to find process definition for the given id:'{id}'");
                }
                processDefinitionId = definition.Id;
            }

            if (definition is object)
            {
                builder.SetProcessDefinitionId(definition.Id);
                if (string.IsNullOrWhiteSpace(startCmd.ProcessInstanceName))
                {
                    startCmd.ProcessInstanceName = definition.Name;
                }
            }
            builder.SetVariables(startCmd.Variables);
            builder.SetBusinessKey(startCmd.BusinessKey);
            builder.SetName(startCmd.ProcessInstanceName);
            builder.SetTenantId(startCmd.TenantId);
            builder.SetInitialFlowElement(startCmd.InitialFlowElementId);

            return builder.Start();
        }
    }

}