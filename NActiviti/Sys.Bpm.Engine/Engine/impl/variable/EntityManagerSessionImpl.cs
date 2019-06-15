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

namespace org.activiti.engine.impl.variable
{
    using org.activiti.engine.impl.cfg;
    using org.activiti.engine.impl.context;
    using org.activiti.engine.impl.interceptor;
    using Sys.Data;
    using System;
    using System.Data.Common;

    /// 
    /// 
    public class EntityManagerSessionImpl : IEntityManagerSession
    {
        private readonly IEntityManagerFactory entityManagerFactory;
        private IEntityManager entityManager;
        private readonly bool handleTransactions;
        private readonly bool closeEntityManager;

        public EntityManagerSessionImpl(IEntityManagerFactory entityManagerFactory, IEntityManager entityManager, bool handleTransactions, bool closeEntityManager) : this(entityManagerFactory, handleTransactions, closeEntityManager)
        {
            this.entityManager = entityManager;
        }

        public EntityManagerSessionImpl(IEntityManagerFactory entityManagerFactory, bool handleTransactions, bool closeEntityManager)
        {
            this.entityManagerFactory = entityManagerFactory;
            this.handleTransactions = handleTransactions;
            this.closeEntityManager = closeEntityManager;
        }

        public virtual void Flush()
        {
            if (entityManager != null && !handleTransactions)
            {
                try
                {
                    entityManager.Flush();
                }
                catch (InvalidOperationException ise)
                {
                    throw new ActivitiException("Error while flushing EntityManager, illegal state", ise);
                }
                //catch (Exception tre)
                //{
                //    throw new ActivitiException("Cannot flush EntityManager, an active transaction is required", tre);
                //}
                catch (Exception pe)
                {
                    throw new ActivitiException("Error while flushing EntityManager: " + pe.Message, pe);
                }
            }
        }

        public virtual void Close()
        {
            if (closeEntityManager && entityManager != null && !entityManager.Open)
            {
                try
                {
                    entityManager.Close();
                }
                catch (InvalidOperationException ise)
                {
                    throw new ActivitiException("Error while closing EntityManager, may have already been closed or it is container-managed", ise);
                }
            }
        }

        public virtual IEntityManager EntityManager
        {
            get
            {
                if (entityManager == null)
                {
                    entityManager = EntityManagerFactory.CreateEntityManager();

                    if (handleTransactions)
                    {
                        // Add transaction listeners, if transactions should be handled
                        ITransactionListener jpaTransactionCommitListener = new TransactionListenerAnonymousInnerClass(this);

                        ITransactionListener jpaTransactionRollbackListener = new TransactionListenerAnonymousInnerClass2(this);

                        ITransactionContext transactionContext = Context.TransactionContext;
                        transactionContext.AddTransactionListener(TransactionState.COMMITTED, jpaTransactionCommitListener);
                        transactionContext.AddTransactionListener(TransactionState.ROLLED_BACK, jpaTransactionRollbackListener);

                        // Also, start a transaction, if one isn't started already
                        //if (!TransactionActive)
                        //{
                        //    entityManager.Transaction..begin();
                        //}
                    }
                }

                return entityManager;
            }
        }

        private class TransactionListenerAnonymousInnerClass : ITransactionListener
        {
            private readonly EntityManagerSessionImpl outerInstance;

            public TransactionListenerAnonymousInnerClass(EntityManagerSessionImpl outerInstance)
            {
                this.outerInstance = outerInstance;
            }

            public virtual void Execute(ICommandContext commandContext)
            {
                outerInstance.entityManager.Transaction.Commit();

                //if (outerInstance.TransactionActive)
                //{
                //    outerInstance.entityManager.Transaction.commit();
                //}
            }
        }

        private class TransactionListenerAnonymousInnerClass2 : ITransactionListener
        {
            private readonly EntityManagerSessionImpl outerInstance;

            public TransactionListenerAnonymousInnerClass2(EntityManagerSessionImpl outerInstance)
            {
                this.outerInstance = outerInstance;
            }

            public virtual void Execute(ICommandContext commandContext)
            {
                outerInstance.entityManager.Transaction.Rollback();
                //if (outerInstance.TransactionActive)
                //{
                //    outerInstance.entityManager.Transaction.rollback();
                //}
            }
        }

        private IEntityManagerFactory EntityManagerFactory
        {
            get
            {
                return entityManagerFactory;
            }
        }
    }

}