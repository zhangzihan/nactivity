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

namespace Sys.Workflow.Engine.Impl.Variable
{
    using Sys.Workflow.Engine.Impl.Interceptor;
    using Sys.Data;

    /// 
    public class EntityManagerSessionFactory : ISessionFactory
    {

        protected internal IEntityManagerFactory entityManagerFactory;
        protected internal bool handleTransactions;
        protected internal bool closeEntityManager;

        public EntityManagerSessionFactory(object entityManagerFactory, bool handleTransactions, bool closeEntityManager)
        {
            if (entityManagerFactory == null)
            {
                throw new ActivitiIllegalArgumentException("entityManagerFactory is null");
            }
            if (!(entityManagerFactory is EntityManagerFactory))
            {
                throw new ActivitiIllegalArgumentException("EntityManagerFactory must implement 'javax.persistence.EntityManagerFactory'");
            }

            this.entityManagerFactory = (IEntityManagerFactory)entityManagerFactory;
            this.handleTransactions = handleTransactions;
            this.closeEntityManager = closeEntityManager;
        }

        public virtual Type SessionType
        {
            get
            {
                return typeof(IEntityManagerSession);
            }
        }

        public virtual ISession OpenSession(ICommandContext commandContext)
        {
            return new EntityManagerSessionImpl(entityManagerFactory, handleTransactions, closeEntityManager);
        }

        public virtual IEntityManagerFactory EntityManagerFactory
        {
            get
            {
                return entityManagerFactory;
            }
        }
    }

}