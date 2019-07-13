///////////////////////////////////////////////////////////
//  AddContersignCmd.cs
//  Implementation of the Class AddContersignCmd
//  Created on:      1-2月-2019 8:32:00
//  Original author: 张楠

using Sys.Workflow.Bpmn.Models;
using Sys.Workflow.Engine.Delegate;
using Sys.Workflow.Engine.Delegate.Events;
using Sys.Workflow.Engine.Delegate.Events.Impl;
using Sys.Workflow.Engine.Exceptions;
using Sys.Workflow.Engine.Impl.Cfg;
using Sys.Workflow.Engine.Impl.Contexts;
using Sys.Workflow.Engine.Impl.Identities;
using Sys.Workflow.Engine.Impl.Interceptor;
using Sys.Workflow.Engine.Impl.Persistence.Entity;
using Sys.Workflow.Engine.Impl.Util;
using Sys.Workflow.Engine.Runtime;
using Sys.Workflow.Engine.Tasks;
using Sys.Workflow.Services.Api.Commands;
using Sys.Workflow;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

namespace Sys.Workflow.Engine.Impl.Cmd
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
            this.completeReason = transferTaskCmd.Description;
        }

        public override object Execute(ICommandContext commandContext)
        {
            base.Execute(commandContext);

            return tasks;
        }

        private void Transfer(ICommandContext commandContext)
        {
            ITaskService taskService = commandContext.ProcessEngineConfiguration.TaskService;

            ITaskEntity task = taskService.CreateTaskQuery().SetTaskId(taskCmd.TaskId).SingleResult() as ITaskEntity;
            if (task == null)
            {
                throw new ActivitiObjectNotFoundException("Parent task with id " + taskCmd.TaskId + " was not found");
            }

            //查找当前待追加人员是否已经存在在任务列表中,proc_inst_id_
            IList<ITask> assignTasks = taskService.CreateTaskQuery()
                .SetProcessInstanceId(task.ProcessInstanceId)
                .SetTaskAssigneeIds(taskCmd.Assignees)
                .List();

            var users = taskCmd.Assignees.Where(x => assignTasks.Any(y => x == y.Assignee) == false).ToArray();

            if (users.Length == 0)
            {
                throw new NotFoundAssigneeException();
            }
            else
            {
                tasks = commandContext.ProcessEngineConfiguration.ManagementService.ExecuteCommand(new AddCountersignCmd(task.Id, users, task.TenantId));
            }
        }

        protected internal override void ExecuteTaskComplete(ICommandContext commandContext, ITaskEntity taskEntity, IDictionary<string, object> variables, bool localScope)
        {
            Transfer(commandContext);

            // Task complete logic

            if (taskEntity.DelegationState.HasValue && taskEntity.DelegationState.Value == DelegationState.PENDING)
            {
                throw new ActivitiException("A delegated task cannot be completed, but should be resolved instead.");
            }

            commandContext.ProcessEngineConfiguration.ListenerNotificationHelper.ExecuteTaskListeners(taskEntity, BaseTaskListenerFields.EVENTNAME_COMPLETE);
            IUserInfo user = Authentication.AuthenticatedUser;
            if (user != null && string.IsNullOrWhiteSpace(taskEntity.ProcessInstanceId) == false)
            {
                IExecutionEntity processInstanceEntity = commandContext.ExecutionEntityManager.FindById<IExecutionEntity>(taskEntity.ProcessInstanceId);//这里为什么取ProcessInstance而不是Exceution.
                commandContext.IdentityLinkEntityManager.InvolveUser(processInstanceEntity, user.Id, IdentityLinkType.PARTICIPANT);
            }

            IActivitiEventDispatcher eventDispatcher = Context.ProcessEngineConfiguration.EventDispatcher;
            if (eventDispatcher.Enabled)
            {
                if (variables != null)
                {
                    eventDispatcher.DispatchEvent(ActivitiEventBuilder.CreateEntityWithVariablesEvent(ActivitiEventType.TASK_COMPLETED, taskEntity, variables, localScope));
                }
                else
                {
                    eventDispatcher.DispatchEvent(ActivitiEventBuilder.CreateEntityEvent(ActivitiEventType.TASK_COMPLETED, taskEntity));
                }
            }

            commandContext.TaskEntityManager.DeleteTask(taskEntity, completeReason, false, false);

            if (eventDispatcher.Enabled)
            {
                eventDispatcher.DispatchEvent(ActivitiEventBuilder.CreateCustomTaskCompletedEvent(taskEntity, ActivitiEventType.TASK_TRANSFERED));
            }

            // Continue process (if not a standalone task)
            if (taskEntity.ExecutionId is object)
            {
                IExecutionEntity executionEntity = commandContext.ExecutionEntityManager.FindById<IExecutionEntity>(taskEntity.ExecutionId);
                Context.Agenda.PlanTriggerExecutionOperation(executionEntity);
            }
        }
    }
}
