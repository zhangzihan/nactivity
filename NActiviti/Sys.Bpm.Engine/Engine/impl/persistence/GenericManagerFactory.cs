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

namespace Sys.Workflow.engine.impl.persistence
{
    using Sys.Workflow.engine.impl.interceptor;

    /// 
    /// 
    public class GenericManagerFactory : ISessionFactory
    {

        protected internal Type typeClass;
        protected internal Type implementationClass;

        public GenericManagerFactory(Type typeClass, Type implementationClass)
        {
            this.typeClass = typeClass;
            this.implementationClass = implementationClass;
        }

        public GenericManagerFactory(Type implementationClass) : this(implementationClass, implementationClass)
        {
        }

        public virtual Type SessionType
        {
            get
            {
                return typeClass;
            }
        }

        public virtual ISession OpenSession(ICommandContext commandContext)
        {
            try
            {
                return Activator.CreateInstance(implementationClass) as ISession;
            }
            catch (Exception e)
            {
                throw new ActivitiException("couldn't instantiate " + implementationClass.FullName + ": " + e.Message, e);
            }
        }
    }

}