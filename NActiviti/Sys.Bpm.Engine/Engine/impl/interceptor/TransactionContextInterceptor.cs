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
    using Sys.Workflow.Engine.Impl.Cfg;
    using Sys.Workflow.Engine.Impl.Contexts;
    using System;

    /// 
    public class TransactionContextInterceptor : AbstractCommandInterceptor
    {

        protected internal ITransactionContextFactory transactionContextFactory;

        public TransactionContextInterceptor()
        {
        }

        public TransactionContextInterceptor(ITransactionContextFactory transactionContextFactory)
        {
            this.transactionContextFactory = transactionContextFactory;
        }

        public override T Execute<T>(CommandConfig config, ICommand<T> command)
        {

            ICommandContext commandContext = Context.CommandContext;
            // Storing it in a variable, to reference later (it can change during command execution)
            bool isReused = commandContext.Reused;

            try
            {
                if (transactionContextFactory != null && !isReused)
                {
                    ITransactionContext transactionContext = transactionContextFactory.OpenTransactionContext(commandContext);
                    Context.TransactionContext = transactionContext;
                    commandContext.AddCloseListener(new TransactionCommandContextCloseListener(transactionContext));
                }

                return next.Execute(config, command);
            }
            finally
            {
                if (transactionContextFactory != null && !isReused)
                {
                    Context.RemoveTransactionContext();
                }
            }
        }

        public virtual ITransactionContextFactory TransactionContextFactory
        {
            get
            {
                return transactionContextFactory;
            }
            set
            {
                this.transactionContextFactory = value;
            }
        }
    }
}