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
using Sys.Workflow.Transactions;
using System;

namespace Sys.Workflow.Engine.Impl.Interceptor
{
    /// <summary>
    /// We cannot perform a retry if we are called in an existing transaction. In that case, the transaction will be marked "rollback-only" after the first ActivitiOptimisticLockingException.
    /// 
    /// 
    /// </summary>
    public class JtaRetryInterceptor : RetryInterceptor
    {
        protected internal readonly TransactionManager transactionManager;

        public JtaRetryInterceptor(TransactionManager transactionManager)
        {
            this.transactionManager = transactionManager;
        }

        public override T execute<T>(CommandConfig config, ICommand<T> command)
        {
            if (calledInsideTransaction())
            {
                //log.trace("Called inside transaction, skipping the retry interceptor.");
                return next.execute(config, command);
            }
            else
            {
                return base.execute(config, command);
            }
        }

        protected internal virtual bool calledInsideTransaction()
        {
            try
            {
                return transactionManager.Status != Status.STATUS_NO_TRANSACTION;
            }
            catch (Exception e)
            {
                throw new ActivitiException("Could not determine the current status of the transaction manager: " + e.Message, e);
            }
        }

    }

}