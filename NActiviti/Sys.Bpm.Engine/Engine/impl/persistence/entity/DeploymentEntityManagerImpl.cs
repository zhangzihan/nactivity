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
    using org.activiti.engine.@delegate.@event;
    using org.activiti.engine.@delegate.@event.impl;
    using org.activiti.engine.impl.cfg;
    using org.activiti.engine.impl.jobexecutor;
    using org.activiti.engine.impl.persistence.entity.data;
    using org.activiti.engine.impl.util;
    using org.activiti.engine.repository;

    /// 
    /// 
    public class DeploymentEntityManagerImpl : AbstractEntityManager<IDeploymentEntity>, IDeploymentEntityManager
    {

        protected internal IDeploymentDataManager deploymentDataManager;

        public DeploymentEntityManagerImpl(ProcessEngineConfigurationImpl processEngineConfiguration, IDeploymentDataManager deploymentDataManager) : base(processEngineConfiguration)
        {
            this.deploymentDataManager = deploymentDataManager;
        }

        protected internal override IDataManager<IDeploymentEntity> DataManager
        {
            get
            {
                return deploymentDataManager;
            }
        }

        public override void insert(IDeploymentEntity deployment)
        {
            insert(deployment, false);

            var resources = deployment.GetResources().Values;
            foreach (IResourceEntity resource in resources)
            {
                resource.DeploymentId = deployment.Id;
                ResourceEntityManager.insert(resource);
            }
        }

        public virtual void deleteDeployment(string deploymentId, bool cascade)
        {
            IList<IProcessDefinition> processDefinitions = (new ProcessDefinitionQueryImpl()).deploymentId(deploymentId).list();

            updateRelatedModels(deploymentId);

            if (cascade)
            {
                deleteProcessInstancesForProcessDefinitions(processDefinitions);
            }

            foreach (IProcessDefinition processDefinition in processDefinitions)
            {
                deleteProcessDefinitionIdentityLinks(processDefinition);
                deleteEventSubscriptions(processDefinition);
                deleteProcessDefinitionInfo(processDefinition.Id);

                removeTimerStartJobs(processDefinition);

                // If previous process definition version has a timer/signal/message start event, it must be added
                // Only if the currently deleted process definition is the latest version, 
                // we fall back to the previous timer/signal/message start event

                restorePreviousStartEventsIfNeeded(processDefinition);
            }

            deleteProcessDefinitionForDeployment(deploymentId);
            ResourceEntityManager.deleteResourcesByDeploymentId(deploymentId);
            delete(findById<DeploymentEntityImpl>(new KeyValuePair<string, object>("id", deploymentId)), false);
        }

        protected internal virtual void updateRelatedModels(string deploymentId)
        {
            // Remove the deployment link from any model.
            // The model will still exists, as a model is a source for a deployment model and has a different lifecycle
            IList<IModel> models =  this.CommandContext.ProcessEngineConfiguration.repositoryService.createModelQuery().deploymentId(deploymentId).list();
            //IList<IModel> models = new ModelQueryImpl(this.CommandContext)
            //    .deploymentId(deploymentId).list();

            foreach (IModel model in models)
            {
                IModelEntity modelEntity = (IModelEntity)model;
                modelEntity.DeploymentId = null;
                ModelEntityManager.updateModel(modelEntity);
            }
        }

        protected internal virtual void deleteProcessDefinitionIdentityLinks(IProcessDefinition processDefinition)
        {
            IdentityLinkEntityManager.deleteIdentityLinksByProcDef(processDefinition.Id);
        }

        protected internal virtual void deleteEventSubscriptions(IProcessDefinition processDefinition)
        {
            IEventSubscriptionEntityManager eventSubscriptionEntityManager = EventSubscriptionEntityManager;
            eventSubscriptionEntityManager.deleteEventSubscriptionsForProcessDefinition(processDefinition.Id);
        }

        protected internal virtual void deleteProcessDefinitionInfo(string processDefinitionId)
        {
            ProcessDefinitionInfoEntityManager.deleteProcessDefinitionInfo(processDefinitionId);
        }

        protected internal virtual void deleteProcessDefinitionForDeployment(string deploymentId)
        {
            ProcessDefinitionEntityManager.deleteProcessDefinitionsByDeploymentId(deploymentId);
        }

        protected internal virtual void deleteProcessInstancesForProcessDefinitions(IList<IProcessDefinition> processDefinitions)
        {
            foreach (IProcessDefinition processDefinition in processDefinitions)
            {
                ExecutionEntityManager.deleteProcessInstancesByProcessDefinition(processDefinition.Id, "deleted deployment", true);
            }
        }

        protected internal virtual void removeRelatedJobs(IProcessDefinition processDefinition)
        {
            IList<IJobEntity> timerJobs = JobEntityManager.findJobsByProcessDefinitionId(processDefinition.Id);
            if (timerJobs != null && timerJobs.Count > 0)
            {
                foreach (IJobEntity timerJob in timerJobs)
                {
                    if (EventDispatcher.Enabled)
                    {
                        EventDispatcher.dispatchEvent(ActivitiEventBuilder.createEntityEvent(ActivitiEventType.JOB_CANCELED, timerJob, null, null, processDefinition.Id));
                    }

                    JobEntityManager.delete(timerJob);
                }
            }
        }

        protected internal virtual void removeTimerSuspendProcesDefJobs(IProcessDefinition processDefinition)
        {
            IList<IJobEntity> timerJobs = JobEntityManager.findJobsByTypeAndProcessDefinitionId(TimerSuspendProcessDefinitionHandler.TYPE, processDefinition.Id);
            if (timerJobs != null && timerJobs.Count > 0)
            {
                foreach (IJobEntity timerJob in timerJobs)
                {
                    if (EventDispatcher.Enabled)
                    {
                        EventDispatcher.dispatchEvent(ActivitiEventBuilder.createEntityEvent(ActivitiEventType.JOB_CANCELED, timerJob, null, null, processDefinition.Id));
                    }

                    JobEntityManager.delete(timerJob);
                }
            }
        }

        protected internal virtual void removeTimerStartJobs(IProcessDefinition processDefinition)
        {
            IList<ITimerJobEntity> timerStartJobs = TimerJobEntityManager.findJobsByTypeAndProcessDefinitionId(TimerStartEventJobHandler.TYPE, processDefinition.Id);
            if (timerStartJobs != null && timerStartJobs.Count > 0)
            {
                foreach (ITimerJobEntity timerStartJob in timerStartJobs)
                {
                    if (EventDispatcher.Enabled)
                    {
                        EventDispatcher.dispatchEvent(ActivitiEventBuilder.createEntityEvent(ActivitiEventType.JOB_CANCELED, timerStartJob, null, null, processDefinition.Id));
                    }

                    TimerJobEntityManager.delete(timerStartJob);
                }
            }
        }

        protected internal virtual void restorePreviousStartEventsIfNeeded(IProcessDefinition processDefinition)
        {
            IProcessDefinitionEntity latestProcessDefinition = findLatestProcessDefinition(processDefinition);
            if (processDefinition.Id.Equals(latestProcessDefinition.Id))
            {

                // Try to find a previous version (it could be some versions are missing due to deletions)
                IProcessDefinition previousProcessDefinition = findNewLatestProcessDefinitionAfterRemovalOf(processDefinition);
                if (previousProcessDefinition != null)
                {

                    BpmnModel bpmnModel = ProcessDefinitionUtil.getBpmnModel(previousProcessDefinition.Id);
                    org.activiti.bpmn.model.Process previousProcess = ProcessDefinitionUtil.getProcess(previousProcessDefinition.Id);
                    if (CollectionUtil.IsNotEmpty(previousProcess.FlowElements))
                    {

                        IList<StartEvent> startEvents = previousProcess.findFlowElementsOfType<StartEvent>();

                        if (CollectionUtil.IsNotEmpty(startEvents))
                        {
                            foreach (StartEvent startEvent in startEvents)
                            {

                                if (CollectionUtil.IsNotEmpty(startEvent.EventDefinitions))
                                {
                                    EventDefinition eventDefinition = startEvent.EventDefinitions[0];
                                    if (eventDefinition is TimerEventDefinition)
                                    {
                                        restoreTimerStartEvent(previousProcessDefinition, startEvent, eventDefinition);
                                    }
                                    else if (eventDefinition is SignalEventDefinition)
                                    {
                                        restoreSignalStartEvent(previousProcessDefinition, bpmnModel, startEvent, eventDefinition);
                                    }
                                    else if (eventDefinition is MessageEventDefinition)
                                    {
                                        restoreMessageStartEvent(previousProcessDefinition, bpmnModel, startEvent, eventDefinition);
                                    }

                                }

                            }
                        }

                    }

                }
            }
        }

        protected internal virtual void restoreTimerStartEvent(IProcessDefinition previousProcessDefinition, StartEvent startEvent, EventDefinition eventDefinition)
        {
            TimerEventDefinition timerEventDefinition = (TimerEventDefinition)eventDefinition;
            ITimerJobEntity timer = TimerUtil.createTimerEntityForTimerEventDefinition((TimerEventDefinition)eventDefinition, false, null, TimerStartEventJobHandler.TYPE, TimerEventHandler.createConfiguration(startEvent.Id, timerEventDefinition.EndDate, timerEventDefinition.CalendarName));

            if (timer != null)
            {
                ITimerJobEntity timerJob = JobManager.createTimerJob((TimerEventDefinition)eventDefinition, false, null, TimerStartEventJobHandler.TYPE, TimerEventHandler.createConfiguration(startEvent.Id, timerEventDefinition.EndDate, timerEventDefinition.CalendarName));

                timerJob.ProcessDefinitionId = previousProcessDefinition.Id;

                if (!ReferenceEquals(previousProcessDefinition.TenantId, null))
                {
                    timerJob.TenantId = previousProcessDefinition.TenantId;
                }

                JobManager.scheduleTimerJob(timerJob);
            }
        }

        protected internal virtual void restoreSignalStartEvent(IProcessDefinition previousProcessDefinition, BpmnModel bpmnModel, StartEvent startEvent, EventDefinition eventDefinition)
        {
            SignalEventDefinition signalEventDefinition = (SignalEventDefinition)eventDefinition;
            ISignalEventSubscriptionEntity subscriptionEntity = EventSubscriptionEntityManager.createSignalEventSubscription();
            Signal signal = bpmnModel.getSignal(signalEventDefinition.SignalRef);
            if (signal != null)
            {
                subscriptionEntity.EventName = signal.Name;
            }
            else
            {
                subscriptionEntity.EventName = signalEventDefinition.SignalRef;
            }
            subscriptionEntity.ActivityId = startEvent.Id;
            subscriptionEntity.ProcessDefinitionId = previousProcessDefinition.Id;
            if (!ReferenceEquals(previousProcessDefinition.TenantId, null))
            {
                subscriptionEntity.TenantId = previousProcessDefinition.TenantId;
            }

            EventSubscriptionEntityManager.insert(subscriptionEntity);
        }

        protected internal virtual void restoreMessageStartEvent(IProcessDefinition previousProcessDefinition, BpmnModel bpmnModel, StartEvent startEvent, EventDefinition eventDefinition)
        {
            MessageEventDefinition messageEventDefinition = (MessageEventDefinition)eventDefinition;
            if (bpmnModel.containsMessageId(messageEventDefinition.MessageRef))
            {
                Message message = bpmnModel.getMessage(messageEventDefinition.MessageRef);
                messageEventDefinition.MessageRef = message.Name;
            }

            IMessageEventSubscriptionEntity newSubscription = EventSubscriptionEntityManager.createMessageEventSubscription();
            newSubscription.EventName = messageEventDefinition.MessageRef;
            newSubscription.ActivityId = startEvent.Id;
            newSubscription.Configuration = previousProcessDefinition.Id;
            newSubscription.ProcessDefinitionId = previousProcessDefinition.Id;

            if (!ReferenceEquals(previousProcessDefinition.TenantId, null))
            {
                newSubscription.TenantId = previousProcessDefinition.TenantId;
            }

            EventSubscriptionEntityManager.insert(newSubscription);
        }

        protected internal virtual IProcessDefinitionEntity findLatestProcessDefinition(IProcessDefinition processDefinition)
        {
            IProcessDefinitionEntity latestProcessDefinition = null;
            if (!ReferenceEquals(processDefinition.TenantId, null) && !engine.ProcessEngineConfiguration.NO_TENANT_ID.Equals(processDefinition.TenantId))
            {
                latestProcessDefinition = ProcessDefinitionEntityManager.findLatestProcessDefinitionByKeyAndTenantId(processDefinition.Key, processDefinition.TenantId);
            }
            else
            {
                latestProcessDefinition = ProcessDefinitionEntityManager.findLatestProcessDefinitionByKey(processDefinition.Key);
            }
            return latestProcessDefinition;
        }

        protected internal virtual IProcessDefinition findNewLatestProcessDefinitionAfterRemovalOf(IProcessDefinition processDefinitionToBeRemoved)
        {

            // The latest process definition is not necessarily the one with 'version -1' (some versions could have been deleted)
            // Hence, the following logic

            ProcessDefinitionQueryImpl query = new ProcessDefinitionQueryImpl();
            query.processDefinitionKey(processDefinitionToBeRemoved.Key);

            if (!ReferenceEquals(processDefinitionToBeRemoved.TenantId, null) && !engine.ProcessEngineConfiguration.NO_TENANT_ID.Equals(processDefinitionToBeRemoved.TenantId))
            {
                query.processDefinitionTenantId(processDefinitionToBeRemoved.TenantId);
            }
            else
            {
                query.processDefinitionWithoutTenantId();
            }

            query.processDefinitionVersionLowerThan(processDefinitionToBeRemoved.Version);
            query.orderByProcessDefinitionVersion().desc();

            IList<IProcessDefinition> processDefinitions = ProcessDefinitionEntityManager.findProcessDefinitionsByQueryCriteria(query, new Page(0, 1));
            if (processDefinitions != null && processDefinitions.Count > 0)
            {
                return processDefinitions[0];
            }
            return null;
        }

        public virtual IDeploymentEntity findLatestDeploymentByName(string deploymentName)
        {
            return deploymentDataManager.findLatestDeploymentByName(deploymentName);
        }

        public virtual long findDeploymentCountByQueryCriteria(DeploymentQueryImpl deploymentQuery)
        {
            return deploymentDataManager.findDeploymentCountByQueryCriteria(deploymentQuery);
        }

        public virtual IList<IDeployment> findDeploymentsByQueryCriteria(DeploymentQueryImpl deploymentQuery, Page page)
        {
            return deploymentDataManager.findDeploymentsByQueryCriteria(deploymentQuery, page);
        }

        public virtual IList<string> getDeploymentResourceNames(string deploymentId)
        {
            return deploymentDataManager.getDeploymentResourceNames(deploymentId);
        }

        public virtual IList<IDeployment> findDeploymentsByNativeQuery(IDictionary<string, object> parameterMap, int firstResult, int maxResults)
        {
            return deploymentDataManager.findDeploymentsByNativeQuery(parameterMap, firstResult, maxResults);
        }

        public virtual long findDeploymentCountByNativeQuery(IDictionary<string, object> parameterMap)
        {
            return deploymentDataManager.findDeploymentCountByNativeQuery(parameterMap);
        }


        public virtual IDeploymentDataManager DeploymentDataManager
        {
            get
            {
                return deploymentDataManager;
            }
            set
            {
                this.deploymentDataManager = value;
            }
        }



    }

}