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

namespace org.activiti.engine.impl.cmd
{
    using org.activiti.engine.@delegate;
    using org.activiti.engine.@delegate.@event;
    using org.activiti.engine.@delegate.@event.impl;
    using org.activiti.engine.impl.bpmn.behavior;
    using org.activiti.engine.impl.context;
    using org.activiti.engine.impl.identity;
    using org.activiti.engine.impl.interceptor;
    using org.activiti.engine.impl.persistence.entity;
    using org.activiti.engine.impl.util;
    using org.activiti.engine.task;
    using Sys.Workflow;

    /// 
    /// 
    [Serializable]
    public class TerminateTaskCmd : CompleteTaskCmd
    {
        private const long serialVersionUID = 1L;

        private readonly bool isTerminateExecution;

        public TerminateTaskCmd(string taskId, string terminateReason, bool isTerminateExecution, IDictionary<string, object> variables = null)
            : base(taskId, variables)
        {
            this.deleteReason = terminateReason;
            this.isTerminateExecution = isTerminateExecution;
            this.variables = variables;
        }

        protected internal override object execute(ICommandContext commandContext, ITaskEntity task)
        {
            return base.execute(commandContext, task);
        }


        protected internal override void executeTaskComplete(ICommandContext commandContext, ITaskEntity taskEntity, IDictionary<string, object> variables, bool localScope)
        {
            // Task complete logic

            if (taskEntity.DelegationState.HasValue && taskEntity.DelegationState.Value == DelegationState.PENDING)
            {
                throw new ActivitiException("A delegated task cannot be completed, but should be resolved instead.");
            }

            commandContext.ProcessEngineConfiguration.ListenerNotificationHelper.executeTaskListeners(taskEntity, BaseTaskListener_Fields.EVENTNAME_COMPLETE);
            IUserInfo user = Authentication.AuthenticatedUser;
            if (user != null && string.IsNullOrWhiteSpace(taskEntity.ProcessInstanceId) == false)
            {
                IExecutionEntity processInstanceEntity = commandContext.ExecutionEntityManager.findById<IExecutionEntity>(taskEntity.ProcessInstanceId);//这里为什么取ProcessInstance而不是Exceution.
                commandContext.IdentityLinkEntityManager.involveUser(processInstanceEntity, user.Id, IdentityLinkType.PARTICIPANT);
            }

            IActivitiEventDispatcher eventDispatcher = Context.ProcessEngineConfiguration.EventDispatcher;
            if (eventDispatcher.Enabled)
            {
                if (variables != null)
                {
                    eventDispatcher.dispatchEvent(ActivitiEventBuilder.createEntityWithVariablesEvent(ActivitiEventType.TASK_COMPLETED, taskEntity, variables, localScope));
                }
                else
                {
                    eventDispatcher.dispatchEvent(ActivitiEventBuilder.createEntityEvent(ActivitiEventType.TASK_COMPLETED, taskEntity));
                }
            }

            commandContext.TaskEntityManager.deleteTask(taskEntity, deleteReason, false, false);

            if (eventDispatcher.Enabled)
            {
                eventDispatcher.dispatchEvent(ActivitiEventBuilder.createCustomTaskCompletedEvent(taskEntity, ActivitiEventType.TASK_TERMINATED));
            }

            // Continue process (if not a standalone task)
            if (!ReferenceEquals(taskEntity.ExecutionId, null))
            {
                IExecutionEntity executionEntity = commandContext.ExecutionEntityManager.findById<IExecutionEntity>(taskEntity.ExecutionId);
                Context.Agenda.planTriggerExecutionOperation(executionEntity);
            }
        }
    }
}