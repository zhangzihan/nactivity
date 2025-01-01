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
    class UserTaskReleaseListener : ITaskListener, IActivitiEventListener
    {
        public bool FailOnException => true;

        public void Notify(IDelegateTask task)
        {
        }

        public void OnEvent(IActivitiEvent @event)
        {
            if (@event is IActivitiEntityEvent entity
                && entity.Entity is ITaskEntity task
                && task.GetExecution().CurrentFlowElement is UserTask)
            {
                Notify(task);
            }
        }
    }
}
