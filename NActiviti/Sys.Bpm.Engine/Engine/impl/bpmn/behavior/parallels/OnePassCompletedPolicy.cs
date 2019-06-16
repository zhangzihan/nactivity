﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;
using Sys.Workflow.bpmn.constants;
using Sys.Workflow.bpmn.model;
using Sys.Workflow.engine.history;
using Sys.Workflow.engine.impl.context;
using Sys.Workflow.engine.impl.interceptor;
using Sys.Workflow.engine.impl.persistence.entity;
using Sys.Workflow.engine.impl.persistence.entity.data;
using Sys.Workflow.engine.task;
using Sys.Workflow.services.api.commands;
using Sys.Workflow;

namespace Sys.Workflow.engine.impl.bpmn.behavior
{
    class OnePassCompletedPolicy : DefaultMultiInstanceCompletedPolicy
    {
        private static readonly ILogger log = ProcessEngineServiceProvider.LoggerService<OnePassCompletedPolicy>();

        private string completeConditionVarName = null;
        public override string CompleteConditionVarName
        {
            get
            {
                if (string.IsNullOrWhiteSpace(completeConditionVarName))
                {
                    completeConditionVarName = WorkflowVariable.GLOBAL_APPROVALED_VARIABLE;
                }

                return completeConditionVarName;
            }
            set { completeConditionVarName = value; }
        }


        /// <summary>
        /// 完成条件判断，根据signalData传递的数据判断当前提交人是否同意，如果有一个提交同意，该任务就结束.同时记录流程变量为true.
        /// </summary>
        /// <param name="parent">当前任务节点的运行实例</param>
        /// <param name="multiInstanceActivity">多任务活动行为</param>
        /// <param name="signalData">提交的变量数据</param>
        /// <returns></returns>
        public override bool CompletionConditionSatisfied(IExecutionEntity parent, MultiInstanceActivityBehavior multiInstanceActivity, object signalData)
        {
            object passed = parent.GetVariable(CompleteConditionVarName, true);
            if (passed is null || (bool.TryParse(passed.ToString(), out var p) && p))
            {
                return true;
            }

            var tf = (signalData as Dictionary<string, object>)[CompleteConditionVarName];
            bool.TryParse(tf?.ToString(), out bool approvaled);

            if (approvaled)
            {
                parent.SetVariable(CompleteConditionVarName, true);
            }
            else
            {
                parent.SetVariable(CompleteConditionVarName, false);
            }

            return approvaled;
        }
    }
}
