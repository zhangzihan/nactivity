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

        public TransferTaskCmd(ITransferTaskCmd transferTaskCmd) : base(transferTaskCmd.TaskId, transferTaskCmd.Variables, transferTaskCmd.TransientVariables)
        {
            this.taskCmd = transferTaskCmd;
            this.completeReason = transferTaskCmd.Description;
        }

        public override object Execute(ICommandContext commandContext)
        {
            base.Execute(commandContext);

            return tasks;
        }

        private void Transfer(ICommandContext commandContext, ITaskEntity task)
        {
            ITaskService taskService = commandContext.ProcessEngineConfiguration.TaskService;

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
                tasks = taskService.AddCountersign(task.Id, users, task.TenantId);
            }
        }

        protected internal override void ExecuteTaskComplete(ICommandContext commandContext, ITaskEntity taskEntity, IDictionary<string, object> variables, bool localScope)
        {
            var userTask = taskEntity.Execution.CurrentFlowElement as UserTask;
            if (userTask is null)
            {
                throw new Exception("当前任务不是用户任务");
            }

            if (!userTask.CanTransfer)
            {
                throw new Exception("当前任务不允许转办.");
            }

            Transfer(commandContext, taskEntity);

            if (string.IsNullOrWhiteSpace(completeReason) == false)
            {
                commandContext.ProcessEngineConfiguration.commandExecutor
                     .Execute(new CommandConfig(true), new AddCommentCmd(taskEntity.Id, taskEntity.ProcessInstanceId, completeReason));
            }
            // Task complete logic
            CompleteTask(commandContext, taskEntity, variables, localScope);

            IActivitiEventDispatcher eventDispatcher = Context.ProcessEngineConfiguration.EventDispatcher;

            if (eventDispatcher.Enabled)
            {
                eventDispatcher.DispatchEvent(ActivitiEventBuilder.CreateCustomTaskCompletedEvent(taskEntity, ActivitiEventType.TASK_TRANSFERED));
            }

            // Continue process (if not a standalone task)
            if (taskEntity.ExecutionId is not null)
            {
                IExecutionEntity executionEntity = commandContext.ExecutionEntityManager.FindById<IExecutionEntity>(taskEntity.ExecutionId);
                Context.Agenda.PlanTriggerExecutionOperation(executionEntity, variables ?? new Dictionary<string, object>());
            }
        }
    }
}
