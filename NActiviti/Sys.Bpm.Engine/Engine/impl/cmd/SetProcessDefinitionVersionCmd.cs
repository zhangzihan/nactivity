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

namespace Sys.Workflow.Engine.Impl.Cmd
{
    using Sys.Workflow.Engine.Impl.Interceptor;
    using Sys.Workflow.Engine.Impl.Persistence.Deploies;
    using Sys.Workflow.Engine.Impl.Persistence.Entity;
    using Sys.Workflow.Engine.Impl.Util;
    using Sys.Workflow.Engine.Repository;
    using Sys.Workflow.Engine.Runtime;
    using Sys.Workflow.Bpmn.Models;

    /// <summary>
    /// <seealso cref="Command"/> that changes the process definition version of an existing process instance.
    /// 
    /// Warning: This command will NOT perform any migration magic and simply set the process definition version in the database, assuming that the user knows, what he or she is doing.
    /// 
    /// This is only useful for simple migrations. The new process definition MUST have the exact same activity id to make it still run.
    /// 
    /// Furthermore, activities referenced by sub-executions and jobs that belong to the process instance MUST exist in the new process definition version.
    /// 
    /// The command will fail, if there is already a <seealso cref="IProcessInstance"/> or <seealso cref="IHistoricProcessInstance"/> using the new process definition version and the same business key as the
    /// <seealso cref="IProcessInstance"/> that is to be migrated.
    /// 
    /// If the process instance is not currently waiting but actively running, then this would be a case for optimistic locking, meaning either the version update or the "real work" wins, i.e., this is a
    /// race condition.
    /// </summary>
    /// <seealso cref= http://forums.activiti.org/en/viewtopic.php?t=2918
    ///  </seealso>
    [Serializable]
    public class SetProcessDefinitionVersionCmd : ICommand<object>
    {

        private const long serialVersionUID = 1L;

        private readonly string processInstanceId;
        private readonly int? processDefinitionVersion;

        public SetProcessDefinitionVersionCmd(string processInstanceId, int? processDefinitionVersion)
        {
            if (processInstanceId is null || processInstanceId.Length < 1)
            {
                throw new ActivitiIllegalArgumentException("The process instance id is mandatory, but '" + processInstanceId + "' has been provided.");
            }
            if (processDefinitionVersion == null)
            {
                throw new ActivitiIllegalArgumentException("The process definition version is mandatory, but 'null' has been provided.");
            }
            if (processDefinitionVersion < 1)
            {
                throw new ActivitiIllegalArgumentException("The process definition version must be positive, but '" + processDefinitionVersion + "' has been provided.");
            }
            this.processInstanceId = processInstanceId;
            this.processDefinitionVersion = processDefinitionVersion;
        }

        public virtual object Execute(ICommandContext commandContext)
        {
            // check that the new process definition is just another version of the same
            // process definition that the process instance is using
            IExecutionEntityManager executionManager = commandContext.ExecutionEntityManager;
            IExecutionEntity processInstance = executionManager.FindById<IExecutionEntity>(new KeyValuePair<string, object>("id", processInstanceId));
            if (processInstance == null)
            {
                throw new ActivitiObjectNotFoundException("No process instance found for id = '" + processInstanceId + "'.", typeof(IProcessInstance));
            }
            else if (!processInstance.ProcessInstanceType)
            {
                throw new ActivitiIllegalArgumentException("A process instance id is required, but the provided id " + "'" + processInstanceId + "' " + "points to a child execution of process instance " + "'" + processInstance.ProcessInstanceId + "'. " + "Please invoke the " + this.GetType().Name + " with a root execution id.");
            }

            DeploymentManager deploymentCache = commandContext.ProcessEngineConfiguration.DeploymentManager;
            IProcessDefinition currentProcessDefinition = deploymentCache.FindDeployedProcessDefinitionById(processInstance.ProcessDefinitionId);

            IProcessDefinition newProcessDefinition = deploymentCache.FindDeployedProcessDefinitionByKeyAndVersionAndTenantId(currentProcessDefinition.Key, processDefinitionVersion, currentProcessDefinition.TenantId);

            ValidateAndSwitchVersionOfExecution(commandContext, processInstance, newProcessDefinition);

            // switch the historic process instance to the new process definition version
            commandContext.HistoryManager.RecordProcessDefinitionChange(processInstanceId, newProcessDefinition.Id);

            // switch all sub-executions of the process instance to the new process definition version
            ICollection<IExecutionEntity> childExecutions = executionManager.FindChildExecutionsByProcessInstanceId(processInstanceId);
            foreach (IExecutionEntity executionEntity in childExecutions)
            {
                ValidateAndSwitchVersionOfExecution(commandContext, executionEntity, newProcessDefinition);
            }

            return null;
        }

        protected internal virtual void ValidateAndSwitchVersionOfExecution(ICommandContext commandContext, IExecutionEntity execution, IProcessDefinition newProcessDefinition)
        {
            // check that the new process definition version contains the current activity
            Process process = ProcessDefinitionUtil.GetProcess(newProcessDefinition.Id);
            if (execution.ActivityId is object && process.GetFlowElement(execution.ActivityId, true) is null)
            {
                throw new ActivitiException("The new process definition " + "(key = '" + newProcessDefinition.Key + "') " + "does not contain the current activity " + "(id = '" + execution.ActivityId + "') " + "of the process instance " + "(id = '" + processInstanceId + "').");
            }

            // switch the process instance to the new process definition version
            execution.ProcessDefinitionId = newProcessDefinition.Id;
            execution.ProcessDefinitionName = newProcessDefinition.Name;
            execution.ProcessDefinitionKey = newProcessDefinition.Key;

            // and change possible existing tasks (as the process definition id is stored there too)
            IList<ITaskEntity> tasks = commandContext.TaskEntityManager.FindTasksByExecutionId(execution.Id);
            foreach (ITaskEntity taskEntity in tasks)
            {
                taskEntity.ProcessDefinitionId = newProcessDefinition.Id;
                commandContext.HistoryManager.RecordTaskProcessDefinitionChange(taskEntity.Id, newProcessDefinition.Id);
            }
        }

    }

}