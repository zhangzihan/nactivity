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
namespace org.activiti.engine.impl.cfg.standalone
{

    using org.activiti.engine.impl.db;
    using org.activiti.engine.impl.interceptor;

    /// 
    public class StandaloneMybatisTransactionContext : ITransactionContext
    {
        protected internal ICommandContext commandContext;
        protected internal IDictionary<TransactionState, IList<ITransactionListener>> stateTransactionListeners;

        public StandaloneMybatisTransactionContext(ICommandContext commandContext)
        {
            this.commandContext = commandContext;
        }

        public virtual void addTransactionListener(TransactionState transactionState, ITransactionListener transactionListener)
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

        public virtual void commit()
        {

            //log.debug("firing event committing...");
            fireTransactionEvent(TransactionState.COMMITTING, false);

            //log.debug("committing the ibatis sql session...");
            DbSqlSession.commit();
            //log.debug("firing event committed...");
            fireTransactionEvent(TransactionState.COMMITTED, true);

        }

        /// <summary>
        /// Fires the event for the provided <seealso cref="TransactionState"/>.
        /// </summary>
        /// <param name="transactionState"> The <seealso cref="TransactionState"/> for which the listeners will be called. </param>
        /// <param name="executeInNewContext"> If true, the listeners will be called in a new command context.
        ///                            This is needed for example when firing the <seealso cref="TransactionState#COMMITTED"/>
        ///                            event: the transacation is already committed and executing logic in the same
        ///                            context could lead to strange behaviour (for example doing a <seealso cref="SqlSession#update(String)"/>
        ///                            would actually roll back the update (as the MyBatis context is already committed
        ///                            and the internal flags have not been correctly set). </param>
        protected internal virtual void fireTransactionEvent(TransactionState transactionState, bool executeInNewContext)
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
                commandExecutor.execute(commandConfig, new CommandAnonymousInnerClass(this, transactionListeners));
            }
            else
            {
                executeTransactionListeners(transactionListeners, commandContext);
            }

        }

        private class CommandAnonymousInnerClass : ICommand<object>
        {
            private readonly StandaloneMybatisTransactionContext outerInstance;

            private IList<ITransactionListener> transactionListeners;

            public CommandAnonymousInnerClass(StandaloneMybatisTransactionContext outerInstance, IList<ITransactionListener> transactionListeners)
            {
                this.outerInstance = outerInstance;
                this.transactionListeners = transactionListeners;
            }

            public virtual object execute(ICommandContext commandContext)
            {
                outerInstance.executeTransactionListeners(transactionListeners, commandContext);
                return commandContext.GetResult();
            }
        }

        protected internal virtual void executeTransactionListeners(IList<ITransactionListener> transactionListeners, ICommandContext commandContext)
        {
            foreach (ITransactionListener transactionListener in transactionListeners)
            {
                transactionListener.execute(commandContext);
            }
        }

        protected internal virtual DbSqlSession DbSqlSession
        {
            get
            {
                return commandContext.DbSqlSession;
            }
        }

        public virtual void rollback()
        {
            try
            {
                try
                {
                    //log.debug("firing event rolling back...");
                    fireTransactionEvent(TransactionState.ROLLINGBACK, false);

                }
                catch (Exception exception)
                {
                    //log.info("Exception during transaction: {}", exception.Message);
                    commandContext.exception(exception);
                }
                finally
                {
                    //log.debug("rolling back ibatis sql session...");
                    DbSqlSession.rollback();
                }

            }
            catch (Exception exception)
            {
                //log.info("Exception during transaction: {}", exception.Message);
                commandContext.exception(exception);

            }
            finally
            {
                //log.debug("firing event rolled back...");
                fireTransactionEvent(TransactionState.ROLLED_BACK, true);
            }
        }
    }

}