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
namespace Sys.Workflow.Engine.Delegate.Events.Impl
{
    using Sys.Workflow.Bpmn.Models;
    using Sys.Workflow.Engine.Impl.Contexts;
    using Sys.Workflow.Engine.Impl.Interceptor;
    using Sys.Workflow.Engine.Impl.Util;
    using Sys.Workflow.Engine.Repository;

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


        public virtual void AddEventListener(IActivitiEventListener listenerToAdd)
        {
            eventSupport.AddEventListener(listenerToAdd);
        }

        public virtual void AddEventListener(IActivitiEventListener listenerToAdd, params ActivitiEventType[] types)
        {
            eventSupport.AddEventListener(listenerToAdd, types);
        }

        public virtual void RemoveEventListener(IActivitiEventListener listenerToRemove)
        {
            eventSupport.RemoveEventListener(listenerToRemove);
        }

        public virtual void DispatchEvent(IActivitiEvent @event)
        {
            if (enabled)
            {
                eventSupport.DispatchEvent(@event);
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
            if (commandContext is object)
            {
                BpmnModel bpmnModel = ExtractBpmnModelFromEvent(@event);
                if (bpmnModel is not null)
                {
                    ((ActivitiEventSupport)bpmnModel.EventSupport).DispatchEvent(@event);
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
        protected internal virtual BpmnModel ExtractBpmnModelFromEvent(IActivitiEvent @event)
        {
            BpmnModel result = null;

            if (@event.ProcessDefinitionId is not null)
            {
                IProcessDefinition processDefinition = ProcessDefinitionUtil.GetProcessDefinition(@event.ProcessDefinitionId, true);
                if (processDefinition is object)
                {
                    result = Context.ProcessEngineConfiguration.DeploymentManager.ResolveProcessDefinition(processDefinition).BpmnModel;
                }
            }

            return result;
        }

    }

}