using Sys.Workflow.Bpmn.Models;
using Sys.Workflow.Engine.Delegate.Events;
using Sys.Workflow.Engine.Delegate.Events.Impl;
using Sys.Workflow.Engine.Impl.Bpmn.Helper;
using Sys.Workflow.Engine.Impl.Contexts;
using Sys.Workflow.Engine.Impl.Interceptor;
using Sys.Workflow.Engine.Impl.Persistence.Entity;
using System;
using System.Collections.Generic;
using System.Text;
using Sys.Workflow.Engine.Impl.Util;
using System.Text.RegularExpressions;
using Sys.Workflow.Engine.Impl.Bpmn.Behavior;
using System.Linq;

namespace Sys.Workflow.Engine.Impl.Bpmn.Helper
{
    /// <summary>
    /// 自定义任务完成时的事件监听处理,主要用来处理转派和追加任务的完成
    /// </summary>
    public class CustomTaskCompletedEventListener : BaseDelegateEventListener
    {
        public override bool FailOnException => true;

        public override void OnEvent(IActivitiEvent @event)
        {
            if (@event is CustomTaskCompletedEntityEventImpl taskEvent)
            {
                ITaskEntity taskEntity = taskEvent.Entity as ITaskEntity;

                ICommandContext commandContext = Context.CommandContext;

                IExecutionEntity execution = taskEntity.Execution;

                UserTask userTask = execution.CurrentFlowElement as UserTask;

                if (userTask.HasMultiInstanceLoopCharacteristics() &&
                    (taskEntity.IsAppend.GetValueOrDefault(false) ||
                    taskEntity.IsTransfer.GetValueOrDefault(false)))
                {
                    IExecutionEntity parent = execution.FindMultiInstanceParentExecution();

                    var collection = userTask.LoopCharacteristics.GetCollectionVarName();

                    IList<string> users = parent.GetLoopVariable<IList<string>>(collection);

                    string usr = users.FirstOrDefault(x => x.Equals(taskEntity.Assignee, StringComparison.OrdinalIgnoreCase));

                    if (usr != null)
                    {
                        users.Remove(usr);
                        parent.SetLoopVariable(collection, users);
                    }
                }
            }
        }
    }
}
