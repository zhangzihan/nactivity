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

    using Sys.Workflow.Engine.Impl.Interceptor;
    using Sys.Workflow.Engine.Impl.Persistence.Deploies;
    using Sys.Workflow.Engine.Impl.Persistence.Entity;
    using Sys.Workflow.Engine.Impl.Runtimes;
    using Sys.Workflow.Engine.Impl.Util;
    using Sys.Workflow.Engine.Repository;
    using Sys.Workflow.Engine.Runtime;

    /// 
    /// 
    public class StartProcessInstanceByMessageCmd : ICommand<IProcessInstance>
    {
        private string messageName;
        private readonly string businessKey;
        private IDictionary<string, object> processVariables;
        private IDictionary<string, object> transientVariables;
        private string tenantId;

        ///<inheritdoc />
        public StartProcessInstanceByMessageCmd(string messageName, string businessKey, IDictionary<string, object> processVariables, string tenantId)
        {
            this.messageName = messageName;
            this.businessKey = businessKey;
            this.processVariables = processVariables;
            this.tenantId = tenantId;
        }

        ///<inheritdoc />
        public StartProcessInstanceByMessageCmd(ProcessInstanceBuilderImpl processInstanceBuilder)
        {
            this.messageName = processInstanceBuilder.MessageName;
            this.businessKey = processInstanceBuilder.BusinessKey;
            this.processVariables = processInstanceBuilder.Variables;
            this.transientVariables = processInstanceBuilder.TransientVariables;
            this.tenantId = processInstanceBuilder.TenantId;
        }

        ///<inheritdoc />
        public virtual IProcessInstance Execute(ICommandContext commandContext)
        {

            if (messageName is null)
            {
                throw new ActivitiIllegalArgumentException("Cannot start process instance by message: message name is null");
            }

            IMessageEventSubscriptionEntity messageEventSubscription = commandContext.EventSubscriptionEntityManager.FindMessageStartEventSubscriptionByName(messageName, tenantId);

            if (messageEventSubscription is null)
            {
                throw new ActivitiObjectNotFoundException("Cannot start process instance by message: no subscription to message with name '" + messageName + "' found.", typeof(IMessageEventSubscriptionEntity));
            }

            string processDefinitionId = messageEventSubscription.Configuration;
            if (string.IsNullOrWhiteSpace(processDefinitionId))
            {
                throw new ActivitiException("Cannot start process instance by message: subscription to message with name '" + messageName + "' is not a message start event.");
            }

            DeploymentManager deploymentCache = commandContext.ProcessEngineConfiguration.DeploymentManager;

            IProcessDefinition processDefinition = deploymentCache.FindDeployedProcessDefinitionById(processDefinitionId);
            if (processDefinition is null)
            {
                throw new ActivitiObjectNotFoundException("No process definition found for id '" + processDefinitionId + "'", typeof(IProcessDefinition));
            }

            ProcessInstanceHelper processInstanceHelper = commandContext.ProcessEngineConfiguration.ProcessInstanceHelper;
            IProcessInstance processInstance = processInstanceHelper.CreateAndStartProcessInstanceByMessage(processDefinition, messageName, processVariables, transientVariables);

            return processInstance;
        }

    }

}