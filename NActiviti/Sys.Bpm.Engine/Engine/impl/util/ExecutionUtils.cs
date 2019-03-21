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
            object value = execution.getVariableLocal(variableName);
            IExecutionEntity parent = execution.Parent;
            while (value == null && parent != null)
            {
                value = parent.GetLoopVariable<T>(variableName);
                parent = parent.Parent;
            }
            return value == null ? default(T) : JToken.FromObject(value).ToObject<T>();
        }

        /// <summary>
        /// 设置流程实例变量
        /// </summary>
        /// <param name="execution"></param>
        /// <param name="variableName"></param>
        /// <param name="value"></param>
        internal static void SetLoopVariable(this IExecutionEntity execution, string variableName, object value)
        {
            execution.setVariableLocal(variableName, value);
        }

        internal static bool TryGetMultiInstance(this IExecutionEntity execution, out IExecutionEntity miRoot)
        {
            miRoot = execution.findMultiInstanceParentExecution();

            return miRoot != null;
        }

        internal static IExecutionEntity findMultiInstanceParentExecution(this IExecutionEntity execution)
        {
            IExecutionEntity multiInstanceExecution = null;
            IExecutionEntity parentExecution = execution.Parent;
            if (parentExecution != null && parentExecution.CurrentFlowElement != null)
            {
                FlowElement flowElement = parentExecution.CurrentFlowElement;
                if (flowElement is Activity)
                {
                    Activity activity = (Activity)flowElement;
                    if (activity.LoopCharacteristics != null)
                    {
                        multiInstanceExecution = parentExecution;
                    }
                }

                if (multiInstanceExecution == null)
                {
                    IExecutionEntity potentialMultiInstanceExecution = findMultiInstanceParentExecution(parentExecution);
                    if (potentialMultiInstanceExecution != null)
                    {
                        multiInstanceExecution = potentialMultiInstanceExecution;
                    }
                }
            }

            return multiInstanceExecution;
        }
    }
}
