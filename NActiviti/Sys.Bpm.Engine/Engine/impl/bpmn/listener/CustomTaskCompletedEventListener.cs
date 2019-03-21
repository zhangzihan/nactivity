using org.activiti.bpmn.model;
using org.activiti.engine.@delegate.@event;
using org.activiti.engine.@delegate.@event.impl;
using org.activiti.engine.impl.bpmn.helper;
using org.activiti.engine.impl.context;
using org.activiti.engine.impl.interceptor;
using org.activiti.engine.impl.persistence.entity;
using System;
using System.Collections.Generic;
using System.Text;
using org.activiti.engine.impl.util;
using System.Text.RegularExpressions;
using org.activiti.engine.impl.bpmn.behavior;
using System.Linq;

namespace org.activiti.engine.impl.bpmn.helper
{
    /// <summary>
    /// 自定义任务完成时的事件监听处理,主要用来处理转派和追加任务的完成
    /// </summary>
    public class CustomTaskCompletedEventListener : BaseDelegateEventListener
    {
        public override bool FailOnException => true;

        public override void onEvent(IActivitiEvent @event)
        {
            if (@event is CustomTaskCompletedEntityEventImpl taskEvent)
            {
                ITaskEntity taskEntity = taskEvent.Entity as ITaskEntity;

                ICommandContext commandContext = Context.CommandContext;

                IExecutionEntity execution = taskEntity.Execution;

                UserTask userTask = execution.CurrentFlowElement as UserTask;

                if (userTask.hasMultiInstanceLoopCharacteristics() &&
                    (taskEntity.IsAppend.GetValueOrDefault(false) ||
                    taskEntity.IsTransfer.GetValueOrDefault(false)))
                {
                    IExecutionEntity parent = execution.findMultiInstanceParentExecution();

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
