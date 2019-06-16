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
namespace Sys.Workflow.Engine.Impl.Cfg
{
    using Sys.Workflow.Transactions;
    using Sys.Workflow.Engine.Impl.Cfg.Jta;
    using Sys.Workflow.Engine.Impl.Interceptor;

    /// 
    public class JtaProcessEngineConfiguration : ProcessEngineConfigurationImpl
    {

        protected internal TransactionManager transactionManager;

        public JtaProcessEngineConfiguration()
        {
            this.transactionsExternallyManaged = true;
        }

        public override ICommandInterceptor createTransactionInterceptor()
        {
            if (transactionManager == null)
            {
                throw new ActivitiException("transactionManager is required property for JtaProcessEngineConfiguration, use " + typeof(StandaloneProcessEngineConfiguration).FullName + " otherwise");
            }

            return new JtaTransactionInterceptor(transactionManager);
        }

        public override void initTransactionContextFactory()
        {
            if (transactionContextFactory == null)
            {
                transactionContextFactory = new JtaTransactionContextFactory(transactionManager);
            }
        }

        public override ProcessEngineConfiguration getProcessEngineConfiguration()
        {
            return this;
        }

        public virtual TransactionManager TransactionManager
        {
            get
            {
                return transactionManager;
            }
            set
            {
                this.transactionManager = value;
            }
        }

    }

}