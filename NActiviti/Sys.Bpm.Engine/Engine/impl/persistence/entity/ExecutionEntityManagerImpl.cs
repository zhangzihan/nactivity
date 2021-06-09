using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Sys.Net.Http;
using Sys.Workflow;
using Sys.Workflow.Engine.Bpmn.Rules;
using Sys.Workflow.Engine.Delegate.Events;
using Sys.Workflow.Engine.Delegate.Events.Impl;
using Sys.Workflow.Engine.History;
using Sys.Workflow.Engine.Impl.Cfg;
using Sys.Workflow.Engine.Impl.Contexts;
using Sys.Workflow.Engine.Impl.Identities;
using Sys.Workflow.Engine.Impl.Persistence.Entity.Data;
using Sys.Workflow.Engine.Repository;
using Sys.Workflow.Engine.Runtime;
using Sys.Workflow.Engine.Tasks;
using Sys.Workflow.Services.Api.Commands;

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
    /// <summary>
    /// 
    /// </summary>
    public class ExecutionEntityManagerImpl : AbstractEntityManager<IExecutionEntity>, IExecutionEntityManager
    {
        /// <summary>
        /// 
        /// </summary>
        protected internal IExecutionDataManager executionDataManager;

        private readonly ILogger<ExecutionEntityManagerImpl> logger = ProcessEngineServiceProvider.LoggerService<ExecutionEntityManagerImpl>();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="processEngineConfiguration"></param>
        /// <param name="executionDataManager"></param>
        public ExecutionEntityManagerImpl(ProcessEngineConfigurationImpl processEngineConfiguration, IExecutionDataManager executionDataManager) : base(processEngineConfiguration)
        {
            this.executionDataManager = executionDataManager;
        }
        /// <summary>
        /// 
        /// </summary>
        protected internal override IDataManager<IExecutionEntity> DataManager
        {
            get
            {
                return executionDataManager;
            }
        }

        // Overriding the default delete methods to set the 'isDeleted' flag
        /// <summary>
        /// 
        /// </summary>
        /// <param name="entity"></param>
        public override void Delete(IExecutionEntity entity)
        {
            Delete(entity, true);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="fireDeleteEvent"></param>
        public override void Delete(IExecutionEntity entity, bool fireDeleteEvent)
        {
            base.Delete(entity, fireDeleteEvent);
            entity.Deleted = true;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="entity"></param>
        public override void Insert(IExecutionEntity entity)
        {
            base.Insert(entity);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="fireCreateEvent"></param>
        public override void Insert(IExecutionEntity entity, bool fireCreateEvent)
        {
            base.Insert(entity, fireCreateEvent);
        }

        // FIND METHODS
        /// <summary>
        /// 
        /// </summary>
        /// <param name="superExecutionId"></param>
        /// <returns></returns>
        public virtual IExecutionEntity FindSubProcessInstanceBySuperExecutionId(string superExecutionId)
        {
            return executionDataManager.FindSubProcessInstanceBySuperExecutionId(superExecutionId);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="parentExecutionId"></param>
        /// <returns></returns>
        public virtual IList<IExecutionEntity> FindChildExecutionsByParentExecutionId(string parentExecutionId)
        {
            return executionDataManager.FindChildExecutionsByParentExecutionId(parentExecutionId);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="processInstanceId"></param>
        /// <returns></returns>
        public virtual IList<IExecutionEntity> FindChildExecutionsByProcessInstanceId(string processInstanceId)
        {
            return executionDataManager.FindChildExecutionsByProcessInstanceId(processInstanceId);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="parentExecutionId"></param>
        /// <param name="activityIds"></param>
        /// <returns></returns>
        public virtual IList<IExecutionEntity> FindExecutionsByParentExecutionAndActivityIds(string parentExecutionId, ICollection<string> activityIds)
        {
            return executionDataManager.FindExecutionsByParentExecutionAndActivityIds(parentExecutionId, activityIds);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="executionQuery"></param>
        /// <returns></returns>
        public virtual long FindExecutionCountByQueryCriteria(IExecutionQuery executionQuery)
        {
            return executionDataManager.FindExecutionCountByQueryCriteria(executionQuery);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="executionQuery"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        public virtual IList<IExecutionEntity> FindExecutionsByQueryCriteria(IExecutionQuery executionQuery, Page page)
        {
            return executionDataManager.FindExecutionsByQueryCriteria(executionQuery, page);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="executionQuery"></param>
        /// <returns></returns>
        public virtual long FindProcessInstanceCountByQueryCriteria(IProcessInstanceQuery executionQuery)
        {
            return executionDataManager.FindProcessInstanceCountByQueryCriteria(executionQuery);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="executionQuery"></param>
        /// <returns></returns>
        public virtual IList<IProcessInstance> FindProcessInstanceByQueryCriteria(IProcessInstanceQuery executionQuery)
        {
            return executionDataManager.FindProcessInstanceByQueryCriteria(executionQuery);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="rootProcessInstanceId"></param>
        /// <returns></returns>
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
                if (executionEntity.SuperExecution is object)
                {
                    IExecutionEntity superExecutionEntity = executionMap[executionEntity.SuperExecutionId];
                    executionEntity.SuperExecution = superExecutionEntity;
                    superExecutionEntity.SubProcessInstance = executionEntity;
                }

            }
            return rootExecution;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="executionQuery"></param>
        /// <returns></returns>
        public virtual IList<IProcessInstance> FindProcessInstanceAndVariablesByQueryCriteria(ProcessInstanceQueryImpl executionQuery)
        {
            return executionDataManager.FindProcessInstanceAndVariablesByQueryCriteria(executionQuery);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="processInstanceId"></param>
        /// <returns></returns>
        public virtual ICollection<IExecutionEntity> FindInactiveExecutionsByProcessInstanceId(string processInstanceId)
        {
            return executionDataManager.FindInactiveExecutionsByProcessInstanceId(processInstanceId);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="activityId"></param>
        /// <param name="processInstanceId"></param>
        /// <returns></returns>
        public virtual ICollection<IExecutionEntity> FindInactiveExecutionsByActivityIdAndProcessInstanceId(string activityId, string processInstanceId)
        {
            return executionDataManager.FindInactiveExecutionsByActivityIdAndProcessInstanceId(activityId, processInstanceId);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="parameterMap"></param>
        /// <param name="firstResult"></param>
        /// <param name="maxResults"></param>
        /// <returns></returns>
        public virtual IList<IExecution> FindExecutionsByNativeQuery(IDictionary<string, object> parameterMap, int firstResult, int maxResults)
        {
            return executionDataManager.FindExecutionsByNativeQuery(parameterMap, firstResult, maxResults);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="parameterMap"></param>
        /// <param name="firstResult"></param>
        /// <param name="maxResults"></param>
        /// <returns></returns>
        public virtual IList<IProcessInstance> FindProcessInstanceByNativeQuery(IDictionary<string, object> parameterMap, int firstResult, int maxResults)
        {
            return executionDataManager.FindProcessInstanceByNativeQuery(parameterMap, firstResult, maxResults);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="parameterMap"></param>
        /// <returns></returns>
        public virtual long FindExecutionCountByNativeQuery(IDictionary<string, object> parameterMap)
        {
            return executionDataManager.FindExecutionCountByNativeQuery(parameterMap);
        }

        // CREATE METHODS
        /// <summary>
        /// 
        /// </summary>
        /// <param name="processDefinition"></param>
        /// <param name="businessKey"></param>
        /// <param name="tenantId"></param>
        /// <param name="initiatorVariableName"></param>
        /// <returns></returns>
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

            // Store in database
            Insert(processInstanceExecution, false);

            IUserInfo starter = null;
            if (string.IsNullOrWhiteSpace(authenticatedUserId) == false)
            {
                starter = GetUser(processInstanceExecution, authenticatedUserId);

                //保存调用用户变量
                processInstanceExecution.SetVariable(processInstanceExecution.StartUserId, starter);
            }
            processInstanceExecution.StartUser = starter.FullName;

            if (initiatorVariableName is object)
            {
                processInstanceExecution.SetVariable(initiatorVariableName, authenticatedUserId);
            }

            // Need to be after insert, cause we need the id
            processInstanceExecution.ProcessInstanceId = processInstanceExecution.Id;
            processInstanceExecution.RootProcessInstanceId = processInstanceExecution.Id;
            //TODO: 当没有用户任务的时候，添加identity会报错
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

        private IUserInfo GetUser(IExecutionEntity execution, string authenticatedUserId)
        {
            IUserInfo starter;
            IUserServiceProxy userService = ProcessEngineServiceProvider.Resolve<IUserServiceProxy>();

            try
            {
                starter = userService.GetUser(authenticatedUserId)
                    .ConfigureAwait(false)
                    .GetAwaiter()
                    .GetResult();
                starter.TenantId = execution.TenantId;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                starter = new UserInfo
                {
                    Id = execution.StartUserId,
                    FullName = execution.StartUserId,
                    TenantId = execution.TenantId
                };
            }

            return starter;
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="processDefinition"></param>
        /// <param name="superExecutionEntity"></param>
        /// <param name="businessKey"></param>
        /// <returns></returns>
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
            IUserInfo starter = null;
            if (string.IsNullOrWhiteSpace(Authentication.AuthenticatedUser.Id) == false)
            {
                starter = GetUser(subProcessInstance, Authentication.AuthenticatedUser.Id);

                //保存调用用户变量
                subProcessInstance.SetVariable(subProcessInstance.StartUserId, starter);
            }
            subProcessInstance.StartUser = starter.FullName;
            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug("Child execution {} created with super execution {}", subProcessInstance, superExecutionEntity.Id);
            }

            subProcessInstance.ProcessInstanceId = subProcessInstance.Id;
            superExecutionEntity.SubProcessInstance = subProcessInstance;

            ProcessEngineConfigurationImpl processEngineConfiguration1 = Context.ProcessEngineConfiguration;
            if (processEngineConfiguration1 is object && processEngineConfiguration1.EventDispatcher.Enabled)
            {
                processEngineConfiguration1.EventDispatcher.DispatchEvent(ActivitiEventBuilder.CreateEntityEvent(ActivitiEventType.ENTITY_CREATED, subProcessInstance));
            }

            return subProcessInstance;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="parentExecutionEntity"></param>
        /// <param name="childExecution"></param>
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="deploymentId"></param>
        /// <param name="newTenantId"></param>
        public virtual void UpdateExecutionTenantIdForDeployment(string deploymentId, string newTenantId)
        {
            executionDataManager.UpdateExecutionTenantIdForDeployment(deploymentId, newTenantId);
        }

        // DELETE METHODS
        /// <summary>
        /// 
        /// </summary>
        /// <param name="processDefinitionId"></param>
        /// <param name="deleteReason"></param>
        /// <param name="cascade"></param>
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="processInstanceId"></param>
        /// <param name="deleteReason"></param>
        /// <param name="cascade"></param>
        public virtual void DeleteProcessInstance(string processInstanceId, string deleteReason, bool cascade)
        {
            IExecutionEntity execution = FindById<IExecutionEntity>(new KeyValuePair<string, object>("id", processInstanceId));

            if (execution is null)
            {
                throw new ActivitiObjectNotFoundException("No process instance found for id '" + processInstanceId + "'", typeof(IProcessInstance));
            }

            DeleteProcessInstanceCascade(execution, deleteReason, cascade);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="execution"></param>
        /// <param name="deleteReason"></param>
        /// <param name="deleteHistory"></param>
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
                        if (miExecutionEntity.SubProcessInstance is object)
                        {
                            DeleteProcessInstanceCascade(miExecutionEntity.SubProcessInstance, deleteReason, deleteHistory);
                        }
                    }

                }
                else if (subExecutionEntity.SubProcessInstance is object)
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
            if (processInstanceExecutionEntity is null)
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="executionEntity"></param>
        /// <param name="deleteReason"></param>
        /// <param name="cancel"></param>
        public virtual void DeleteExecutionAndRelatedData(IExecutionEntity executionEntity, string deleteReason, bool cancel)
        {
            HistoryManager.RecordActivityEnd(executionEntity, deleteReason);
            DeleteDataForExecution(executionEntity, deleteReason, cancel);
            Delete(executionEntity);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="processInstanceId"></param>
        /// <param name="currentFlowElementId"></param>
        /// <param name="deleteReason"></param>
        /// <param name="cascade"></param>
        /// <param name="cancel"></param>
        public virtual void DeleteProcessInstanceExecutionEntity(string processInstanceId, string currentFlowElementId, string deleteReason, bool cascade, bool cancel)
        {

            IExecutionEntity processInstanceEntity = FindById<IExecutionEntity>(new KeyValuePair<string, object>("id", processInstanceId));

            if (processInstanceEntity is null)
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
                if (subExecutionEntity.SubProcessInstance is object && !subExecutionEntity.Ended)
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="executionEntity"></param>
        /// <param name="deleteReason"></param>
        /// <param name="cancel"></param>
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="executionEntity"></param>
        /// <returns></returns>
        public virtual IList<IExecutionEntity> CollectChildren(IExecutionEntity executionEntity)
        {
            IList<IExecutionEntity> childExecutions = new List<IExecutionEntity>();
            CollectChildren(executionEntity, childExecutions);
            return childExecutions;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="executionEntity"></param>
        /// <param name="collectedChildExecution"></param>
        protected internal virtual void CollectChildren(IExecutionEntity executionEntity, IList<IExecutionEntity> collectedChildExecution)
        {
            IList<IExecutionEntity> childExecutions = executionEntity.Executions;
            if (childExecutions is object && childExecutions.Count > 0)
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
            if (subProcessInstance is object && !subProcessInstance.Deleted)
            {
                collectedChildExecution.Add(subProcessInstance);
                CollectChildren(subProcessInstance, collectedChildExecution);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="executionEntity"></param>
        /// <returns></returns>
        public virtual IExecutionEntity FindFirstScope(IExecutionEntity executionEntity)
        {
            IExecutionEntity currentExecutionEntity = executionEntity;
            while (currentExecutionEntity is object)
            {
                if (currentExecutionEntity.IsScope)
                {
                    return currentExecutionEntity;
                }

                IExecutionEntity parentExecutionEntity = currentExecutionEntity.Parent;
                if (parentExecutionEntity is null)
                {
                    parentExecutionEntity = currentExecutionEntity.SuperExecution;
                }
                currentExecutionEntity = parentExecutionEntity;
            }
            return null;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="executionEntity"></param>
        /// <returns></returns>
        public virtual IExecutionEntity FindFirstMultiInstanceRoot(IExecutionEntity executionEntity)
        {
            IExecutionEntity currentExecutionEntity = executionEntity;
            while (currentExecutionEntity is object)
            {
                if (currentExecutionEntity.IsMultiInstanceRoot)
                {
                    return currentExecutionEntity;
                }

                IExecutionEntity parentExecutionEntity = currentExecutionEntity.Parent;
                if (parentExecutionEntity is null)
                {
                    parentExecutionEntity = currentExecutionEntity.SuperExecution;
                }
                currentExecutionEntity = parentExecutionEntity;
            }
            return null;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="executionEntity"></param>
        /// <param name="deleteReason"></param>
        /// <param name="cancel"></param>
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
                        if (variableInstanceEntity.ByteArrayRef is object && variableInstanceEntity.ByteArrayRef.Id is object)
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="processInstanceId"></param>
        public virtual void UpdateProcessInstanceLockTime(string processInstanceId)
        {
            DateTime expirationTime = Clock.CurrentTime;
            int lockMillis = AsyncExecutor.AsyncJobLockTimeInMillis;

            DateTime lockCal = DateTime.Now;
            lockCal.AddMilliseconds(lockMillis);

            executionDataManager.UpdateProcessInstanceLockTime(processInstanceId, lockCal, expirationTime);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="processInstanceId"></param>
        public virtual void ClearProcessInstanceLockTime(string processInstanceId)
        {
            executionDataManager.ClearProcessInstanceLockTime(processInstanceId);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="executionEntity"></param>
        /// <param name="businessKey"></param>
        /// <returns></returns>
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
        /// <summary>
        /// 
        /// </summary>
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