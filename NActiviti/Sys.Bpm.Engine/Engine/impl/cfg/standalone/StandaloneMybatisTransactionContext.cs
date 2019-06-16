using System;
using System.Collections.Generic;

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
namespace Sys.Workflow.engine.impl.cfg.standalone
{
    using Microsoft.Extensions.Logging;
    using Sys.Workflow.engine.impl.db;
    using Sys.Workflow.engine.impl.interceptor;
    using Sys.Workflow;

    /// 
    public class StandaloneMybatisTransactionContext : ITransactionContext
    {
        /// <summary>
        /// 
        /// </summary>
        protected internal ICommandContext commandContext;

        /// <summary>
        /// 
        /// </summary>
        protected internal IDictionary<TransactionState, IList<ITransactionListener>> stateTransactionListeners;

        private static readonly ILogger log = ProcessEngineServiceProvider.LoggerService<StandaloneMybatisTransactionContext>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="commandContext"></param>
        public StandaloneMybatisTransactionContext(ICommandContext commandContext)
        {
            this.commandContext = commandContext;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="transactionState"></param>
        /// <param name="transactionListener"></param>
        public virtual void AddTransactionListener(TransactionState transactionState, ITransactionListener transactionListener)
        {
            if (stateTransactionListeners == null)
            {
                stateTransactionListeners = new Dictionary<TransactionState, IList<ITransactionListener>>();
            }
            IList<ITransactionListener> transactionListeners = stateTransactionListeners[transactionState];
            if (transactionListeners == null)
            {
                transactionListeners = new List<ITransactionListener>();
                stateTransactionListeners[transactionState] = transactionListeners;
            }
            transactionListeners.Add(transactionListener);
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void Commit()
        {
            log.LogDebug("firing event committing...");
            FireTransactionEvent(TransactionState.COMMITTING, false);

            log.LogDebug("committing the ibatis sql session...");
            DbSqlSession.Commit();
            log.LogDebug("firing event committed...");
            FireTransactionEvent(TransactionState.COMMITTED, true);
        }

        /// <summary>
        /// Fires the event for the provided <seealso cref="TransactionState"/>.
        /// </summary>
        /// <param name="transactionState"> The <seealso cref="TransactionState"/> for which the listeners will be called. </param>
        /// <param name="executeInNewContext"> If true, the listeners will be called in a new command context.
        ///                            This is needed for example when firing the <seealso cref="TransactionState"/>
        ///                            event: the transacation is already committed and executing logic in the same
        ///                            context could lead to strange behaviour (for example doing a <seealso cref="DbSqlSession"/>
        ///                            would actually roll back the update (as the MyBatis context is already committed
        ///                            and the internal flags have not been correctly set). </param>
        protected internal virtual void FireTransactionEvent(TransactionState transactionState, bool executeInNewContext)
        {
            if (stateTransactionListeners == null)
            {
                return;
            }

            IList<ITransactionListener> transactionListeners = stateTransactionListeners[transactionState];
            if (transactionListeners == null)
            {
                return;
            }

            if (executeInNewContext)
            {
                ICommandExecutor commandExecutor = commandContext.ProcessEngineConfiguration.CommandExecutor;
                CommandConfig commandConfig = new CommandConfig(false, TransactionPropagation.REQUIRES_NEW);
                commandExecutor.Execute(commandConfig, new CommandAnonymousInnerClass(this, transactionListeners));
            }
            else
            {
                ExecuteTransactionListeners(transactionListeners, commandContext);
            }

        }

        private class CommandAnonymousInnerClass : ICommand<object>
        {
            private readonly StandaloneMybatisTransactionContext outerInstance;

            private readonly IList<ITransactionListener> transactionListeners;

            public CommandAnonymousInnerClass(StandaloneMybatisTransactionContext outerInstance, IList<ITransactionListener> transactionListeners)
            {
                this.outerInstance = outerInstance;
                this.transactionListeners = transactionListeners;
            }

            public virtual object Execute(ICommandContext commandContext)
            {
                outerInstance.ExecuteTransactionListeners(transactionListeners, commandContext);
                return commandContext.GetResult();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="transactionListeners"></param>
        /// <param name="commandContext"></param>
        protected internal virtual void ExecuteTransactionListeners(IList<ITransactionListener> transactionListeners, ICommandContext commandContext)
        {
            foreach (ITransactionListener transactionListener in transactionListeners)
            {
                transactionListener.Execute(commandContext);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected internal virtual DbSqlSession DbSqlSession
        {
            get
            {
                return commandContext.DbSqlSession;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void Rollback()
        {
            try
            {
                try
                {
                    log.LogDebug("firing event rolling back...");
                    FireTransactionEvent(TransactionState.ROLLINGBACK, false);

                }
                catch (Exception exception)
                {
                    log.LogInformation("Exception during transaction: {}", exception.Message);
                    commandContext.SetException(exception);
                }
                finally
                {
                    log.LogDebug("rolling back ibatis sql session...");
                    DbSqlSession.Rollback();
                }

            }
            catch (Exception exception)
            {
                log.LogInformation("Exception during transaction: {}", exception.Message);
                commandContext.SetException(exception);

            }
            finally
            {
                log.LogDebug("firing event rolled back...");
                FireTransactionEvent(TransactionState.ROLLED_BACK, true);
            }
        }
    }
}