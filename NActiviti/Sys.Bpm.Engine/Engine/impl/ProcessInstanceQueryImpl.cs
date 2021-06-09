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
    using Sys.Workflow.Engine.Runtime;


    /// 
    /// 
    /// 
    /// 
    /// 
    /// 
    [Serializable]
    public class ProcessInstanceQueryImpl : AbstractVariableQueryImpl<IProcessInstanceQuery, IProcessInstance>, IProcessInstanceQuery
    {
        private const long serialVersionUID = 1L;
        protected internal string executionId;
        protected internal string businessKey;
        protected internal bool? includeChildExecutionsWithBusinessKeyQuery;

        protected internal string _processDefinitionId;

        protected internal string[] _processDefinitionIds;

        protected internal string _processDefinitionCategory;

        protected internal string _processDefinitionName;

        protected internal int? _processDefinitionVersion;

        protected internal string[] _processInstanceIds;

        protected internal string _processDefinitionKey;

        protected internal string[] _processDefinitionKeys;

        protected internal string _deploymentId;
        protected internal IList<string> deploymentIds;

        protected internal string _superProcessInstanceId;

        protected internal string _subProcessInstanceId;

        protected internal bool? _excludeSubprocesses;

        protected internal string _involvedUser;
        protected internal ISuspensionState suspensionState;

        protected internal bool? _includeProcessVariables;
        protected internal int? processInstanceVariablesLimit;

        protected internal bool? _withJobException;
        protected internal string name;
        protected internal string nameLike;
        protected internal string nameLikeIgnoreCase;

        protected internal string _locale;

        protected internal bool? _withLocalizationFallback;

        protected internal string tenantId;
        protected internal string tenantIdLike;
        protected internal bool? withoutTenantId;

        protected internal IList<IProcessInstanceQuery> orQueryObjects = new List<IProcessInstanceQuery>();
        protected internal ProcessInstanceQueryImpl currentOrQueryObject = null;
        protected internal bool inOrStatement = false;

        protected internal DateTime? _startedBefore;
        protected internal DateTime? _startedAfter;

        protected internal string _startedBy;

        // Unused, see dynamic query
        protected internal string activityId;
        protected internal IList<EventSubscriptionQueryValue> eventSubscriptions;
        protected internal bool? onlyChildExecutions;
        protected internal bool? onlyProcessInstanceExecutions;
        protected internal bool? onlySubProcessExecutions;
        protected internal string rootProcessInstanceId;
        protected internal bool onlyProcessInstances = true;

        public ProcessInstanceQueryImpl()
        {
        }

        public ProcessInstanceQueryImpl(ICommandContext commandContext) : base(commandContext)
        {
        }

        public ProcessInstanceQueryImpl(ICommandExecutor commandExecutor) : base(commandExecutor)
        {
        }

        public virtual IProcessInstanceQuery SetProcessInstanceId(string processInstanceId)
        {
            //if (ReferenceEquals(processInstanceId, null))
            //{
            //    throw new ActivitiIllegalArgumentException("Process instance id is null");
            //}
            if (string.IsNullOrWhiteSpace(processInstanceId))
            {
                this.executionId = null;
                if (this.currentOrQueryObject is object)
                {
                    currentOrQueryObject.executionId = null;
                }
                return this;
            }
            if (inOrStatement)
            {
                this.currentOrQueryObject.executionId = processInstanceId;
            }
            else
            {
                this.executionId = processInstanceId;
            }
            return this;
        }

        public virtual IProcessInstanceQuery SetProcessInstanceIds(string[] processInstanceIds)
        {
            //if (processInstanceIds is null)
            //{
            //    throw new ActivitiIllegalArgumentException("Set of process instance ids is null");
            //}
            //if (processInstanceIds.Count == 0)
            //{
            //    throw new ActivitiIllegalArgumentException("Set of process instance ids is empty");
            //}
            if ((processInstanceIds ?? new string[0]).Length == 0)
            {
                this._processInstanceIds = null;
                if (this.currentOrQueryObject is object)
                {
                    this.currentOrQueryObject._processInstanceIds = null;
                }
                return this;
            }
            if (inOrStatement)
            {
                this.currentOrQueryObject._processInstanceIds = processInstanceIds;
            }
            else
            {
                this._processInstanceIds = processInstanceIds;
            }
            return this;
        }

        public virtual IProcessInstanceQuery SetProcessInstanceBusinessKey(string businessKey)
        {
            if (string.IsNullOrWhiteSpace(businessKey))
            {
                this.businessKey = null;
                if (this.currentOrQueryObject is object)
                {
                    this.currentOrQueryObject.businessKey = null;
                }
                //throw new ActivitiIllegalArgumentException("Business key is null");
                return this;
            }
            if (inOrStatement)
            {
                this.currentOrQueryObject.businessKey = businessKey;
            }
            else
            {
                this.businessKey = businessKey;
            }
            return this;
        }

        public virtual IProcessInstanceQuery SetProcessInstanceBusinessKey(string businessKey, string processDefinitionKey)
        {
            if (string.IsNullOrWhiteSpace(businessKey))
            {
                this.businessKey = null;
                if (this.currentOrQueryObject is object)
                {
                    this.currentOrQueryObject.businessKey = null;
                }
                return this;
                //throw new ActivitiIllegalArgumentException("Business key is null");
            }
            if (inOrStatement)
            {
                throw new ActivitiIllegalArgumentException("This method is not supported in an OR statement");
            }

            this.businessKey = businessKey;
            this._processDefinitionKey = processDefinitionKey;
            return this;
        }

        public virtual IProcessInstanceQuery SetProcessInstanceTenantId(string tenantId)
        {
            if (string.IsNullOrWhiteSpace(tenantId))
            {
                if (this.currentOrQueryObject is object)
                {
                    this.currentOrQueryObject.tenantId = null;
                }
                this.tenantId = "";
                return this;
                //throw new ActivitiIllegalArgumentException("process instance tenant id is null");
            }
            if (inOrStatement)
            {
                this.currentOrQueryObject.tenantId = tenantId;
            }
            else
            {
                this.tenantId = tenantId;
            }
            return this;
        }

        public virtual IProcessInstanceQuery SetProcessInstanceTenantIdLike(string tenantIdLike)
        {
            if (string.IsNullOrWhiteSpace(tenantIdLike))
            {
                this.tenantIdLike = null;
                if (this.currentOrQueryObject is object)
                {
                    this.currentOrQueryObject.tenantIdLike = null;
                }
                return this;
                //throw new ActivitiIllegalArgumentException("process instance tenant id is null");
            }
            if (inOrStatement)
            {
                this.currentOrQueryObject.tenantIdLike = tenantIdLike;
            }
            else
            {
                this.tenantIdLike = tenantIdLike;
            }
            return this;
        }

        public virtual IProcessInstanceQuery ProcessInstanceWithoutTenantId()
        {
            if (inOrStatement)
            {
                this.currentOrQueryObject.withoutTenantId = true;
            }
            else
            {
                this.withoutTenantId = true;
            }
            return this;
        }

        public virtual IProcessInstanceQuery SetProcessDefinitionCategory(string processDefinitionCategory)
        {
            if (string.IsNullOrWhiteSpace(processDefinitionCategory))
            {
                this._processDefinitionCategory = null;
                if (this.currentOrQueryObject is object)
                {
                    this.currentOrQueryObject._processDefinitionCategory = null;
                }
                return this;
                //throw new ActivitiIllegalArgumentException("Process definition category is null");
            }

            if (inOrStatement)
            {
                this.currentOrQueryObject._processDefinitionCategory = processDefinitionCategory;
            }
            else
            {
                this._processDefinitionCategory = processDefinitionCategory;
            }
            return this;
        }

        public virtual IProcessInstanceQuery SetProcessDefinitionName(string processDefinitionName)
        {
            if (string.IsNullOrWhiteSpace(processDefinitionName))
            {
                this._processDefinitionName = null;
                if (this.currentOrQueryObject is object)
                {
                    this.currentOrQueryObject._processDefinitionName = null;
                }
                return this;
                //throw new ActivitiIllegalArgumentException("Process definition name is null");
            }

            if (inOrStatement)
            {
                this.currentOrQueryObject._processDefinitionName = processDefinitionName;
            }
            else
            {
                this._processDefinitionName = processDefinitionName;
            }
            return this;
        }

        public virtual IProcessInstanceQuery SetProcessDefinitionVersion(int? processDefinitionVersion)
        {
            if (processDefinitionVersion.HasValue == false)
            {
                this._processDefinitionVersion = null;
                if (this.currentOrQueryObject is object)
                {
                    this.currentOrQueryObject._processDefinitionVersion = null;
                }
                return this;
                //throw new ActivitiIllegalArgumentException("Process definition version is null");
            }

            if (inOrStatement)
            {
                this.currentOrQueryObject._processDefinitionVersion = processDefinitionVersion;
            }
            else
            {
                this._processDefinitionVersion = processDefinitionVersion;
            }
            return this;
        }

        public virtual IProcessInstanceQuery SetProcessDefinitionId(string processDefinitionId)
        {
            if (string.IsNullOrWhiteSpace(processDefinitionId))
            {
                this._processDefinitionId = null;
                if (this.currentOrQueryObject is object)
                {
                    this.currentOrQueryObject._processDefinitionId = null;
                }
                return this;
                //throw new ActivitiIllegalArgumentException("Process definition id is null");
            }

            if (inOrStatement)
            {
                this.currentOrQueryObject._processDefinitionId = processDefinitionId;
            }
            else
            {
                this._processDefinitionId = processDefinitionId;
            }
            return this;
        }

        public virtual IProcessInstanceQuery SetProcessDefinitionIds(string[] processDefinitionIds)
        {
            if ((processDefinitionIds ?? new string[0]).Length == 0)
            {
                this._processDefinitionIds = null;
                if (this.currentOrQueryObject is object)
                {
                    this.currentOrQueryObject._processDefinitionIds = null;
                }
                return this;
                //throw new ActivitiIllegalArgumentException("Set of process definition ids is null");
            }

            if (inOrStatement)
            {
                this.currentOrQueryObject._processDefinitionIds = processDefinitionIds;
            }
            else
            {
                this._processDefinitionIds = processDefinitionIds;
            }
            return this;
        }

        public virtual IProcessInstanceQuery SetProcessDefinitionKey(string processDefinitionKey)
        {
            if (string.IsNullOrWhiteSpace(processDefinitionKey))
            {
                this._processDefinitionKey = null;
                if (this.currentOrQueryObject is object)
                {
                    this.currentOrQueryObject._processDefinitionKey = null;
                }
                return this;
                //throw new ActivitiIllegalArgumentException("Process definition key is null");
            }

            if (inOrStatement)
            {
                this.currentOrQueryObject._processDefinitionKey = processDefinitionKey;
            }
            else
            {
                this._processDefinitionKey = processDefinitionKey;
            }
            return this;
        }

        public virtual IProcessInstanceQuery SetProcessDefinitionKeys(string[] processDefinitionKeys)
        {
            if ((processDefinitionKeys ?? new string[0]).Length == 0)
            {
                this._processDefinitionKeys = null;
                if (this.currentOrQueryObject is object)
                {
                    this.currentOrQueryObject._processDefinitionKeys = null;
                }
                return this;
                //throw new ActivitiIllegalArgumentException("Set of process definition keys is null");
            }

            if (inOrStatement)
            {
                this.currentOrQueryObject._processDefinitionKeys = processDefinitionKeys;
            }
            else
            {
                this._processDefinitionKeys = processDefinitionKeys;
            }
            return this;
        }

        public virtual IProcessInstanceQuery SetDeploymentId(string deploymentId)
        {
            if (inOrStatement)
            {
                this.currentOrQueryObject._deploymentId = deploymentId;
            }
            else
            {
                this._deploymentId = deploymentId;
            }
            return this;
        }

        public virtual IProcessInstanceQuery SetDeploymentIdIn(IList<string> deploymentIds)
        {
            if (inOrStatement)
            {
                this.currentOrQueryObject.deploymentIds = deploymentIds;
            }
            else
            {
                this.deploymentIds = deploymentIds;
            }
            return this;
        }

        public virtual IProcessInstanceQuery SetSuperProcessInstanceId(string superProcessInstanceId)
        {
            if (inOrStatement)
            {
                this.currentOrQueryObject._superProcessInstanceId = superProcessInstanceId;
            }
            else
            {
                this._superProcessInstanceId = superProcessInstanceId;
            }
            return this;
        }

        public virtual IProcessInstanceQuery SetSubProcessInstanceId(string subProcessInstanceId)
        {
            if (inOrStatement)
            {
                this.currentOrQueryObject._subProcessInstanceId = subProcessInstanceId;
            }
            else
            {
                this._subProcessInstanceId = subProcessInstanceId;
            }
            return this;
        }

        public virtual IProcessInstanceQuery SetExcludeSubprocesses(bool excludeSubprocesses)
        {
            if (inOrStatement)
            {
                this.currentOrQueryObject._excludeSubprocesses = excludeSubprocesses;
            }
            else
            {
                this._excludeSubprocesses = excludeSubprocesses;
            }
            return this;
        }

        public virtual IProcessInstanceQuery SetInvolvedUser(string involvedUser)
        {
            if (string.IsNullOrWhiteSpace(involvedUser))
            {
                this._involvedUser = null;
                if (this.currentOrQueryObject is object)
                {
                    this.currentOrQueryObject._involvedUser = null;
                }
                return this;
                //throw new ActivitiIllegalArgumentException("Involved user is null");
            }

            if (inOrStatement)
            {
                this.currentOrQueryObject._involvedUser = involvedUser;
            }
            else
            {
                this._involvedUser = involvedUser;
            }
            return this;
        }

        public virtual IProcessInstanceQuery Active()
        {
            if (inOrStatement)
            {
                this.currentOrQueryObject.suspensionState = SuspensionStateProvider.ACTIVE;
            }
            else
            {
                this.suspensionState = SuspensionStateProvider.ACTIVE;
            }
            return this;
        }

        public virtual IProcessInstanceQuery Suspended()
        {
            if (inOrStatement)
            {
                this.currentOrQueryObject.suspensionState = SuspensionStateProvider.SUSPENDED;
            }
            else
            {
                this.suspensionState = SuspensionStateProvider.SUSPENDED;
            }
            return this;
        }

        public virtual IProcessInstanceQuery SetIncludeProcessVariables()
        {
            this._includeProcessVariables = true;
            return this;
        }

        public virtual IProcessInstanceQuery SetLimitProcessInstanceVariables(int? processInstanceVariablesLimit)
        {
            this.processInstanceVariablesLimit = processInstanceVariablesLimit;
            return this;
        }

        public virtual int? ProcessInstanceVariablesLimit
        {
            get
            {
                return processInstanceVariablesLimit;
            }
        }

        public virtual IProcessInstanceQuery WithJobException()
        {
            this._withJobException = true;
            return this;
        }

        public virtual IProcessInstanceQuery SetProcessInstanceName(string name)
        {
            if (inOrStatement)
            {
                this.currentOrQueryObject.name = name;
            }
            else
            {
                this.name = name;
            }
            return this;
        }

        public virtual IProcessInstanceQuery SetProcessInstanceNameLike(string nameLike)
        {
            if (inOrStatement)
            {
                this.currentOrQueryObject.nameLike = nameLike;
            }
            else
            {
                this.nameLike = nameLike;
            }
            return this;
        }

        public virtual IProcessInstanceQuery SetProcessInstanceNameLikeIgnoreCase(string nameLikeIgnoreCase)
        {
            if (inOrStatement)
            {
                this.currentOrQueryObject.nameLikeIgnoreCase = nameLikeIgnoreCase.ToLower();
            }
            else
            {
                this.nameLikeIgnoreCase = nameLikeIgnoreCase.ToLower();
            }
            return this;
        }

        public virtual IProcessInstanceQuery Or()
        {
            if (inOrStatement)
            {
                throw new ActivitiException("the query is already in an or statement");
            }

            inOrStatement = true;
            currentOrQueryObject = new ProcessInstanceQueryImpl();
            orQueryObjects.Add(currentOrQueryObject);
            return this;
        }

        public virtual IProcessInstanceQuery EndOr()
        {
            if (!inOrStatement)
            {
                throw new ActivitiException("endOr() can only be called after calling or()");
            }

            inOrStatement = false;
            currentOrQueryObject = null;
            return this;
        }

        public override IProcessInstanceQuery VariableValueEquals(string variableName, object variableValue)
        {
            if (inOrStatement)
            {
                currentOrQueryObject.VariableValueEquals(variableName, variableValue, false);
                return this;
            }
            else
            {
                return VariableValueEquals(variableName, variableValue, false);
            }
        }

        public override IProcessInstanceQuery VariableValueNotEquals(string variableName, object variableValue)
        {
            if (inOrStatement)
            {
                currentOrQueryObject.VariableValueNotEquals(variableName, variableValue, false);
                return this;
            }
            else
            {
                return VariableValueNotEquals(variableName, variableValue, false);
            }
        }

        public override IProcessInstanceQuery VariableValueEquals(object variableValue)
        {
            if (inOrStatement)
            {
                currentOrQueryObject.VariableValueEquals(variableValue, false);
                return this;
            }
            else
            {
                return VariableValueEquals(variableValue, false);
            }
        }

        public override IProcessInstanceQuery VariableValueEqualsIgnoreCase(string name, string value)
        {
            if (inOrStatement)
            {
                currentOrQueryObject.VariableValueEqualsIgnoreCase(name, value, false);
                return this;
            }
            else
            {
                return VariableValueEqualsIgnoreCase(name, value, false);
            }
        }

        public override IProcessInstanceQuery VariableValueNotEqualsIgnoreCase(string name, string value)
        {
            if (inOrStatement)
            {
                currentOrQueryObject.VariableValueNotEqualsIgnoreCase(name, value, false);
                return this;
            }
            else
            {
                return VariableValueNotEqualsIgnoreCase(name, value, false);
            }
        }

        public override IProcessInstanceQuery VariableValueGreaterThan(string name, object value)
        {
            if (inOrStatement)
            {
                currentOrQueryObject.VariableValueGreaterThan(name, value, false);
                return this;
            }
            else
            {
                return VariableValueGreaterThan(name, value, false);
            }
        }

        public override IProcessInstanceQuery VariableValueGreaterThanOrEqual(string name, object value)
        {
            if (inOrStatement)
            {
                currentOrQueryObject.VariableValueGreaterThanOrEqual(name, value, false);
                return this;
            }
            else
            {
                return VariableValueGreaterThanOrEqual(name, value, false);
            }
        }

        public override IProcessInstanceQuery VariableValueLessThan(string name, object value)
        {
            if (inOrStatement)
            {
                currentOrQueryObject.VariableValueLessThan(name, value, false);
                return this;
            }
            else
            {
                return VariableValueLessThan(name, value, false);
            }
        }

        public override IProcessInstanceQuery VariableValueLessThanOrEqual(string name, object value)
        {
            if (inOrStatement)
            {
                currentOrQueryObject.VariableValueLessThanOrEqual(name, value, false);
                return this;
            }
            else
            {
                return VariableValueLessThanOrEqual(name, value, false);
            }
        }

        public override IProcessInstanceQuery VariableValueLike(string name, string value)
        {
            if (inOrStatement)
            {
                currentOrQueryObject.VariableValueLike(name, value, false);
                return this;
            }
            else
            {
                return VariableValueLike(name, value, false);
            }
        }

        public override IProcessInstanceQuery VariableValueLikeIgnoreCase(string name, string value)
        {
            if (inOrStatement)
            {
                currentOrQueryObject.VariableValueLikeIgnoreCase(name, value, false);
                return this;
            }
            else
            {
                return VariableValueLikeIgnoreCase(name, value, false);
            }
        }

        public virtual IProcessInstanceQuery SetLocale(string locale)
        {
            this._locale = locale;
            return this;
        }

        public virtual IProcessInstanceQuery WithLocalizationFallback()
        {
            _withLocalizationFallback = true;
            return this;
        }

        public virtual IProcessInstanceQuery SetStartedBefore(DateTime beforeTime)
        {
            if (inOrStatement)
            {
                this.currentOrQueryObject._startedBefore = beforeTime;
            }
            else
            {
                this._startedBefore = beforeTime;
            }
            return this;
        }

        public virtual IProcessInstanceQuery SetStartedAfter(DateTime afterTime)
        {
            if (inOrStatement)
            {
                this.currentOrQueryObject._startedAfter = afterTime;
            }
            else
            {
                this._startedAfter = afterTime;
            }
            return this;
        }

        public virtual IProcessInstanceQuery SetStartedBy(string userId)
        {
            if (inOrStatement)
            {
                this.currentOrQueryObject._startedBy = userId;
            }
            else
            {
                this._startedBy = userId;
            }
            return this;
        }

        public virtual IProcessInstanceQuery OrderByProcessInstanceId()
        {
            this.orderProperty = ProcessInstanceQueryProperty.PROCESS_INSTANCE_ID;
            return this;
        }

        public virtual IProcessInstanceQuery OrderByProcessDefinitionId()
        {
            this.orderProperty = ProcessInstanceQueryProperty.PROCESS_DEFINITION_ID;
            return this;
        }

        public virtual IProcessInstanceQuery OrderByProcessDefinitionKey()
        {
            this.orderProperty = ProcessInstanceQueryProperty.PROCESS_DEFINITION_KEY;
            return this;
        }

        public virtual IProcessInstanceQuery OrderByTenantId()
        {
            this.orderProperty = ProcessInstanceQueryProperty.TENANT_ID;
            return this;
        }

        public virtual string MssqlOrDB2OrderBy
        {
            get
            {
                string specialOrderBy = base.OrderBy;
                if (specialOrderBy is object && specialOrderBy.Length > 0)
                {
                    specialOrderBy = specialOrderBy.Replace("RES.", "TEMPRES_");
                    specialOrderBy = specialOrderBy.Replace("ProcessDefinitionKey", "TEMPP_KEY_");
                    specialOrderBy = specialOrderBy.Replace("ProcessDefinitionId", "TEMPP_ID_");
                }
                return specialOrderBy;
            }
        }

        // results /////////////////////////////////////////////////////////////////

        public override long ExecuteCount(ICommandContext commandContext)
        {
            CheckQueryOk();
            EnsureVariablesInitialized();
            return commandContext.ExecutionEntityManager.FindProcessInstanceCountByQueryCriteria(this);
        }

        public override IList<IProcessInstance> ExecuteList(ICommandContext commandContext, Page page)
        {
            CheckQueryOk();
            EnsureVariablesInitialized();
            IList<IProcessInstance> processInstances = null;
            if (_includeProcessVariables.GetValueOrDefault())
            {
                processInstances = commandContext.ExecutionEntityManager.FindProcessInstanceAndVariablesByQueryCriteria(this);
            }
            else
            {
                processInstances = commandContext.ExecutionEntityManager.FindProcessInstanceByQueryCriteria(this);
            }

            if (Context.ProcessEngineConfiguration.PerformanceSettings.EnableLocalization)
            {
                foreach (IProcessInstance processInstance in processInstances)
                {
                    localize(processInstance);
                }
            }

            return processInstances;
        }

        protected internal override void EnsureVariablesInitialized()
        {
            base.EnsureVariablesInitialized();

            foreach (ProcessInstanceQueryImpl orQueryObject in orQueryObjects)
            {
                orQueryObject.EnsureVariablesInitialized();
            }
        }

        protected internal virtual void localize(IProcessInstance processInstance)
        {
            IExecutionEntity processInstanceExecution = (IExecutionEntity)processInstance;
            processInstanceExecution.LocalizedName = null;
            processInstanceExecution.LocalizedDescription = null;

            if (!string.IsNullOrWhiteSpace(_locale))
            {
                string processDefinitionId = processInstanceExecution.ProcessDefinitionId;
                if (!string.IsNullOrWhiteSpace(processDefinitionId))
                {
                    JToken languageNode = Context.GetLocalizationElementProperties(_locale, processInstanceExecution.ProcessDefinitionKey, processDefinitionId, _withLocalizationFallback.GetValueOrDefault());
                    if (languageNode is object)
                    {
                        JToken languageNameNode = languageNode[DynamicBpmnConstants.LOCALIZATION_NAME];
                        if (languageNameNode is object)
                        {
                            processInstanceExecution.LocalizedName = languageNameNode.ToString();
                        }

                        JToken languageDescriptionNode = languageNode[DynamicBpmnConstants.LOCALIZATION_DESCRIPTION];
                        if (languageDescriptionNode is object)
                        {
                            processInstanceExecution.LocalizedDescription = languageDescriptionNode.ToString();
                        }
                    }
                }
            }
        }

        // getters /////////////////////////////////////////////////////////////////

        public virtual bool? OnlyProcessInstances
        {
            get
            {
                return onlyProcessInstances; // See dynamic query in runtime.mapping.xml
            }
            set
            {
                onlyProcessInstances = value.GetValueOrDefault(true);
            }
        }

        public virtual string ProcessInstanceId
        {
            get
            {
                return executionId;
            }
            set => SetProcessInstanceId(value);
        }

        public virtual string RootProcessInstanceId
        {
            get
            {
                return rootProcessInstanceId;
            }
            set => rootProcessInstanceId = value;
        }

        public virtual string[] ProcessInstanceIds
        {
            get
            {
                return _processInstanceIds;
            }
            set => SetProcessInstanceIds(value);
        }

        public virtual string BusinessKey
        {
            get
            {
                return businessKey;
            }
            set => SetProcessInstanceBusinessKey(value);
        }

        public virtual bool? IncludeChildExecutionsWithBusinessKeyQuery
        {
            get
            {
                return includeChildExecutionsWithBusinessKeyQuery;
            }
            set => includeChildExecutionsWithBusinessKeyQuery = value;
        }

        public virtual string ProcessDefinitionId
        {
            get
            {
                return _processDefinitionId;
            }
            set => SetProcessDefinitionId(value);
        }

        public virtual string[] ProcessDefinitionIds
        {
            get
            {
                return _processDefinitionIds;
            }
            set => SetProcessDefinitionIds(value);
        }

        public virtual string ProcessDefinitionCategory
        {
            get
            {
                return _processDefinitionCategory;
            }
            set => SetProcessDefinitionCategory(value);
        }

        public virtual string ProcessDefinitionName
        {
            get
            {
                return _processDefinitionName;
            }
            set => SetProcessDefinitionName(value);
        }

        public virtual int? ProcessDefinitionVersion
        {
            get
            {
                return _processDefinitionVersion;
            }
            set => SetProcessDefinitionVersion(value);
        }

        public virtual string ProcessDefinitionKey
        {
            get
            {
                return _processDefinitionKey;
            }
            set => SetProcessDefinitionKey(value);
        }

        public virtual string[] ProcessDefinitionKeys
        {
            get
            {
                return _processDefinitionKeys;
            }
            set => SetProcessDefinitionKeys(value);
        }

        public virtual string ActivityId
        {
            get
            {
                return null; // Unused, see dynamic query
            }
        }

        public virtual string SuperProcessInstanceId
        {
            get
            {
                return _superProcessInstanceId;
            }
            set => SetSuperProcessInstanceId(value);
        }

        public virtual string SubProcessInstanceId
        {
            get
            {
                return _subProcessInstanceId;
            }
            set => SetSubProcessInstanceId(value);
        }

        public virtual bool? ExcludeSubprocesses
        {
            get
            {
                return _excludeSubprocesses;
            }
            set => SetExcludeSubprocesses(value.GetValueOrDefault());
        }

        public virtual string InvolvedUser
        {
            get
            {
                return _involvedUser;
            }
            set => SetInvolvedUser(value);
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


        public virtual string TenantId
        {
            get
            {
                return tenantId;
            }
            set => SetProcessInstanceTenantId(value);
        }

        public virtual string TenantIdLike
        {
            get
            {
                return tenantIdLike;
            }
            set => SetProcessInstanceTenantIdLike(value);
        }

        public virtual bool? WithoutTenantId
        {
            get
            {
                return withoutTenantId;
            }
            set
            {
                if (value.GetValueOrDefault())
                {
                    ProcessInstanceWithoutTenantId();
                }
                else
                {
                    withoutTenantId = false;
                    if (currentOrQueryObject is object)
                    {
                        currentOrQueryObject.withoutTenantId = false;
                    }
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



        public virtual string ExecutionId
        {
            get
            {
                return executionId;
            }
            set => SetProcessInstanceId(value);
        }

        public virtual string DeploymentId
        {
            get
            {
                return _deploymentId;
            }
            set => SetDeploymentId(value);
        }

        public virtual IList<string> DeploymentIds
        {
            get
            {
                return deploymentIds;
            }
            set => SetDeploymentIdIn(value);
        }

        public virtual bool? IncludeProcessVariables
        {
            get
            {
                return _includeProcessVariables;
            }
            set => _includeProcessVariables = value;
        }

        public virtual bool? IsWithException
        {
            get
            {
                return _withJobException;
            }
            set => _withJobException = value;
        }

        public virtual string NameLikeIgnoreCase
        {
            get
            {
                return nameLikeIgnoreCase;
            }
            set => SetProcessInstanceNameLikeIgnoreCase(value);
        }

        public virtual IList<IProcessInstanceQuery> OrQueryObjects
        {
            get
            {
                return orQueryObjects;
            }
        }

        /// <summary>
        /// Methods needed for ibatis because of re-use of query-xml for executions. ExecutionQuery contains a parentId property.
        /// </summary>

        public virtual string ParentId
        {
            get
            {
                return null;
            }
        }

        public virtual bool? OnlyChildExecutions
        {
            get
            {
                return onlyChildExecutions;
            }
            set => onlyChildExecutions = value;
        }

        public virtual bool? OnlyProcessInstanceExecutions
        {
            get
            {
                return onlyProcessInstanceExecutions;
            }
            set => onlyProcessInstanceExecutions = value;
        }

        public virtual bool? OnlySubProcessExecutions
        {
            get
            {
                return onlySubProcessExecutions;
            }
            set => onlySubProcessExecutions = value;
        }

        public virtual DateTime? StartedBefore
        {
            get
            {
                return _startedBefore;
            }
            set
            {
                this._startedBefore = value;
            }
        }


        public virtual DateTime? StartedAfter
        {
            get
            {
                return _startedAfter;
            }
            set
            {
                this._startedAfter = value;
            }
        }


        public virtual string StartedBy
        {
            get
            {
                return _startedBy;
            }
            set
            {
                this._startedBy = value;
            }
        }
    }
}