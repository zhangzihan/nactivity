///////////////////////////////////////////////////////////
//  AddContersignCmd.cs
//  Implementation of the Class AddContersignCmd
//  Created on:      1-2月-2019 8:32:00
//  Original author: 张楠

using org.activiti.bpmn.model;
using org.activiti.engine.@delegate;
using org.activiti.engine.@delegate.@event;
using org.activiti.engine.@delegate.@event.impl;
using org.activiti.engine.exceptions;
using org.activiti.engine.impl.cfg;
using org.activiti.engine.impl.context;
using org.activiti.engine.impl.identity;
using org.activiti.engine.impl.interceptor;
using org.activiti.engine.impl.persistence.entity;
using org.activiti.engine.impl.util;
using org.activiti.engine.runtime;
using org.activiti.engine.task;
using org.activiti.services.api.commands;
using Sys.Workflow;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

namespace org.activiti.engine.impl.cmd
{
    /// <summary>
    /// 任务转派
    /// </summary>
    public class TransferTaskCmd : CompleteTaskCmd
    {
        private readonly ITransferTaskCmd taskCmd;

        private ITask[] tasks = null;

        public TransferTaskCmd(ITransferTaskCmd transferTaskCmd) : base(transferTaskCmd.TaskId, transferTaskCmd.Variables)
        {
            this.taskCmd = transferTaskCmd;
            this.deleteReason = transferTaskCmd.Description;
        }

        public override object execute(ICommandContext commandContext)
        {
            base.execute(commandContext);

            return tasks;
        }

        private void transfer(ICommandContext commandContext)
        {
            ITaskService taskService = commandContext.ProcessEngineConfiguration.TaskService;

            ITaskEntity task = taskService.createTaskQuery().taskId(taskCmd.TaskId).singleResult() as ITaskEntity;
            if (task == null)
            {
                throw new ActivitiObjectNotFoundException("Parent task with id " + taskCmd.TaskId + " was not found");
            }

            UserTask userTask = task.Execution.CurrentFlowElement as UserTask;

            userTask.ExtensionElements.TryGetValue("property", out IList<ExtensionElement> elems);
            if (bool.TryParse(elems?.GetAttributeValue("canTransfer"), out bool canTransfer) && canTransfer == false)
            {
                throw new NotSupportTransferException();
            }

            IList<ITask> assignTasks = taskService.createTaskQuery().taskAssigneeIds(taskCmd.Assignees).taskTenantId(task.TenantId).list();

            var users = taskCmd.Assignees.Where(x => assignTasks.Any(y => x == y.Assignee) == false);

            if (users.Count() == 0)
            {
                throw new NotFoundAssigneeException();
            }
            else
            {
                tasks = commandContext.ProcessEngineConfiguration.ManagementService.executeCommand(new AddCountersignCmd(task.Id, users.ToArray(), task.TenantId));
            }
        }

        protected internal override void executeTaskComplete(ICommandContext commandContext, ITaskEntity taskEntity, IDictionary<string, object> variables, bool localScope)
        {
            transfer(commandContext);

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
                eventDispatcher.dispatchEvent(ActivitiEventBuilder.createCustomTaskCompletedEvent(taskEntity, ActivitiEventType.TASK_TRANSFERED));
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
