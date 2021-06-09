/* Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
namespace Sys.Workflow.Engine.Impl.Interceptor
{
    using Microsoft.Extensions.Logging;
    using Sys.Workflow.Engine.Impl.Agenda;
    using Sys.Workflow.Engine.Impl.Contexts;
    using Sys.Workflow;
    using System;

    /// 
    public class CommandInvoker : AbstractCommandInterceptor
    {
        private static readonly ILogger<CommandInvoker> log = ProcessEngineServiceProvider.LoggerService<CommandInvoker>();

        public override T Execute<T>(CommandConfig config, ICommand<T> command)
        {
            object value;

            ICommandContext commandContext = Context.CommandContext;

            try
            {
                // Execute the command.
                // This will produce operations that will be put on the agenda.
                commandContext.Agenda.PlanOperation(new RunableOperation(() =>
                {
                    T result = command.Execute(commandContext);

                    commandContext.SetResult(result);
                }));

                // Run loop for agenda
                ExecuteOperations(commandContext);

                // At the end, call the execution tree change listeners.
                // TODO: optimization: only do this when the tree has actually changed (ie check dbSqlSession).
                if (commandContext.HasInvolvedExecutions())
                {
                    Context.Agenda.PlanExecuteInactiveBehaviorsOperation();
                    ExecuteOperations(commandContext);
                }

                value = commandContext.GetResult();

                return (T)value;
            }
            catch (InvalidCastException ex)
            {
                // TODO: commandcontext和command不在一个线程导致异常
                if (log.IsEnabled(LogLevel.Debug))
                {
                    log.LogDebug($"【线程异常】:{ex.Message}");
                }
                return default;
                //throw ex;
            }
            catch (Exception ex)
            {
                log.LogError($"{(command is null ? "command is null " : command.GetType().FullName)} execute failed!");
                throw ex;
            }
        }

        private void ExecuteOperations(ICommandContext commandContext)
        {
            while (!commandContext.Agenda.Empty)
            {
                AbstractOperation runnable = commandContext.Agenda.NextOperation();
                if (runnable is object)
                {
                    ExecuteOperation(runnable);
                }
            }
        }

        public virtual void ExecuteOperation(AbstractOperation runnable)
        {
            if (runnable is AbstractOperation operation)
            {
                // Execute the operation if the operation has no execution (i.e. it's an operation not working on a process instance)
                // or the operation has an execution and it is not ended
                if (operation.Execution is null || !operation.Execution.Ended)
                {
                    //if (log.IsEnabled(LogLevel.Debug))
                    //{
                    //    log.LogDebug($"Executing operation {operation.GetType()} ");
                    //}

                    runnable.Run();
                }
            }
            else
            {
                runnable.Run();
            }
        }

        public override ICommandInterceptor Next
        {
            get
            {
                return null;
            }
            set
            {
                throw new System.NotSupportedException("CommandInvoker must be the last interceptor in the chain");
            }
        }


    }

}