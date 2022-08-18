using Microsoft.Extensions.Logging;
using Sys.Workflow.Engine.Delegate;
using Sys.Workflow.Engine.Exceptions;
using Sys.Workflow.Engine.Impl.Cmd;
using Sys.Workflow.Engine.Impl.Contexts;
using Sys.Workflow.Engine.Impl.Util;
using Sys.Workflow.Services.Api.Commands;
using Sys;
using Sys.Workflow;
using Sys.Workflow.Engine.Bpmn.Rules;
using System;
using System.Collections.Generic;
using System.Text;
using Sys.Workflow.Engine.Impl.Agenda;
using Sys.Workflow.Engine.Delegate.Events;
using Sys.Workflow.Engine.Impl.Persistence.Entity;
using Sys.Workflow.Bpmn.Models;

namespace Sys.Workflow.Engine.Impl.Bpmn.Listeners
{
    class UserTaskAssignmentListener : ITaskListener, IActivitiEventListener
    {
        public bool FailOnException => true;

        public void Notify(IDelegateTask task)
        {
            if (task is null || task.Execution.CurrentFlowElement is not UserTask)
            {
                return;
            }

            var logger = ProcessEngineServiceProvider.LoggerService<UserTaskAssignmentListener>();

            string assignee = task.Assignee;
            if (string.IsNullOrWhiteSpace(assignee))
            {
                return;
            }

            IUserInfo user;
            if (task.HasVariableLocal(assignee))
            {
                user = task.GetVariableLocal<IUserInfo>(assignee);
                if (user is object && string.Equals(user.Id, assignee, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }
            }

            IUserServiceProxy userService = ProcessEngineServiceProvider.Resolve<IUserServiceProxy>();

            user = userService.GetUser(assignee)
                .ConfigureAwait(false)
                .GetAwaiter()
                .GetResult();
            if (user is null)
            {
                logger.LogError($"找不到执行人{assignee}");
            }
            else
            {
                user.TenantId = task.TenantId;
                task.AssigneeUser = user.FullName;
            }

            task.SetVariableLocal($"{assignee}", user);
        }

        public void OnEvent(IActivitiEvent @event)
        {
            if (@event is IActivitiEntityEvent entity
                && entity.Entity is ITaskEntity task
                && task.Execution.CurrentFlowElement is UserTask)
            {
                Notify(task as IDelegateTask);
            }
        }
    }
}
