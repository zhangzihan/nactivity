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
namespace org.activiti.engine.impl
{
    using Newtonsoft.Json.Linq;
    using org.activiti.engine.impl.context;
    using org.activiti.engine.impl.interceptor;
    using org.activiti.engine.impl.persistence.entity;
    using org.activiti.engine.repository;
    using org.activiti.engine.runtime;
    using System.Linq;

    /// 
    /// 
    /// 
    public class ExecutionQueryImpl : AbstractVariableQueryImpl<IExecutionQuery, IExecution>, IExecutionQuery
    {

        private const long serialVersionUID = 1L;

        protected internal string processDefinitionId_Renamed;

        protected internal string processDefinitionKey_Renamed;

        protected internal string processDefinitionCategory_Renamed;

        protected internal string processDefinitionName_Renamed;

        protected internal int? processDefinitionVersion_Renamed;

        protected internal string activityId_Renamed;

        protected internal string executionId_Renamed;

        protected internal string parentId_Renamed;

        protected internal bool onlyChildExecutions_Renamed;

        protected internal bool onlySubProcessExecutions_Renamed;

        protected internal bool onlyProcessInstanceExecutions_Renamed;

        protected internal string processInstanceId_Renamed;

        protected internal string rootProcessInstanceId_Renamed;
        protected internal IList<EventSubscriptionQueryValue> eventSubscriptions;

        protected internal string tenantId;
        protected internal string tenantIdLike;
        protected internal bool withoutTenantId;

        protected internal string locale_Renamed;

        protected internal bool withLocalizationFallback_Renamed;

        protected internal DateTime? startedBefore_Renamed;
        protected internal DateTime? startedAfter_Renamed;

        protected internal string startedBy_Renamed;

        // Not used by end-users, but needed for dynamic ibatis query
        protected internal string superProcessInstanceId;
        protected internal string subProcessInstanceId;
        protected internal bool excludeSubprocesses;
        protected internal ISuspensionState suspensionState;
        protected internal string businessKey;
        protected internal bool includeChildExecutionsWithBusinessKeyQuery;
        protected internal bool isActive;
        protected internal string involvedUser;

        protected internal ISet<string> processDefinitionKeys_Renamed;
        protected internal ISet<string> processDefinitionIds;

        // Not exposed in API, but here for the ProcessInstanceQuery support, since
        // the name lives on the
        // Execution entity/table
        protected internal string name;
        protected internal string nameLike;
        protected internal string nameLikeIgnoreCase;
        protected internal string deploymentId;
        protected internal IList<string> deploymentIds;
        protected internal IList<ExecutionQueryImpl> orQueryObjects = new List<ExecutionQueryImpl>();

        public ExecutionQueryImpl()
        {
        }

        public ExecutionQueryImpl(ICommandContext commandContext) : base(commandContext)
        {
        }

        public ExecutionQueryImpl(ICommandExecutor commandExecutor) : base(commandExecutor)
        {
        }

        public virtual bool ProcessInstancesOnly
        {
            get
            {
                return false; // see dynamic query
            }
        }

        public virtual IExecutionQuery processDefinitionId(string processDefinitionId)
        {
            if (string.IsNullOrWhiteSpace(processDefinitionId))
            {
                throw new ActivitiIllegalArgumentException("Process definition id is null");
            }
            this.processDefinitionId_Renamed = processDefinitionId;
            return this;
        }

        public virtual IExecutionQuery processDefinitionKey(string processDefinitionKey)
        {
            if (string.IsNullOrWhiteSpace(processDefinitionKey))
            {
                throw new ActivitiIllegalArgumentException("Process definition key is null");
            }
            this.processDefinitionKey_Renamed = processDefinitionKey;
            return this;
        }

        public virtual IExecutionQuery processDefinitionCategory(string processDefinitionCategory)
        {
            if (string.IsNullOrWhiteSpace(processDefinitionCategory))
            {
                throw new ActivitiIllegalArgumentException("Process definition category is null");
            }
            this.processDefinitionCategory_Renamed = processDefinitionCategory;
            return this;
        }

        public virtual IExecutionQuery processDefinitionName(string processDefinitionName)
        {
            if (string.IsNullOrWhiteSpace(processDefinitionName))
            {
                throw new ActivitiIllegalArgumentException("Process definition name is null");
            }
            this.processDefinitionName_Renamed = processDefinitionName;
            return this;
        }

        public virtual IExecutionQuery processDefinitionVersion(int? processDefinitionVersion)
        {
            if (processDefinitionVersion == null)
            {
                throw new ActivitiIllegalArgumentException("Process definition version is null");
            }
            this.processDefinitionVersion_Renamed = processDefinitionVersion;
            return this;
        }

        public virtual IExecutionQuery processInstanceId(string processInstanceId)
        {
            if (ReferenceEquals(processInstanceId, null))
            {
                throw new ActivitiIllegalArgumentException("Process instance id is null");
            }
            this.processInstanceId_Renamed = processInstanceId;
            return this;
        }

        public virtual IExecutionQuery rootProcessInstanceId(string rootProcessInstanceId)
        {
            if (string.IsNullOrWhiteSpace(rootProcessInstanceId))
            {
                throw new ActivitiIllegalArgumentException("Root process instance id is null");
            }
            this.rootProcessInstanceId_Renamed = rootProcessInstanceId;
            return this;
        }

        public virtual IExecutionQuery processInstanceBusinessKey(string businessKey)
        {
            if (string.IsNullOrWhiteSpace(businessKey))
            {
                throw new ActivitiIllegalArgumentException("Business key is null");
            }
            this.businessKey = businessKey;
            return this;
        }

        public virtual IExecutionQuery processInstanceBusinessKey(string processInstanceBusinessKey, bool includeChildExecutions)
        {
            if (!includeChildExecutions)
            {
                return this.processInstanceBusinessKey(processInstanceBusinessKey);
            }
            else
            {
                if (string.IsNullOrWhiteSpace(processInstanceBusinessKey))
                {
                    throw new ActivitiIllegalArgumentException("Business key is null");
                }
                this.businessKey = processInstanceBusinessKey;
                this.includeChildExecutionsWithBusinessKeyQuery = includeChildExecutions;
                return this;
            }
        }

        public virtual IExecutionQuery processDefinitionKeys(ISet<string> processDefinitionKeys)
        {
            if (processDefinitionKeys == null)
            {
                throw new ActivitiIllegalArgumentException("Process definition keys is null");
            }
            this.processDefinitionKeys_Renamed = processDefinitionKeys;
            return this;
        }

        public virtual IExecutionQuery executionId(string executionId)
        {
            if (string.IsNullOrWhiteSpace(executionId))
            {
                throw new ActivitiIllegalArgumentException("Execution id is null");
            }
            this.executionId_Renamed = executionId;
            return this;
        }

        public virtual IExecutionQuery activityId(string activityId)
        {
            this.activityId_Renamed = activityId;

            if (!string.IsNullOrWhiteSpace(activityId))
            {
                isActive = true;
            }
            return this;
        }

        public virtual IExecutionQuery parentId(string parentId)
        {
            if (string.IsNullOrWhiteSpace(parentId))
            {
                throw new ActivitiIllegalArgumentException("Parent id is null");
            }
            this.parentId_Renamed = parentId;
            return this;
        }

        public virtual IExecutionQuery onlyChildExecutions()
        {
            this.onlyChildExecutions_Renamed = true;
            return this;
        }

        public virtual IExecutionQuery onlySubProcessExecutions()
        {
            this.onlySubProcessExecutions_Renamed = true;
            return this;
        }

        public virtual IExecutionQuery onlyProcessInstanceExecutions()
        {
            this.onlyProcessInstanceExecutions_Renamed = true;
            return this;
        }

        public virtual IExecutionQuery executionTenantId(string tenantId)
        {
            if (tenantId == null)
            {
                throw new ActivitiIllegalArgumentException("execution tenant id is null");
            }
            this.tenantId = tenantId;
            return this;
        }

        public virtual IExecutionQuery executionTenantIdLike(string tenantIdLike)
        {
            if (tenantIdLike == null)
            {
                throw new ActivitiIllegalArgumentException("execution tenant id is null");
            }
            this.tenantIdLike = tenantIdLike;
            return this;
        }

        public virtual IExecutionQuery executionWithoutTenantId()
        {
            this.withoutTenantId = true;
            return this;
        }

        public virtual IExecutionQuery signalEventSubscription(string signalName)
        {
            return eventSubscription("signal", signalName);
        }

        public virtual IExecutionQuery signalEventSubscriptionName(string signalName)
        {
            return eventSubscription("signal", signalName);
        }

        public virtual IExecutionQuery messageEventSubscriptionName(string messageName)
        {
            return eventSubscription("message", messageName);
        }

        public virtual IExecutionQuery eventSubscription(string eventType, string eventName)
        {
            if (string.IsNullOrWhiteSpace(eventName))
            {
                throw new ActivitiIllegalArgumentException("event name is null");
            }
            if (string.IsNullOrWhiteSpace(eventType))
            {
                throw new ActivitiIllegalArgumentException("event type is null");
            }
            if (eventSubscriptions == null)
            {
                eventSubscriptions = new List<EventSubscriptionQueryValue>();
            }
            eventSubscriptions.Add(new EventSubscriptionQueryValue(eventName, eventType));
            return this;
        }

        public virtual IExecutionQuery processVariableValueEquals(string variableName, object variableValue)
        {
            return variableValueEquals(variableName, variableValue, false);
        }

        public virtual IExecutionQuery processVariableValueEquals(object variableValue)
        {
            return variableValueEquals(variableValue, false);
        }

        public virtual IExecutionQuery processVariableValueNotEquals(string variableName, object variableValue)
        {
            return variableValueNotEquals(variableName, variableValue, false);
        }

        public virtual IExecutionQuery processVariableValueEqualsIgnoreCase(string name, string value)
        {
            return variableValueEqualsIgnoreCase(name, value, false);
        }

        public virtual IExecutionQuery processVariableValueNotEqualsIgnoreCase(string name, string value)
        {
            return variableValueNotEqualsIgnoreCase(name, value, false);
        }

        public virtual IExecutionQuery processVariableValueLike(string name, string value)
        {
            return variableValueLike(name, value, false);
        }

        public virtual IExecutionQuery processVariableValueLikeIgnoreCase(string name, string value)
        {
            return variableValueLikeIgnoreCase(name, value, false);
        }

        public virtual IExecutionQuery locale(string locale)
        {
            this.locale_Renamed = locale;
            return this;
        }

        public virtual IExecutionQuery withLocalizationFallback()
        {
            withLocalizationFallback_Renamed = true;
            return this;
        }

        public virtual IExecutionQuery startedBefore(DateTime beforeTime)
        {
            if (beforeTime == null)
            {
                throw new ActivitiIllegalArgumentException("before time is null");
            }
            this.startedBefore_Renamed = beforeTime;

            return this;
        }

        public virtual IExecutionQuery startedAfter(DateTime afterTime)
        {
            if (afterTime == null)
            {
                throw new ActivitiIllegalArgumentException("after time is null");
            }
            this.startedAfter_Renamed = afterTime;

            return this;
        }

        public virtual IExecutionQuery startedBy(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ActivitiIllegalArgumentException("user id is null");
            }
            this.startedBy_Renamed = userId;

            return this;
        }

        // ordering ////////////////////////////////////////////////////

        public virtual IExecutionQuery orderByProcessInstanceId()
        {
            this.orderProperty = ExecutionQueryProperty.PROCESS_INSTANCE_ID;
            return this;
        }

        public virtual IExecutionQuery orderByProcessDefinitionId()
        {
            this.orderProperty = ExecutionQueryProperty.PROCESS_DEFINITION_ID;
            return this;
        }

        public virtual IExecutionQuery orderByProcessDefinitionKey()
        {
            this.orderProperty = ExecutionQueryProperty.PROCESS_DEFINITION_KEY;
            return this;
        }

        public virtual IExecutionQuery orderByTenantId()
        {
            this.orderProperty = ExecutionQueryProperty.TENANT_ID;
            return this;
        }

        // results ////////////////////////////////////////////////////

        public override long executeCount(ICommandContext commandContext)
        {
            checkQueryOk();
            ensureVariablesInitialized();
            return commandContext.ExecutionEntityManager.findExecutionCountByQueryCriteria(this);
        }

        public override IList<IExecution> executeList(ICommandContext commandContext, Page page)
        {
            checkQueryOk();
            ensureVariablesInitialized();

            IList<IExecutionEntity> executions = commandContext.ExecutionEntityManager.findExecutionsByQueryCriteria(this, page) ?? new List<IExecutionEntity>();

            if (Context.ProcessEngineConfiguration.PerformanceSettings.EnableLocalization)
            {
                foreach (IExecutionEntity execution in (IList<IExecutionEntity>)executions)
                {
                    string activityId = null;
                    if (execution.Id.Equals(execution.ProcessInstanceId))
                    {
                        if (!ReferenceEquals(execution.ProcessDefinitionId, null))
                        {
                            IProcessDefinition processDefinition = commandContext.ProcessEngineConfiguration.DeploymentManager.findDeployedProcessDefinitionById(execution.ProcessDefinitionId);
                            activityId = processDefinition.Key;
                        }

                    }
                    else
                    {
                        activityId = execution.ActivityId;
                    }

                    if (!ReferenceEquals(activityId, null))
                    {
                        localize(execution, activityId);
                    }
                }
            }

            return executions.Cast<IExecution>().ToList();
        }

        protected internal virtual void localize(IExecution execution, string activityId)
        {
            IExecutionEntity executionEntity = (IExecutionEntity)execution;
            executionEntity.LocalizedName = null;
            executionEntity.LocalizedDescription = null;

            string processDefinitionId = executionEntity.ProcessDefinitionId;
            if (!string.IsNullOrWhiteSpace(locale_Renamed) && !string.IsNullOrWhiteSpace(processDefinitionId))
            {
                JToken languageNode = Context.getLocalizationElementProperties(locale_Renamed, activityId, processDefinitionId, withLocalizationFallback_Renamed);
                if (languageNode != null)
                {
                    JToken languageNameNode = languageNode[DynamicBpmnConstants_Fields.LOCALIZATION_NAME];
                    if (languageNameNode != null)
                    {
                        executionEntity.LocalizedName = languageNameNode.ToString();
                    }

                    JToken languageDescriptionNode = languageNode[DynamicBpmnConstants_Fields.LOCALIZATION_DESCRIPTION];
                    if (languageDescriptionNode != null)
                    {
                        executionEntity.LocalizedDescription = languageDescriptionNode.ToString();
                    }
                }
            }
        }

        // getters ////////////////////////////////////////////////////

        public virtual bool OnlyProcessInstances
        {
            get
            {
                return false;
            }
        }

        public virtual string ProcessDefinitionKey
        {
            get
            {
                return processDefinitionKey_Renamed;
            }
        }

        public virtual string ProcessDefinitionId
        {
            get
            {
                return processDefinitionId_Renamed;
            }
        }

        public virtual string ProcessDefinitionCategory
        {
            get
            {
                return processDefinitionCategory_Renamed;
            }
        }

        public virtual string ProcessDefinitionName
        {
            get
            {
                return processDefinitionName_Renamed;
            }
        }

        public virtual int? ProcessDefinitionVersion
        {
            get
            {
                return processDefinitionVersion_Renamed;
            }
        }

        public virtual string ActivityId
        {
            get
            {
                return activityId_Renamed;
            }
        }

        public virtual string ProcessInstanceId
        {
            get
            {
                return processInstanceId_Renamed;
            }
        }

        public virtual string RootProcessInstanceId
        {
            get
            {
                return rootProcessInstanceId_Renamed;
            }
        }

        public virtual string ProcessInstanceIds
        {
            get
            {
                return null;
            }
        }

        public virtual string BusinessKey
        {
            get
            {
                return businessKey;
            }
        }

        public virtual string ExecutionId
        {
            get
            {
                return executionId_Renamed;
            }
        }

        public virtual string SuperProcessInstanceId
        {
            get
            {
                return superProcessInstanceId;
            }
        }

        public virtual string SubProcessInstanceId
        {
            get
            {
                return subProcessInstanceId;
            }
        }

        public virtual bool ExcludeSubprocesses
        {
            get
            {
                return excludeSubprocesses;
            }
        }

        public virtual ISuspensionState SuspensionState
        {
            get
            {
                return suspensionState;
            }
            set
            {
                this.suspensionState = value;
            }
        }


        public virtual IList<EventSubscriptionQueryValue> EventSubscriptions
        {
            get
            {
                return eventSubscriptions;
            }
            set
            {
                this.eventSubscriptions = value;
            }
        }

        public virtual bool IncludeChildExecutionsWithBusinessKeyQuery
        {
            get
            {
                return includeChildExecutionsWithBusinessKeyQuery;
            }
        }


        public virtual bool Active
        {
            get
            {
                return isActive;
            }
        }

        public virtual string InvolvedUser
        {
            get
            {
                return involvedUser;
            }
            set
            {
                this.involvedUser = value;
            }
        }


        public virtual ISet<string> ProcessDefinitionIds
        {
            get
            {
                return processDefinitionIds;
            }
        }

        public virtual ISet<string> ProcessDefinitionKeys
        {
            get
            {
                return processDefinitionKeys_Renamed;
            }
        }

        public virtual string ParentId
        {
            get
            {
                return parentId_Renamed;
            }
        }

        public virtual bool OnlyChildExecutions
        {
            get
            {
                return onlyChildExecutions_Renamed;
            }
        }

        public virtual bool OnlySubProcessExecutions
        {
            get
            {
                return onlySubProcessExecutions_Renamed;
            }
        }

        public virtual bool OnlyProcessInstanceExecutions
        {
            get
            {
                return onlyProcessInstanceExecutions_Renamed;
            }
        }

        public virtual string TenantId
        {
            get
            {
                return tenantId;
            }
        }

        public virtual string TenantIdLike
        {
            get
            {
                return tenantIdLike;
            }
        }

        public virtual bool WithoutTenantId
        {
            get
            {
                return withoutTenantId;
            }
        }

        public virtual string Name
        {
            get
            {
                return name;
            }
            set
            {
                this.name = value;
            }
        }

        public virtual string NameLike
        {
            get
            {
                return nameLike;
            }
            set
            {
                this.nameLike = value;
            }
        }



        public virtual string NameLikeIgnoreCase
        {
            get
            {
                return nameLikeIgnoreCase;
            }
            set
            {
                this.nameLikeIgnoreCase = value;
            }
        }


        public virtual DateTime? StartedBefore
        {
            get
            {
                return startedBefore_Renamed;
            }
            set
            {
                this.startedBefore_Renamed = value;
            }
        }


        public virtual DateTime? StartedAfter
        {
            get
            {
                return startedAfter_Renamed;
            }
            set
            {
                this.startedAfter_Renamed = value;
            }
        }


        public virtual string StartedBy
        {
            get
            {
                return startedBy_Renamed;
            }
            set
            {
                this.startedBy_Renamed = value;
            }
        }

    }

}