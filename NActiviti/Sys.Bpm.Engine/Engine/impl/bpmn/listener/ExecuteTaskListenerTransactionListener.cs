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
namespace Sys.Workflow.Engine.Impl.Bpmn.Listeners
{
    using Sys.Workflow.Engine.Delegate;
    using Sys.Workflow.Engine.Impl.Cfg;
    using Sys.Workflow.Engine.Impl.Interceptor;

    /// <summary>
    /// A <seealso cref="ITransactionListener"/> that invokes an <seealso cref="IExecutionListener"/>.
    /// 
    /// 
    /// </summary>
    public class ExecuteTaskListenerTransactionListener : ITransactionListener
    {

        protected internal ITransactionDependentTaskListener listener;
        protected internal TransactionDependentTaskListenerExecutionScope scope;

        public ExecuteTaskListenerTransactionListener(ITransactionDependentTaskListener listener, TransactionDependentTaskListenerExecutionScope scope)
        {
            this.listener = listener;
            this.scope = scope;
        }

        public virtual void Execute(ICommandContext commandContext)
        {
            ICommandExecutor commandExecutor = commandContext.ProcessEngineConfiguration.CommandExecutor;
            CommandConfig commandConfig = new CommandConfig(false, TransactionPropagation.REQUIRES_NEW);
            commandExecutor.Execute(commandConfig, new CommandAnonymousInnerClass(this));
        }

        private class CommandAnonymousInnerClass : ICommand<object>
        {
            private readonly ExecuteTaskListenerTransactionListener outerInstance;

            public CommandAnonymousInnerClass(ExecuteTaskListenerTransactionListener outerInstance)
            {
                this.outerInstance = outerInstance;
            }

            public virtual object Execute(ICommandContext commandContext)
            {
                outerInstance.listener.Notify(outerInstance.scope.ProcessInstanceId, outerInstance.scope.ExecutionId, outerInstance.scope.Task, outerInstance.scope.ExecutionVariables, outerInstance.scope.CustomPropertiesMap);
                return commandContext.GetResult();
            }
        }
    }
}