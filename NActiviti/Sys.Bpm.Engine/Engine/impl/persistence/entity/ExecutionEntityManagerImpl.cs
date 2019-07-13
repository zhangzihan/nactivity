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

namespace Sys.Workflow.Engine.Impl.Persistence.Entity
{
    using Microsoft.Extensions.Logging;
    using Sys.Workflow.Engine.Delegate.Events;
    using Sys.Workflow.Engine.Delegate.Events.Impl;
    using Sys.Workflow.Engine.History;
    using Sys.Workflow.Engine.Impl.Cfg;
    using Sys.Workflow.Engine.Impl.Cmd;
    using Sys.Workflow.Engine.Impl.Contexts;
    using Sys.Workflow.Engine.Impl.Identities;
    using Sys.Workflow.Engine.Impl.Persistence.Entity.Data;
    using Sys.Workflow.Engine.Repository;
    using Sys.Workflow.Engine.Runtime;
    using Sys.Workflow.Engine.Tasks;
    using Sys;
    using Sys.Net.Http;
    using Sys.Workflow;
    using Sys.Workflow.Engine.Bpmn.Rules;
    using System.Globalization;

    /// 
    /// 
    public class ExecutionEntityManagerImpl : AbstractEntityManager<IExecutionEntity>, IExecutionEntityManager
    {
        protected internal IExecutionDataManager executionDataManager;

        private readonly ILogger<ExecutionEntityManagerImpl> logger = ProcessEngineServiceProvider.LoggerService<ExecutionEntityManagerImpl>();

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

        public override void Delete(IExecutionEntity entity)
        {
            Delete(entity, true);
        }

        public override void Delete(IExecutionEntity entity, bool fireDeleteEvent)
        {
            base.Delete(entity, fireDeleteEvent);
            entity.Deleted = true;
        }

        public override void Insert(IExecutionEntity entity)
        {
            base.Insert(entity);
        }

        public override void Insert(IExecutionEntity entity, bool fireCreateEvent)
        {
            base.Insert(entity, fireCreateEvent);
        }

        // FIND METHODS

        public virtual IExecutionEntity FindSubProcessInstanceBySuperExecutionId(string superExecutionId)
        {
            return executionDataManager.FindSubProcessInstanceBySuperExecutionId(superExecutionId);
        }

        public virtual IList<IExecutionEntity> FindChildExecutionsByParentExecutionId(string parentExecutionId)
        {
            return executionDataManager.FindChildExecutionsByParentExecutionId(parentExecutionId);
        }

        public virtual IList<IExecutionEntity> FindChildExecutionsByProcessInstanceId(string processInstanceId)
        {
            return executionDataManager.FindChildExecutionsByProcessInstanceId(processInstanceId);
        }

        public virtual IList<IExecutionEntity> FindExecutionsByParentExecutionAndActivityIds(string parentExecutionId, ICollection<string> activityIds)
        {
            return executionDataManager.FindExecutionsByParentExecutionAndActivityIds(parentExecutionId, activityIds);
        }

        public virtual long FindExecutionCountByQueryCriteria(IExecutionQuery executionQuery)
        {
            return executionDataManager.FindExecutionCountByQueryCriteria(executionQuery);
        }

        public virtual IList<IExecutionEntity> FindExecutionsByQueryCriteria(IExecutionQuery executionQuery, Page page)
        {
            return executionDataManager.FindExecutionsByQueryCriteria(executionQuery, page);
        }

        public virtual long FindProcessInstanceCountByQueryCriteria(IProcessInstanceQuery executionQuery)
        {
            return executionDataManager.FindProcessInstanceCountByQueryCriteria(executionQuery);
        }

        public virtual IList<IProcessInstance> FindProcessInstanceByQueryCriteria(IProcessInstanceQuery executionQuery)
        {
            return executionDataManager.FindProcessInstanceByQueryCriteria(executionQuery);
        }

        public virtual IExecutionEntity FindByRootProcessInstanceId(string rootProcessInstanceId)
        {
            IList<IExecutionEntity> executions = executionDataManager.FindExecutionsByRootProcessInstanceId(rootProcessInstanceId);
            return ProcessExecutionTree(rootProcessInstanceId, executions);

        }

        /// <summary>
        /// Processes a collection of <seealso cref="IExecutionEntity"/> instances, which form on execution tree.
        /// All the executions share the same rootProcessInstanceId (which is provided).
        /// The return value will be the root <seealso cref="IExecutionEntity"/> instance, with all child <seealso cref="IExecutionEntity"/>
        /// instances populated and set using the <seealso cref="IExecutionEntity"/> instances from the provided collections
        /// </summary>
        protected internal virtual IExecutionEntity ProcessExecutionTree(string rootProcessInstanceId, IList<IExecutionEntity> executions)
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
                if (executionEntity.RootProcessInstanceId is object)
                {
                    executionEntity.RootProcessInstance = executionMap[executionEntity.RootProcessInstanceId];
                }

                // Process instance relationship
                if (executionEntity.ProcessInstanceId is object)
                {
                    executionEntity.ProcessInstance = executionMap[executionEntity.ProcessInstanceId];
                }

                // Parent - child relationship
                if (executionEntity.ParentId is object)
                {
                    IExecutionEntity parentExecutionEntity = executionMap[executionEntity.ParentId];
                    executionEntity.Parent = parentExecutionEntity;
                    parentExecutionEntity.AddChildExecution(executionEntity);
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

        public virtual IList<IProcessInstance> FindProcessInstanceAndVariablesByQueryCriteria(ProcessInstanceQueryImpl executionQuery)
        {
            return executionDataManager.FindProcessInstanceAndVariablesByQueryCriteria(executionQuery);
        }

        public virtual ICollection<IExecutionEntity> FindInactiveExecutionsByProcessInstanceId(string processInstanceId)
        {
            return executionDataManager.FindInactiveExecutionsByProcessInstanceId(processInstanceId);
        }

        public virtual ICollection<IExecutionEntity> FindInactiveExecutionsByActivityIdAndProcessInstanceId(string activityId, string processInstanceId)
        {
            return executionDataManager.FindInactiveExecutionsByActivityIdAndProcessInstanceId(activityId, processInstanceId);
        }

        public virtual IList<IExecution> FindExecutionsByNativeQuery(IDictionary<string, object> parameterMap, int firstResult, int maxResults)
        {
            return executionDataManager.FindExecutionsByNativeQuery(parameterMap, firstResult, maxResults);
        }

        public virtual IList<IProcessInstance> FindProcessInstanceByNativeQuery(IDictionary<string, object> parameterMap, int firstResult, int maxResults)
        {
            return executionDataManager.FindProcessInstanceByNativeQuery(parameterMap, firstResult, maxResults);
        }

        public virtual long FindExecutionCountByNativeQuery(IDictionary<string, object> parameterMap)
        {
            return executionDataManager.FindExecutionCountByNativeQuery(parameterMap);
        }

        // CREATE METHODS

        public virtual IExecutionEntity CreateProcessInstanceExecution(IProcessDefinition processDefinition, string businessKey, string tenantId, string initiatorVariableName)
        {
            IExecutionEntity processInstanceExecution = executionDataManager.Create();

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
            if (tenantId is object)
            {
                processInstanceExecution.TenantId = tenantId;
            }

            string authenticatedUserId = Authentication.AuthenticatedUser.Id;

            processInstanceExecution.StartTime = Context.ProcessEngineConfiguration.Clock.CurrentTime;
            processInstanceExecution.StartUserId = authenticatedUserId;
            processInstanceExecution.StartUser = Authentication.AuthenticatedUser.FullName;

            // Store in database
            Insert(processInstanceExecution, false);

            if (string.IsNullOrWhiteSpace(processInstanceExecution.StartUserId) == false)
            {
                IUserInfo starter = null;
                IUserServiceProxy userService = ProcessEngineServiceProvider.Resolve<IUserServiceProxy>();
                ExternalConnectorProvider externalConnector = ProcessEngineServiceProvider.Resolve<ExternalConnectorProvider>();

                starter = new UserInfo
                {
                    Id = processInstanceExecution.StartUserId,
                    FullName = processInstanceExecution.StartUserId
                };
                //TODO: 考虑性能问题，暂时不要获取人员信息
                //try
                //{
                //    starter = AsyncHelper.RunSync<IUserInfo>(() => userService.GetUser(processInstanceExecution.StartUserId));
                //    starter.TenantId = processInstanceExecution.TenantId;
                //}
                //catch (Exception ex)
                //{
                //    logger.LogError(ex, ex.Message);
                //    starter = new UserInfo
                //    {
                //        Id = processInstanceExecution.StartUserId,
                //        FullName = processInstanceExecution.StartUserId
                //    };
                //}

                //保存调用用户变量
                processInstanceExecution.SetVariable(processInstanceExecution.StartUserId, starter);
            }

            if (initiatorVariableName is object)
            {
                processInstanceExecution.SetVariable(initiatorVariableName, authenticatedUserId);
            }

            // Need to be after insert, cause we need the id
            processInstanceExecution.ProcessInstanceId = processInstanceExecution.Id;
            processInstanceExecution.RootProcessInstanceId = processInstanceExecution.Id;
            if (authenticatedUserId is object)
            {
                IdentityLinkEntityManager.AddIdentityLink(processInstanceExecution, authenticatedUserId, null, IdentityLinkType.STARTER);
            }

            // Fire events
            if (EventDispatcher.Enabled)
            {
                EventDispatcher.DispatchEvent(ActivitiEventBuilder.CreateEntityEvent(ActivitiEventType.ENTITY_CREATED, processInstanceExecution));
            }

            return processInstanceExecution;
        }

        /// <summary>
        /// Creates a new execution. properties processDefinition, processInstance and activity will be initialized.
        /// </summary>
        public virtual IExecutionEntity CreateChildExecution(IExecutionEntity parentExecutionEntity)
        {
            IExecutionEntity childExecution = executionDataManager.Create();
            childExecution.Name = parentExecutionEntity.Name;
            InheritCommonProperties(parentExecutionEntity, childExecution);
            childExecution.Parent = parentExecutionEntity;
            childExecution.ProcessDefinitionId = parentExecutionEntity.ProcessDefinitionId;
            childExecution.ProcessDefinitionKey = parentExecutionEntity.ProcessDefinitionKey;
            childExecution.ProcessInstanceId = parentExecutionEntity.ProcessInstanceId is object ? parentExecutionEntity.ProcessInstanceId : parentExecutionEntity.Id;
            childExecution.IsScope = false;
            childExecution.ActivityId = parentExecutionEntity.ActivityId;
            childExecution.BusinessKey = parentExecutionEntity.BusinessKey;
            childExecution.DeploymentId = parentExecutionEntity.DeploymentId;
            childExecution.TenantId = parentExecutionEntity.TenantId;

            // manage the bidirectional parent-child relation
            parentExecutionEntity.AddChildExecution(childExecution);

            // Insert the child execution
            Insert(childExecution, false);

            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug("Child execution {} created with parent {}", childExecution, parentExecutionEntity.Id);
            }

            if (EventDispatcher.Enabled)
            {
                EventDispatcher.DispatchEvent(ActivitiEventBuilder.CreateEntityEvent(ActivitiEventType.ENTITY_CREATED, childExecution));
                EventDispatcher.DispatchEvent(ActivitiEventBuilder.CreateEntityEvent(ActivitiEventType.ENTITY_INITIALIZED, childExecution));
            }

            return childExecution;
        }

        public virtual IExecutionEntity CreateSubprocessInstance(IProcessDefinition processDefinition, IExecutionEntity superExecutionEntity, string businessKey)
        {
            IExecutionEntity subProcessInstance = executionDataManager.Create();
            InheritCommonProperties(superExecutionEntity, subProcessInstance);
            subProcessInstance.ProcessDefinitionId = processDefinition.Id;
            subProcessInstance.ProcessDefinitionKey = processDefinition.Key;
            subProcessInstance.SuperExecution = superExecutionEntity;
            subProcessInstance.RootProcessInstanceId = superExecutionEntity.RootProcessInstanceId;
            subProcessInstance.IsScope = true; // process instance is always a scope for all child executions
            subProcessInstance.StartUserId = Authentication.AuthenticatedUser.Id;
            subProcessInstance.BusinessKey = businessKey;

            // Store in database
            Insert(subProcessInstance, false);

            //if (logger.DebugEnabled)
            //{
            //    logger.debug("Child execution {} created with super execution {}", subProcessInstance, superExecutionEntity.Id);
            //}

            subProcessInstance.ProcessInstanceId = subProcessInstance.Id;
            superExecutionEntity.SubProcessInstance = subProcessInstance;

            ProcessEngineConfigurationImpl processEngineConfiguration1 = Context.ProcessEngineConfiguration;
            if (processEngineConfiguration1 is object && processEngineConfiguration1.EventDispatcher.Enabled)
            {
                processEngineConfiguration1.EventDispatcher.DispatchEvent(ActivitiEventBuilder.CreateEntityEvent(ActivitiEventType.ENTITY_CREATED, subProcessInstance));
            }

            return subProcessInstance;
        }

        protected internal virtual void InheritCommonProperties(IExecutionEntity parentExecutionEntity, IExecutionEntity childExecution)
        {

            // Inherits the 'count' feature from the parent. 
            // If the parent was not 'counting', we can't make the child 'counting' again.
            if (parentExecutionEntity is ICountingExecutionEntity countingParentExecutionEntity)
            {
                ((ICountingExecutionEntity)childExecution).IsCountEnabled = countingParentExecutionEntity.IsCountEnabled;
            }

            childExecution.Name = parentExecutionEntity.Name;
            childExecution.RootProcessInstanceId = parentExecutionEntity.RootProcessInstanceId;
            childExecution.IsActive = true;
            childExecution.StartTime = processEngineConfiguration.Clock.CurrentTime;

            if (parentExecutionEntity.TenantId is object)
            {
                childExecution.TenantId = parentExecutionEntity.TenantId;
            }

        }

        // UPDATE METHODS

        public virtual void UpdateExecutionTenantIdForDeployment(string deploymentId, string newTenantId)
        {
            executionDataManager.UpdateExecutionTenantIdForDeployment(deploymentId, newTenantId);
        }

        // DELETE METHODS

        public virtual void DeleteProcessInstancesByProcessDefinition(string processDefinitionId, string deleteReason, bool cascade)
        {
            IList<string> processInstanceIds = executionDataManager.FindProcessInstanceIdsByProcessDefinitionId(processDefinitionId);

            foreach (string processInstanceId in processInstanceIds)
            {
                DeleteProcessInstance(processInstanceId, deleteReason, cascade);
            }

            if (cascade)
            {
                HistoricProcessInstanceEntityManager.DeleteHistoricProcessInstanceByProcessDefinitionId(processDefinitionId);
            }
        }

        public virtual void DeleteProcessInstance(string processInstanceId, string deleteReason, bool cascade)
        {
            IExecutionEntity execution = FindById<IExecutionEntity>(new KeyValuePair<string, object>("id", processInstanceId));

            if (execution == null)
            {
                throw new ActivitiObjectNotFoundException("No process instance found for id '" + processInstanceId + "'", typeof(IProcessInstance));
            }

            DeleteProcessInstanceCascade(execution, deleteReason, cascade);
        }

        protected internal virtual void DeleteProcessInstanceCascade(IExecutionEntity execution, string deleteReason, bool deleteHistory)
        {

            // fill default reason if none provided
            if (deleteReason is null)
            {
                deleteReason = DeleteReasonFields.PROCESS_INSTANCE_DELETED;
            }

            foreach (IExecutionEntity subExecutionEntity in execution.Executions)
            {
                if (subExecutionEntity.IsMultiInstanceRoot)
                {
                    foreach (IExecutionEntity miExecutionEntity in subExecutionEntity.Executions)
                    {
                        if (miExecutionEntity.SubProcessInstance != null)
                        {
                            DeleteProcessInstanceCascade(miExecutionEntity.SubProcessInstance, deleteReason, deleteHistory);
                        }
                    }

                }
                else if (subExecutionEntity.SubProcessInstance != null)
                {
                    DeleteProcessInstanceCascade(subExecutionEntity.SubProcessInstance, deleteReason, deleteHistory);
                }
            }

            TaskEntityManager.DeleteTasksByProcessInstanceId(execution.Id, deleteReason, deleteHistory);

            if (EventDispatcher.Enabled)
            {
                EventDispatcher.DispatchEvent(ActivitiEventBuilder.CreateCancelledEvent(execution.ProcessInstanceId, execution.ProcessInstanceId, null, deleteReason));
            }

            // delete the execution BEFORE we delete the history, otherwise we will
            // produce orphan HistoricVariableInstance instances

            IExecutionEntity processInstanceExecutionEntity = execution.ProcessInstance;
            if (processInstanceExecutionEntity == null)
            {
                return;
            }

            IList<IExecutionEntity> childExecutions = CollectChildren(execution.ProcessInstance);
            for (int i = childExecutions.Count - 1; i >= 0; i--)
            {
                IExecutionEntity childExecutionEntity = childExecutions[i];
                DeleteExecutionAndRelatedData(childExecutionEntity, deleteReason, false);
            }

            DeleteExecutionAndRelatedData(execution, deleteReason, false);

            if (deleteHistory)
            {
                HistoricProcessInstanceEntityManager.Delete(new KeyValuePair<string, object>("id", execution.Id));
            }

            HistoryManager.RecordProcessInstanceEnd(processInstanceExecutionEntity.Id, deleteReason, null);
            processInstanceExecutionEntity.Deleted = true;
        }

        public virtual void DeleteExecutionAndRelatedData(IExecutionEntity executionEntity, string deleteReason, bool cancel)
        {
            HistoryManager.RecordActivityEnd(executionEntity, deleteReason);
            DeleteDataForExecution(executionEntity, deleteReason, cancel);
            Delete(executionEntity);
        }

        public virtual void DeleteProcessInstanceExecutionEntity(string processInstanceId, string currentFlowElementId, string deleteReason, bool cascade, bool cancel)
        {

            IExecutionEntity processInstanceEntity = FindById<IExecutionEntity>(new KeyValuePair<string, object>("id", processInstanceId));

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
                    DeleteProcessInstanceCascade(subExecutionEntity.SubProcessInstance, deleteReason, cascade);
                }
            }

            // delete event scope executions
            foreach (IExecutionEntity childExecution in processInstanceEntity.Executions)
            {
                if (childExecution.IsEventScope)
                {
                    DeleteExecutionAndRelatedData(childExecution, null, false);
                }
            }

            DeleteChildExecutions(processInstanceEntity, deleteReason, cancel);
            DeleteExecutionAndRelatedData(processInstanceEntity, deleteReason, cancel);

            if (EventDispatcher.Enabled)
            {
                if (!cancel)
                {
                    EventDispatcher.DispatchEvent(ActivitiEventBuilder.CreateEntityEvent(ActivitiEventType.PROCESS_COMPLETED, processInstanceEntity));
                }
                else
                {
                    EventDispatcher.DispatchEvent(ActivitiEventBuilder.CreateCancelledEvent(processInstanceEntity.Id, processInstanceEntity.Id, processInstanceEntity.ProcessDefinitionId, deleteReason));
                }
            }

            // TODO: what about delete reason?
            HistoryManager.RecordProcessInstanceEnd(processInstanceEntity.Id, deleteReason, currentFlowElementId);
            processInstanceEntity.Deleted = true;
        }

        public virtual void DeleteChildExecutions(IExecutionEntity executionEntity, string deleteReason, bool cancel)
        {

            // The children of an execution for a tree. For correct deletions
            // (taking care of foreign keys between child-parent)
            // the leafs of this tree must be deleted first before the parents elements.
            IList<IExecutionEntity> childExecutions = CollectChildren(executionEntity);
            for (int i = childExecutions.Count - 1; i >= 0; i--)
            {
                IExecutionEntity childExecutionEntity = childExecutions[i];
                if (!childExecutionEntity.Ended)
                {
                    DeleteExecutionAndRelatedData(childExecutionEntity, deleteReason, cancel);
                }
            }
        }

        public virtual IList<IExecutionEntity> CollectChildren(IExecutionEntity executionEntity)
        {
            IList<IExecutionEntity> childExecutions = new List<IExecutionEntity>();
            CollectChildren(executionEntity, childExecutions);
            return childExecutions;
        }

        protected internal virtual void CollectChildren(IExecutionEntity executionEntity, IList<IExecutionEntity> collectedChildExecution)
        {
            IList<IExecutionEntity> childExecutions = executionEntity.Executions;
            if (childExecutions != null && childExecutions.Count > 0)
            {
                foreach (IExecutionEntity childExecution in childExecutions)
                {
                    if (!childExecution.Deleted)
                    {
                        collectedChildExecution.Add(childExecution);
                        CollectChildren(childExecution, collectedChildExecution);
                    }
                }
            }

            IExecutionEntity subProcessInstance = executionEntity.SubProcessInstance;
            if (subProcessInstance != null && !subProcessInstance.Deleted)
            {
                collectedChildExecution.Add(subProcessInstance);
                CollectChildren(subProcessInstance, collectedChildExecution);
            }
        }

        public virtual IExecutionEntity FindFirstScope(IExecutionEntity executionEntity)
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

        public virtual IExecutionEntity FindFirstMultiInstanceRoot(IExecutionEntity executionEntity)
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

        public virtual void DeleteDataForExecution(IExecutionEntity executionEntity, string deleteReason, bool cancel)
        {

            // To start, deactivate the current incoming execution
            executionEntity.Ended = true;
            executionEntity.IsActive = false;

            bool enableExecutionRelationshipCounts = IsExecutionRelatedEntityCountEnabled(executionEntity);

            if (executionEntity.Id.Equals(executionEntity.ProcessInstanceId) && (!enableExecutionRelationshipCounts || (enableExecutionRelationshipCounts && ((ICountingExecutionEntity)executionEntity).IdentityLinkCount > 0)))
            {
                IIdentityLinkEntityManager identityLinkEntityManager = IdentityLinkEntityManager;
                ICollection<IIdentityLinkEntity> identityLinks = identityLinkEntityManager.FindIdentityLinksByProcessInstanceId(executionEntity.ProcessInstanceId);
                foreach (IIdentityLinkEntity identityLink in identityLinks)
                {
                    identityLinkEntityManager.Delete(identityLink);
                }
            }

            // Get variables related to execution and delete them
            if (!enableExecutionRelationshipCounts || (enableExecutionRelationshipCounts && ((ICountingExecutionEntity)executionEntity).VariableCount > 0))
            {
                ICollection<IVariableInstance> executionVariables = executionEntity.VariableInstancesLocal.Values;
                foreach (IVariableInstance variableInstance in executionVariables)
                {
                    if (variableInstance is IVariableInstanceEntity variableInstanceEntity)
                    {
                        IVariableInstanceEntityManager variableInstanceEntityManager = VariableInstanceEntityManager;
                        variableInstanceEntityManager.Delete(variableInstanceEntity);
                        if (variableInstanceEntity.ByteArrayRef != null && variableInstanceEntity.ByteArrayRef.Id is object)
                        {
                            ByteArrayEntityManager.DeleteByteArrayById(variableInstanceEntity.ByteArrayRef.Id);
                        }
                    }
                }
            }

            // Delete current user tasks
            if (!enableExecutionRelationshipCounts || (enableExecutionRelationshipCounts && ((ICountingExecutionEntity)executionEntity).TaskCount > 0))
            {
                ITaskEntityManager taskEntityManager = TaskEntityManager;
                ICollection<ITaskEntity> tasksForExecution = taskEntityManager.FindTasksByExecutionId(executionEntity.Id);
                foreach (ITaskEntity taskEntity in tasksForExecution)
                {
                    taskEntityManager.DeleteTask(taskEntity, deleteReason, false, cancel);
                }
            }

            // Delete jobs

            if (!enableExecutionRelationshipCounts || (enableExecutionRelationshipCounts && ((ICountingExecutionEntity)executionEntity).TimerJobCount > 0))
            {
                ITimerJobEntityManager timerJobEntityManager = TimerJobEntityManager;
                ICollection<ITimerJobEntity> timerJobsForExecution = timerJobEntityManager.FindJobsByExecutionId(executionEntity.Id);
                foreach (ITimerJobEntity job in timerJobsForExecution)
                {
                    timerJobEntityManager.Delete(job);
                    if (EventDispatcher.Enabled)
                    {
                        EventDispatcher.DispatchEvent(ActivitiEventBuilder.CreateEntityEvent(ActivitiEventType.JOB_CANCELED, job));
                    }
                }
            }

            if (!enableExecutionRelationshipCounts || (enableExecutionRelationshipCounts && ((ICountingExecutionEntity)executionEntity).JobCount > 0))
            {
                IJobEntityManager jobEntityManager = JobEntityManager;
                ICollection<IJobEntity> jobsForExecution = jobEntityManager.FindJobsByExecutionId(executionEntity.Id);
                foreach (IJobEntity job in jobsForExecution)
                {
                    JobEntityManager.Delete(job);
                    if (EventDispatcher.Enabled)
                    {
                        EventDispatcher.DispatchEvent(ActivitiEventBuilder.CreateEntityEvent(ActivitiEventType.JOB_CANCELED, job));
                    }
                }
            }

            if (!enableExecutionRelationshipCounts || (enableExecutionRelationshipCounts && ((ICountingExecutionEntity)executionEntity).SuspendedJobCount > 0))
            {
                ISuspendedJobEntityManager suspendedJobEntityManager = SuspendedJobEntityManager;
                ICollection<ISuspendedJobEntity> suspendedJobsForExecution = suspendedJobEntityManager.FindJobsByExecutionId(executionEntity.Id);
                foreach (ISuspendedJobEntity job in suspendedJobsForExecution)
                {
                    suspendedJobEntityManager.Delete(job);
                    if (EventDispatcher.Enabled)
                    {
                        EventDispatcher.DispatchEvent(ActivitiEventBuilder.CreateEntityEvent(ActivitiEventType.JOB_CANCELED, job));
                    }
                }
            }

            if (!enableExecutionRelationshipCounts || (enableExecutionRelationshipCounts && ((ICountingExecutionEntity)executionEntity).DeadLetterJobCount > 0))
            {
                IDeadLetterJobEntityManager deadLetterJobEntityManager = DeadLetterJobEntityManager;
                ICollection<IDeadLetterJobEntity> deadLetterJobsForExecution = deadLetterJobEntityManager.FindJobsByExecutionId(executionEntity.Id);
                foreach (IDeadLetterJobEntity job in deadLetterJobsForExecution)
                {
                    deadLetterJobEntityManager.Delete(job);
                    if (EventDispatcher.Enabled)
                    {
                        EventDispatcher.DispatchEvent(ActivitiEventBuilder.CreateEntityEvent(ActivitiEventType.JOB_CANCELED, job));
                    }
                }
            }

            // Delete event subscriptions
            if (!enableExecutionRelationshipCounts || (enableExecutionRelationshipCounts && ((ICountingExecutionEntity)executionEntity).EventSubscriptionCount > 0))
            {
                IEventSubscriptionEntityManager eventSubscriptionEntityManager = EventSubscriptionEntityManager;
                IList<IEventSubscriptionEntity> eventSubscriptions = eventSubscriptionEntityManager.FindEventSubscriptionsByExecution(executionEntity.Id);
                foreach (IEventSubscriptionEntity eventSubscription in eventSubscriptions)
                {
                    eventSubscriptionEntityManager.Delete(eventSubscription);
                }
            }

        }

        // OTHER METHODS

        public virtual void UpdateProcessInstanceLockTime(string processInstanceId)
        {
            DateTime expirationTime = Clock.CurrentTime;
            int lockMillis = AsyncExecutor.AsyncJobLockTimeInMillis;

            DateTime lockCal = DateTime.Now;
            lockCal.AddMilliseconds(lockMillis);

            executionDataManager.UpdateProcessInstanceLockTime(processInstanceId, lockCal, expirationTime);
        }

        public virtual void ClearProcessInstanceLockTime(string processInstanceId)
        {
            executionDataManager.ClearProcessInstanceLockTime(processInstanceId);
        }

        public virtual string UpdateProcessInstanceBusinessKey(IExecutionEntity executionEntity, string businessKey)
        {
            if (executionEntity.ProcessInstanceType && businessKey is object)
            {
                executionEntity.BusinessKey = businessKey;
                HistoryManager.UpdateProcessBusinessKeyInHistory(executionEntity);

                if (EventDispatcher.Enabled)
                {
                    EventDispatcher.DispatchEvent(ActivitiEventBuilder.CreateEntityEvent(ActivitiEventType.ENTITY_UPDATED, executionEntity));
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