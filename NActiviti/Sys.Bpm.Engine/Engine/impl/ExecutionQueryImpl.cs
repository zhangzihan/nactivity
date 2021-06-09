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
namespace Sys.Workflow.Engine.Impl
{
    using Newtonsoft.Json.Linq;
    using Sys.Workflow.Engine.Impl.Contexts;
    using Sys.Workflow.Engine.Impl.Interceptor;
    using Sys.Workflow.Engine.Impl.Persistence.Entity;
    using Sys.Workflow.Engine.Repository;
    using Sys.Workflow.Engine.Runtime;
    using System.Linq;

    /// 
    /// 
    /// 
    public class ExecutionQueryImpl : AbstractVariableQueryImpl<IExecutionQuery, IExecution>, IExecutionQuery
    {

        private const long serialVersionUID = 1L;

        protected internal string processDefinitionId_;

        protected internal string processDefinitionKey_;

        protected internal string processDefinitionCategory_;

        protected internal string processDefinitionName_;

        protected internal int? processDefinitionVersion_;

        protected internal string activityId_;

        protected internal string executionId_;

        protected internal string parentId_;

        protected internal bool onlyChildExecutions_;

        protected internal bool onlySubProcessExecutions_;

        protected internal bool onlyProcessInstanceExecutions_;

        protected internal string processInstanceId_;

        protected internal string rootProcessInstanceId_;
        protected internal IList<EventSubscriptionQueryValue> eventSubscriptions;

        protected internal string tenantId;
        protected internal string tenantIdLike;
        protected internal bool withoutTenantId;

        protected internal string locale_Renamed;

        protected internal bool withLocalizationFallback_;

        protected internal DateTime? startedBefore_;
        protected internal DateTime? startedAfter_;

        protected internal string startedBy_;

        // Not used by end-users, but needed for dynamic ibatis query
        protected internal string superProcessInstanceId;
        protected internal string subProcessInstanceId;
        protected internal bool excludeSubprocesses;
        protected internal ISuspensionState suspensionState;
        protected internal string businessKey;
        protected internal bool includeChildExecutionsWithBusinessKeyQuery;
        protected internal bool isActive;
        protected internal string involvedUser;

        protected internal string[] processDefinitionKeys_;
        protected internal string[] processDefinitionIds;

        private bool isWithException = false;

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

        public virtual IExecutionQuery SetProcessDefinitionId(string processDefinitionId)
        {
            //if (string.IsNullOrWhiteSpace(processDefinitionId))
            //{
            //    throw new ActivitiIllegalArgumentException("Process definition id is null");
            //}
            this.processDefinitionId_ = processDefinitionId;
            return this;
        }

        public virtual IExecutionQuery SetProcessDeploymentId(string deploymentId)
        {
            //if (string.IsNullOrWhiteSpace(deploymentId))
            //{
            //    throw new ActivitiIllegalArgumentException("Process definition id is null");
            //}
            this.deploymentId = deploymentId;
            return this;
        }

        public virtual IExecutionQuery SetProcessDefinitionKey(string processDefinitionKey)
        {
            //if (string.IsNullOrWhiteSpace(processDefinitionKey))
            //{
            //    throw new ActivitiIllegalArgumentException("Process definition key is null");
            //}
            this.processDefinitionKey_ = processDefinitionKey;
            return this;
        }

        public virtual IExecutionQuery SetProcessDefinitionCategory(string processDefinitionCategory)
        {
            //if (string.IsNullOrWhiteSpace(processDefinitionCategory))
            //{
            //    throw new ActivitiIllegalArgumentException("Process definition category is null");
            //}
            this.processDefinitionCategory_ = processDefinitionCategory;
            return this;
        }

        public virtual IExecutionQuery SetProcessDefinitionName(string processDefinitionName)
        {
            //if (string.IsNullOrWhiteSpace(processDefinitionName))
            //{
            //    throw new ActivitiIllegalArgumentException("Process definition name is null");
            //}
            this.processDefinitionName_ = processDefinitionName;
            return this;
        }

        public virtual IExecutionQuery SetProcessDefinitionVersion(int? processDefinitionVersion)
        {
            //if (processDefinitionVersion is null)
            //{
            //    throw new ActivitiIllegalArgumentException("Process definition version is null");
            //}
            this.processDefinitionVersion_ = processDefinitionVersion;
            return this;
        }

        public virtual IExecutionQuery SetProcessInstanceId(string processInstanceId)
        {
            //if (ReferenceEquals(processInstanceId, null))
            //{
            //    throw new ActivitiIllegalArgumentException("Process instance id is null");
            //}
            this.processInstanceId_ = processInstanceId;
            return this;
        }

        public virtual IExecutionQuery SetRootProcessInstanceId(string rootProcessInstanceId)
        {
            //if (string.IsNullOrWhiteSpace(rootProcessInstanceId))
            //{
            //    throw new ActivitiIllegalArgumentException("Root process instance id is null");
            //}
            this.rootProcessInstanceId_ = rootProcessInstanceId;
            return this;
        }

        public virtual IExecutionQuery SetProcessInstanceBusinessKey(string businessKey)
        {
            //if (string.IsNullOrWhiteSpace(businessKey))
            //{
            //    throw new ActivitiIllegalArgumentException("Business key is null");
            //}
            this.businessKey = businessKey;
            return this;
        }

        public virtual IExecutionQuery ProcessInstanceBusinessKey(string processInstanceBusinessKey, bool includeChildExecutions)
        {
            if (!includeChildExecutions)
            {
                return this.SetProcessInstanceBusinessKey(processInstanceBusinessKey);
            }
            else
            {
                //if (string.IsNullOrWhiteSpace(processInstanceBusinessKey))
                //{
                //    throw new ActivitiIllegalArgumentException("Business key is null");
                //}
                this.businessKey = processInstanceBusinessKey;
                this.includeChildExecutionsWithBusinessKeyQuery = includeChildExecutions;
                return this;
            }
        }

        public virtual IExecutionQuery SetProcessDefinitionKeys(string[] processDefinitionKeys)
        {
            //if (processDefinitionKeys is null)
            //{
            //    throw new ActivitiIllegalArgumentException("Process definition keys is null");
            //}
            this.processDefinitionKeys_ = processDefinitionKeys;
            return this;
        }

        public virtual IExecutionQuery SetExecutionId(string executionId)
        {
            //if (string.IsNullOrWhiteSpace(executionId))
            //{
            //    throw new ActivitiIllegalArgumentException("Execution id is null");
            //}
            this.executionId_ = executionId;
            return this;
        }

        public virtual IExecutionQuery SetActivityId(string activityId)
        {
            this.activityId_ = activityId;

            if (!string.IsNullOrWhiteSpace(activityId))
            {
                isActive = true;
            }
            return this;
        }

        public virtual IExecutionQuery SetParentId(string parentId)
        {
            //if (string.IsNullOrWhiteSpace(parentId))
            //{
            //    throw new ActivitiIllegalArgumentException("Parent id is null");
            //}
            this.parentId_ = parentId;
            return this;
        }

        public virtual IExecutionQuery SetOnlyChildExecutions()
        {
            this.onlyChildExecutions_ = true;
            return this;
        }

        public virtual IExecutionQuery SetOnlySubProcessExecutions()
        {
            this.onlySubProcessExecutions_ = true;
            return this;
        }

        public virtual IExecutionQuery SetOnlyProcessInstanceExecutions()
        {
            this.onlyProcessInstanceExecutions_ = true;
            return this;
        }

        public virtual IExecutionQuery SetExecutionTenantId(string tenantId)
        {
            //if (tenantId is null)
            //{
            //    throw new ActivitiIllegalArgumentException("execution tenant id is null");
            //}
            this.tenantId = tenantId;
            return this;
        }

        public virtual IExecutionQuery SetExecutionTenantIdLike(string tenantIdLike)
        {
            //if (tenantIdLike is null)
            //{
            //    throw new ActivitiIllegalArgumentException("execution tenant id is null");
            //}
            this.tenantIdLike = tenantIdLike;
            return this;
        }

        public virtual IExecutionQuery SetExecutionWithoutTenantId()
        {
            this.withoutTenantId = true;
            return this;
        }

        public virtual IExecutionQuery SignalEventSubscription(string signalName)
        {
            return EventSubscription("signal", signalName);
        }

        public virtual IExecutionQuery SignalEventSubscriptionName(string signalName)
        {
            return EventSubscription("signal", signalName);
        }

        public virtual IExecutionQuery MessageEventSubscriptionName(string messageName)
        {
            return EventSubscription("message", messageName);
        }

        public virtual IExecutionQuery EventSubscription(string eventType, string eventName)
        {
            if (string.IsNullOrWhiteSpace(eventName))
            {
                throw new ActivitiIllegalArgumentException("event name is null");
            }
            if (string.IsNullOrWhiteSpace(eventType))
            {
                throw new ActivitiIllegalArgumentException("event type is null");
            }
            if (eventSubscriptions is null)
            {
                eventSubscriptions = new List<EventSubscriptionQueryValue>();
            }
            eventSubscriptions.Add(new EventSubscriptionQueryValue(eventName, eventType));
            return this;
        }

        public virtual IExecutionQuery ProcessVariableValueEquals(string variableName, object variableValue)
        {
            return VariableValueEquals(variableName, variableValue, false);
        }

        public virtual IExecutionQuery ProcessVariableValueEquals(object variableValue)
        {
            return VariableValueEquals(variableValue, false);
        }

        public virtual IExecutionQuery ProcessVariableValueNotEquals(string variableName, object variableValue)
        {
            return VariableValueNotEquals(variableName, variableValue, false);
        }

        public virtual IExecutionQuery ProcessVariableValueEqualsIgnoreCase(string name, string value)
        {
            return VariableValueEqualsIgnoreCase(name, value, false);
        }

        public virtual IExecutionQuery ProcessVariableValueNotEqualsIgnoreCase(string name, string value)
        {
            return VariableValueNotEqualsIgnoreCase(name, value, false);
        }

        public virtual IExecutionQuery ProcessVariableValueLike(string name, string value)
        {
            return VariableValueLike(name, value, false);
        }

        public virtual IExecutionQuery ProcessVariableValueLikeIgnoreCase(string name, string value)
        {
            return VariableValueLikeIgnoreCase(name, value, false);
        }

        public virtual IExecutionQuery Locale(string locale)
        {
            this.locale_Renamed = locale;
            return this;
        }

        public virtual IExecutionQuery WithLocalizationFallback()
        {
            withLocalizationFallback_ = true;
            return this;
        }

        public virtual IExecutionQuery SetStartedBefore(DateTime beforeTime)
        {
            //if (beforeTime is null)
            //{
            //    throw new ActivitiIllegalArgumentException("before time is null");
            //}
            this.startedBefore_ = beforeTime;

            return this;
        }

        public virtual IExecutionQuery SetStartedAfter(DateTime afterTime)
        {
            //if (afterTime is null)
            //{
            //    throw new ActivitiIllegalArgumentException("after time is null");
            //}
            this.startedAfter_ = afterTime;

            return this;
        }

        public virtual IExecutionQuery SetStartedBy(string userId)
        {
            //if (string.IsNullOrWhiteSpace(userId))
            //{
            //    throw new ActivitiIllegalArgumentException("user id is null");
            //}
            this.startedBy_ = userId;

            return this;
        }

        // ordering ////////////////////////////////////////////////////

        public virtual IExecutionQuery OrderByProcessInstanceId()
        {
            this.orderProperty = ExecutionQueryProperty.PROCESS_INSTANCE_ID;
            return this;
        }

        public virtual IExecutionQuery OrderByProcessDefinitionId()
        {
            this.orderProperty = ExecutionQueryProperty.PROCESS_DEFINITION_ID;
            return this;
        }

        public virtual IExecutionQuery OrderByProcessDefinitionKey()
        {
            this.orderProperty = ExecutionQueryProperty.PROCESS_DEFINITION_KEY;
            return this;
        }

        public virtual IExecutionQuery OrderByTenantId()
        {
            this.orderProperty = ExecutionQueryProperty.TENANT_ID;
            return this;
        }

        // results ////////////////////////////////////////////////////

        public override long ExecuteCount(ICommandContext commandContext)
        {
            CheckQueryOk();
            EnsureVariablesInitialized();
            return commandContext.ExecutionEntityManager.FindExecutionCountByQueryCriteria(this);
        }

        public override IList<IExecution> ExecuteList(ICommandContext commandContext, Page page)
        {
            CheckQueryOk();
            EnsureVariablesInitialized();

            IList<IExecutionEntity> executions = commandContext.ExecutionEntityManager.FindExecutionsByQueryCriteria(this, page) ?? new List<IExecutionEntity>();

            if (Context.ProcessEngineConfiguration.PerformanceSettings.EnableLocalization)
            {
                foreach (IExecutionEntity execution in executions)
                {
                    string activityId = null;
                    if (execution.Id.Equals(execution.ProcessInstanceId))
                    {
                        if (execution.ProcessDefinitionId is object)
                        {
                            IProcessDefinition processDefinition = commandContext.ProcessEngineConfiguration.DeploymentManager.FindDeployedProcessDefinitionById(execution.ProcessDefinitionId);
                            activityId = processDefinition.Key;
                        }

                    }
                    else
                    {
                        activityId = execution.ActivityId;
                    }

                    if (activityId is object)
                    {
                        Localize(execution, activityId);
                    }
                }
            }

            return executions.Cast<IExecution>().ToList();
        }

        protected internal virtual void Localize(IExecution execution, string activityId)
        {
            IExecutionEntity executionEntity = (IExecutionEntity)execution;
            executionEntity.LocalizedName = null;
            executionEntity.LocalizedDescription = null;

            string processDefinitionId = executionEntity.ProcessDefinitionId;
            if (!string.IsNullOrWhiteSpace(locale_Renamed) && !string.IsNullOrWhiteSpace(processDefinitionId))
            {
                JToken languageNode = Context.GetLocalizationElementProperties(locale_Renamed, activityId, processDefinitionId, withLocalizationFallback_);
                if (languageNode is object)
                {
                    JToken languageNameNode = languageNode[DynamicBpmnConstants.LOCALIZATION_NAME];
                    if (languageNameNode is object)
                    {
                        executionEntity.LocalizedName = languageNameNode.ToString();
                    }

                    JToken languageDescriptionNode = languageNode[DynamicBpmnConstants.LOCALIZATION_DESCRIPTION];
                    if (languageDescriptionNode is object)
                    {
                        executionEntity.LocalizedDescription = languageDescriptionNode.ToString();
                    }
                }
            }
        }

        public virtual IExecutionQuery SetIsWithException()
        {
            isWithException = true;

            return this;
        }

        public IList<string> DeploymentIds
        {
            get => deploymentIds;
            set => deploymentIds = value;
        }

        public virtual IList<ExecutionQueryImpl> OrQueryObjects
        {
            get => orQueryObjects;
            set => orQueryObjects = value;
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
                return processDefinitionKey_;
            }
            set => SetProcessDefinitionKey(value);
        }

        public virtual string ProcessDefinitionId
        {
            get
            {
                return processDefinitionId_;
            }
            set => SetProcessDefinitionId(value);
        }

        public virtual string ProcessDefinitionCategory
        {
            get
            {
                return processDefinitionCategory_;
            }
            set => SetProcessDefinitionCategory(value);
        }

        public virtual string ProcessDefinitionName
        {
            get
            {
                return processDefinitionName_;
            }
            set => SetProcessDefinitionName(value);
        }

        public virtual int? ProcessDefinitionVersion
        {
            get
            {
                return processDefinitionVersion_;
            }
            set => SetProcessDefinitionVersion(value);
        }

        public virtual string ActivityId
        {
            get
            {
                return activityId_;
            }
            set => SetActivityId(value);
        }

        public virtual string ProcessInstanceId
        {
            get
            {
                return processInstanceId_;
            }
            set => SetProcessInstanceId(value);
        }

        public virtual string RootProcessInstanceId
        {
            get
            {
                return rootProcessInstanceId_;
            }
            set => SetRootProcessInstanceId(value);
        }

        public virtual string[] ProcessInstanceIds
        {
            get;
            set;
        }

        public virtual string BusinessKey
        {
            get
            {
                return businessKey;
            }
            set => SetProcessInstanceBusinessKey(value);
        }

        public virtual string ExecutionId
        {
            get
            {
                return executionId_;
            }
            set => SetExecutionId(value);
        }

        public virtual string SuperProcessInstanceId
        {
            get
            {
                return superProcessInstanceId;
            }
            set => superProcessInstanceId = value;
        }

        public virtual string SubProcessInstanceId
        {
            get
            {
                return subProcessInstanceId;
            }
            set => subProcessInstanceId = value;
        }

        public virtual bool ExcludeSubprocesses
        {
            get
            {
                return excludeSubprocesses;
            }
            set => excludeSubprocesses = value;
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
            set => includeChildExecutionsWithBusinessKeyQuery = value;
        }


        public virtual bool Active
        {
            get
            {
                return isActive;
            }
            set => isActive = value;
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


        public virtual string[] ProcessDefinitionIds
        {
            get
            {
                return processDefinitionIds;
            }
            set => processDefinitionIds = value;
        }

        public virtual string[] ProcessDefinitionKeys
        {
            get
            {
                return processDefinitionKeys_;
            }
            set => SetProcessDefinitionKeys(value);
        }

        public virtual string ParentId
        {
            get
            {
                return parentId_;
            }
            set => SetParentId(value);
        }

        public virtual bool OnlyChildExecutions
        {
            get
            {
                return onlyChildExecutions_;
            }
            set
            {
                if (value)
                {
                    SetOnlyChildExecutions();
                }
                else
                {
                    onlyChildExecutions_ = false;
                }
            }
        }

        public virtual bool OnlySubProcessExecutions
        {
            get
            {
                return onlySubProcessExecutions_;
            }
            set
            {
                if (value)
                {
                    SetOnlySubProcessExecutions();
                }
                else
                {
                    onlySubProcessExecutions_ = false;
                }
            }
        }

        public virtual bool OnlyProcessInstanceExecutions
        {
            get
            {
                return onlyProcessInstanceExecutions_;
            }
            set
            {
                if (value)
                {
                    SetOnlyProcessInstanceExecutions();
                }
                else
                {
                    onlyProcessInstanceExecutions_ = false;
                }
            }
        }

        public virtual string TenantId
        {
            get
            {
                return tenantId;
            }
            set => SetExecutionTenantId(value);
        }

        public virtual string TenantIdLike
        {
            get
            {
                return tenantIdLike;
            }
            set => SetExecutionTenantIdLike(value);
        }

        public virtual bool WithoutTenantId
        {
            get
            {
                return withoutTenantId;
            }
            set
            {
                if (value)
                {
                    SetExecutionWithoutTenantId();
                }
                else
                {
                    withoutTenantId = value;
                }
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
                return startedBefore_;
            }
            set
            {
                this.startedBefore_ = value;
            }
        }


        public virtual DateTime? StartedAfter
        {
            get
            {
                return startedAfter_;
            }
            set
            {
                this.startedAfter_ = value;
            }
        }


        public virtual string StartedBy
        {
            get
            {
                return startedBy_;
            }
            set
            {
                this.startedBy_ = value;
            }
        }

        public virtual string DeploymentId
        {
            get => deploymentId;
            set => SetProcessDeploymentId(value);
        }

        public virtual bool IsWithException
        {
            get => isWithException;
            set
            {
                if (value)
                {
                    SetIsWithException();
                }
                else
                {
                    isWithException = false;
                }
            }
        }
    }
}