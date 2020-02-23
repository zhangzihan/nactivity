using Microsoft.Extensions.Logging;
using Sys.Workflow.Engine.Delegate;
using Sys.Workflow.Engine.Impl.Contexts;
using Sys.Workflow.Engine.Impl.Persistence.Entity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace Sys.Workflow.Engine.Impl.Bpmn.Helper
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

                string log = $"{(File.Exists(file) ? "\r\n" : "")}Task '{execution.ActivityId}' debug_logger {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}:{doc}";

                File.AppendAllText(file, log);

                ILogger logger = (ProcessEngineServiceProvider.ServiceProvider.GetService(typeof(ILoggerFactory)) as ILoggerFactory).CreateLogger(execution.GetType());

                if (logger.IsEnabled(LogLevel.Information))
                {
                    logger.LogInformation(log);
                }
            }
        }
    }
}
