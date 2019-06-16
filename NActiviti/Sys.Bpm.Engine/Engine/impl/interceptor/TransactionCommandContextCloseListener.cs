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

    /// 
    public class TransactionCommandContextCloseListener : ICommandContextCloseListener
    {

        protected internal ITransactionContext transactionContext;

        public TransactionCommandContextCloseListener(ITransactionContext transactionContext)
        {
            this.transactionContext = transactionContext;
        }

        public virtual void Closing(ICommandContext commandContext)
        {

        }

        public virtual void AfterSessionsFlush(ICommandContext commandContext)
        {
            transactionContext.Commit();
        }

        public virtual void Closed(ICommandContext commandContext)
        {

        }

        public virtual void CloseFailure(ICommandContext commandContext)
        {
            transactionContext.Rollback();
        }

    }

}