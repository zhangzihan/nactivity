using System;

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
    using javax.transaction;
    using org.activiti.engine.impl.cfg;

    /// 
    public class JtaTransactionInterceptor : AbstractCommandInterceptor
    {
        private readonly TransactionManager transactionManager;

        public JtaTransactionInterceptor(TransactionManager transactionManager)
        {
            this.transactionManager = transactionManager;
        }

        public override T execute<T>(CommandConfig config, ICommand<T> command)
        {
            //LOGGER.debug("Running command with propagation {}", config.TransactionPropagation);

            if (config.TransactionPropagation == TransactionPropagation.NOT_SUPPORTED)
            {
                return next.execute(config, command);
            }

            bool requiresNew = config.TransactionPropagation == TransactionPropagation.REQUIRES_NEW;
            Transaction oldTx = null;
            try
            {
                bool existing = Existing;
                bool isNew = !existing || requiresNew;
                if (existing && requiresNew)
                {
                    oldTx = doSuspend();
                }
                if (isNew)
                {
                    doBegin();
                }
                T result;
                try
                {
                    result = next.execute(config, command);
                }
                catch (Exception ex)
                {
                    doRollback(isNew, ex);
                    throw new Exception("TransactionCallback threw undeclared checked exception", ex);
                }
                if (isNew)
                {
                    doCommit();
                }
                return result;
            }
            finally
            {
                doResume(oldTx);
            }
        }

        private void doBegin()
        {
            try
            {
                transactionManager.begin();
            }
            catch (NotSupportedException e)
            {
                throw new TransactionException("Unable to begin transaction", e);
            }
            catch (SystemException e)
            {
                throw new TransactionException("Unable to begin transaction", e);
            }
        }

        private bool Existing
        {
            get
            {
                try
                {
                    return transactionManager.Status != Status.STATUS_NO_TRANSACTION;
                }
                catch (SystemException e)
                {
                    throw new TransactionException("Unable to retrieve transaction status", e);
                }
            }
        }

        private Transaction doSuspend()
        {
            try
            {
                return transactionManager.suspend();
            }
            catch (SystemException e)
            {
                throw new TransactionException("Unable to suspend transaction", e);
            }
        }

        private void doResume(Transaction tx)
        {
            if (tx != null)
            {
                try
                {
                    transactionManager.resume(tx);
                }
                catch (SystemException e)
                {
                    throw new TransactionException("Unable to resume transaction", e);
                }
            }
        }

        private void doCommit()
        {
            try
            {
                transactionManager.commit();
            }
            catch (TransactionException e)
            {
                throw new TransactionException("Unable to commit transaction", e);
            }
            catch (Exception e)
            {
                doRollback(true, e);
                throw e;
            }
        }

        private void doRollback(bool isNew, Exception originalException)
        {
            Exception rollbackEx = null;
            try
            {
                if (isNew)
                {
                    transactionManager.rollback();
                }
                else
                {
                    transactionManager.setRollbackOnly();
                }
            }
            catch (SystemException e)
            {
                //LOGGER.debug("Error when rolling back transaction", e);
            }
            catch (Exception e)
            {
                rollbackEx = e;
                throw e;
            }
            finally
            {
                if (rollbackEx != null && originalException != null)
                {
                    //LOGGER.error("Error when rolling back transaction, original exception was:", originalException);
                }
            }
        }

        private class TransactionException : Exception
        {

            internal const long serialVersionUID = 1L;

            internal TransactionException()
            {
            }

            internal TransactionException(string s) : base(s)
            {
            }

            internal TransactionException(string s, Exception throwable) : base(s, throwable)
            {
            }

            internal TransactionException(Exception throwable) : base(throwable.Message, throwable)
            {
            }
        }
    }

}