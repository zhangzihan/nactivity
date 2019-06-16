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
namespace Sys.Workflow.engine.@delegate.@event
{
    /// <summary>
    /// Base event listener that can be used when implementing an <seealso cref="IActivitiEventListener"/> to get notified when an entity is created, updated, deleted or if another entity-related event occurs.
    /// 
    /// Override the <code>onXX(..)</code> methods to respond to entity changes accordingly.
    /// 
    /// 
    /// 
    /// </summary>
    public class BaseEntityEventListener : IActivitiEventListener
    {

        protected internal bool failOnException;
        protected internal Type entityClass;

        /// <summary>
        /// Create a new BaseEntityEventListener, notified when an event that targets any type of entity is received. Returning true when <seealso cref="#isFailOnException()"/> is called.
        /// </summary>
        public BaseEntityEventListener() : this(true, null)
        {
        }

        /// <summary>
        /// Create a new BaseEntityEventListener.
        /// </summary>
        /// <param name="failOnException">
        ///          return value for <seealso cref="#isFailOnException()"/>. </param>
        public BaseEntityEventListener(bool failOnException) : this(failOnException, null)
        {
        }

        public BaseEntityEventListener(bool failOnException, Type entityClass)
        {
            this.failOnException = failOnException;
            this.entityClass = entityClass;
        }

        public void OnEvent(IActivitiEvent @event)
        {
            if (IsValidEvent(@event))
            {
                // Check if this event
                if (@event.Type == ActivitiEventType.ENTITY_CREATED)
                {
                    OnCreate(@event);
                }
                else if (@event.Type == ActivitiEventType.ENTITY_INITIALIZED)
                {
                    OnInitialized(@event);
                }
                else if (@event.Type == ActivitiEventType.ENTITY_DELETED)
                {
                    OnDelete(@event);
                }
                else if (@event.Type == ActivitiEventType.ENTITY_UPDATED)
                {
                    OnUpdate(@event);
                }
                else
                {
                    // Entity-specific event
                    OnEntityEvent(@event);
                }
            }
        }

        public virtual bool FailOnException
        {
            get
            {
                return failOnException;
            }
        }

        /// <returns> true, if the event is an <seealso cref="IActivitiEntityEvent"/> and (if needed) the entityClass set in this instance, is assignable from the entity class in the event. </returns>
        protected internal virtual bool IsValidEvent(IActivitiEvent @event)
        {
            bool valid = false;
            if (@event is IActivitiEntityEvent)
            {
                if (entityClass == null)
                {
                    valid = true;
                }
                else
                {
                    valid = entityClass.IsAssignableFrom(((IActivitiEntityEvent)@event).Entity.GetType());
                }
            }
            return valid;
        }

        /// <summary>
        /// Called when an entity create event is received.
        /// </summary>
        protected internal virtual void OnCreate(IActivitiEvent @event)
        {
            // Default implementation is a NO-OP
        }

        /// <summary>
        /// Called when an entity initialized event is received.
        /// </summary>
        protected internal virtual void OnInitialized(IActivitiEvent @event)
        {
            // Default implementation is a NO-OP
        }

        /// <summary>
        /// Called when an entity delete event is received.
        /// </summary>
        protected internal virtual void OnDelete(IActivitiEvent @event)
        {
            // Default implementation is a NO-OP
        }

        /// <summary>
        /// Called when an entity update event is received.
        /// </summary>
        protected internal virtual void OnUpdate(IActivitiEvent @event)
        {
            // Default implementation is a NO-OP
        }

        /// <summary>
        /// Called when an event is received, which is not a create, an update or delete.
        /// </summary>
        protected internal virtual void OnEntityEvent(IActivitiEvent @event)
        {
            // Default implementation is a NO-OP
        }
    }
}