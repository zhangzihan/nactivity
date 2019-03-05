using System;
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

namespace org.activiti.engine.impl
{

    using org.activiti.bpmn.model;
    using org.activiti.engine.impl.cfg;
    using org.activiti.engine.impl.cmd;
    using org.activiti.engine.impl.interceptor;
    using org.activiti.engine.impl.persistence.entity;
    using org.activiti.engine.impl.repository;
    using org.activiti.engine.repository;
    using org.activiti.engine.task;
    using org.activiti.validation;

    /// 
    /// 
    /// 
    public class RepositoryServiceImpl : ServiceImpl, IRepositoryService
    {
        public virtual IDeploymentBuilder createDeployment()
        {
            return commandExecutor.execute(new CommandAnonymousInnerClass(this));
        }

        private class CommandAnonymousInnerClass : ICommand<IDeploymentBuilder>
        {
            private readonly RepositoryServiceImpl outerInstance;

            public CommandAnonymousInnerClass(RepositoryServiceImpl outerInstance)
            {
                this.outerInstance = outerInstance;
            }

            public virtual IDeploymentBuilder execute(ICommandContext commandContext)
            {
                return new DeploymentBuilderImpl(outerInstance);
            }
        }

        public virtual IDeployment deploy(DeploymentBuilderImpl deploymentBuilder)
        {
            return commandExecutor.execute(new DeployCmd<IDeployment>(deploymentBuilder));
        }

        public virtual IDeployment save(DeploymentBuilderImpl deploymentBuilder)
        {
            return commandExecutor.execute(new DeploySaveCmd<IDeployment>(deploymentBuilder));
        }

        public virtual void deleteDeployment(string deploymentId)
        {
            commandExecutor.execute(new DeleteDeploymentCmd(deploymentId, false));
        }

        public virtual void deleteDeploymentCascade(string deploymentId)
        {
            commandExecutor.execute(new DeleteDeploymentCmd(deploymentId, true));
        }

        public virtual void deleteDeployment(string deploymentId, bool cascade)
        {
            commandExecutor.execute(new DeleteDeploymentCmd(deploymentId, cascade));
        }

        public virtual void setDeploymentCategory(string deploymentId, string category)
        {
            commandExecutor.execute(new SetDeploymentCategoryCmd(deploymentId, category));
        }

        public virtual void setDeploymentKey(string deploymentId, string key)
        {
            commandExecutor.execute(new SetDeploymentKeyCmd(deploymentId, key));
        }

        public virtual IProcessDefinitionQuery createProcessDefinitionQuery()
        {
            return new ProcessDefinitionQueryImpl(commandExecutor);
        }

        public virtual INativeProcessDefinitionQuery createNativeProcessDefinitionQuery()
        {
            return new NativeProcessDefinitionQueryImpl(commandExecutor);
        }

        public virtual IList<string> getDeploymentResourceNames(string deploymentId)
        {
            return commandExecutor.execute(new GetDeploymentResourceNamesCmd(deploymentId));
        }

        public virtual System.IO.Stream getResourceAsStream(string deploymentId, string resourceName)
        {
            return commandExecutor.execute(new GetDeploymentResourceCmd(deploymentId, resourceName));
        }

        public virtual void changeDeploymentTenantId(string deploymentId, string newTenantId)
        {
            commandExecutor.execute(new ChangeDeploymentTenantIdCmd(deploymentId, newTenantId));
        }

        public virtual IDeploymentQuery createDeploymentQuery()
        {
            return new DeploymentQueryImpl(commandExecutor);
        }

        public virtual INativeDeploymentQuery createNativeDeploymentQuery()
        {
            return new NativeDeploymentQueryImpl(commandExecutor);
        }

        public virtual IProcessDefinition getProcessDefinition(string processDefinitionId)
        {
            return commandExecutor.execute(new GetDeploymentProcessDefinitionCmd(processDefinitionId));
        }

        public virtual BpmnModel getBpmnModel(string processDefinitionId)
        {
            return commandExecutor.execute(new GetBpmnModelCmd(processDefinitionId));
        }

        public virtual IProcessDefinition getDeployedProcessDefinition(string processDefinitionId)
        {
            return commandExecutor.execute(new GetDeploymentProcessDefinitionCmd(processDefinitionId));
        }

        public virtual bool isProcessDefinitionSuspended(string processDefinitionId)
        {
            return commandExecutor.execute(new IsProcessDefinitionSuspendedCmd(processDefinitionId));
        }

        public virtual void suspendProcessDefinitionById(string processDefinitionId)
        {
            commandExecutor.execute(new SuspendProcessDefinitionCmd(processDefinitionId, null, false, null, null));
        }

        public virtual void suspendProcessDefinitionById(string processDefinitionId, bool suspendProcessInstances, DateTime suspensionDate)
        {
            commandExecutor.execute(new SuspendProcessDefinitionCmd(processDefinitionId, null, suspendProcessInstances, suspensionDate, null));
        }

        public virtual void suspendProcessDefinitionByKey(string processDefinitionKey)
        {
            commandExecutor.execute(new SuspendProcessDefinitionCmd(null, processDefinitionKey, false, null, null));
        }

        public virtual void suspendProcessDefinitionByKey(string processDefinitionKey, bool suspendProcessInstances, DateTime suspensionDate)
        {
            commandExecutor.execute(new SuspendProcessDefinitionCmd(null, processDefinitionKey, suspendProcessInstances, suspensionDate, null));
        }

        public virtual void suspendProcessDefinitionByKey(string processDefinitionKey, string tenantId)
        {
            commandExecutor.execute(new SuspendProcessDefinitionCmd(null, processDefinitionKey, false, null, tenantId));
        }

        public virtual void suspendProcessDefinitionByKey(string processDefinitionKey, bool suspendProcessInstances, DateTime suspensionDate, string tenantId)
        {
            commandExecutor.execute(new SuspendProcessDefinitionCmd(null, processDefinitionKey, suspendProcessInstances, suspensionDate, tenantId));
        }

        public virtual void activateProcessDefinitionById(string processDefinitionId)
        {
            commandExecutor.execute(new ActivateProcessDefinitionCmd(processDefinitionId, null, false, DateTime.Now, null));
        }

        public virtual void activateProcessDefinitionById(string processDefinitionId, bool activateProcessInstances, DateTime activationDate)
        {
            commandExecutor.execute(new ActivateProcessDefinitionCmd(processDefinitionId, null, activateProcessInstances, activationDate, null));
        }

        public virtual void activateProcessDefinitionByKey(string processDefinitionKey)
        {
            commandExecutor.execute(new ActivateProcessDefinitionCmd(null, processDefinitionKey, false, DateTime.Now, null));
        }

        public virtual void activateProcessDefinitionByKey(string processDefinitionKey, bool activateProcessInstances, DateTime activationDate)
        {
            commandExecutor.execute(new ActivateProcessDefinitionCmd(null, processDefinitionKey, activateProcessInstances, activationDate, null));
        }

        public virtual void activateProcessDefinitionByKey(string processDefinitionKey, string tenantId)
        {
            commandExecutor.execute(new ActivateProcessDefinitionCmd(null, processDefinitionKey, false, DateTime.Now, tenantId));
        }

        public virtual void activateProcessDefinitionByKey(string processDefinitionKey, bool activateProcessInstances, DateTime activationDate, string tenantId)
        {
            commandExecutor.execute(new ActivateProcessDefinitionCmd(null, processDefinitionKey, activateProcessInstances, activationDate, tenantId));
        }

        public virtual void setProcessDefinitionCategory(string processDefinitionId, string category)
        {
            commandExecutor.execute(new SetProcessDefinitionCategoryCmd(processDefinitionId, category));
        }

        public virtual System.IO.Stream getProcessModel(string processDefinitionId)
        {
            return commandExecutor.execute(new GetDeploymentProcessModelCmd(processDefinitionId));
        }

        public virtual IModel newModel()
        {
            return commandExecutor.execute(new CreateModelCmd());
        }

        public virtual void saveModel(IModel model)
        {
            commandExecutor.execute(new SaveModelCmd((IModelEntity)model));
        }

        public virtual void deleteModel(string modelId)
        {
            commandExecutor.execute(new DeleteModelCmd(modelId));
        }

        public virtual void addModelEditorSource(string modelId, byte[] bytes)
        {
            commandExecutor.execute(new AddEditorSourceForModelCmd(modelId, bytes));
        }

        public virtual void addModelEditorSourceExtra(string modelId, byte[] bytes)
        {
            commandExecutor.execute(new AddEditorSourceExtraForModelCmd(modelId, bytes));
        }

        public virtual IModelQuery createModelQuery()
        {
            return new ModelQueryImpl(commandExecutor);
        }

        public virtual INativeModelQuery createNativeModelQuery()
        {
            return new NativeModelQueryImpl(commandExecutor);
        }

        public virtual IModel getModel(string modelId)
        {
            return commandExecutor.execute(new GetModelCmd(modelId));
        }

        public virtual byte[] getModelEditorSource(string modelId)
        {
            return commandExecutor.execute(new GetModelEditorSourceCmd(modelId));
        }

        public virtual byte[] getModelEditorSourceExtra(string modelId)
        {
            return commandExecutor.execute(new GetModelEditorSourceExtraCmd(modelId));
        }

        public virtual void addCandidateStarterUser(string processDefinitionId, string userId)
        {
            commandExecutor.execute(new AddIdentityLinkForProcessDefinitionCmd(processDefinitionId, userId, null));
        }

        public virtual void addCandidateStarterGroup(string processDefinitionId, string groupId)
        {
            commandExecutor.execute(new AddIdentityLinkForProcessDefinitionCmd(processDefinitionId, null, groupId));
        }

        public virtual void deleteCandidateStarterGroup(string processDefinitionId, string groupId)
        {
            commandExecutor.execute(new DeleteIdentityLinkForProcessDefinitionCmd(processDefinitionId, null, groupId));
        }

        public virtual void deleteCandidateStarterUser(string processDefinitionId, string userId)
        {
            commandExecutor.execute(new DeleteIdentityLinkForProcessDefinitionCmd(processDefinitionId, userId, null));
        }

        public virtual IList<IIdentityLink> getIdentityLinksForProcessDefinition(string processDefinitionId)
        {
            return commandExecutor.execute(new GetIdentityLinksForProcessDefinitionCmd(processDefinitionId));
        }

        public virtual IList<ValidationError> validateProcess(BpmnModel bpmnModel)
        {
            return commandExecutor.execute(new ValidateBpmnModelCmd(bpmnModel));
        }

    }

}