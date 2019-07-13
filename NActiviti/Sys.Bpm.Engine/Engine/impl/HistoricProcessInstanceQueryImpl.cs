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
    using Sys.Workflow.Engine.History;
    using Sys.Workflow.Engine.Impl.Contexts;
    using Sys.Workflow.Engine.Impl.Interceptor;
    using Sys.Workflow.Engine.Impl.Persistence.Entity;
    using Sys.Workflow.Engine.Repository;

    /// 
    /// 
    /// 
    /// 
    /// 
    public class HistoricProcessInstanceQueryImpl : AbstractVariableQueryImpl<IHistoricProcessInstanceQuery, IHistoricProcessInstance>, IHistoricProcessInstanceQuery
    {

        private const long serialVersionUID = 1L;
        protected internal string _processInstanceId;
        protected internal string _processDefinitionId;
        protected internal string businessKey;
        protected internal string _deploymentId;
        protected internal string[] deploymentIds;
        protected internal bool _finished;
        protected internal bool _unfinished;
        protected internal bool _deleted;
        protected internal bool _notDeleted;
        protected internal string _startedBy;
        protected internal string _superProcessInstanceId;
        protected internal bool _excludeSubprocesses;
        protected internal string[] _processDefinitionKeyIn;
        protected internal string[] processKeyNotIn;
        protected internal DateTime? _startedBefore;
        protected internal DateTime? _startedAfter;
        protected internal DateTime? _finishedBefore;
        protected internal DateTime? _finishedAfter;
        protected internal string _processDefinitionKey;
        protected internal string _processDefinitionCategory;
        protected internal string _processDefinitionName;
        protected internal int? _processDefinitionVersion;
        protected internal string[] _processInstanceIds;
        protected internal string _involvedUser;
        protected internal string[] _involvedGroups;
        protected internal bool _includeProcessVariables;
        protected internal int? processInstanceVariablesLimit;
        protected internal bool _withJobException;
        protected internal string tenantId;
        protected internal string tenantIdLike;
        protected internal bool? withoutTenantId;
        protected internal string name;
        protected internal string nameLike;
        protected internal string nameLikeIgnoreCase;
        protected internal string _locale;
        protected internal bool _withLocalizationFallback;
        protected internal IList<HistoricProcessInstanceQueryImpl> orQueryObjects = new List<HistoricProcessInstanceQueryImpl>();
        protected internal HistoricProcessInstanceQueryImpl currentOrQueryObject = null;
        protected internal bool inOrStatement = false;
        private string _processDefinitionIdLike;

        public HistoricProcessInstanceQueryImpl()
        {
        }

        public HistoricProcessInstanceQueryImpl(ICommandContext commandContext) : base(commandContext)
        {
        }

        public HistoricProcessInstanceQueryImpl(ICommandExecutor commandExecutor) : base(commandExecutor)
        {
        }

        public virtual IHistoricProcessInstanceQuery SetProcessInstanceId(string processInstanceId)
        {
            if (inOrStatement)
            {
                this.currentOrQueryObject._processInstanceId = processInstanceId;
            }
            else
            {
                this._processInstanceId = processInstanceId;
            }
            return this;
        }

        public virtual IHistoricProcessInstanceQuery SetProcessInstanceIds(string[] processInstanceIds)
        {
            if ((processInstanceIds?.Length).GetValueOrDefault() == 0)
            {
                if (inOrStatement)
                {
                    this.currentOrQueryObject._processInstanceIds = null;
                }
                this._processInstanceIds = null;
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

        public virtual IHistoricProcessInstanceQuery SetProcessDefinitionId(string processDefinitionId)
        {
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

        public virtual IHistoricProcessInstanceQuery SetProcessDefinitionKey(string processDefinitionKey)
        {
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

        public virtual IHistoricProcessInstanceQuery SetProcessDefinitionKeyIn(string[] processDefinitionKeys)
        {
            if (inOrStatement)
            {
                currentOrQueryObject._processDefinitionKeyIn = processDefinitionKeys;
            }
            else
            {
                this._processDefinitionKeyIn = processDefinitionKeys;
            }
            return this;
        }

        public virtual IHistoricProcessInstanceQuery SetProcessDefinitionCategory(string processDefinitionCategory)
        {
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

        public virtual IHistoricProcessInstanceQuery SetProcessDefinitionName(string processDefinitionName)
        {
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

        public virtual IHistoricProcessInstanceQuery SetProcessDefinitionVersion(int? processDefinitionVersion)
        {
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

        public virtual IHistoricProcessInstanceQuery SetProcessInstanceBusinessKey(string businessKey)
        {
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

        public virtual IHistoricProcessInstanceQuery SetDeploymentId(string deploymentId)
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

        public virtual IHistoricProcessInstanceQuery SetDeploymentIdIn(string[] deploymentIds)
        {
            if (inOrStatement)
            {
                currentOrQueryObject.deploymentIds = deploymentIds;
            }
            else
            {
                this.deploymentIds = deploymentIds;
            }
            return this;
        }

        public virtual IHistoricProcessInstanceQuery SetFinished()
        {
            if (inOrStatement)
            {
                this.currentOrQueryObject._finished = true;
            }
            else
            {
                this._finished = true;
            }
            return this;
        }

        public virtual IHistoricProcessInstanceQuery SetUnfinished()
        {
            if (inOrStatement)
            {
                this.currentOrQueryObject._unfinished = true;
            }
            else
            {
                this._unfinished = true;
            }
            return this;
        }

        public virtual IHistoricProcessInstanceQuery SetDeleted()
        {
            if (inOrStatement)
            {
                this.currentOrQueryObject._deleted = true;
            }
            else
            {
                this._deleted = true;
            }
            return this;
        }

        public virtual IHistoricProcessInstanceQuery SetNotDeleted()
        {
            if (inOrStatement)
            {
                this.currentOrQueryObject._notDeleted = true;
            }
            else
            {
                this._notDeleted = true;
            }
            return this;
        }

        public virtual IHistoricProcessInstanceQuery SetStartedBy(string startedBy)
        {
            if (inOrStatement)
            {
                this.currentOrQueryObject._startedBy = startedBy;
            }
            else
            {
                this._startedBy = startedBy;
            }
            return this;
        }

        public virtual IHistoricProcessInstanceQuery SetProcessDefinitionKeyNotIn(string[] processDefinitionKeys)
        {
            if (inOrStatement)
            {
                this.currentOrQueryObject.processKeyNotIn = processDefinitionKeys;
            }
            else
            {
                this.processKeyNotIn = processDefinitionKeys;
            }
            return this;
        }

        public virtual IHistoricProcessInstanceQuery SetStartedAfter(DateTime? startedAfter)
        {
            if (inOrStatement)
            {
                this.currentOrQueryObject._startedAfter = startedAfter;
            }
            else
            {
                this._startedAfter = startedAfter;
            }
            return this;
        }

        public virtual IHistoricProcessInstanceQuery SetStartedBefore(DateTime? startedBefore)
        {
            if (inOrStatement)
            {
                this.currentOrQueryObject._startedBefore = startedBefore;
            }
            else
            {
                this._startedBefore = startedBefore;
            }
            return this;
        }

        public virtual IHistoricProcessInstanceQuery SetFinishedAfter(DateTime? finishedAfter)
        {
            if (inOrStatement)
            {
                this.currentOrQueryObject._finishedAfter = finishedAfter;
            }
            else
            {
                this._finishedAfter = finishedAfter;
                this._finished = true;
            }
            return this;
        }

        public virtual IHistoricProcessInstanceQuery SetFinishedBefore(DateTime? finishedBefore)
        {
            if (inOrStatement)
            {
                this.currentOrQueryObject._finishedBefore = finishedBefore;
            }
            else
            {
                this._finishedBefore = finishedBefore;
                this._finished = true;
            }
            return this;
        }

        public virtual IHistoricProcessInstanceQuery SetSuperProcessInstanceId(string superProcessInstanceId)
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

        public virtual IHistoricProcessInstanceQuery SetExcludeSubprocesses(bool excludeSubprocesses)
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

        public virtual IHistoricProcessInstanceQuery SetInvolvedUser(string involvedUser)
        {
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

        public virtual IHistoricProcessInstanceQuery SetInvolvedGroups(string[] involvedGroups)
        {
            if (inOrStatement)
            {
                this.currentOrQueryObject._involvedGroups = involvedGroups;
            }
            else
            {
                this._involvedGroups = involvedGroups;
            }
            return this;
        }

        public virtual IHistoricProcessInstanceQuery SeIncludeProcessVariables()
        {
            this._includeProcessVariables = true;
            return this;
        }

        public virtual IHistoricProcessInstanceQuery SetLimitProcessInstanceVariables(int? processInstanceVariablesLimit)
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

        public virtual IHistoricProcessInstanceQuery SetWithJobException()
        {
            this._withJobException = true;
            return this;
        }

        public virtual IHistoricProcessInstanceQuery SetProcessInstanceTenantId(string tenantId)
        {
            if (string.IsNullOrWhiteSpace(tenantId))
            {
                if (inOrStatement)
                {
                    this.currentOrQueryObject.tenantId = null;
                }
                this.tenantId = null;
                return this;
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

        public virtual IHistoricProcessInstanceQuery SetProcessInstanceTenantIdLike(string tenantIdLike)
        {
            if (string.IsNullOrWhiteSpace(tenantIdLike))
            {
                this.tenantIdLike = null;

                if (inOrStatement)
                {
                    this.currentOrQueryObject.tenantIdLike = null;
                }
                return this;
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

        public virtual IHistoricProcessInstanceQuery SetProcessInstanceWithoutTenantId()
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

        public virtual IHistoricProcessInstanceQuery SetProcessInstanceName(string name)
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

        public virtual IHistoricProcessInstanceQuery SetProcessInstanceNameLike(string nameLike)
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

        public virtual IHistoricProcessInstanceQuery SetProcessInstanceNameLikeIgnoreCase(string nameLikeIgnoreCase)
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

        public override IHistoricProcessInstanceQuery VariableValueEquals(string variableName, object variableValue)
        {
            if (inOrStatement)
            {
                currentOrQueryObject.VariableValueEquals(variableName, variableValue, true);
                return this;
            }
            else
            {
                return VariableValueEquals(variableName, variableValue, true);
            }
        }

        public override IHistoricProcessInstanceQuery VariableValueNotEquals(string variableName, object variableValue)
        {
            if (inOrStatement)
            {
                currentOrQueryObject.VariableValueNotEquals(variableName, variableValue, true);
                return this;
            }
            else
            {
                return VariableValueNotEquals(variableName, variableValue, true);
            }
        }

        public override IHistoricProcessInstanceQuery VariableValueEquals(object variableValue)
        {
            if (inOrStatement)
            {
                currentOrQueryObject.VariableValueEquals(variableValue, true);
                return this;
            }
            else
            {
                return VariableValueEquals(variableValue, true);
            }
        }

        public override IHistoricProcessInstanceQuery VariableValueEqualsIgnoreCase(string name, string value)
        {
            if (inOrStatement)
            {
                currentOrQueryObject.VariableValueEqualsIgnoreCase(name, value, true);
                return this;
            }
            else
            {
                return VariableValueEqualsIgnoreCase(name, value, true);
            }
        }

        public override IHistoricProcessInstanceQuery VariableValueNotEqualsIgnoreCase(string name, string value)
        {
            if (inOrStatement)
            {
                currentOrQueryObject.VariableValueNotEqualsIgnoreCase(name, value, true);
                return this;
            }
            else
            {
                return VariableValueNotEqualsIgnoreCase(name, value, true);
            }
        }

        public override IHistoricProcessInstanceQuery VariableValueGreaterThan(string name, object value)
        {
            if (inOrStatement)
            {
                currentOrQueryObject.VariableValueGreaterThan(name, value, true);
                return this;
            }
            else
            {
                return VariableValueGreaterThan(name, value, true);
            }
        }

        public override IHistoricProcessInstanceQuery VariableValueGreaterThanOrEqual(string name, object value)
        {
            if (inOrStatement)
            {
                currentOrQueryObject.VariableValueGreaterThanOrEqual(name, value, true);
                return this;
            }
            else
            {
                return VariableValueGreaterThanOrEqual(name, value, true);
            }
        }

        public override IHistoricProcessInstanceQuery VariableValueLessThan(string name, object value)
        {
            if (inOrStatement)
            {
                currentOrQueryObject.VariableValueLessThan(name, value, true);
                return this;
            }
            else
            {
                return VariableValueLessThan(name, value, true);
            }
        }

        public override IHistoricProcessInstanceQuery VariableValueLessThanOrEqual(string name, object value)
        {
            if (inOrStatement)
            {
                currentOrQueryObject.VariableValueLessThanOrEqual(name, value, true);
                return this;
            }
            else
            {
                return VariableValueLessThanOrEqual(name, value, true);
            }
        }

        public override IHistoricProcessInstanceQuery VariableValueLike(string name, string value)
        {
            if (inOrStatement)
            {
                currentOrQueryObject.VariableValueLike(name, value, true);
                return this;
            }
            else
            {
                return VariableValueLike(name, value, true);
            }
        }

        public virtual IHistoricProcessInstanceQuery Locale(string locale)
        {
            this._locale = locale;
            return this;
        }

        public virtual IHistoricProcessInstanceQuery WithLocalizationFallback()
        {
            _withLocalizationFallback = true;
            return this;
        }

        public override IHistoricProcessInstanceQuery VariableValueLikeIgnoreCase(string name, string value)
        {
            if (inOrStatement)
            {
                currentOrQueryObject.VariableValueLikeIgnoreCase(name, value, true);
                return this;
            }
            else
            {
                return VariableValueLikeIgnoreCase(name, value, true);
            }
        }

        public virtual IHistoricProcessInstanceQuery Or()
        {
            if (inOrStatement)
            {
                throw new ActivitiException("the query is already in an or statement");
            }

            inOrStatement = true;
            currentOrQueryObject = new HistoricProcessInstanceQueryImpl();
            orQueryObjects.Add(currentOrQueryObject);
            return this;
        }

        public virtual IHistoricProcessInstanceQuery EndOr()
        {
            if (!inOrStatement)
            {
                throw new ActivitiException("endOr() can only be called after calling or()");
            }

            inOrStatement = false;
            currentOrQueryObject = null;
            return this;
        }

        public virtual IHistoricProcessInstanceQuery OrderByProcessInstanceBusinessKey()
        {
            return SetOrderBy(HistoricProcessInstanceQueryProperty.BUSINESS_KEY);
        }

        public virtual IHistoricProcessInstanceQuery OrderByProcessInstanceDuration()
        {
            return SetOrderBy(HistoricProcessInstanceQueryProperty.DURATION);
        }

        public virtual IHistoricProcessInstanceQuery OrderByProcessInstanceStartTime()
        {
            return SetOrderBy(HistoricProcessInstanceQueryProperty.START_TIME);
        }

        public virtual IHistoricProcessInstanceQuery OrderByProcessInstanceEndTime()
        {
            return SetOrderBy(HistoricProcessInstanceQueryProperty.END_TIME);
        }

        public virtual IHistoricProcessInstanceQuery OrderByProcessDefinitionId()
        {
            return SetOrderBy(HistoricProcessInstanceQueryProperty.PROCESS_DEFINITION_ID);
        }

        public virtual IHistoricProcessInstanceQuery OrderByProcessInstanceId()
        {
            return SetOrderBy(HistoricProcessInstanceQueryProperty.PROCESS_INSTANCE_ID_);
        }

        public virtual IHistoricProcessInstanceQuery OrderByTenantId()
        {
            return SetOrderBy(HistoricProcessInstanceQueryProperty.TENANT_ID);
        }

        public virtual string MssqlOrDB2OrderBy
        {
            get
            {
                string specialOrderBy = base.OrderBy;
                if (specialOrderBy is object && specialOrderBy.Length > 0)
                {
                    specialOrderBy = specialOrderBy.Replace("RES.", "TEMPRES_");
                    specialOrderBy = specialOrderBy.Replace("VAR.", "TEMPVAR_");
                }
                return specialOrderBy;
            }
        }

        public override long ExecuteCount(ICommandContext commandContext)
        {
            CheckQueryOk();
            EnsureVariablesInitialized();
            return commandContext.HistoricProcessInstanceEntityManager.FindHistoricProcessInstanceCountByQueryCriteria(this);
        }

        public override IList<IHistoricProcessInstance> ExecuteList(ICommandContext commandContext, Page page)
        {
            CheckQueryOk();
            EnsureVariablesInitialized();
            IList<IHistoricProcessInstance> results;
            if (_includeProcessVariables)
            {
                results = commandContext.HistoricProcessInstanceEntityManager.FindHistoricProcessInstancesAndVariablesByQueryCriteria(this);
            }
            else
            {
                results = commandContext.HistoricProcessInstanceEntityManager.FindHistoricProcessInstancesByQueryCriteria(this);
            }

            if (Context.ProcessEngineConfiguration.PerformanceSettings.EnableLocalization)
            {
                foreach (IHistoricProcessInstance processInstance in results)
                {
                    Localize(processInstance, commandContext);
                }
            }

            return results;
        }

        protected internal virtual void Localize(IHistoricProcessInstance processInstance, ICommandContext commandContext)
        {
            IHistoricProcessInstanceEntity processInstanceEntity = (IHistoricProcessInstanceEntity)processInstance;
            processInstanceEntity.LocalizedName = null;
            processInstanceEntity.LocalizedDescription = null;

            if (_locale is object && processInstance.ProcessDefinitionId is object)
            {
                IProcessDefinition processDefinition = commandContext.ProcessEngineConfiguration.DeploymentManager.FindDeployedProcessDefinitionById(processInstanceEntity.ProcessDefinitionId);
                JToken languageNode = Context.GetLocalizationElementProperties(_locale, processDefinition.Key, processInstanceEntity.ProcessDefinitionId, _withLocalizationFallback);

                if (languageNode != null)
                {
                    JToken languageNameNode = languageNode[DynamicBpmnConstants.LOCALIZATION_NAME];
                    if (languageNameNode != null)
                    {
                        processInstanceEntity.LocalizedName = languageNameNode.ToString();
                    }

                    JToken languageDescriptionNode = languageNode[DynamicBpmnConstants.LOCALIZATION_DESCRIPTION];
                    if (languageDescriptionNode != null)
                    {
                        processInstanceEntity.LocalizedDescription = languageDescriptionNode.ToString();
                    }
                }
            }
        }

        protected internal override void EnsureVariablesInitialized()
        {
            base.EnsureVariablesInitialized();

            foreach (HistoricProcessInstanceQueryImpl orQueryObject in orQueryObjects)
            {
                orQueryObject.EnsureVariablesInitialized();
            }
        }

        protected internal override void CheckQueryOk()
        {
            base.CheckQueryOk();

            if (_includeProcessVariables)
            {
                this.SetOrderBy(HistoricProcessInstanceQueryProperty.INCLUDED_VARIABLE_TIME).Asc();
            }
        }

        public virtual string BusinessKey
        {
            get
            {
                return businessKey;
            }
            set => SetProcessInstanceBusinessKey(value);
        }

        public virtual bool Open
        {
            get
            {
                return _unfinished;
            }
            set
            {
                if (value)
                {
                    SetUnfinished();
                }
                else
                {
                    _unfinished = value;
                }
            }
        }

        public virtual string ProcessDefinitionId
        {
            get
            {
                return _processDefinitionId;
            }
            set => SetProcessDefinitionId(value);
        }

        public virtual string ProcessDefinitionKey
        {
            get
            {
                return _processDefinitionKey;
            }
            set => SetProcessDefinitionKey(value);
        }

        public virtual IList<string> ProcessDefinitionKeyIn
        {
            get
            {
                return _processDefinitionKeyIn;
            }
        }

        public virtual string ProcessDefinitionIdLike
        {
            get
            {
                return _processDefinitionIdLike;
            }
            set => _processDefinitionIdLike = value;
        }

        public virtual string ProcessDefinitionName
        {
            get
            {
                return _processDefinitionName;
            }
            set => SetProcessDefinitionName(value);
        }
        public virtual string ProcessDefinitionCategory
        {
            get
            {
                return _processDefinitionCategory;
            }
            set => SetProcessDefinitionCategory(value);
        }
        public virtual int? ProcessDefinitionVersion
        {
            get
            {
                return _processDefinitionVersion;
            }
            set => SetProcessDefinitionVersion(value);
        }

        public virtual string ProcessInstanceId
        {
            get
            {
                return _processInstanceId;
            }
            set => SetProcessInstanceId(value);
        }

        public virtual string[] ProcessInstanceIds
        {
            get
            {
                return _processInstanceIds;
            }
            set => SetProcessInstanceIds(value);
        }

        public virtual string StartedBy
        {
            get
            {
                return _startedBy;
            }
            set => SetStartedBy(value);
        }

        public virtual string SuperProcessInstanceId
        {
            get
            {
                return _superProcessInstanceId;
            }
            set => SetSuperProcessInstanceId(value);
        }

        public virtual bool ExcludeSubprocesses
        {
            get
            {
                return _excludeSubprocesses;
            }
            set => SetExcludeSubprocesses(value);
        }

        public virtual string[] ProcessKeyNotIn
        {
            get
            {
                return processKeyNotIn;
            }
            set => processKeyNotIn = value;
        }

        public virtual DateTime? StartedAfter
        {
            get
            {
                return _startedAfter;
            }
            set => SetStartedAfter(value);
        }

        public virtual DateTime? StartedBefore
        {
            get
            {
                return _startedBefore;
            }
            set => SetStartedBefore(value);
        }

        public virtual DateTime? FinishedAfter
        {
            get
            {
                return _finishedAfter;
            }
            set => SetFinishedAfter(value);
        }

        public virtual DateTime? FinishedBefore
        {
            get
            {
                return _finishedBefore;
            }
            set => SetFinishedBefore(value);
        }

        public virtual string InvolvedUser
        {
            get
            {
                return _involvedUser;
            }
            set => SetInvolvedUser(value);
        }

        public virtual string[] InvolvedGroups
        {
            get => _involvedGroups;
            set => SetInvolvedGroups(value);
        }

        public virtual string Name
        {
            get
            {
                return name;
            }
            set => SetProcessInstanceName(value);
        }

        public virtual string NameLike
        {
            get
            {
                return nameLike;
            }
            set => SetProcessInstanceNameLike(value);
        }

        public virtual string DeploymentId
        {
            get
            {
                return _deploymentId;
            }
            set => SetDeploymentId(value);
        }

        public virtual string[] DeploymentIds
        {
            get
            {
                return deploymentIds;
            }
            set => SetDeploymentIdIn(value);
        }

        public virtual bool Finished
        {
            get
            {
                return _finished;
            }
            set
            {
                if (value)
                {
                    SetFinished();
                }
                else
                {
                    SetUnfinished();
                }
            }
        }

        public virtual bool Unfinished
        {
            get
            {
                return _unfinished;
            }
            set
            {
                if (value)
                {
                    SetUnfinished();
                }
                else
                {
                    SetFinished();
                }
            }
        }

        public virtual bool Deleted
        {
            get
            {
                return _deleted;
            }
            set
            {
                if (value)
                {
                    SetDeleted();
                }
                else
                {
                    SetNotDeleted();
                }
            }
        }

        public virtual bool NotDeleted
        {
            get
            {
                return _notDeleted;
            }
            set
            {
                if (value)
                {
                    SetNotDeleted();
                }
                else
                {
                    SetDeleted();
                }
            }
        }

        public virtual bool IncludeProcessVariables
        {
            get
            {
                return _includeProcessVariables;
            }
            set
            {
                if (value)
                {
                    SeIncludeProcessVariables();
                }
                else
                {
                    _includeProcessVariables = value;
                }
            }
        }

        public virtual bool WithJobException
        {
            get
            {
                return _withJobException;
            }
            set
            {
                if (value)
                {
                    SetWithJobException();
                }
                else
                {
                    _withJobException = false;
                }
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
                    SetProcessInstanceWithoutTenantId();
                }
                else
                {
                    withoutTenantId = false;
                }
            }
        }

        public virtual string NameLikeIgnoreCase
        {
            get
            {
                return nameLikeIgnoreCase;
            }
            set => SetProcessInstanceNameLikeIgnoreCase(value);
        }

        public virtual IList<HistoricProcessInstanceQueryImpl> OrQueryObjects
        {
            get
            {
                return orQueryObjects;
            }
        }
    }
}