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
    using org.activiti.engine.history;
    using org.activiti.engine.impl.context;
    using org.activiti.engine.impl.interceptor;
    using org.activiti.engine.impl.persistence.entity;
    using org.activiti.engine.repository;

    /// 
    /// 
    /// 
    /// 
    /// 
    public class HistoricProcessInstanceQueryImpl : AbstractVariableQueryImpl<IHistoricProcessInstanceQuery, IHistoricProcessInstance>, IHistoricProcessInstanceQuery
    {

        private const long serialVersionUID = 1L;
        protected internal string processInstanceId_;
        protected internal string processDefinitionId_;
        protected internal string businessKey;
        protected internal string deploymentId_;
        protected internal string[] deploymentIds;
        protected internal bool finished_;
        protected internal bool unfinished_;
        protected internal bool deleted_;
        protected internal bool notDeleted_;
        protected internal string startedBy_;
        protected internal string superProcessInstanceId_;
        protected internal bool excludeSubprocesses_;
        protected internal string[] processDefinitionKeyIn_;
        protected internal string[] processKeyNotIn;
        protected internal DateTime? startedBefore_;
        protected internal DateTime? startedAfter_;
        protected internal DateTime? finishedBefore_;
        protected internal DateTime? finishedAfter_;
        protected internal string processDefinitionKey_;
        protected internal string processDefinitionCategory_;
        protected internal string processDefinitionName_;
        protected internal int? processDefinitionVersion_;
        protected internal string[] processInstanceIds_;
        protected internal string involvedUser_;
        protected internal string[] involvedGroups_;
        protected internal bool includeProcessVariables_;
        protected internal int? processInstanceVariablesLimit;
        protected internal bool withJobException_;
        protected internal string tenantId;
        protected internal string tenantIdLike;
        protected internal bool? withoutTenantId;
        protected internal string name;
        protected internal string nameLike;
        protected internal string nameLikeIgnoreCase;
        protected internal string locale_;
        protected internal bool withLocalizationFallback_;
        protected internal IList<HistoricProcessInstanceQueryImpl> orQueryObjects = new List<HistoricProcessInstanceQueryImpl>();
        protected internal HistoricProcessInstanceQueryImpl currentOrQueryObject = null;
        protected internal bool inOrStatement = false;
        private string processDefinitionIdLike_;

        public HistoricProcessInstanceQueryImpl()
        {
        }

        public HistoricProcessInstanceQueryImpl(ICommandContext commandContext) : base(commandContext)
        {
        }

        public HistoricProcessInstanceQueryImpl(ICommandExecutor commandExecutor) : base(commandExecutor)
        {
        }

        public virtual IHistoricProcessInstanceQuery processInstanceId(string processInstanceId)
        {
            if (inOrStatement)
            {
                this.currentOrQueryObject.processInstanceId_ = processInstanceId;
            }
            else
            {
                this.processInstanceId_ = processInstanceId;
            }
            return this;
        }

        public virtual IHistoricProcessInstanceQuery processInstanceIds(string[] processInstanceIds)
        {
            if (processInstanceIds?.Length == 0)
            {
                if (inOrStatement)
                {
                    this.currentOrQueryObject.processInstanceIds_ = null;
                }
                this.processInstanceIds_ = null;
                return this;
            }

            if (inOrStatement)
            {
                this.currentOrQueryObject.processInstanceIds_ = processInstanceIds;
            }
            else
            {
                this.processInstanceIds_ = processInstanceIds;
            }
            return this;
        }

        public virtual IHistoricProcessInstanceQuery processDefinitionId(string processDefinitionId)
        {
            if (inOrStatement)
            {
                this.currentOrQueryObject.processDefinitionId_ = processDefinitionId;
            }
            else
            {
                this.processDefinitionId_ = processDefinitionId;
            }
            return this;
        }

        public virtual IHistoricProcessInstanceQuery processDefinitionKey(string processDefinitionKey)
        {
            if (inOrStatement)
            {
                this.currentOrQueryObject.processDefinitionKey_ = processDefinitionKey;
            }
            else
            {
                this.processDefinitionKey_ = processDefinitionKey;
            }
            return this;
        }

        public virtual IHistoricProcessInstanceQuery processDefinitionKeyIn(string[] processDefinitionKeys)
        {
            if (inOrStatement)
            {
                currentOrQueryObject.processDefinitionKeyIn_ = processDefinitionKeys;
            }
            else
            {
                this.processDefinitionKeyIn_ = processDefinitionKeys;
            }
            return this;
        }

        public virtual IHistoricProcessInstanceQuery processDefinitionCategory(string processDefinitionCategory)
        {
            if (inOrStatement)
            {
                this.currentOrQueryObject.processDefinitionCategory_ = processDefinitionCategory;
            }
            else
            {
                this.processDefinitionCategory_ = processDefinitionCategory;
            }
            return this;
        }

        public virtual IHistoricProcessInstanceQuery processDefinitionName(string processDefinitionName)
        {
            if (inOrStatement)
            {
                this.currentOrQueryObject.processDefinitionName_ = processDefinitionName;
            }
            else
            {
                this.processDefinitionName_ = processDefinitionName;
            }
            return this;
        }

        public virtual IHistoricProcessInstanceQuery processDefinitionVersion(int? processDefinitionVersion)
        {
            if (inOrStatement)
            {
                this.currentOrQueryObject.processDefinitionVersion_ = processDefinitionVersion;
            }
            else
            {
                this.processDefinitionVersion_ = processDefinitionVersion;
            }
            return this;
        }

        public virtual IHistoricProcessInstanceQuery processInstanceBusinessKey(string businessKey)
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

        public virtual IHistoricProcessInstanceQuery deploymentId(string deploymentId)
        {
            if (inOrStatement)
            {
                this.currentOrQueryObject.deploymentId_ = deploymentId;
            }
            else
            {
                this.deploymentId_ = deploymentId;
            }
            return this;
        }

        public virtual IHistoricProcessInstanceQuery deploymentIdIn(string[] deploymentIds)
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

        public virtual IHistoricProcessInstanceQuery finished()
        {
            if (inOrStatement)
            {
                this.currentOrQueryObject.finished_ = true;
            }
            else
            {
                this.finished_ = true;
            }
            return this;
        }

        public virtual IHistoricProcessInstanceQuery unfinished()
        {
            if (inOrStatement)
            {
                this.currentOrQueryObject.unfinished_ = true;
            }
            else
            {
                this.unfinished_ = true;
            }
            return this;
        }

        public virtual IHistoricProcessInstanceQuery deleted()
        {
            if (inOrStatement)
            {
                this.currentOrQueryObject.deleted_ = true;
            }
            else
            {
                this.deleted_ = true;
            }
            return this;
        }

        public virtual IHistoricProcessInstanceQuery notDeleted()
        {
            if (inOrStatement)
            {
                this.currentOrQueryObject.notDeleted_ = true;
            }
            else
            {
                this.notDeleted_ = true;
            }
            return this;
        }

        public virtual IHistoricProcessInstanceQuery startedBy(string startedBy)
        {
            if (inOrStatement)
            {
                this.currentOrQueryObject.startedBy_ = startedBy;
            }
            else
            {
                this.startedBy_ = startedBy;
            }
            return this;
        }

        public virtual IHistoricProcessInstanceQuery processDefinitionKeyNotIn(string[] processDefinitionKeys)
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

        public virtual IHistoricProcessInstanceQuery startedAfter(DateTime? startedAfter)
        {
            if (inOrStatement)
            {
                this.currentOrQueryObject.startedAfter_ = startedAfter;
            }
            else
            {
                this.startedAfter_ = startedAfter;
            }
            return this;
        }

        public virtual IHistoricProcessInstanceQuery startedBefore(DateTime? startedBefore)
        {
            if (inOrStatement)
            {
                this.currentOrQueryObject.startedBefore_ = startedBefore;
            }
            else
            {
                this.startedBefore_ = startedBefore;
            }
            return this;
        }

        public virtual IHistoricProcessInstanceQuery finishedAfter(DateTime? finishedAfter)
        {
            if (inOrStatement)
            {
                this.currentOrQueryObject.finishedAfter_ = finishedAfter;
            }
            else
            {
                this.finishedAfter_ = finishedAfter;
                this.finished_ = true;
            }
            return this;
        }

        public virtual IHistoricProcessInstanceQuery finishedBefore(DateTime? finishedBefore)
        {
            if (inOrStatement)
            {
                this.currentOrQueryObject.finishedBefore_ = finishedBefore;
            }
            else
            {
                this.finishedBefore_ = finishedBefore;
                this.finished_ = true;
            }
            return this;
        }

        public virtual IHistoricProcessInstanceQuery superProcessInstanceId(string superProcessInstanceId)
        {
            if (inOrStatement)
            {
                this.currentOrQueryObject.superProcessInstanceId_ = superProcessInstanceId;
            }
            else
            {
                this.superProcessInstanceId_ = superProcessInstanceId;
            }
            return this;
        }

        public virtual IHistoricProcessInstanceQuery excludeSubprocesses(bool excludeSubprocesses)
        {
            if (inOrStatement)
            {
                this.currentOrQueryObject.excludeSubprocesses_ = excludeSubprocesses;
            }
            else
            {
                this.excludeSubprocesses_ = excludeSubprocesses;
            }
            return this;
        }

        public virtual IHistoricProcessInstanceQuery involvedUser(string involvedUser)
        {
            if (inOrStatement)
            {
                this.currentOrQueryObject.involvedUser_ = involvedUser;
            }
            else
            {
                this.involvedUser_ = involvedUser;
            }
            return this;
        }

        public virtual IHistoricProcessInstanceQuery involvedGroups(string[] involvedGroups)
        {
            if (inOrStatement)
            {
                this.currentOrQueryObject.involvedGroups_ = involvedGroups;
            }
            else
            {
                this.involvedGroups_ = involvedGroups;
            }
            return this;
        }

        public virtual IHistoricProcessInstanceQuery includeProcessVariables()
        {
            this.includeProcessVariables_ = true;
            return this;
        }

        public virtual IHistoricProcessInstanceQuery limitProcessInstanceVariables(int? processInstanceVariablesLimit)
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

        public virtual IHistoricProcessInstanceQuery withJobException()
        {
            this.withJobException_ = true;
            return this;
        }

        public virtual IHistoricProcessInstanceQuery processInstanceTenantId(string tenantId)
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

        public virtual IHistoricProcessInstanceQuery processInstanceTenantIdLike(string tenantIdLike)
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

        public virtual IHistoricProcessInstanceQuery processInstanceWithoutTenantId()
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

        public virtual IHistoricProcessInstanceQuery processInstanceName(string name)
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

        public virtual IHistoricProcessInstanceQuery processInstanceNameLike(string nameLike)
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

        public virtual IHistoricProcessInstanceQuery processInstanceNameLikeIgnoreCase(string nameLikeIgnoreCase)
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

        public override IHistoricProcessInstanceQuery variableValueEquals(string variableName, object variableValue)
        {
            if (inOrStatement)
            {
                currentOrQueryObject.variableValueEquals(variableName, variableValue, true);
                return this;
            }
            else
            {
                return variableValueEquals(variableName, variableValue, true);
            }
        }

        public override IHistoricProcessInstanceQuery variableValueNotEquals(string variableName, object variableValue)
        {
            if (inOrStatement)
            {
                currentOrQueryObject.variableValueNotEquals(variableName, variableValue, true);
                return this;
            }
            else
            {
                return variableValueNotEquals(variableName, variableValue, true);
            }
        }

        public override IHistoricProcessInstanceQuery variableValueEquals(object variableValue)
        {
            if (inOrStatement)
            {
                currentOrQueryObject.variableValueEquals(variableValue, true);
                return this;
            }
            else
            {
                return variableValueEquals(variableValue, true);
            }
        }

        public override IHistoricProcessInstanceQuery variableValueEqualsIgnoreCase(string name, string value)
        {
            if (inOrStatement)
            {
                currentOrQueryObject.variableValueEqualsIgnoreCase(name, value, true);
                return this;
            }
            else
            {
                return variableValueEqualsIgnoreCase(name, value, true);
            }
        }

        public override IHistoricProcessInstanceQuery variableValueNotEqualsIgnoreCase(string name, string value)
        {
            if (inOrStatement)
            {
                currentOrQueryObject.variableValueNotEqualsIgnoreCase(name, value, true);
                return this;
            }
            else
            {
                return variableValueNotEqualsIgnoreCase(name, value, true);
            }
        }

        public override IHistoricProcessInstanceQuery variableValueGreaterThan(string name, object value)
        {
            if (inOrStatement)
            {
                currentOrQueryObject.variableValueGreaterThan(name, value, true);
                return this;
            }
            else
            {
                return variableValueGreaterThan(name, value, true);
            }
        }

        public override IHistoricProcessInstanceQuery variableValueGreaterThanOrEqual(string name, object value)
        {
            if (inOrStatement)
            {
                currentOrQueryObject.variableValueGreaterThanOrEqual(name, value, true);
                return this;
            }
            else
            {
                return variableValueGreaterThanOrEqual(name, value, true);
            }
        }

        public override IHistoricProcessInstanceQuery variableValueLessThan(string name, object value)
        {
            if (inOrStatement)
            {
                currentOrQueryObject.variableValueLessThan(name, value, true);
                return this;
            }
            else
            {
                return variableValueLessThan(name, value, true);
            }
        }

        public override IHistoricProcessInstanceQuery variableValueLessThanOrEqual(string name, object value)
        {
            if (inOrStatement)
            {
                currentOrQueryObject.variableValueLessThanOrEqual(name, value, true);
                return this;
            }
            else
            {
                return variableValueLessThanOrEqual(name, value, true);
            }
        }

        public override IHistoricProcessInstanceQuery variableValueLike(string name, string value)
        {
            if (inOrStatement)
            {
                currentOrQueryObject.variableValueLike(name, value, true);
                return this;
            }
            else
            {
                return variableValueLike(name, value, true);
            }
        }

        public virtual IHistoricProcessInstanceQuery locale(string locale)
        {
            this.locale_ = locale;
            return this;
        }

        public virtual IHistoricProcessInstanceQuery withLocalizationFallback()
        {
            withLocalizationFallback_ = true;
            return this;
        }

        public override IHistoricProcessInstanceQuery variableValueLikeIgnoreCase(string name, string value)
        {
            if (inOrStatement)
            {
                currentOrQueryObject.variableValueLikeIgnoreCase(name, value, true);
                return this;
            }
            else
            {
                return variableValueLikeIgnoreCase(name, value, true);
            }
        }

        public virtual IHistoricProcessInstanceQuery or()
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

        public virtual IHistoricProcessInstanceQuery endOr()
        {
            if (!inOrStatement)
            {
                throw new ActivitiException("endOr() can only be called after calling or()");
            }

            inOrStatement = false;
            currentOrQueryObject = null;
            return this;
        }

        public virtual IHistoricProcessInstanceQuery orderByProcessInstanceBusinessKey()
        {
            return orderBy(HistoricProcessInstanceQueryProperty.BUSINESS_KEY);
        }

        public virtual IHistoricProcessInstanceQuery orderByProcessInstanceDuration()
        {
            return orderBy(HistoricProcessInstanceQueryProperty.DURATION);
        }

        public virtual IHistoricProcessInstanceQuery orderByProcessInstanceStartTime()
        {
            return orderBy(HistoricProcessInstanceQueryProperty.START_TIME);
        }

        public virtual IHistoricProcessInstanceQuery orderByProcessInstanceEndTime()
        {
            return orderBy(HistoricProcessInstanceQueryProperty.END_TIME);
        }

        public virtual IHistoricProcessInstanceQuery orderByProcessDefinitionId()
        {
            return orderBy(HistoricProcessInstanceQueryProperty.PROCESS_DEFINITION_ID);
        }

        public virtual IHistoricProcessInstanceQuery orderByProcessInstanceId()
        {
            return orderBy(HistoricProcessInstanceQueryProperty.PROCESS_INSTANCE_ID_);
        }

        public virtual IHistoricProcessInstanceQuery orderByTenantId()
        {
            return orderBy(HistoricProcessInstanceQueryProperty.TENANT_ID);
        }

        public virtual string MssqlOrDB2OrderBy
        {
            get
            {
                string specialOrderBy = base.OrderBy;
                if (!ReferenceEquals(specialOrderBy, null) && specialOrderBy.Length > 0)
                {
                    specialOrderBy = specialOrderBy.Replace("RES.", "TEMPRES_");
                    specialOrderBy = specialOrderBy.Replace("VAR.", "TEMPVAR_");
                }
                return specialOrderBy;
            }
        }

        public override long executeCount(ICommandContext commandContext)
        {
            checkQueryOk();
            ensureVariablesInitialized();
            return commandContext.HistoricProcessInstanceEntityManager.findHistoricProcessInstanceCountByQueryCriteria(this);
        }

        public override IList<IHistoricProcessInstance> executeList(ICommandContext commandContext, Page page)
        {
            checkQueryOk();
            ensureVariablesInitialized();
            IList<IHistoricProcessInstance> results = null;
            if (includeProcessVariables_)
            {
                results = commandContext.HistoricProcessInstanceEntityManager.findHistoricProcessInstancesAndVariablesByQueryCriteria(this);
            }
            else
            {
                results = commandContext.HistoricProcessInstanceEntityManager.findHistoricProcessInstancesByQueryCriteria(this);
            }

            if (Context.ProcessEngineConfiguration.PerformanceSettings.EnableLocalization)
            {
                foreach (IHistoricProcessInstance processInstance in results)
                {
                    localize(processInstance, commandContext);
                }
            }

            return results;
        }

        protected internal virtual void localize(IHistoricProcessInstance processInstance, ICommandContext commandContext)
        {
            IHistoricProcessInstanceEntity processInstanceEntity = (IHistoricProcessInstanceEntity)processInstance;
            processInstanceEntity.LocalizedName = null;
            processInstanceEntity.LocalizedDescription = null;

            if (!ReferenceEquals(locale_, null) && !ReferenceEquals(processInstance.ProcessDefinitionId, null))
            {
                IProcessDefinition processDefinition = commandContext.ProcessEngineConfiguration.DeploymentManager.findDeployedProcessDefinitionById(processInstanceEntity.ProcessDefinitionId);
                JToken languageNode = Context.getLocalizationElementProperties(locale_, processDefinition.Key, processInstanceEntity.ProcessDefinitionId, withLocalizationFallback_);

                if (languageNode != null)
                {
                    JToken languageNameNode = languageNode[DynamicBpmnConstants_Fields.LOCALIZATION_NAME];
                    if (languageNameNode != null)
                    {
                        processInstanceEntity.LocalizedName = languageNameNode.ToString();
                    }

                    JToken languageDescriptionNode = languageNode[DynamicBpmnConstants_Fields.LOCALIZATION_DESCRIPTION];
                    if (languageDescriptionNode != null)
                    {
                        processInstanceEntity.LocalizedDescription = languageDescriptionNode.ToString();
                    }
                }
            }
        }

        protected internal override void ensureVariablesInitialized()
        {
            base.ensureVariablesInitialized();

            foreach (HistoricProcessInstanceQueryImpl orQueryObject in orQueryObjects)
            {
                orQueryObject.ensureVariablesInitialized();
            }
        }

        protected internal override void checkQueryOk()
        {
            base.checkQueryOk();

            if (includeProcessVariables_)
            {
                this.orderBy(HistoricProcessInstanceQueryProperty.INCLUDED_VARIABLE_TIME).asc();
            }
        }

        public virtual string BusinessKey
        {
            get
            {
                return businessKey;
            }
            set => processInstanceBusinessKey(value);
        }

        public virtual bool Open
        {
            get
            {
                return unfinished_;
            }
            set
            {
                if (value)
                {
                    unfinished();
                }
                else
                {
                    unfinished_ = value;
                }
            }
        }

        public virtual string ProcessDefinitionId
        {
            get
            {
                return processDefinitionId_;
            }
            set => processDefinitionId(value);
        }

        public virtual string ProcessDefinitionKey
        {
            get
            {
                return processDefinitionKey_;
            }
            set => processDefinitionKey(value);
        }

        public virtual IList<string> ProcessDefinitionKeyIn
        {
            get
            {
                return processDefinitionKeyIn_;
            }
        }

        public virtual string ProcessDefinitionIdLike
        {
            get
            {
                return processDefinitionIdLike_;
            }
            set => processDefinitionIdLike_ = value;
        }

        public virtual string ProcessDefinitionName
        {
            get
            {
                return processDefinitionName_;
            }
            set => processDefinitionName(value);
        }
        public virtual string ProcessDefinitionCategory
        {
            get
            {
                return processDefinitionCategory_;
            }
            set => processDefinitionCategory(value);
        }
        public virtual int? ProcessDefinitionVersion
        {
            get
            {
                return processDefinitionVersion_;
            }
            set => processDefinitionVersion(value);
        }

        public virtual string ProcessInstanceId
        {
            get
            {
                return processInstanceId_;
            }
            set => processInstanceId(value);
        }

        public virtual string[] ProcessInstanceIds
        {
            get
            {
                return processInstanceIds_;
            }
            set => processInstanceIds(value);
        }

        public virtual string StartedBy
        {
            get
            {
                return startedBy_;
            }
            set => startedBy(value);
        }

        public virtual string SuperProcessInstanceId
        {
            get
            {
                return superProcessInstanceId_;
            }
            set => superProcessInstanceId(value);
        }

        public virtual bool ExcludeSubprocesses
        {
            get
            {
                return excludeSubprocesses_;
            }
            set => excludeSubprocesses(value);
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
                return startedAfter_;
            }
            set => startedAfter(value);
        }

        public virtual DateTime? StartedBefore
        {
            get
            {
                return startedBefore_;
            }
            set => startedBefore(value);
        }

        public virtual DateTime? FinishedAfter
        {
            get
            {
                return finishedAfter_;
            }
            set => finishedAfter(value);
        }

        public virtual DateTime? FinishedBefore
        {
            get
            {
                return finishedBefore_;
            }
            set => finishedBefore(value);
        }

        public virtual string InvolvedUser
        {
            get
            {
                return involvedUser_;
            }
            set => involvedUser(value);
        }

        public virtual string[] InvolvedGroups
        {
            get => involvedGroups_;
            set => involvedGroups(value);
        }

        public virtual string Name
        {
            get
            {
                return name;
            }
            set => processInstanceName(value);
        }

        public virtual string NameLike
        {
            get
            {
                return nameLike;
            }
            set => processInstanceNameLike(value);
        }

        public virtual string DeploymentId
        {
            get
            {
                return deploymentId_;
            }
            set => deploymentId(value);
        }

        public virtual string[] DeploymentIds
        {
            get
            {
                return deploymentIds;
            }
            set => deploymentIdIn(value);
        }

        public virtual bool Finished
        {
            get
            {
                return finished_;
            }
            set
            {
                if (value)
                {
                    finished();
                }
                else
                {
                    unfinished();
                }
            }
        }

        public virtual bool Unfinished
        {
            get
            {
                return unfinished_;
            }
            set
            {
                if (value)
                {
                    unfinished();
                }
                else
                {
                    finished();
                }
            }
        }

        public virtual bool Deleted
        {
            get
            {
                return deleted_;
            }
            set
            {
                if (value)
                {
                    deleted();
                }
                else
                {
                    notDeleted();
                }
            }
        }

        public virtual bool NotDeleted
        {
            get
            {
                return notDeleted_;
            }
            set
            {
                if (value)
                {
                    notDeleted();
                }
                else
                {
                    deleted();
                }
            }
        }

        public virtual bool IncludeProcessVariables
        {
            get
            {
                return includeProcessVariables_;
            }
            set
            {
                if (value)
                {
                    includeProcessVariables();
                }
                else
                {
                    includeProcessVariables_ = value;
                }
            }
        }

        public virtual bool WithJobException
        {
            get
            {
                return withJobException_;
            }
            set
            {
                if (value)
                {
                    withJobException();
                }
                else
                {
                    withJobException_ = false;
                }
            }
        }

        public virtual string TenantId
        {
            get
            {
                return tenantId;
            }
            set => processInstanceTenantId(value);
        }

        public virtual string TenantIdLike
        {
            get
            {
                return tenantIdLike;
            }
            set => processInstanceTenantIdLike(value);
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
                    processInstanceWithoutTenantId();
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
            set => processInstanceNameLikeIgnoreCase(value);
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