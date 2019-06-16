using System.Collections.Generic;

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
    using Sys.Workflow.Engine.Impl.Contexts;
    using Sys.Workflow.Engine.Impl.Interceptor;
    using Sys.Workflow.Engine.Impl.Persistence.Entity;
    using Sys.Workflow.Engine.Repository;

    /// <summary>
    /// An <seealso cref="IActivitiEventListener"/> that throws a signal event when an event is dispatched to it.
    /// 
    /// 
    /// 
    /// </summary>
    public class SignalThrowingEventListener : BaseDelegateEventListener
    {

        protected internal string signalName;
        protected internal bool processInstanceScope = true;

        public override void OnEvent(IActivitiEvent @event)
        {
            if (IsValidEvent(@event))
            {

                if (@event.ProcessInstanceId is null && processInstanceScope)
                {
                    throw new ActivitiIllegalArgumentException("Cannot throw process-instance scoped signal, since the dispatched event is not part of an ongoing process instance");
                }

                ICommandContext commandContext = Context.CommandContext;
                IEventSubscriptionEntityManager eventSubscriptionEntityManager = commandContext.EventSubscriptionEntityManager;
                IList<ISignalEventSubscriptionEntity> subscriptionEntities;
                if (processInstanceScope)
                {
                    subscriptionEntities = eventSubscriptionEntityManager.FindSignalEventSubscriptionsByProcessInstanceAndEventName(@event.ProcessInstanceId, signalName);
                }
                else
                {
                    string tenantId = null;
                    if (!(@event.ProcessDefinitionId is null))
                    {
                        IProcessDefinition processDefinition = commandContext.ProcessEngineConfiguration.DeploymentManager.FindDeployedProcessDefinitionById(@event.ProcessDefinitionId);
                        tenantId = processDefinition.TenantId;
                    }
                    subscriptionEntities = eventSubscriptionEntityManager.FindSignalEventSubscriptionsByEventName(signalName, tenantId);
                }

                foreach (ISignalEventSubscriptionEntity signalEventSubscriptionEntity in subscriptionEntities)
                {
                    eventSubscriptionEntityManager.EventReceived(signalEventSubscriptionEntity, null, false);
                }
            }
        }

        public virtual string SignalName
        {
            set
            {
                this.signalName = value;
            }
        }

        public virtual bool ProcessInstanceScope
        {
            set
            {
                this.processInstanceScope = value;
            }
        }

        public override bool FailOnException
        {
            get
            {
                return true;
            }
        }
    }

}