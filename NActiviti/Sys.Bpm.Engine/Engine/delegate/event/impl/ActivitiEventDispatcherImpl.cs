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
    using org.activiti.bpmn.model;
    using org.activiti.engine.impl.context;
    using org.activiti.engine.impl.interceptor;
    using org.activiti.engine.impl.util;
    using org.activiti.engine.repository;

    /// <summary>
    /// Class capable of dispatching events.
    /// 
    /// 
    /// </summary>
    public class ActivitiEventDispatcherImpl : IActivitiEventDispatcher
    {

        protected internal ActivitiEventSupport eventSupport;
        protected internal bool enabled = true;

        public ActivitiEventDispatcherImpl()
        {
            eventSupport = new ActivitiEventSupport();
        }

        public virtual bool Enabled
        {
            set
            {
                this.enabled = value;
            }
            get
            {
                return enabled;
            }
        }


        public virtual void addEventListener(IActivitiEventListener listenerToAdd)
        {
            eventSupport.addEventListener(listenerToAdd);
        }

        public virtual void addEventListener(IActivitiEventListener listenerToAdd, params ActivitiEventType[] types)
        {
            eventSupport.addEventListener(listenerToAdd, types);
        }

        public virtual void removeEventListener(IActivitiEventListener listenerToRemove)
        {
            eventSupport.removeEventListener(listenerToRemove);
        }

        public virtual void dispatchEvent(IActivitiEvent @event)
        {
            if (enabled)
            {
                eventSupport.dispatchEvent(@event);
            }

            if (@event.Type == ActivitiEventType.ENTITY_DELETED && @event is IActivitiEntityEvent)
            {
                IActivitiEntityEvent entityEvent = (IActivitiEntityEvent)@event;
                if (entityEvent.Entity is IProcessDefinition)
                {
                    // process definition deleted event doesn't need to be dispatched to event listeners
                    return;
                }
            }

            // Try getting hold of the Process definition, based on the process definition key, if a context is active
            ICommandContext commandContext = Context.CommandContext;
            if (commandContext != null)
            {
                BpmnModel bpmnModel = extractBpmnModelFromEvent(@event);
                if (bpmnModel != null)
                {
                    ((ActivitiEventSupport)bpmnModel.EventSupport).dispatchEvent(@event);
                }
            }

        }

        /// <summary>
        /// In case no process-context is active, this method attempts to extract a process-definition based on the event. In case it's an event related to an entity, this can be deducted by inspecting the
        /// entity, without additional queries to the database.
        /// 
        /// If not an entity-related event, the process-definition will be retrieved based on the processDefinitionId (if filled in). This requires an additional query to the database in case not already
        /// cached. However, queries will only occur when the definition is not yet in the cache, which is very unlikely to happen, unless evicted.
        /// </summary>
        /// <param name="event">
        /// @return </param>
        protected internal virtual BpmnModel extractBpmnModelFromEvent(IActivitiEvent @event)
        {
            BpmnModel result = null;

            if (result == null && !ReferenceEquals(@event.ProcessDefinitionId, null))
            {
                IProcessDefinition processDefinition = ProcessDefinitionUtil.getProcessDefinition(@event.ProcessDefinitionId, true);
                if (processDefinition != null)
                {
                    result = Context.ProcessEngineConfiguration.DeploymentManager.resolveProcessDefinition(processDefinition).BpmnModel;
                }
            }

            return result;
        }

    }

}