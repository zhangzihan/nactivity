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
namespace org.activiti.engine.impl.persistence.entity
{

	using org.activiti.bpmn.model;

	/// 
	public interface IEventSubscriptionEntityManager : IEntityManager<IEventSubscriptionEntity>
	{

	  /* Create entity */

	  IMessageEventSubscriptionEntity createMessageEventSubscription();

	  ISignalEventSubscriptionEntity createSignalEventSubscription();

	  ICompensateEventSubscriptionEntity createCompensateEventSubscription();


	  /* Create and insert */

	  ISignalEventSubscriptionEntity insertSignalEvent(string signalName, Signal signal, IExecutionEntity execution);

	  IMessageEventSubscriptionEntity insertMessageEvent(string messageName, IExecutionEntity execution);

	  ICompensateEventSubscriptionEntity insertCompensationEvent(IExecutionEntity execution, string activityId);


	  /* Update */

	  void updateEventSubscriptionTenantId(string oldTenantId, string newTenantId);


	  /* Delete */

	  void deleteEventSubscriptionsForProcessDefinition(string processDefinitionId);


	  /* Event receival */

	  void eventReceived(IEventSubscriptionEntity eventSubscriptionEntity, object payload, bool processASync);


	  /* Find (generic) */

	  IList<IEventSubscriptionEntity> findEventSubscriptionsByName(string type, string eventName, string tenantId);

	  IList<IEventSubscriptionEntity> findEventSubscriptionsByNameAndExecution(string type, string eventName, string executionId);

	  IList<IEventSubscriptionEntity> findEventSubscriptionsByExecution(string executionId);

	  IList<IEventSubscriptionEntity> findEventSubscriptionsByExecutionAndType(string executionId, string type);

	  IList<IEventSubscriptionEntity> findEventSubscriptionsByProcessInstanceAndActivityId(string processInstanceId, string activityId, string type);

	  IList<IEventSubscriptionEntity> findEventSubscriptionsByTypeAndProcessDefinitionId(string type, string processDefinitionId, string tenantId);

	  IList<IEventSubscriptionEntity> findEventSubscriptionsByQueryCriteria(EventSubscriptionQueryImpl eventSubscriptionQueryImpl, Page page);

	  long findEventSubscriptionCountByQueryCriteria(EventSubscriptionQueryImpl eventSubscriptionQueryImpl);


	  /* Find (signal) */

	  IList<ISignalEventSubscriptionEntity> findSignalEventSubscriptionsByEventName(string eventName, string tenantId);

	  IList<ISignalEventSubscriptionEntity> findSignalEventSubscriptionsByProcessInstanceAndEventName(string processInstanceId, string eventName);

	  IList<ISignalEventSubscriptionEntity> findSignalEventSubscriptionsByNameAndExecution(string name, string executionId);


	  /* Find (message) */

	  IMessageEventSubscriptionEntity findMessageStartEventSubscriptionByName(string messageName, string tenantId);

	  IList<IMessageEventSubscriptionEntity> findMessageEventSubscriptionsByProcessInstanceAndEventName(string processInstanceId, string eventName);


	  /* Find (compensation) */

	  IList<ICompensateEventSubscriptionEntity> findCompensateEventSubscriptionsByExecutionId(string executionId);

	  IList<ICompensateEventSubscriptionEntity> findCompensateEventSubscriptionsByExecutionIdAndActivityId(string executionId, string activityId);

	  IList<ICompensateEventSubscriptionEntity> findCompensateEventSubscriptionsByProcessInstanceIdAndActivityId(string processInstanceId, string activityId);


	}
}