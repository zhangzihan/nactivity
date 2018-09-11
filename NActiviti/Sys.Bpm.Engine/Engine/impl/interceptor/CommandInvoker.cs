using System.Threading;

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
namespace org.activiti.engine.impl.interceptor
{
    using Microsoft.Extensions.Logging;
    using org.activiti.engine.impl.agenda;
    using org.activiti.engine.impl.context;
    using Sys;
    using System;
    using System.Threading.Tasks;

    /// 
    public class CommandInvoker : AbstractCommandInterceptor
    {
        private static readonly ILogger<CommandInvoker> log = ProcessEngineServiceProvider.LoggerService<CommandInvoker>();

        public override T execute<T>(CommandConfig config, ICommand<T> command)
        {
            ICommandContext commandContext = Context.CommandContext;

            // Execute the command.
            // This will produce operations that will be put on the agenda.
            commandContext.Agenda.planOperation(new RunableOperation(() =>
            {
                commandContext.Result = command.execute(commandContext);
            }));

            // Run loop for agenda
            executeOperations(commandContext);

            // At the end, call the execution tree change listeners.
            // TODO: optimization: only do this when the tree has actually changed (ie check dbSqlSession).
            if (commandContext.hasInvolvedExecutions())
            {
                Context.Agenda.planExecuteInactiveBehaviorsOperation();
                executeOperations(commandContext);
            }

            var value = (T)commandContext.Result;

            return value;
        }

        protected internal virtual void executeOperations(ICommandContext commandContext)
        {
            while (!commandContext.Agenda.Empty)
            {
                AbstractOperation runnable = commandContext.Agenda.NextOperation;
                if (runnable != null)
                {
                    executeOperation(runnable);
                }
            }
        }

        public virtual void executeOperation(AbstractOperation runnable)
        {
            if (runnable is AbstractOperation operation)
            {
                // Execute the operation if the operation has no execution (i.e. it's an operation not working on a process instance)
                // or the operation has an execution and it is not ended
                if (operation.Execution == null || !operation.Execution.Ended)
                {
                    if (log.IsEnabled(LogLevel.Debug))
                    {
                        log.LogDebug($"Executing operation {operation.GetType()} ");
                    }

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