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

namespace Sys.Workflow.engine.impl.cfg.jta
{
    using javax.transaction;
    using Sys.Workflow.engine.impl.context;
    using Sys.Workflow.engine.impl.interceptor;
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;

    /// 
    public class JtaTransactionContext : ITransactionContext
    {

        protected internal readonly TransactionManager transactionManager;

        public JtaTransactionContext(TransactionManager transactionManager)
        {
            this.transactionManager = transactionManager;
        }

        public virtual void Commit()
        {
            // managed transaction, ignore
        }

        public virtual void Rollback()
        {
            // managed transaction, mark rollback-only if not done so already.
            try
            {
                Transaction transaction = Transaction;
                Status status = transaction.Status;
                if (status != Status.STATUS_NO_TRANSACTION && status != Status.STATUS_ROLLEDBACK)
                {
                    transaction.SetRollbackOnly();
                }
            }
            catch (System.InvalidOperationException)
            {
                throw new ActivitiException("Unexpected IllegalStateException while marking transaction rollback only");
            }
            catch (Exception)
            {
                throw new ActivitiException("SystemException while marking transaction rollback only");
            }
        }

        protected internal virtual Transaction Transaction
        {
            get
            {
                try
                {
                    return transactionManager.Transaction;
                }
                catch (SystemException e)
                {
                    throw new ActivitiException("SystemException while getting transaction ", e);
                }
            }
        }

        public virtual void AddTransactionListener(TransactionState transactionState, ITransactionListener transactionListener)
        {
            Transaction transaction = Transaction;
            ICommandContext commandContext = Context.CommandContext;
            try
            {
                transaction.RegisterSynchronization(new TransactionStateSynchronization(transactionState, transactionListener, commandContext));
            }
            catch (System.InvalidOperationException e)
            {
                throw new ActivitiException("IllegalStateException while registering synchronization ", e);
            }
            catch (RollbackException e)
            {
                throw new ActivitiException("RollbackException while registering synchronization ", e);
            }
            catch (SystemException e)
            {
                throw new ActivitiException("SystemException while registering synchronization ", e);
            }
        }

        public class TransactionStateSynchronization
        {

            protected internal readonly ITransactionListener transactionListener;
            protected internal readonly TransactionState transactionState;
            internal readonly ICommandContext commandContext;

            public TransactionStateSynchronization(TransactionState transactionState, ITransactionListener transactionListener, ICommandContext commandContext)
            {
                this.transactionState = transactionState;
                this.transactionListener = transactionListener;
                this.commandContext = commandContext;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public virtual void BeforeCompletion()
            {
                if (TransactionState.COMMITTING.Equals(transactionState) || TransactionState.ROLLINGBACK.Equals(transactionState))
                {
                    transactionListener.Execute(commandContext);
                }
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            public virtual void AfterCompletion(int status)
            {
                if (Status.STATUS_ROLLEDBACK == (Status)status && TransactionState.ROLLED_BACK.Equals(transactionState))
                {
                    transactionListener.Execute(commandContext);
                }
                else if (Status.STATUS_COMMITTED == (Status)status && TransactionState.COMMITTED.Equals(transactionState))
                {
                    transactionListener.Execute(commandContext);
                }
            }
        }
    }
}