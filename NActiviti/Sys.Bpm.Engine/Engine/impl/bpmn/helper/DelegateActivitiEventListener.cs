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
namespace org.activiti.engine.impl.bpmn.helper
{
    using org.activiti.engine.@delegate.@event;
    using org.activiti.engine.impl.util;

    /// <summary>
    /// An <seealso cref="IActivitiEventListener"/> implementation which uses a classname to create a delegate <seealso cref="IActivitiEventListener"/> instance to use for event notification. <br>
    /// <br>
    /// 
    /// In case an entityClass was passed in the constructor, only events that are <seealso cref="IActivitiEntityEvent"/>'s that target an entity of the given type, are dispatched to the delegate.
    /// 
    /// 
    /// </summary>
    public class DelegateActivitiEventListener : BaseDelegateEventListener
    {

        protected internal string className;
        protected internal IActivitiEventListener delegateInstance;
        protected internal bool failOnException = false;

        public DelegateActivitiEventListener(string className, Type entityClass)
        {
            this.className = className;
            EntityClass = entityClass;
        }

        public override void OnEvent(IActivitiEvent @event)
        {
            if (IsValidEvent(@event))
            {
                DelegateInstance.OnEvent(@event);
            }
        }

        public override bool FailOnException
        {
            get
            {
                if (delegateInstance != null)
                {
                    return delegateInstance.FailOnException;
                }
                return failOnException;
            }
        }

        protected internal virtual IActivitiEventListener DelegateInstance
        {
            get
            {
                if (delegateInstance == null)
                {
                    object instance = ReflectUtil.Instantiate(className);
                    if (instance is IActivitiEventListener)
                    {
                        delegateInstance = (IActivitiEventListener)instance;
                    }
                    else
                    {
                        // Force failing of the listener invocation, since the delegate
                        // cannot be created
                        failOnException = true;
                        throw new ActivitiIllegalArgumentException("Class " + className + " does not implement " + typeof(IActivitiEventListener).FullName);
                    }
                }
                return delegateInstance;
            }
        }
    }

}