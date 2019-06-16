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
namespace Sys.Workflow.engine.impl.persistence.entity
{

    using Sys.Workflow.bpmn.model;

    /// 
    public interface IEventSubscriptionEntityManager : IEntityManager<IEventSubscriptionEntity>
    {

        /* Create entity */

        IMessageEventSubscriptionEntity CreateMessageEventSubscription();

        ISignalEventSubscriptionEntity CreateSignalEventSubscription();

        ICompensateEventSubscriptionEntity CreateCompensateEventSubscription();


        /* Create and insert */

        ISignalEventSubscriptionEntity InsertSignalEvent(string signalName, Signal signal, IExecutionEntity execution);

        IMessageEventSubscriptionEntity InsertMessageEvent(string messageName, IExecutionEntity execution);

        ICompensateEventSubscriptionEntity InsertCompensationEvent(IExecutionEntity execution, string activityId);


        /* Update */

        void UpdateEventSubscriptionTenantId(string oldTenantId, string newTenantId);


        /* Delete */

        void DeleteEventSubscriptionsForProcessDefinition(string processDefinitionId);


        /* Event receival */

        void EventReceived(IEventSubscriptionEntity eventSubscriptionEntity, object payload, bool processASync);


        /* Find (generic) */

        IList<IEventSubscriptionEntity> FindEventSubscriptionsByName(string type, string eventName, string tenantId);

        IList<IEventSubscriptionEntity> FindEventSubscriptionsByNameAndExecution(string type, string eventName, string executionId);

        IList<IEventSubscriptionEntity> FindEventSubscriptionsByExecution(string executionId);

        IList<IEventSubscriptionEntity> FindEventSubscriptionsByExecutionAndType(string executionId, string type);

        IList<IEventSubscriptionEntity> FindEventSubscriptionsByProcessInstanceAndActivityId(string processInstanceId, string activityId, string type);

        IList<IEventSubscriptionEntity> FindEventSubscriptionsByTypeAndProcessDefinitionId(string type, string processDefinitionId, string tenantId);

        IList<IEventSubscriptionEntity> FindEventSubscriptionsByQueryCriteria(EventSubscriptionQueryImpl eventSubscriptionQueryImpl, Page page);

        long FindEventSubscriptionCountByQueryCriteria(EventSubscriptionQueryImpl eventSubscriptionQueryImpl);


        /* Find (signal) */

        IList<ISignalEventSubscriptionEntity> FindSignalEventSubscriptionsByEventName(string eventName, string tenantId);

        IList<ISignalEventSubscriptionEntity> FindSignalEventSubscriptionsByProcessInstanceAndEventName(string processInstanceId, string eventName);

        IList<ISignalEventSubscriptionEntity> FindSignalEventSubscriptionsByNameAndExecution(string name, string executionId);


        /* Find (message) */

        IMessageEventSubscriptionEntity FindMessageStartEventSubscriptionByName(string messageName, string tenantId);

        IList<IMessageEventSubscriptionEntity> FindMessageEventSubscriptionsByProcessInstanceAndEventName(string processInstanceId, string eventName);


        /* Find (compensation) */

        IList<ICompensateEventSubscriptionEntity> FindCompensateEventSubscriptionsByExecutionId(string executionId);

        IList<ICompensateEventSubscriptionEntity> FindCompensateEventSubscriptionsByExecutionIdAndActivityId(string executionId, string activityId);

        IList<ICompensateEventSubscriptionEntity> FindCompensateEventSubscriptionsByProcessInstanceIdAndActivityId(string processInstanceId, string activityId);
    }
}