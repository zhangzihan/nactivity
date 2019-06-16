using System;

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
    using Sys.Workflow.Engine.Impl.Persistence.Entity;
    using Sys.Workflow.Engine.Runtime;
    using ICommandExecutor = Interceptor.ICommandExecutor;

    /// 
    [Serializable]
    public class DeleteProcessInstanceCmd : ICommand<object>
    {

        private const long serialVersionUID = 1L;
        protected internal string processInstanceId;
        protected internal string deleteReason;

        public DeleteProcessInstanceCmd(string processInstanceId, string deleteReason)
        {
            this.processInstanceId = processInstanceId;
            this.deleteReason = deleteReason;
        }

        public virtual object Execute(ICommandContext commandContext)
        {
            if (processInstanceId is null)
            {
                throw new ActivitiIllegalArgumentException("processInstanceId is null");
            }

            ICommandExecutor commandExecutor = commandContext.ProcessEngineConfiguration.CommandExecutor;

            IExecutionEntity processInstanceEntity = commandContext.ExecutionEntityManager.FindById<IExecutionEntity>(processInstanceId);

            if (processInstanceEntity is null)
            {
                throw new ActivitiObjectNotFoundException("No process instance found for id '" + processInstanceId + "'", typeof(IProcessInstance));
            }

            commandContext.ExecutionEntityManager.DeleteProcessInstance(processInstanceEntity.ProcessInstanceId, deleteReason, false);

            return null;
        }
    }

}