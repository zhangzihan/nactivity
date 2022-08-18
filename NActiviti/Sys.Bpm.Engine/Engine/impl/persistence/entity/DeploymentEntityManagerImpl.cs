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

namespace Sys.Workflow.Engine.Impl.Persistence.Entity
{

    using Sys.Workflow.Bpmn.Models;
    using Sys.Workflow.Engine.Delegate.Events;
    using Sys.Workflow.Engine.Delegate.Events.Impl;
    using Sys.Workflow.Engine.Impl.Cfg;
    using Sys.Workflow.Engine.Impl.JobExecutors;
    using Sys.Workflow.Engine.Impl.Persistence.Entity.Data;
    using Sys.Workflow.Engine.Impl.Util;
    using Sys.Workflow.Engine.Repository;
    using System;

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

        public override void Insert(IDeploymentEntity deployment)
        {
            Insert(deployment, false);

            var resources = deployment.GetResources().Values;
            foreach (IResourceEntity resource in resources)
            {
                resource.DeploymentId = deployment.Id;
                ResourceEntityManager.Insert(resource);
            }
        }

        public virtual void DeleteDeployment(string deploymentId, bool cascade)
        {
            IList<IProcessDefinition> processDefinitions = new ProcessDefinitionQueryImpl().SetDeploymentId(deploymentId).List();

            //判断是否存在正在执行的流程
            long count = new ExecutionQueryImpl()
                .SetProcessDeploymentId(deploymentId)
                .Count();

            //判断是否存在历史流程
            count = count > 0 ? count : new HistoricProcessInstanceQueryImpl()
                .SetDeploymentId(deploymentId)
                .Count();

            if (count > 0)
            {
                throw new ExistsProcessInstanceException(processDefinitions[0].Name);
            }

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
            ResourceEntityManager.DeleteResourcesByDeploymentId(deploymentId);
            Delete(FindById<DeploymentEntityImpl>(new KeyValuePair<string, object>("id", deploymentId)), false);
        }

        protected internal virtual void updateRelatedModels(string deploymentId)
        {
            // Remove the deployment link from any model.
            // The model will still exists, as a model is a source for a deployment model and has a different lifecycle
            IList<IModel> models = this.CommandContext.ProcessEngineConfiguration.repositoryService.CreateModelQuery().SetDeploymentId(deploymentId).List();
            //IList<IModel> models = new ModelQueryImpl(this.CommandContext)
            //    .deploymentId(deploymentId).list();

            foreach (IModel model in models)
            {
                IModelEntity modelEntity = (IModelEntity)model;
                modelEntity.DeploymentId = null;
                ModelEntityManager.UpdateModel(modelEntity);
            }
        }

        protected internal virtual void deleteProcessDefinitionIdentityLinks(IProcessDefinition processDefinition)
        {
            IdentityLinkEntityManager.DeleteIdentityLinksByProcDef(processDefinition.Id);
        }

        protected internal virtual void deleteEventSubscriptions(IProcessDefinition processDefinition)
        {
            IEventSubscriptionEntityManager eventSubscriptionEntityManager = EventSubscriptionEntityManager;
            eventSubscriptionEntityManager.DeleteEventSubscriptionsForProcessDefinition(processDefinition.Id);
        }

        protected internal virtual void deleteProcessDefinitionInfo(string processDefinitionId)
        {
            ProcessDefinitionInfoEntityManager.DeleteProcessDefinitionInfo(processDefinitionId);
        }

        protected internal virtual void deleteProcessDefinitionForDeployment(string deploymentId)
        {
            ProcessDefinitionEntityManager.DeleteProcessDefinitionsByDeploymentId(deploymentId);
        }

        protected internal virtual void deleteProcessInstancesForProcessDefinitions(IList<IProcessDefinition> processDefinitions)
        {
            foreach (IProcessDefinition processDefinition in processDefinitions)
            {
                ExecutionEntityManager.DeleteProcessInstancesByProcessDefinition(processDefinition.Id, "deleted deployment", true);
            }
        }

        protected internal virtual void removeRelatedJobs(IProcessDefinition processDefinition)
        {
            IList<IJobEntity> timerJobs = JobEntityManager.FindJobsByProcessDefinitionId(processDefinition.Id);
            if (timerJobs is object && timerJobs.Count > 0)
            {
                foreach (IJobEntity timerJob in timerJobs)
                {
                    if (EventDispatcher.Enabled)
                    {
                        EventDispatcher.DispatchEvent(ActivitiEventBuilder.CreateEntityEvent(ActivitiEventType.JOB_CANCELED, timerJob, null, null, processDefinition.Id));
                    }

                    JobEntityManager.Delete(timerJob);
                }
            }
        }

        protected internal virtual void removeTimerSuspendProcesDefJobs(IProcessDefinition processDefinition)
        {
            IList<IJobEntity> timerJobs = JobEntityManager.FindJobsByTypeAndProcessDefinitionId(TimerSuspendProcessDefinitionHandler.TYPE, processDefinition.Id);
            if (timerJobs is object && timerJobs.Count > 0)
            {
                foreach (IJobEntity timerJob in timerJobs)
                {
                    if (EventDispatcher.Enabled)
                    {
                        EventDispatcher.DispatchEvent(ActivitiEventBuilder.CreateEntityEvent(ActivitiEventType.JOB_CANCELED, timerJob, null, null, processDefinition.Id));
                    }

                    JobEntityManager.Delete(timerJob);
                }
            }
        }

        protected internal virtual void removeTimerStartJobs(IProcessDefinition processDefinition)
        {
            IList<ITimerJobEntity> timerStartJobs = TimerJobEntityManager.FindJobsByTypeAndProcessDefinitionId(TimerStartEventJobHandler.TYPE, processDefinition.Id);
            if (timerStartJobs is object && timerStartJobs.Count > 0)
            {
                foreach (ITimerJobEntity timerStartJob in timerStartJobs)
                {
                    if (EventDispatcher.Enabled)
                    {
                        EventDispatcher.DispatchEvent(ActivitiEventBuilder.CreateEntityEvent(ActivitiEventType.JOB_CANCELED, timerStartJob, null, null, processDefinition.Id));
                    }

                    TimerJobEntityManager.Delete(timerStartJob);
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
                if (previousProcessDefinition is object)
                {

                    BpmnModel bpmnModel = ProcessDefinitionUtil.GetBpmnModel(previousProcessDefinition.Id);
                    Process previousProcess = ProcessDefinitionUtil.GetProcess(previousProcessDefinition.Id);
                    if (CollectionUtil.IsNotEmpty(previousProcess.FlowElements))
                    {

                        IList<StartEvent> startEvents = previousProcess.FindFlowElementsOfType<StartEvent>();

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
            ITimerJobEntity timer = TimerUtil.CreateTimerEntityForTimerEventDefinition((TimerEventDefinition)eventDefinition, false, null, TimerStartEventJobHandler.TYPE, TimerEventHandler.CreateConfiguration(startEvent.Id, timerEventDefinition.EndDate, timerEventDefinition.CalendarName));

            if (timer is object)
            {
                ITimerJobEntity timerJob = JobManager.CreateTimerJob((TimerEventDefinition)eventDefinition, false, null, TimerStartEventJobHandler.TYPE, TimerEventHandler.CreateConfiguration(startEvent.Id, timerEventDefinition.EndDate, timerEventDefinition.CalendarName));

                timerJob.ProcessDefinitionId = previousProcessDefinition.Id;

                if (!ReferenceEquals(previousProcessDefinition.TenantId, null))
                {
                    timerJob.TenantId = previousProcessDefinition.TenantId;
                }

                JobManager.ScheduleTimerJob(timerJob);
            }
        }

        protected internal virtual void restoreSignalStartEvent(IProcessDefinition previousProcessDefinition, BpmnModel bpmnModel, StartEvent startEvent, EventDefinition eventDefinition)
        {
            SignalEventDefinition signalEventDefinition = (SignalEventDefinition)eventDefinition;
            ISignalEventSubscriptionEntity subscriptionEntity = EventSubscriptionEntityManager.CreateSignalEventSubscription();
            Signal signal = bpmnModel.GetSignal(signalEventDefinition.SignalRef);
            if (signal is not null)
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

            EventSubscriptionEntityManager.Insert(subscriptionEntity);
        }

        protected internal virtual void restoreMessageStartEvent(IProcessDefinition previousProcessDefinition, BpmnModel bpmnModel, StartEvent startEvent, EventDefinition eventDefinition)
        {
            MessageEventDefinition messageEventDefinition = (MessageEventDefinition)eventDefinition;
            if (bpmnModel.ContainsMessageId(messageEventDefinition.MessageRef))
            {
                Message message = bpmnModel.GetMessage(messageEventDefinition.MessageRef);
                messageEventDefinition.MessageRef = message.Name;
            }

            IMessageEventSubscriptionEntity newSubscription = EventSubscriptionEntityManager.CreateMessageEventSubscription();
            newSubscription.EventName = messageEventDefinition.MessageRef;
            newSubscription.ActivityId = startEvent.Id;
            newSubscription.Configuration = previousProcessDefinition.Id;
            newSubscription.ProcessDefinitionId = previousProcessDefinition.Id;

            if (!ReferenceEquals(previousProcessDefinition.TenantId, null))
            {
                newSubscription.TenantId = previousProcessDefinition.TenantId;
            }

            EventSubscriptionEntityManager.Insert(newSubscription);
        }

        protected internal virtual IProcessDefinitionEntity findLatestProcessDefinition(IProcessDefinition processDefinition)
        {
            IProcessDefinitionEntity latestProcessDefinition = null;
            if (!ReferenceEquals(processDefinition.TenantId, null) && !Engine.ProcessEngineConfiguration.NO_TENANT_ID.Equals(processDefinition.TenantId))
            {
                latestProcessDefinition = ProcessDefinitionEntityManager.FindLatestProcessDefinitionByKeyAndTenantId(processDefinition.Key, processDefinition.TenantId);
            }
            else
            {
                latestProcessDefinition = ProcessDefinitionEntityManager.FindLatestProcessDefinitionByKey(processDefinition.Key);
            }
            return latestProcessDefinition;
        }

        protected internal virtual IProcessDefinition findNewLatestProcessDefinitionAfterRemovalOf(IProcessDefinition processDefinitionToBeRemoved)
        {

            // The latest process definition is not necessarily the one with 'version -1' (some versions could have been deleted)
            // Hence, the following logic

            ProcessDefinitionQueryImpl query = new ProcessDefinitionQueryImpl();
            query.SetProcessDefinitionKey(processDefinitionToBeRemoved.Key);

            if (!ReferenceEquals(processDefinitionToBeRemoved.TenantId, null) && !Engine.ProcessEngineConfiguration.NO_TENANT_ID.Equals(processDefinitionToBeRemoved.TenantId))
            {
                query.SetProcessDefinitionTenantId(processDefinitionToBeRemoved.TenantId);
            }
            else
            {
                query.SetProcessDefinitionWithoutTenantId();
            }

            query.SetProcessDefinitionVersionLowerThan(processDefinitionToBeRemoved.Version);
            query.OrderByProcessDefinitionVersion().Desc();

            IList<IProcessDefinition> processDefinitions = ProcessDefinitionEntityManager.FindProcessDefinitionsByQueryCriteria(query, new Page(0, 1));
            if (processDefinitions is object && processDefinitions.Count > 0)
            {
                return processDefinitions[0];
            }
            return null;
        }

        public virtual IDeploymentEntity FindLatestDeploymentByName(string deploymentName)
        {
            return deploymentDataManager.FindLatestDeploymentByName(deploymentName);
        }

        public virtual long FindDeploymentCountByQueryCriteria(IDeploymentQuery deploymentQuery)
        {
            return deploymentDataManager.FindDeploymentCountByQueryCriteria(deploymentQuery);
        }

        public virtual IList<IDeployment> FindDeploymentsByQueryCriteria(IDeploymentQuery deploymentQuery, Page page)
        {
            return deploymentDataManager.FindDeploymentsByQueryCriteria(deploymentQuery, page);
        }

        public virtual IList<string> GetDeploymentResourceNames(string deploymentId)
        {
            return deploymentDataManager.GetDeploymentResourceNames(deploymentId);
        }

        public virtual IList<IDeployment> FindDeploymentsByNativeQuery(IDictionary<string, object> parameterMap, int firstResult, int maxResults)
        {
            return deploymentDataManager.FindDeploymentsByNativeQuery(parameterMap, firstResult, maxResults);
        }

        public virtual long FindDeploymentCountByNativeQuery(IDictionary<string, object> parameterMap)
        {
            return deploymentDataManager.FindDeploymentCountByNativeQuery(parameterMap);
        }

        public virtual IDeploymentEntity SaveDraft(IDeploymentEntity deployment)
        {
            IDeploymentEntity exist = this.FindLatestDeploymentByName(deployment.Name);

            if (exist is object)
            {
                IProcessDefinition process = new ProcessDefinitionQueryImpl()
                    .SetDeploymentId(exist.Id)
                    .SetLatestVersion()
                    .SingleResult();

                if (process is null)
                {
                    this.DeleteDeployment(exist.Id, true);
                }
            }

            deployment.New = true;

            this.Insert(deployment);

            return deployment;
        }

        public virtual void RemoveDrafts(string tenantId, string name)
        {
            IDeploymentQuery query = new DeploymentQueryImpl();
            query.SetDeploymentTenantId(tenantId).SetDeploymentName(name);

            IList<IDeployment> drafts = this.deploymentDataManager.FindDeploymentDrafts(query);

            foreach (var draft in drafts)
            {
                this.DeleteDeployment(draft.Id, true);
            }
        }

        public virtual IList<IDeployment> FindDrafts(IDeploymentQuery query)
        {
            IList<IDeployment> drafts = this.deploymentDataManager.FindDeploymentDrafts(query);

            return drafts;
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