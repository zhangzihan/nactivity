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
    using System.IO;

    /// 
    /// 
    /// 
    public class RepositoryServiceImpl : ServiceImpl, IRepositoryService
    {
        public virtual IDeploymentBuilder CreateDeployment()
        {
            return commandExecutor.Execute(new CommandAnonymousInnerClass(this));
        }

        private class CommandAnonymousInnerClass : ICommand<IDeploymentBuilder>
        {
            private readonly RepositoryServiceImpl outerInstance;

            public CommandAnonymousInnerClass(RepositoryServiceImpl outerInstance)
            {
                this.outerInstance = outerInstance;
            }

            public virtual IDeploymentBuilder Execute(ICommandContext commandContext)
            {
                return new DeploymentBuilderImpl(outerInstance);
            }
        }

        public virtual IDeployment Deploy(DeploymentBuilderImpl deploymentBuilder)
        {
            return commandExecutor.Execute(new DeployCmd(deploymentBuilder));
        }

        public virtual IDeployment Save(DeploymentBuilderImpl deploymentBuilder)
        {
            return commandExecutor.Execute(new DeploySaveCmd(deploymentBuilder));
        }

        public virtual void DeleteDeployment(string deploymentId)
        {
            commandExecutor.Execute(new DeleteDeploymentCmd(deploymentId, false));
        }

        public virtual void DeleteDeploymentCascade(string deploymentId)
        {
            commandExecutor.Execute(new DeleteDeploymentCmd(deploymentId, true));
        }

        public virtual void DeleteDeployment(string deploymentId, bool cascade)
        {
            commandExecutor.Execute(new DeleteDeploymentCmd(deploymentId, cascade));
        }

        public virtual void SetDeploymentCategory(string deploymentId, string category)
        {
            commandExecutor.Execute(new SetDeploymentCategoryCmd(deploymentId, category));
        }

        public virtual void SetDeploymentKey(string deploymentId, string key)
        {
            commandExecutor.Execute(new SetDeploymentKeyCmd(deploymentId, key));
        }

        public virtual IProcessDefinitionQuery CreateProcessDefinitionQuery()
        {
            return new ProcessDefinitionQueryImpl(commandExecutor);
        }

        public virtual INativeProcessDefinitionQuery CreateNativeProcessDefinitionQuery()
        {
            return new NativeProcessDefinitionQueryImpl(commandExecutor);
        }

        public virtual IList<string> GetDeploymentResourceNames(string deploymentId)
        {
            return commandExecutor.Execute(new GetDeploymentResourceNamesCmd(deploymentId));
        }

        public virtual Stream GetResourceAsStream(string deploymentId, string resourceName)
        {
            return commandExecutor.Execute(new GetDeploymentResourceCmd(deploymentId, resourceName));
        }

        public virtual void ChangeDeploymentTenantId(string deploymentId, string newTenantId)
        {
            commandExecutor.Execute(new ChangeDeploymentTenantIdCmd(deploymentId, newTenantId));
        }

        public virtual IDeploymentQuery CreateDeploymentQuery()
        {
            return new DeploymentQueryImpl(commandExecutor);
        }

        public virtual INativeDeploymentQuery CreateNativeDeploymentQuery()
        {
            return new NativeDeploymentQueryImpl(commandExecutor);
        }

        public virtual IProcessDefinition GetProcessDefinition(string processDefinitionId)
        {
            return commandExecutor.Execute(new GetDeploymentProcessDefinitionCmd(processDefinitionId));
        }

        public virtual BpmnModel GetBpmnModel(string processDefinitionId)
        {
            return commandExecutor.Execute(new GetBpmnModelCmd(processDefinitionId));
        }

        public virtual IProcessDefinition GetDeployedProcessDefinition(string processDefinitionId)
        {
            return commandExecutor.Execute(new GetDeploymentProcessDefinitionCmd(processDefinitionId));
        }

        public virtual bool IsProcessDefinitionSuspended(string processDefinitionId)
        {
            return commandExecutor.Execute(new IsProcessDefinitionSuspendedCmd(processDefinitionId));
        }

        public virtual void SuspendProcessDefinitionById(string processDefinitionId)
        {
            commandExecutor.Execute(new SuspendProcessDefinitionCmd(processDefinitionId, null, false, null, null));
        }

        public virtual void SuspendProcessDefinitionById(string processDefinitionId, bool suspendProcessInstances, DateTime suspensionDate)
        {
            commandExecutor.Execute(new SuspendProcessDefinitionCmd(processDefinitionId, null, suspendProcessInstances, suspensionDate, null));
        }

        public virtual void SuspendProcessDefinitionByKey(string processDefinitionKey)
        {
            commandExecutor.Execute(new SuspendProcessDefinitionCmd(null, processDefinitionKey, false, null, null));
        }

        public virtual void SuspendProcessDefinitionByKey(string processDefinitionKey, bool suspendProcessInstances, DateTime suspensionDate)
        {
            commandExecutor.Execute(new SuspendProcessDefinitionCmd(null, processDefinitionKey, suspendProcessInstances, suspensionDate, null));
        }

        public virtual void SuspendProcessDefinitionByKey(string processDefinitionKey, string tenantId)
        {
            commandExecutor.Execute(new SuspendProcessDefinitionCmd(null, processDefinitionKey, false, null, tenantId));
        }

        public virtual void SuspendProcessDefinitionByKey(string processDefinitionKey, bool suspendProcessInstances, DateTime suspensionDate, string tenantId)
        {
            commandExecutor.Execute(new SuspendProcessDefinitionCmd(null, processDefinitionKey, suspendProcessInstances, suspensionDate, tenantId));
        }

        public virtual void ActivateProcessDefinitionById(string processDefinitionId)
        {
            commandExecutor.Execute(new ActivateProcessDefinitionCmd(processDefinitionId, null, false, DateTime.Now, null));
        }

        public virtual void ActivateProcessDefinitionById(string processDefinitionId, bool activateProcessInstances, DateTime activationDate)
        {
            commandExecutor.Execute(new ActivateProcessDefinitionCmd(processDefinitionId, null, activateProcessInstances, activationDate, null));
        }

        public virtual void ActivateProcessDefinitionByKey(string processDefinitionKey)
        {
            commandExecutor.Execute(new ActivateProcessDefinitionCmd(null, processDefinitionKey, false, DateTime.Now, null));
        }

        public virtual void ActivateProcessDefinitionByKey(string processDefinitionKey, bool activateProcessInstances, DateTime activationDate)
        {
            commandExecutor.Execute(new ActivateProcessDefinitionCmd(null, processDefinitionKey, activateProcessInstances, activationDate, null));
        }

        public virtual void ActivateProcessDefinitionByKey(string processDefinitionKey, string tenantId)
        {
            commandExecutor.Execute(new ActivateProcessDefinitionCmd(null, processDefinitionKey, false, DateTime.Now, tenantId));
        }

        public virtual void ActivateProcessDefinitionByKey(string processDefinitionKey, bool activateProcessInstances, DateTime activationDate, string tenantId)
        {
            commandExecutor.Execute(new ActivateProcessDefinitionCmd(null, processDefinitionKey, activateProcessInstances, activationDate, tenantId));
        }

        public virtual void SetProcessDefinitionCategory(string processDefinitionId, string category)
        {
            commandExecutor.Execute(new SetProcessDefinitionCategoryCmd(processDefinitionId, category));
        }

        public virtual Stream GetProcessModel(string processDefinitionId)
        {
            return commandExecutor.Execute(new GetDeploymentProcessModelCmd(processDefinitionId));
        }

        public virtual IModel NewModel()
        {
            return commandExecutor.Execute(new CreateModelCmd());
        }

        public virtual void SaveModel(IModel model)
        {
            commandExecutor.Execute(new SaveModelCmd((IModelEntity)model));
        }

        public virtual void DeleteModel(string modelId)
        {
            commandExecutor.Execute(new DeleteModelCmd(modelId));
        }

        public virtual void AddModelEditorSource(string modelId, byte[] bytes)
        {
            commandExecutor.Execute(new AddEditorSourceForModelCmd(modelId, bytes));
        }

        public virtual void AddModelEditorSourceExtra(string modelId, byte[] bytes)
        {
            commandExecutor.Execute(new AddEditorSourceExtraForModelCmd(modelId, bytes));
        }

        public virtual IModelQuery CreateModelQuery()
        {
            return new ModelQueryImpl(commandExecutor);
        }

        public virtual INativeModelQuery CreateNativeModelQuery()
        {
            return new NativeModelQueryImpl(commandExecutor);
        }

        public virtual IModel GetModel(string modelId)
        {
            return commandExecutor.Execute(new GetModelCmd(modelId));
        }

        public virtual byte[] GetModelEditorSource(string modelId)
        {
            return commandExecutor.Execute(new GetModelEditorSourceCmd(modelId));
        }

        public virtual byte[] GetModelEditorSourceExtra(string modelId)
        {
            return commandExecutor.Execute(new GetModelEditorSourceExtraCmd(modelId));
        }

        public virtual void AddCandidateStarterUser(string processDefinitionId, string userId)
        {
            commandExecutor.Execute(new AddIdentityLinkForProcessDefinitionCmd(processDefinitionId, userId, null));
        }

        public virtual void AddCandidateStarterGroup(string processDefinitionId, string groupId)
        {
            commandExecutor.Execute(new AddIdentityLinkForProcessDefinitionCmd(processDefinitionId, null, groupId));
        }

        public virtual void DeleteCandidateStarterGroup(string processDefinitionId, string groupId)
        {
            commandExecutor.Execute(new DeleteIdentityLinkForProcessDefinitionCmd(processDefinitionId, null, groupId));
        }

        public virtual void DeleteCandidateStarterUser(string processDefinitionId, string userId)
        {
            commandExecutor.Execute(new DeleteIdentityLinkForProcessDefinitionCmd(processDefinitionId, userId, null));
        }

        public virtual IList<IIdentityLink> GetIdentityLinksForProcessDefinition(string processDefinitionId)
        {
            return commandExecutor.Execute(new GetIdentityLinksForProcessDefinitionCmd(processDefinitionId));
        }

        public virtual IList<ValidationError> ValidateProcess(BpmnModel bpmnModel)
        {
            return commandExecutor.Execute(new ValidateBpmnModelCmd(bpmnModel));
        }

    }

}