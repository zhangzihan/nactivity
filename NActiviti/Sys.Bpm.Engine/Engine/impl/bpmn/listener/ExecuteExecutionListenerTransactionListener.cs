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
namespace org.activiti.engine.impl.bpmn.listener
{
    using org.activiti.engine.@delegate;
    using org.activiti.engine.impl.cfg;
    using org.activiti.engine.impl.interceptor;

    /// <summary>
    /// A <seealso cref="ITransactionListener"/> that invokes an <seealso cref="IExecutionListener"/>.
    /// 
    /// 
    /// </summary>
    public class ExecuteExecutionListenerTransactionListener : ITransactionListener
    {

        protected internal ITransactionDependentExecutionListener listener;
        protected internal TransactionDependentExecutionListenerExecutionScope scope;

        public ExecuteExecutionListenerTransactionListener(ITransactionDependentExecutionListener listener, TransactionDependentExecutionListenerExecutionScope scope)
        {
            this.listener = listener;
            this.scope = scope;
        }

        public virtual void execute(ICommandContext commandContext)
        {
            ICommandExecutor commandExecutor = commandContext.ProcessEngineConfiguration.CommandExecutor;
            CommandConfig commandConfig = new CommandConfig(false, TransactionPropagation.REQUIRES_NEW);
            commandExecutor.execute(commandConfig, new CommandAnonymousInnerClass(this, commandContext));
        }

        private class CommandAnonymousInnerClass : ICommand<object>
        {
            private readonly ExecuteExecutionListenerTransactionListener outerInstance;

            private ICommandContext commandContext;

            public CommandAnonymousInnerClass(ExecuteExecutionListenerTransactionListener outerInstance, ICommandContext commandContext)
            {
                this.outerInstance = outerInstance;
                this.commandContext = commandContext;
            }

            public virtual object execute(ICommandContext commandContext)
            {
                outerInstance.listener.notify(outerInstance.scope.ProcessInstanceId, outerInstance.scope.ExecutionId, outerInstance.scope.FlowElement, outerInstance.scope.ExecutionVariables, outerInstance.scope.CustomPropertiesMap);
                return null;
            }
        }

    }

}