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

            IUserInfo user = task.GetVariableLocal<IUserInfo>($"{assignee}");
            if (!(user is null) && user.Id == assignee)
            {
                return;
            }

            IUserServiceProxy userService = ProcessEngineServiceProvider.Resolve<IUserServiceProxy>();

            user = AsyncHelper.RunSync(() => userService.GetUser(assignee));
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
