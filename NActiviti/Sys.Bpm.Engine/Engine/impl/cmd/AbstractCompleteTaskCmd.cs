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
    using Sys.Workflow.Engine.Delegate;
    using Sys.Workflow.Engine.Delegate.Events;
    using Sys.Workflow.Engine.Delegate.Events.Impl;
    using Sys.Workflow.Engine.Impl.Agenda;
    using Sys.Workflow.Engine.Impl.Cfg;
    using Sys.Workflow.Engine.Impl.Contexts;
    using Sys.Workflow.Engine.Impl.Identities;
    using Sys.Workflow.Engine.Impl.Interceptor;
    using Sys.Workflow.Engine.Impl.Persistence.Entity;
    using Sys.Workflow.Engine.Tasks;
    using Sys.Workflow;

    /// 
    [Serializable]
    public abstract class AbstractCompleteTaskCmd : NeedsActiveTaskCmd<object>
    {
        private const long serialVersionUID = 1L;

        protected string completeReason = null;

        public AbstractCompleteTaskCmd(string taskId) : base(taskId)
        {
        }

        protected internal virtual void ExecuteTaskComplete(ICommandContext commandContext, ITaskEntity taskEntity, IDictionary<string, object> variables, bool localScope)
        {
            CompleteTask(commandContext, taskEntity, variables, localScope);

            // Continue process (if not a standalone task)
            if (taskEntity.ExecutionId is object)
            {
                IExecutionEntity executionEntity = commandContext.ExecutionEntityManager.FindById<IExecutionEntity>(taskEntity.ExecutionId);
                Context.Agenda.PlanTriggerExecutionOperation(executionEntity, variables ?? new Dictionary<string, object>());
            }
        }

        protected void CompleteTask(ICommandContext commandContext, ITaskEntity taskEntity, IDictionary<string, object> variables, bool localScope)
        {
            // Task complete logic
            if (taskEntity.DelegationState.HasValue && taskEntity.DelegationState.Value == DelegationState.PENDING)
            {
                throw new ActivitiException("A delegated task cannot be completed, but should be resolved instead.");
            }

            ProcessEngineConfigurationImpl processEngineConfiguration = commandContext.ProcessEngineConfiguration;
            processEngineConfiguration.ListenerNotificationHelper.ExecuteTaskListeners(taskEntity, BaseTaskListenerFields.EVENTNAME_COMPLETE);

            IUserInfo user = Authentication.AuthenticatedUser;
            if (user is object && string.IsNullOrWhiteSpace(taskEntity.ProcessInstanceId) == false)
            {
                IExecutionEntity processInstanceEntity = commandContext.ExecutionEntityManager.FindById<IExecutionEntity>(taskEntity.ProcessInstanceId);
                commandContext.IdentityLinkEntityManager.InvolveUser(processInstanceEntity, user.Id, IdentityLinkType.PARTICIPANT);
            }

            IActivitiEventDispatcher eventDispatcher = Context.ProcessEngineConfiguration.EventDispatcher;
            if (eventDispatcher.Enabled)
            {
                if (variables is null)
                {
                    eventDispatcher.DispatchEvent(ActivitiEventBuilder.CreateEntityEvent(ActivitiEventType.TASK_COMPLETED, taskEntity));
                }
                else
                {
                    eventDispatcher.DispatchEvent(ActivitiEventBuilder.CreateEntityWithVariablesEvent(ActivitiEventType.TASK_COMPLETED, taskEntity, variables, localScope));
                }
            }

            commandContext.TaskEntityManager.DeleteTask(taskEntity, completeReason, false, false);
        }
    }
}