using System;
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

    using org.activiti.engine.@delegate.@event;
    using org.activiti.engine.@delegate.@event.impl;
    using org.activiti.engine.impl.cfg;
    using org.activiti.engine.impl.context;
    using org.activiti.engine.impl.identity;
    using org.activiti.engine.impl.persistence.entity.data;
    using org.activiti.engine.repository;
    using org.activiti.engine.runtime;
    using org.activiti.engine.task;
    using System.Globalization;

    /// 
    /// 
    public class ExecutionEntityManagerImpl : AbstractEntityManager<IExecutionEntity>, IExecutionEntityManager
    {
        protected internal IExecutionDataManager executionDataManager;

        public ExecutionEntityManagerImpl(ProcessEngineConfigurationImpl processEngineConfiguration, IExecutionDataManager executionDataManager) : base(processEngineConfiguration)
        {
            this.executionDataManager = executionDataManager;
        }

        protected internal override IDataManager<IExecutionEntity> DataManager
        {
            get
            {
                return executionDataManager;
            }
        }

        // Overriding the default delete methods to set the 'isDeleted' flag

        public override void delete(IExecutionEntity entity)
        {
            delete(entity, true);
        }

        public override void delete(IExecutionEntity entity, bool fireDeleteEvent)
        {
            base.delete(entity, fireDeleteEvent);
            entity.Deleted = true;
        }

        // FIND METHODS

        public virtual IExecutionEntity findSubProcessInstanceBySuperExecutionId(string superExecutionId)
        {
            return executionDataManager.findSubProcessInstanceBySuperExecutionId(superExecutionId);
        }

        public virtual IList<IExecutionEntity> findChildExecutionsByParentExecutionId(string parentExecutionId)
        {
            return executionDataManager.findChildExecutionsByParentExecutionId(parentExecutionId);
        }

        public virtual IList<IExecutionEntity> findChildExecutionsByProcessInstanceId(string processInstanceId)
        {
            return executionDataManager.findChildExecutionsByProcessInstanceId(processInstanceId);
        }

        public virtual IList<IExecutionEntity> findExecutionsByParentExecutionAndActivityIds(string parentExecutionId, ICollection<string> activityIds)
        {
            return executionDataManager.findExecutionsByParentExecutionAndActivityIds(parentExecutionId, activityIds);
        }

        public virtual long findExecutionCountByQueryCriteria(ExecutionQueryImpl executionQuery)
        {
            return executionDataManager.findExecutionCountByQueryCriteria(executionQuery);
        }

        public virtual IList<IExecutionEntity> findExecutionsByQueryCriteria(ExecutionQueryImpl executionQuery, Page page)
        {
            return executionDataManager.findExecutionsByQueryCriteria(executionQuery, page);
        }

        public virtual long findProcessInstanceCountByQueryCriteria(ProcessInstanceQueryImpl executionQuery)
        {
            return executionDataManager.findProcessInstanceCountByQueryCriteria(executionQuery);
        }

        public virtual IList<IProcessInstance> findProcessInstanceByQueryCriteria(ProcessInstanceQueryImpl executionQuery)
        {
            return executionDataManager.findProcessInstanceByQueryCriteria(executionQuery);
        }

        public virtual IExecutionEntity findByRootProcessInstanceId(string rootProcessInstanceId)
        {
            IList<IExecutionEntity> executions = executionDataManager.findExecutionsByRootProcessInstanceId(rootProcessInstanceId);
            return processExecutionTree(rootProcessInstanceId, executions);

        }

        /// <summary>
        /// Processes a collection of <seealso cref="IExecutionEntity"/> instances, which form on execution tree.
        /// All the executions share the same rootProcessInstanceId (which is provided).
        /// The return value will be the root <seealso cref="IExecutionEntity"/> instance, with all child <seealso cref="IExecutionEntity"/>
        /// instances populated and set using the <seealso cref="IExecutionEntity"/> instances from the provided collections
        /// </summary>
        protected internal virtual IExecutionEntity processExecutionTree(string rootProcessInstanceId, IList<IExecutionEntity> executions)
        {
            IExecutionEntity rootExecution = null;

            // Collect executions
            IDictionary<string, IExecutionEntity> executionMap = new Dictionary<string, IExecutionEntity>(executions.Count);
            foreach (IExecutionEntity executionEntity in executions)
            {
                if (executionEntity.Id.Equals(rootProcessInstanceId))
                {
                    rootExecution = executionEntity;
                }
                executionMap[executionEntity.Id] = executionEntity;
            }

            // Set relationships
            foreach (IExecutionEntity executionEntity in executions)
            {

                // Root process instance relationship
                if (!string.ReferenceEquals(executionEntity.RootProcessInstanceId, null))
                {
                    executionEntity.RootProcessInstance = executionMap[executionEntity.RootProcessInstanceId];
                }

                // Process instance relationship
                if (!string.ReferenceEquals(executionEntity.ProcessInstanceId, null))
                {
                    executionEntity.ProcessInstance = executionMap[executionEntity.ProcessInstanceId];
                }

                // Parent - child relationship
                if (!string.ReferenceEquals(executionEntity.ParentId, null))
                {
                    IExecutionEntity parentExecutionEntity = executionMap[executionEntity.ParentId];
                    executionEntity.Parent = parentExecutionEntity;
                    parentExecutionEntity.addChildExecution(executionEntity);
                }

                // Super - sub execution relationship
                if (executionEntity.SuperExecution != null)
                {
                    IExecutionEntity superExecutionEntity = executionMap[executionEntity.SuperExecutionId];
                    executionEntity.SuperExecution = superExecutionEntity;
                    superExecutionEntity.SubProcessInstance = executionEntity;
                }

            }
            return rootExecution;
        }

        public virtual IList<IProcessInstance> findProcessInstanceAndVariablesByQueryCriteria(ProcessInstanceQueryImpl executionQuery)
        {
            return executionDataManager.findProcessInstanceAndVariablesByQueryCriteria(executionQuery);
        }

        public virtual ICollection<IExecutionEntity> findInactiveExecutionsByProcessInstanceId(string processInstanceId)
        {
            return executionDataManager.findInactiveExecutionsByProcessInstanceId(processInstanceId);
        }

        public virtual ICollection<IExecutionEntity> findInactiveExecutionsByActivityIdAndProcessInstanceId(string activityId, string processInstanceId)
        {
            return executionDataManager.findInactiveExecutionsByActivityIdAndProcessInstanceId(activityId, processInstanceId);
        }

        public virtual IList<IExecution> findExecutionsByNativeQuery(IDictionary<string, object> parameterMap, int firstResult, int maxResults)
        {
            return executionDataManager.findExecutionsByNativeQuery(parameterMap, firstResult, maxResults);
        }

        public virtual IList<IProcessInstance> findProcessInstanceByNativeQuery(IDictionary<string, object> parameterMap, int firstResult, int maxResults)
        {
            return executionDataManager.findProcessInstanceByNativeQuery(parameterMap, firstResult, maxResults);
        }

        public virtual long findExecutionCountByNativeQuery(IDictionary<string, object> parameterMap)
        {
            return executionDataManager.findExecutionCountByNativeQuery(parameterMap);
        }

        // CREATE METHODS

        public virtual IExecutionEntity createProcessInstanceExecution(IProcessDefinition processDefinition, string businessKey, string tenantId, string initiatorVariableName)
        {
            IExecutionEntity processInstanceExecution = executionDataManager.create();

            if (ExecutionRelatedEntityCountEnabledGlobally)
            {
                ((ICountingExecutionEntity)processInstanceExecution).IsCountEnabled = true;
            }

            processInstanceExecution.ProcessDefinitionId = processDefinition.Id;
            processInstanceExecution.ProcessDefinitionKey = processDefinition.Key;
            processInstanceExecution.ProcessDefinitionName = processDefinition.Name;
            processInstanceExecution.ProcessDefinitionVersion = processDefinition.Version;
            processInstanceExecution.BusinessKey = businessKey;
            processInstanceExecution.IsScope = true; // process instance is always a scope for all child executions

            // Inherit tenant id (if any)
            if (!string.ReferenceEquals(tenantId, null))
            {
                processInstanceExecution.TenantId = tenantId;
            }

            string authenticatedUserId = Authentication.AuthenticatedUserId;

            processInstanceExecution.StartTime = Context.ProcessEngineConfiguration.Clock.CurrentTime;
            processInstanceExecution.StartUserId = authenticatedUserId;

            // Store in database
            insert(processInstanceExecution, false);

            if (!string.ReferenceEquals(initiatorVariableName, null))
            {
                processInstanceExecution.setVariable(initiatorVariableName, authenticatedUserId);
            }

            // Need to be after insert, cause we need the id
            processInstanceExecution.ProcessInstanceId = processInstanceExecution.Id;
            processInstanceExecution.RootProcessInstanceId = processInstanceExecution.Id;
            if (!string.ReferenceEquals(authenticatedUserId, null))
            {
                IdentityLinkEntityManager.addIdentityLink(processInstanceExecution, authenticatedUserId, null, IdentityLinkType.STARTER);
            }

            // Fire events
            if (EventDispatcher.Enabled)
            {
                EventDispatcher.dispatchEvent(ActivitiEventBuilder.createEntityEvent(ActivitiEventType.ENTITY_CREATED, processInstanceExecution));
            }

            return processInstanceExecution;
        }

        /// <summary>
        /// Creates a new execution. properties processDefinition, processInstance and activity will be initialized.
        /// </summary>
        public virtual IExecutionEntity createChildExecution(IExecutionEntity parentExecutionEntity)
        {
            IExecutionEntity childExecution = executionDataManager.create();
            inheritCommonProperties(parentExecutionEntity, childExecution);
            childExecution.Parent = parentExecutionEntity;
            childExecution.ProcessDefinitionId = parentExecutionEntity.ProcessDefinitionId;
            childExecution.ProcessDefinitionKey = parentExecutionEntity.ProcessDefinitionKey;
            childExecution.ProcessInstanceId = !string.ReferenceEquals(parentExecutionEntity.ProcessInstanceId, null) ? parentExecutionEntity.ProcessInstanceId : parentExecutionEntity.Id;
            childExecution.IsScope = false;

            // manage the bidirectional parent-child relation
            parentExecutionEntity.addChildExecution(childExecution);

            // Insert the child execution
            insert(childExecution, false);

            //if (logger.DebugEnabled)
            //{
            //    logger.debug("Child execution {} created with parent {}", childExecution, parentExecutionEntity.Id);
            //}

            if (EventDispatcher.Enabled)
            {
                EventDispatcher.dispatchEvent(ActivitiEventBuilder.createEntityEvent(ActivitiEventType.ENTITY_CREATED, childExecution));
                EventDispatcher.dispatchEvent(ActivitiEventBuilder.createEntityEvent(ActivitiEventType.ENTITY_INITIALIZED, childExecution));
            }

            return childExecution;
        }

        public virtual IExecutionEntity createSubprocessInstance(IProcessDefinition processDefinition, IExecutionEntity superExecutionEntity, string businessKey)
        {
            IExecutionEntity subProcessInstance = executionDataManager.create();
            inheritCommonProperties(superExecutionEntity, subProcessInstance);
            subProcessInstance.ProcessDefinitionId = processDefinition.Id;
            subProcessInstance.ProcessDefinitionKey = processDefinition.Key;
            subProcessInstance.SuperExecution = superExecutionEntity;
            subProcessInstance.RootProcessInstanceId = superExecutionEntity.RootProcessInstanceId;
            subProcessInstance.IsScope = true; // process instance is always a scope for all child executions
            subProcessInstance.StartUserId = Authentication.AuthenticatedUserId;
            subProcessInstance.BusinessKey = businessKey;

            // Store in database
            insert(subProcessInstance, false);

            //if (logger.DebugEnabled)
            //{
            //    logger.debug("Child execution {} created with super execution {}", subProcessInstance, superExecutionEntity.Id);
            //}

            subProcessInstance.ProcessInstanceId = subProcessInstance.Id;
            superExecutionEntity.SubProcessInstance = subProcessInstance;

            if (Context.ProcessEngineConfiguration != null && Context.ProcessEngineConfiguration.EventDispatcher.Enabled)
            {
                Context.ProcessEngineConfiguration.EventDispatcher.dispatchEvent(ActivitiEventBuilder.createEntityEvent(ActivitiEventType.ENTITY_CREATED, subProcessInstance));
            }

            return subProcessInstance;
        }

        protected internal virtual void inheritCommonProperties(IExecutionEntity parentExecutionEntity, IExecutionEntity childExecution)
        {

            // Inherits the 'count' feature from the parent. 
            // If the parent was not 'counting', we can't make the child 'counting' again.
            if (parentExecutionEntity is ICountingExecutionEntity)
            {
                ICountingExecutionEntity countingParentExecutionEntity = (ICountingExecutionEntity)parentExecutionEntity;
                ((ICountingExecutionEntity)childExecution).IsCountEnabled = countingParentExecutionEntity.IsCountEnabled;
            }

            childExecution.RootProcessInstanceId = parentExecutionEntity.RootProcessInstanceId;
            childExecution.IsActive = true;
            childExecution.StartTime = processEngineConfiguration.Clock.CurrentTime;

            if (!string.ReferenceEquals(parentExecutionEntity.TenantId, null))
            {
                childExecution.TenantId = parentExecutionEntity.TenantId;
            }

        }

        // UPDATE METHODS

        public virtual void updateExecutionTenantIdForDeployment(string deploymentId, string newTenantId)
        {
            executionDataManager.updateExecutionTenantIdForDeployment(deploymentId, newTenantId);
        }

        // DELETE METHODS

        public virtual void deleteProcessInstancesByProcessDefinition(string processDefinitionId, string deleteReason, bool cascade)
        {
            IList<string> processInstanceIds = executionDataManager.findProcessInstanceIdsByProcessDefinitionId(processDefinitionId);

            foreach (string processInstanceId in processInstanceIds)
            {
                deleteProcessInstance(processInstanceId, deleteReason, cascade);
            }

            if (cascade)
            {
                HistoricProcessInstanceEntityManager.deleteHistoricProcessInstanceByProcessDefinitionId(processDefinitionId);
            }
        }

        public virtual void deleteProcessInstance(string processInstanceId, string deleteReason, bool cascade)
        {
            IExecutionEntity execution = findById<IExecutionEntity>(new KeyValuePair<string, object>("id", processInstanceId));

            if (execution == null)
            {
                throw new ActivitiObjectNotFoundException("No process instance found for id '" + processInstanceId + "'", typeof(IProcessInstance));
            }

            deleteProcessInstanceCascade(execution, deleteReason, cascade);
        }

        protected internal virtual void deleteProcessInstanceCascade(IExecutionEntity execution, string deleteReason, bool deleteHistory)
        {

            // fill default reason if none provided
            if (string.ReferenceEquals(deleteReason, null))
            {
                deleteReason = org.activiti.engine.history.DeleteReason_Fields.PROCESS_INSTANCE_DELETED;
            }

            foreach (IExecutionEntity subExecutionEntity in execution.Executions)
            {
                if (subExecutionEntity.IsMultiInstanceRoot)
                {
                    foreach (IExecutionEntity miExecutionEntity in subExecutionEntity.Executions)
                    {
                        if (miExecutionEntity.SubProcessInstance != null)
                        {
                            deleteProcessInstanceCascade(miExecutionEntity.SubProcessInstance, deleteReason, deleteHistory);
                        }
                    }

                }
                else if (subExecutionEntity.SubProcessInstance != null)
                {
                    deleteProcessInstanceCascade(subExecutionEntity.SubProcessInstance, deleteReason, deleteHistory);
                }
            }

            TaskEntityManager.deleteTasksByProcessInstanceId(execution.Id, deleteReason, deleteHistory);

            if (EventDispatcher.Enabled)
            {
                EventDispatcher.dispatchEvent(ActivitiEventBuilder.createCancelledEvent(execution.ProcessInstanceId, execution.ProcessInstanceId, null, deleteReason));
            }

            // delete the execution BEFORE we delete the history, otherwise we will
            // produce orphan HistoricVariableInstance instances

            IExecutionEntity processInstanceExecutionEntity = execution.ProcessInstance;
            if (processInstanceExecutionEntity == null)
            {
                return;
            }

            IList<IExecutionEntity> childExecutions = collectChildren(execution.ProcessInstance);
            for (int i = childExecutions.Count - 1; i >= 0; i--)
            {
                IExecutionEntity childExecutionEntity = childExecutions[i];
                deleteExecutionAndRelatedData(childExecutionEntity, deleteReason, false);
            }

            deleteExecutionAndRelatedData(execution, deleteReason, false);

            if (deleteHistory)
            {
                HistoricProcessInstanceEntityManager.delete(new KeyValuePair<string, object>("id", execution.Id));
            }

            HistoryManager.recordProcessInstanceEnd(processInstanceExecutionEntity.Id, deleteReason, null);
            processInstanceExecutionEntity.Deleted = true;
        }

        public virtual void deleteExecutionAndRelatedData(IExecutionEntity executionEntity, string deleteReason, bool cancel)
        {
            HistoryManager.recordActivityEnd(executionEntity, deleteReason);
            deleteDataForExecution(executionEntity, deleteReason, cancel);
            delete(executionEntity);
        }

        public virtual void deleteProcessInstanceExecutionEntity(string processInstanceId, string currentFlowElementId, string deleteReason, bool cascade, bool cancel)
        {

            IExecutionEntity processInstanceEntity = findById<IExecutionEntity>(new KeyValuePair<string, object>("id", processInstanceId));

            if (processInstanceEntity == null)
            {
                throw new ActivitiObjectNotFoundException("No process instance found for id '" + processInstanceId + "'", typeof(IProcessInstance));
            }

            if (processInstanceEntity.Deleted)
            {
                return;
            }

            // Call activities
            foreach (IExecutionEntity subExecutionEntity in processInstanceEntity.Executions)
            {
                if (subExecutionEntity.SubProcessInstance != null && !subExecutionEntity.Ended)
                {
                    deleteProcessInstanceCascade(subExecutionEntity.SubProcessInstance, deleteReason, cascade);
                }
            }

            // delete event scope executions
            foreach (IExecutionEntity childExecution in processInstanceEntity.Executions)
            {
                if (childExecution.IsEventScope)
                {
                    deleteExecutionAndRelatedData(childExecution, null, false);
                }
            }

            deleteChildExecutions(processInstanceEntity, deleteReason, cancel);
            deleteExecutionAndRelatedData(processInstanceEntity, deleteReason, cancel);

            if (EventDispatcher.Enabled)
            {
                if (!cancel)
                {
                    EventDispatcher.dispatchEvent(ActivitiEventBuilder.createEntityEvent(ActivitiEventType.PROCESS_COMPLETED, processInstanceEntity));
                }
                else
                {
                    EventDispatcher.dispatchEvent(ActivitiEventBuilder.createCancelledEvent(processInstanceEntity.Id, processInstanceEntity.Id, processInstanceEntity.ProcessDefinitionId, deleteReason));
                }
            }

            // TODO: what about delete reason?
            HistoryManager.recordProcessInstanceEnd(processInstanceEntity.Id, deleteReason, currentFlowElementId);
            processInstanceEntity.Deleted = true;
        }

        public virtual void deleteChildExecutions(IExecutionEntity executionEntity, string deleteReason, bool cancel)
        {

            // The children of an execution for a tree. For correct deletions
            // (taking care of foreign keys between child-parent)
            // the leafs of this tree must be deleted first before the parents elements.
            IList<IExecutionEntity> childExecutions = collectChildren(executionEntity);
            for (int i = childExecutions.Count - 1; i >= 0; i--)
            {
                IExecutionEntity childExecutionEntity = childExecutions[i];
                if (!childExecutionEntity.Ended)
                {
                    deleteExecutionAndRelatedData(childExecutionEntity, deleteReason, cancel);
                }
            }
        }

        public virtual IList<IExecutionEntity> collectChildren(IExecutionEntity executionEntity)
        {
            IList<IExecutionEntity> childExecutions = new List<IExecutionEntity>();
            collectChildren(executionEntity, childExecutions);
            return childExecutions;
        }

        protected internal virtual void collectChildren(IExecutionEntity executionEntity, IList<IExecutionEntity> collectedChildExecution)
        {
            IList<IExecutionEntity> childExecutions = (IList<IExecutionEntity>)executionEntity.Executions;
            if (childExecutions != null && childExecutions.Count > 0)
            {
                foreach (IExecutionEntity childExecution in childExecutions)
                {
                    if (!childExecution.Deleted)
                    {
                        collectedChildExecution.Add(childExecution);
                        collectChildren(childExecution, collectedChildExecution);
                    }
                }
            }

            IExecutionEntity subProcessInstance = executionEntity.SubProcessInstance;
            if (subProcessInstance != null && !subProcessInstance.Deleted)
            {
                collectedChildExecution.Add(subProcessInstance);
                collectChildren(subProcessInstance, collectedChildExecution);
            }
        }

        public virtual IExecutionEntity findFirstScope(IExecutionEntity executionEntity)
        {
            IExecutionEntity currentExecutionEntity = executionEntity;
            while (currentExecutionEntity != null)
            {
                if (currentExecutionEntity.IsScope)
                {
                    return currentExecutionEntity;
                }

                IExecutionEntity parentExecutionEntity = currentExecutionEntity.Parent;
                if (parentExecutionEntity == null)
                {
                    parentExecutionEntity = currentExecutionEntity.SuperExecution;
                }
                currentExecutionEntity = parentExecutionEntity;
            }
            return null;
        }

        public virtual IExecutionEntity findFirstMultiInstanceRoot(IExecutionEntity executionEntity)
        {
            IExecutionEntity currentExecutionEntity = executionEntity;
            while (currentExecutionEntity != null)
            {
                if (currentExecutionEntity.IsMultiInstanceRoot)
                {
                    return currentExecutionEntity;
                }

                IExecutionEntity parentExecutionEntity = currentExecutionEntity.Parent;
                if (parentExecutionEntity == null)
                {
                    parentExecutionEntity = currentExecutionEntity.SuperExecution;
                }
                currentExecutionEntity = parentExecutionEntity;
            }
            return null;
        }

        public virtual void deleteDataForExecution(IExecutionEntity executionEntity, string deleteReason, bool cancel)
        {

            // To start, deactivate the current incoming execution
            executionEntity.Ended = true;
            executionEntity.IsActive = false;

            bool enableExecutionRelationshipCounts = isExecutionRelatedEntityCountEnabled(executionEntity);

            if (executionEntity.Id.Equals(executionEntity.ProcessInstanceId) && (!enableExecutionRelationshipCounts || (enableExecutionRelationshipCounts && ((ICountingExecutionEntity)executionEntity).IdentityLinkCount > 0)))
            {
                IdentityLinkEntityManager identityLinkEntityManager = IdentityLinkEntityManager;
                ICollection<IIdentityLinkEntity> identityLinks = identityLinkEntityManager.findIdentityLinksByProcessInstanceId(executionEntity.ProcessInstanceId);
                foreach (IIdentityLinkEntity identityLink in identityLinks)
                {
                    identityLinkEntityManager.delete(identityLink);
                }
            }

            // Get variables related to execution and delete them
            if (!enableExecutionRelationshipCounts || (enableExecutionRelationshipCounts && ((ICountingExecutionEntity)executionEntity).VariableCount > 0))
            {
                ICollection<IVariableInstance> executionVariables = executionEntity.VariableInstancesLocal.Values;
                foreach (IVariableInstance variableInstance in executionVariables)
                {
                    if (variableInstance is IVariableInstanceEntity)
                    {
                        IVariableInstanceEntity variableInstanceEntity = (IVariableInstanceEntity)variableInstance;

                        IVariableInstanceEntityManager variableInstanceEntityManager = VariableInstanceEntityManager;
                        variableInstanceEntityManager.delete(variableInstanceEntity);
                        if (variableInstanceEntity.ByteArrayRef != null && !string.ReferenceEquals(variableInstanceEntity.ByteArrayRef.Id, null))
                        {
                            ByteArrayEntityManager.deleteByteArrayById(variableInstanceEntity.ByteArrayRef.Id);
                        }
                    }
                }
            }

            // Delete current user tasks
            if (!enableExecutionRelationshipCounts || (enableExecutionRelationshipCounts && ((ICountingExecutionEntity)executionEntity).TaskCount > 0))
            {
                ITaskEntityManager taskEntityManager = TaskEntityManager;
                ICollection<ITaskEntity> tasksForExecution = taskEntityManager.findTasksByExecutionId(executionEntity.Id);
                foreach (ITaskEntity taskEntity in tasksForExecution)
                {
                    taskEntityManager.deleteTask(taskEntity, deleteReason, false, cancel);
                }
            }

            // Delete jobs

            if (!enableExecutionRelationshipCounts || (enableExecutionRelationshipCounts && ((ICountingExecutionEntity)executionEntity).TimerJobCount > 0))
            {
                ITimerJobEntityManager timerJobEntityManager = TimerJobEntityManager;
                ICollection<ITimerJobEntity> timerJobsForExecution = timerJobEntityManager.findJobsByExecutionId(executionEntity.Id);
                foreach (ITimerJobEntity job in timerJobsForExecution)
                {
                    timerJobEntityManager.delete(job);
                    if (EventDispatcher.Enabled)
                    {
                        EventDispatcher.dispatchEvent(ActivitiEventBuilder.createEntityEvent(ActivitiEventType.JOB_CANCELED, job));
                    }
                }
            }

            if (!enableExecutionRelationshipCounts || (enableExecutionRelationshipCounts && ((ICountingExecutionEntity)executionEntity).JobCount > 0))
            {
                IJobEntityManager jobEntityManager = JobEntityManager;
                ICollection<IJobEntity> jobsForExecution = jobEntityManager.findJobsByExecutionId(executionEntity.Id);
                foreach (IJobEntity job in jobsForExecution)
                {
                    JobEntityManager.delete(job);
                    if (EventDispatcher.Enabled)
                    {
                        EventDispatcher.dispatchEvent(ActivitiEventBuilder.createEntityEvent(ActivitiEventType.JOB_CANCELED, job));
                    }
                }
            }

            if (!enableExecutionRelationshipCounts || (enableExecutionRelationshipCounts && ((ICountingExecutionEntity)executionEntity).SuspendedJobCount > 0))
            {
                ISuspendedJobEntityManager suspendedJobEntityManager = SuspendedJobEntityManager;
                ICollection<ISuspendedJobEntity> suspendedJobsForExecution = suspendedJobEntityManager.findJobsByExecutionId(executionEntity.Id);
                foreach (ISuspendedJobEntity job in suspendedJobsForExecution)
                {
                    suspendedJobEntityManager.delete(job);
                    if (EventDispatcher.Enabled)
                    {
                        EventDispatcher.dispatchEvent(ActivitiEventBuilder.createEntityEvent(ActivitiEventType.JOB_CANCELED, job));
                    }
                }
            }

            if (!enableExecutionRelationshipCounts || (enableExecutionRelationshipCounts && ((ICountingExecutionEntity)executionEntity).DeadLetterJobCount > 0))
            {
                IDeadLetterJobEntityManager deadLetterJobEntityManager = DeadLetterJobEntityManager;
                ICollection<IDeadLetterJobEntity> deadLetterJobsForExecution = deadLetterJobEntityManager.findJobsByExecutionId(executionEntity.Id);
                foreach (IDeadLetterJobEntity job in deadLetterJobsForExecution)
                {
                    deadLetterJobEntityManager.delete(job);
                    if (EventDispatcher.Enabled)
                    {
                        EventDispatcher.dispatchEvent(ActivitiEventBuilder.createEntityEvent(ActivitiEventType.JOB_CANCELED, job));
                    }
                }
            }

            // Delete event subscriptions
            if (!enableExecutionRelationshipCounts || (enableExecutionRelationshipCounts && ((ICountingExecutionEntity)executionEntity).EventSubscriptionCount > 0))
            {
                IEventSubscriptionEntityManager eventSubscriptionEntityManager = EventSubscriptionEntityManager;
                IList<IEventSubscriptionEntity> eventSubscriptions = eventSubscriptionEntityManager.findEventSubscriptionsByExecution(executionEntity.Id);
                foreach (IEventSubscriptionEntity eventSubscription in eventSubscriptions)
                {
                    eventSubscriptionEntityManager.delete(eventSubscription);
                }
            }

        }

        // OTHER METHODS

        public virtual void updateProcessInstanceLockTime(string processInstanceId)
        {
            DateTime expirationTime = Clock.CurrentTime;
            int lockMillis = AsyncExecutor.AsyncJobLockTimeInMillis;

            DateTime lockCal = DateTime.Now;
            lockCal.AddMilliseconds(lockMillis);

            executionDataManager.updateProcessInstanceLockTime(processInstanceId, lockCal, expirationTime);
        }

        public virtual void clearProcessInstanceLockTime(string processInstanceId)
        {
            executionDataManager.clearProcessInstanceLockTime(processInstanceId);
        }

        public virtual string updateProcessInstanceBusinessKey(IExecutionEntity executionEntity, string businessKey)
        {
            if (executionEntity.ProcessInstanceType && !string.ReferenceEquals(businessKey, null))
            {
                executionEntity.BusinessKey = businessKey;
                HistoryManager.updateProcessBusinessKeyInHistory(executionEntity);

                if (EventDispatcher.Enabled)
                {
                    EventDispatcher.dispatchEvent(ActivitiEventBuilder.createEntityEvent(ActivitiEventType.ENTITY_UPDATED, executionEntity));
                }

                return businessKey;
            }
            return null;
        }

        public virtual IExecutionDataManager ExecutionDataManager
        {
            get
            {
                return executionDataManager;
            }
            set
            {
                this.executionDataManager = value;
            }
        }


    }

}