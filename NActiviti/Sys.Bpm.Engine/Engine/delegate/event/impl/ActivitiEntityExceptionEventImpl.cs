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
namespace org.activiti.engine.@delegate.@event.impl
{

    /// <summary>
    /// Base class for all <seealso cref="IActivitiEvent"/> implementations, represents an exception occurred, related to an entity.
    /// 
    /// 
    /// </summary>
    public class ActivitiEntityExceptionEventImpl : ActivitiEventImpl, IActivitiEntityEvent, IActivitiExceptionEvent
    {

        protected internal object entity;
        protected internal Exception cause;

        public ActivitiEntityExceptionEventImpl(object entity, ActivitiEventType type, Exception cause) : base(type)
        {
            if (entity == null)
            {
                throw new ActivitiIllegalArgumentException("Entity cannot be null.");
            }
            this.entity = entity;
            this.cause = cause;
        }

        public virtual object Entity
        {
            get
            {
                return entity;
            }
        }

        public virtual Exception Cause
        {
            get
            {
                return cause;
            }
        }
    }

}