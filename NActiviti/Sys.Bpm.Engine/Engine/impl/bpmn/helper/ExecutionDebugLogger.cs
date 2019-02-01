using org.activiti.engine.@delegate;
using org.activiti.engine.impl.context;
using org.activiti.engine.impl.persistence.entity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace org.activiti.engine.impl.bpmn.helper
{
    static class ExecutionDebugLogger
    {
        public static void WriteDebugLog(this IExecutionEntity execution)
        {
            string doc = execution.CurrentFlowElement.Documentation;

            if (string.IsNullOrWhiteSpace(doc) == false && Context.ProcessEngineConfiguration.EnableVerboseExecutionTreeLogging)
            {
                string root = Path.GetDirectoryName(new Uri(typeof(ExecutionDebugLogger).Assembly.CodeBase).LocalPath);
                string file = Path.Combine(root, $"task_logs\\{DateTime.Now.ToString("yyyyMMdd")}.txt");
                Directory.CreateDirectory(Path.GetDirectoryName(file));

                IExpression expression = Context.ProcessEngineConfiguration.ExpressionManager.createExpression(doc);

                doc = expression.getValue(execution)?.ToString();

                File.AppendAllText(file, $"{(File.Exists(file) ? "\r\n" : "")}Task '{execution.ActivityId}' debug_logger {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}:{doc}");
            }
        }
    }
}
