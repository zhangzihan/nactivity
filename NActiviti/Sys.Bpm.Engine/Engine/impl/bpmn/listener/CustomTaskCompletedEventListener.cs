using Sys.Workflow.bpmn.model;
using Sys.Workflow.engine.@delegate.@event;
using Sys.Workflow.engine.@delegate.@event.impl;
using Sys.Workflow.engine.impl.bpmn.helper;
using Sys.Workflow.engine.impl.context;
using Sys.Workflow.engine.impl.interceptor;
using Sys.Workflow.engine.impl.persistence.entity;
using System;
using System.Collections.Generic;
using System.Text;
using Sys.Workflow.engine.impl.util;
using System.Text.RegularExpressions;
using Sys.Workflow.engine.impl.bpmn.behavior;
using System.Linq;

namespace Sys.Workflow.engine.impl.bpmn.helper
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
