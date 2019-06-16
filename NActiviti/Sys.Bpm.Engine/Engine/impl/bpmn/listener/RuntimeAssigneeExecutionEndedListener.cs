﻿///////////////////////////////////////////////////////////
//  GetBookmarkRuleProvider.cs
//  Implementation of the Class GetBookmarkRuleProvider
//  Generated by Enterprise Architect
//  Created on:      30-1月-2019 8:32:00
//  Original author: 张楠
///////////////////////////////////////////////////////////
///
using Newtonsoft.Json.Linq;
using Sys.Workflow.bpmn.constants;
using Sys.Workflow.bpmn.model;
using Sys.Workflow.engine.@delegate;
using Sys.Workflow.engine.impl.persistence.entity;
using System.Collections.Generic;
using System.Linq;

namespace Sys.Workflow.engine.impl.bpmn.listener
{
    /// <summary>
    /// 运行时分配节点运行人员,监听节点的Excution End事件,在流程结束时删除动态添加的人员
    /// </summary>
    public class RuntimeAssigneeExecutionEndedListener : IExecutionListener
    {
        /// <summary>
        /// 侦听任务执行完成时接收通知处理
        /// </summary>
        /// <param name="execution"></param>
        public void Notify(IExecutionEntity execution)
        {
            UserTask task = execution.CurrentFlowElement as UserTask;

            if (task.ExtensionElements.TryGetValue(BpmnXMLConstants.ELEMENT_EXTENSIONS_PROPERTY, out IList<ExtensionElement> exts))
            {
                if (bool.TryParse(exts.GetAttributeValue(BpmnXMLConstants.ACTIITI_RUNTIME_ASSIGNEE), out bool result) == false || result == false)
                {
                    return;
                }

                var variable = execution.GetVariableInstance(BpmnXMLConstants.RUNTIME_ASSIGNEE_USER_VARIABLE_NAME);

                RuntimeAssigneeUser user = JToken.FromObject(variable.Value).ToObject<RuntimeAssigneeUser>();

                if ((user?.Users?.Count()).GetValueOrDefault() == 0)
                {
                    return;
                }

                string collection = task.LoopCharacteristics.GetCollectionVarName();
                string elemVariable = task.LoopCharacteristics.ElementVariable;

                execution.RemoveVariableLocal(collection);
                execution.RemoveVariableLocal(elemVariable);
            }
        }
    }
}
