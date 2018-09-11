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
    /// Base class for all <seealso cref="IActivitiEvent"/> implementations, related to entities.
    /// 
    /// 
    /// </summary>
    public class ActivitiEntityEventImpl : ActivitiEventImpl, IActivitiEntityEvent
    {

        protected internal object entity;

        public ActivitiEntityEventImpl(object entity, ActivitiEventType type) : base(type)
        {
            if (entity == null)
            {
                throw new ActivitiIllegalArgumentException("Entity cannot be null.");
            }
            this.entity = entity;
        }

        public virtual object Entity
        {
            get
            {
                return entity;
            }
        }
    }

}