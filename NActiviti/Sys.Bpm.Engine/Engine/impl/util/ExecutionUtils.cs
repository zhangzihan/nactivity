///////////////////////////////////////////////////////////
//  ExecutionUtils.cs
//  Implementation of the Class ExecutionUtils
//  Created on:      1-2月-2019 8:32:00
//  Original author: 张楠

using Newtonsoft.Json.Linq;
using org.activiti.bpmn.model;
using org.activiti.engine.impl.persistence.entity;

namespace org.activiti.engine.impl.util
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
            while (value == null && parent != null)
            {
                value = parent.GetLoopVariable<T>(variableName);
                parent = parent.Parent;
            }
            return value == null ? default : JToken.FromObject(value).ToObject<T>();
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

            return miRoot != null;
        }

        internal static IExecutionEntity FindMultiInstanceParentExecution(this IExecutionEntity execution)
        {
            IExecutionEntity multiInstanceExecution = null;
            IExecutionEntity parentExecution = execution.Parent;
            if (parentExecution != null && parentExecution.CurrentFlowElement != null)
            {
                FlowElement flowElement = parentExecution.CurrentFlowElement;
                if (flowElement is Activity activity)
                {
                    if (activity.LoopCharacteristics != null)
                    {
                        multiInstanceExecution = parentExecution;
                    }
                }

                if (multiInstanceExecution == null)
                {
                    IExecutionEntity potentialMultiInstanceExecution = FindMultiInstanceParentExecution(parentExecution);
                    if (potentialMultiInstanceExecution != null)
                    {
                        multiInstanceExecution = potentialMultiInstanceExecution;
                    }
                }
            }

            return multiInstanceExecution;
        }
        internal static IExecutionEntity FindRootParent(this IExecutionEntity execution)
        {
            if (execution == null)
            {
                return null;
            }
            IExecutionEntity parentExecution = execution.Parent;
            if (parentExecution != null && parentExecution.CurrentFlowElement != null)
            {
                return FindRootParent(parentExecution);
            }

            return parentExecution;
        }
    }
}
