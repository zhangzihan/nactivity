///////////////////////////////////////////////////////////
//  ExecutionUtils.cs
//  Implementation of the Class ExecutionUtils
//  Created on:      1-2月-2019 8:32:00
//  Original author: 张楠

using Newtonsoft.Json.Linq;
using Sys.Workflow.Bpmn.Models;
using Sys.Workflow.Engine.Impl.Persistence.Entity;

namespace Sys.Workflow.Engine.Impl.Util
{
    /// <summary>
    /// 任务执行工具类
    /// </summary>
    public static class ExecutionUtils
    {
        /// <summary>
        /// 循环获取流程实例变量
        /// </summary>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="execution">流程实例</param>
        /// <param name="variableName">变量名</param>
        /// <returns></returns>
        internal static T GetLoopVariable<T>(this IExecutionEntity execution, string variableName)
        {
            object value = execution.GetVariableLocal(variableName);
            IExecutionEntity parent = execution.Parent;
            while (value is null && parent is object)
            {
                value = parent.GetLoopVariable<T>(variableName);
                parent = parent.Parent;
            }
            return value is null ? default : JToken.FromObject(value).ToObject<T>();
        }

        /// <summary>
        /// 设置流程实例变量
        /// </summary>
        /// <param name="execution"></param>
        /// <param name="variableName"></param>
        /// <param name="value"></param>
        internal static void SetLoopVariable(this IExecutionEntity execution, string variableName, object value)
        {
            execution.SetVariableLocal(variableName, value);
        }

        internal static bool TryGetMultiInstance(this IExecutionEntity execution, out IExecutionEntity miRoot)
        {
            miRoot = execution.FindMultiInstanceParentExecution();

            return miRoot is object;
        }

        internal static IExecutionEntity FindMultiInstanceParentExecution(this IExecutionEntity execution)
        {
            IExecutionEntity multiInstanceExecution = null;
            IExecutionEntity parentExecution = execution.Parent;
            if (parentExecution is object && parentExecution.CurrentFlowElement is not null)
            {
                FlowElement flowElement = parentExecution.CurrentFlowElement;
                if (flowElement is Activity activity)
                {
                    if (activity.LoopCharacteristics is not null)
                    {
                        multiInstanceExecution = parentExecution;
                    }
                }

                if (multiInstanceExecution is null)
                {
                    IExecutionEntity potentialMultiInstanceExecution = FindMultiInstanceParentExecution(parentExecution);
                    if (potentialMultiInstanceExecution is object)
                    {
                        multiInstanceExecution = potentialMultiInstanceExecution;
                    }
                }
            }

            return multiInstanceExecution;
        }
        internal static IExecutionEntity FindRootParent(this IExecutionEntity execution)
        {
            if (execution is null)
            {
                return null;
            }
            IExecutionEntity parentExecution = execution.Parent;
            if (parentExecution is object && parentExecution.CurrentFlowElement is not null)
            {
                return FindRootParent(parentExecution);
            }

            return parentExecution;
        }
    }
}
