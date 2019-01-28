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

namespace org.activiti.engine.impl.cmd
{

    using org.activiti.engine.impl.interceptor;
    using org.activiti.engine.impl.persistence.deploy;
    using org.activiti.engine.impl.persistence.entity;
    using org.activiti.engine.impl.runtime;
    using org.activiti.engine.impl.util;
    using org.activiti.engine.repository;
    using org.activiti.engine.runtime;

    /// 
    /// 
    public class StartProcessInstanceByMessageCmd : ICommand<IProcessInstance>
    {

        protected internal string messageName;
        protected internal string businessKey;
        protected internal IDictionary<string, object> processVariables;
        protected internal IDictionary<string, object> transientVariables;
        protected internal string tenantId;

        public StartProcessInstanceByMessageCmd(string messageName, string businessKey, IDictionary<string, object> processVariables, string tenantId)
        {
            this.messageName = messageName;
            this.businessKey = businessKey;
            this.processVariables = processVariables;
            this.tenantId = tenantId;
        }

        public StartProcessInstanceByMessageCmd(ProcessInstanceBuilderImpl processInstanceBuilder)
        {
            this.messageName = processInstanceBuilder.MessageName;
            this.businessKey = processInstanceBuilder.BusinessKey;
            this.processVariables = processInstanceBuilder.Variables;
            this.transientVariables = processInstanceBuilder.TransientVariables;
            this.tenantId = processInstanceBuilder.TenantId;
        }

        public  virtual IProcessInstance  execute(ICommandContext  commandContext)
        {

            if (ReferenceEquals(messageName, null))
            {
                throw new ActivitiIllegalArgumentException("Cannot start process instance by message: message name is null");
            }

            IMessageEventSubscriptionEntity messageEventSubscription = commandContext.EventSubscriptionEntityManager.findMessageStartEventSubscriptionByName(messageName, tenantId);

            if (messageEventSubscription == null)
            {
                throw new ActivitiObjectNotFoundException("Cannot start process instance by message: no subscription to message with name '" + messageName + "' found.", typeof(IMessageEventSubscriptionEntity));
            }

            string processDefinitionId = messageEventSubscription.Configuration;
            if (string.IsNullOrWhiteSpace(processDefinitionId))
            {
                throw new ActivitiException("Cannot start process instance by message: subscription to message with name '" + messageName + "' is not a message start event.");
            }

            DeploymentManager deploymentCache = commandContext.ProcessEngineConfiguration.DeploymentManager;

            IProcessDefinition processDefinition = deploymentCache.findDeployedProcessDefinitionById(processDefinitionId);
            if (processDefinition == null)
            {
                throw new ActivitiObjectNotFoundException("No process definition found for id '" + processDefinitionId + "'", typeof(IProcessDefinition));
            }

            ProcessInstanceHelper processInstanceHelper = commandContext.ProcessEngineConfiguration.ProcessInstanceHelper;
            IProcessInstance processInstance = processInstanceHelper.createAndStartProcessInstanceByMessage(processDefinition, messageName, processVariables, transientVariables);

            return processInstance;
        }

    }

}