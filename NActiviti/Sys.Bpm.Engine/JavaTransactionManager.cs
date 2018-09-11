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

using System;
using System.Runtime.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using org.activiti.engine.impl.cfg.jta;
using Sys;

namespace javax.transaction
{
    public class TransactionManager
    {
        private static  ILogger<TransactionManager> log = ProcessEngineServiceProvider.LoggerService<TransactionManager>();

        public TransactionManager()
        {
            this.Transaction = new Transaction();
        }

        public Transaction Transaction { get; set; }
        public Status Status { get; internal set; }

        internal void begin()
        {
            this.Transaction.begin();
        }

        internal Transaction suspend()
        {
            return this.Transaction.suspend();
        }

        internal void resume(Transaction tx)
        {
            log.LogWarning("mock java TransactionManager.resume!");
        }

        internal void commit()
        {
            log.LogWarning("mock java TransactionManager.commit!");
        }

        internal void rollback()
        {
            log.LogWarning("mock java TransactionManager.rollback!");
        }

        internal void setRollbackOnly()
        {
            log.LogWarning("mock java TransactionManager.setRollbackOnly!");
        }
    }

    public interface ITransactionFactory
    {

    }

    public class JdbcTransactionFactory : ITransactionFactory
    {

    }

    public class ManagedTransactionFactory : ITransactionFactory
    {

    }

    public class Transaction
    {
        private static  ILogger<TransactionManager> log = ProcessEngineServiceProvider.LoggerService<TransactionManager>();

        public Status Status { get; set; }
        public bool Active { get; internal set; }

        internal void setRollbackOnly()
        {
            log.LogWarning("mock java Transaction.setRollbackOnly!");
        }

        internal void registerSynchronization(JtaTransactionContext.TransactionStateSynchronization transactionStateSynchronization)
        {
            log.LogWarning("mock java Transaction.registerSynchronization!");
        }

        internal void commit()
        {
            log.LogWarning("mock java Transaction.commit!");
        }

        internal void rollback()
        {
            log.LogWarning("mock java Transaction.rollback!");
        }

        internal Transaction suspend()
        {
            log.LogWarning("mock java Transaction.suspend!");

            return this;
        }

        internal void begin()
        {
            log.LogWarning("mock java Transaction.begin!");
        }
    }

    public enum Status
    {
        STATUS_ACTIVE = 0,
        STATUS_MARKED_ROLLBACK = 1,
        STATUS_PREPARED = 2,
        STATUS_COMMITTED = 3,
        STATUS_ROLLEDBACK = 4,
        STATUS_UNKNOWN = 5,
        STATUS_NO_TRANSACTION = 6,
        STATUS_PREPARING = 7,
        STATUS_COMMITTING = 8,
        STATUS_ROLLING_BACK = 9,
    }

    [Serializable]
    internal class RollbackException : Exception
    {
        public RollbackException()
        {
        }

        public RollbackException(string message) : base(message)
        {
        }

        public RollbackException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected RollbackException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}