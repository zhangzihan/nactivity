using Sys.Workflow.engine.@delegate;
using Sys.Workflow.engine.impl.context;
using Sys.Workflow.engine.impl.persistence.entity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace Sys.Workflow.engine.impl.bpmn.helper
{
    static class ExecutionDebugLogger
    {
        public static void WriteDebugLog(this IExecutionEntity execution)
        {
            string doc = execution.CurrentFlowElement.Documentation;

            if (string.IsNullOrWhiteSpace(doc) == false && Context.ProcessEngineConfiguration.EnableVerboseExecutionTreeLogging)
            {
                string root = AppDomain.CurrentDomain.BaseDirectory;
                string file = Path.Combine(new string[] { root, "task_logs", $"{DateTime.Now.ToString("yyyyMMdd")}.txt" });
                Directory.CreateDirectory(Path.GetDirectoryName(file));

                IExpression expression = Context.ProcessEngineConfiguration.ExpressionManager.CreateExpression(doc);

                doc = expression.GetValue(execution)?.ToString();

                File.AppendAllText(file, $"{(File.Exists(file) ? "\r\n" : "")}Task '{execution.ActivityId}' debug_logger {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}:{doc}");
            }
        }
    }
}
