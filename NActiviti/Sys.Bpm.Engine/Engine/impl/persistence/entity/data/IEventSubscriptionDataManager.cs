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
namespace Sys.Workflow.Engine.Impl.Persistence.Entity.Data
{


    /// 
    public interface IEventSubscriptionDataManager : IDataManager<IEventSubscriptionEntity>
    {
        IMessageEventSubscriptionEntity CreateMessageEventSubscription();

        ISignalEventSubscriptionEntity CreateSignalEventSubscription();

        ICompensateEventSubscriptionEntity CreateCompensateEventSubscription();

        long FindEventSubscriptionCountByQueryCriteria(IEventSubscriptionQuery eventSubscriptionQueryImpl);

        IList<IEventSubscriptionEntity> FindEventSubscriptionsByQueryCriteria(IEventSubscriptionQuery eventSubscriptionQueryImpl, Page page);

        IList<IMessageEventSubscriptionEntity> FindMessageEventSubscriptionsByProcessInstanceAndEventName(string processInstanceId, string eventName);

        IList<ISignalEventSubscriptionEntity> FindSignalEventSubscriptionsByEventName(string eventName, string tenantId);

        IList<ISignalEventSubscriptionEntity> FindSignalEventSubscriptionsByProcessInstanceAndEventName(string processInstanceId, string eventName);

        IList<ISignalEventSubscriptionEntity> FindSignalEventSubscriptionsByNameAndExecution(string name, string executionId);

        IList<ICompensateEventSubscriptionEntity> FindCompensateEventSubscriptionsByProcessInstanceId(string processInstanceId);

        IList<ICompensateEventSubscriptionEntity> FindCompensateEventSubscriptionsByExecutionId(string executionId);

        IList<ICompensateEventSubscriptionEntity> FindCompensateEventSubscriptionsByProcessInstanceAndActivityId(string processInstanceId, string activityId);

        IList<IEventSubscriptionEntity> FindEventSubscriptionsByExecutionAndType(string executionId, string type);

        IList<IEventSubscriptionEntity> FindEventSubscriptionsByProcessInstanceAndActivityId(string processInstanceId, string activityId, string type);

        IList<IEventSubscriptionEntity> FindEventSubscriptionsByExecution(string executionId);

        IList<IEventSubscriptionEntity> FindEventSubscriptionsByTypeAndProcessDefinitionId(string type, string processDefinitionId, string tenantId);

        IList<IEventSubscriptionEntity> FindEventSubscriptionsByName(string type, string eventName, string tenantId);

        IList<IEventSubscriptionEntity> FindEventSubscriptionsByNameAndExecution(string type, string eventName, string executionId);

        IMessageEventSubscriptionEntity FindMessageStartEventSubscriptionByName(string messageName, string tenantId);

        void UpdateEventSubscriptionTenantId(string oldTenantId, string newTenantId);

        void DeleteEventSubscriptionsForProcessDefinition(string processDefinitionId);
    }
}