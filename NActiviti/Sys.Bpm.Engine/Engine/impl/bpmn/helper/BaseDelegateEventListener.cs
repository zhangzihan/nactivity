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
namespace Sys.Workflow.Engine.Impl.Bpmn.Helper
{
    using Sys.Workflow.Engine.Delegate.Events;

    /// <summary>
    /// Base implementation of a <seealso cref="IActivitiEventListener"/>, used when creating event-listeners that are part of a BPMN definition.
    /// 
    /// 
    /// </summary>
    public abstract class BaseDelegateEventListener : IActivitiEventListener
    {
        public abstract bool FailOnException { get; }
        public abstract void OnEvent(IActivitiEvent @event);

        protected internal Type entityClass;

        public virtual Type EntityClass
        {
            set
            {
                this.entityClass = value;
            }
        }

        protected internal virtual bool IsValidEvent(IActivitiEvent @event)
        {
            bool valid = false;
            if (entityClass is object)
            {
                if (@event is IActivitiEntityEvent)
                {
                    object entity = ((IActivitiEntityEvent)@event).Entity;
                    if (entity is object)
                    {
                        valid = entityClass.IsAssignableFrom(entity.GetType());
                    }
                }
            }
            else
            {
                // If no class is specified, all events are valid
                valid = true;
            }
            return valid;
        }
    }
}