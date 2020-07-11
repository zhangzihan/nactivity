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

namespace Sys.Workflow.Engine.Impl.Bpmn.Listeners
{
    class UserTaskAssignmentListener : ITaskListener
    {
        private static readonly ILogger<UserTaskAssignmentListener> logger = ProcessEngineServiceProvider.LoggerService<UserTaskAssignmentListener>();

        public void Notify(IDelegateTask task)
        {
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
            if (user == null)
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
    }
}
