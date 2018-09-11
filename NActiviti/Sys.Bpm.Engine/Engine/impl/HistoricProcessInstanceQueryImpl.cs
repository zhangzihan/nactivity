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
        protected internal string processInstanceId_Renamed;
        protected internal string processDefinitionId_Renamed;
        protected internal string businessKey;
        protected internal string deploymentId_Renamed;
        protected internal IList<string> deploymentIds;
        protected internal bool finished_Renamed;
        protected internal bool unfinished_Renamed;
        protected internal bool deleted_Renamed;
        protected internal bool notDeleted_Renamed;
        protected internal string startedBy_Renamed;
        protected internal string superProcessInstanceId_Renamed;
        protected internal bool excludeSubprocesses_Renamed;
        protected internal IList<string> processDefinitionKeyIn_Renamed;
        protected internal IList<string> processKeyNotIn;
        protected internal DateTime startedBefore_Renamed;
        protected internal DateTime startedAfter_Renamed;
        protected internal DateTime finishedBefore_Renamed;
        protected internal DateTime finishedAfter_Renamed;
        protected internal string processDefinitionKey_Renamed;
        protected internal string processDefinitionCategory_Renamed;
        protected internal string processDefinitionName_Renamed;
        protected internal int? processDefinitionVersion_Renamed;
        protected internal ISet<string> processInstanceIds_Renamed;
        protected internal string involvedUser_Renamed;
        protected internal bool includeProcessVariables_Renamed;
        protected internal int? processInstanceVariablesLimit;
        protected internal bool withJobException_Renamed;
        protected internal string tenantId;
        protected internal string tenantIdLike;
        protected internal bool withoutTenantId;
        protected internal string name;
        protected internal string nameLike;
        protected internal string nameLikeIgnoreCase;
        protected internal string locale_Renamed;
        protected internal bool withLocalizationFallback_Renamed;
        protected internal IList<HistoricProcessInstanceQueryImpl> orQueryObjects = new List<HistoricProcessInstanceQueryImpl>();
        protected internal HistoricProcessInstanceQueryImpl currentOrQueryObject = null;
        protected internal bool inOrStatement = false;

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
                this.currentOrQueryObject.processInstanceId_Renamed = processInstanceId;
            }
            else
            {
                this.processInstanceId_Renamed = processInstanceId;
            }
            return this;
        }

        public virtual IHistoricProcessInstanceQuery processInstanceIds(ISet<string> processInstanceIds)
        {
            if (processInstanceIds == null)
            {
                throw new ActivitiIllegalArgumentException("Set of process instance ids is null");
            }
            if (processInstanceIds.Count == 0)
            {
                throw new ActivitiIllegalArgumentException("Set of process instance ids is empty");
            }

            if (inOrStatement)
            {
                this.currentOrQueryObject.processInstanceIds_Renamed = processInstanceIds;
            }
            else
            {
                this.processInstanceIds_Renamed = processInstanceIds;
            }
            return this;
        }

        public virtual IHistoricProcessInstanceQuery processDefinitionId(string processDefinitionId)
        {
            if (inOrStatement)
            {
                this.currentOrQueryObject.processDefinitionId_Renamed = processDefinitionId;
            }
            else
            {
                this.processDefinitionId_Renamed = processDefinitionId;
            }
            return this;
        }

        public virtual IHistoricProcessInstanceQuery processDefinitionKey(string processDefinitionKey)
        {
            if (inOrStatement)
            {
                this.currentOrQueryObject.processDefinitionKey_Renamed = processDefinitionKey;
            }
            else
            {
                this.processDefinitionKey_Renamed = processDefinitionKey;
            }
            return this;
        }

        public virtual IHistoricProcessInstanceQuery processDefinitionKeyIn(IList<string> processDefinitionKeys)
        {
            if (inOrStatement)
            {
                currentOrQueryObject.processDefinitionKeyIn_Renamed = processDefinitionKeys;
            }
            else
            {
                this.processDefinitionKeyIn_Renamed = processDefinitionKeys;
            }
            return this;
        }

        public virtual IHistoricProcessInstanceQuery processDefinitionCategory(string processDefinitionCategory)
        {
            if (inOrStatement)
            {
                this.currentOrQueryObject.processDefinitionCategory_Renamed = processDefinitionCategory;
            }
            else
            {
                this.processDefinitionCategory_Renamed = processDefinitionCategory;
            }
            return this;
        }

        public virtual IHistoricProcessInstanceQuery processDefinitionName(string processDefinitionName)
        {
            if (inOrStatement)
            {
                this.currentOrQueryObject.processDefinitionName_Renamed = processDefinitionName;
            }
            else
            {
                this.processDefinitionName_Renamed = processDefinitionName;
            }
            return this;
        }

        public virtual IHistoricProcessInstanceQuery processDefinitionVersion(int? processDefinitionVersion)
        {
            if (inOrStatement)
            {
                this.currentOrQueryObject.processDefinitionVersion_Renamed = processDefinitionVersion;
            }
            else
            {
                this.processDefinitionVersion_Renamed = processDefinitionVersion;
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
                this.currentOrQueryObject.deploymentId_Renamed = deploymentId;
            }
            else
            {
                this.deploymentId_Renamed = deploymentId;
            }
            return this;
        }

        public virtual IHistoricProcessInstanceQuery deploymentIdIn(IList<string> deploymentIds)
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
                this.currentOrQueryObject.finished_Renamed = true;
            }
            else
            {
                this.finished_Renamed = true;
            }
            return this;
        }

        public virtual IHistoricProcessInstanceQuery unfinished()
        {
            if (inOrStatement)
            {
                this.currentOrQueryObject.unfinished_Renamed = true;
            }
            else
            {
                this.unfinished_Renamed = true;
            }
            return this;
        }

        public virtual IHistoricProcessInstanceQuery deleted()
        {
            if (inOrStatement)
            {
                this.currentOrQueryObject.deleted_Renamed = true;
            }
            else
            {
                this.deleted_Renamed = true;
            }
            return this;
        }

        public virtual IHistoricProcessInstanceQuery notDeleted()
        {
            if (inOrStatement)
            {
                this.currentOrQueryObject.notDeleted_Renamed = true;
            }
            else
            {
                this.notDeleted_Renamed = true;
            }
            return this;
        }

        public virtual IHistoricProcessInstanceQuery startedBy(string startedBy)
        {
            if (inOrStatement)
            {
                this.currentOrQueryObject.startedBy_Renamed = startedBy;
            }
            else
            {
                this.startedBy_Renamed = startedBy;
            }
            return this;
        }

        public virtual IHistoricProcessInstanceQuery processDefinitionKeyNotIn(IList<string> processDefinitionKeys)
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

        public virtual IHistoricProcessInstanceQuery startedAfter(DateTime startedAfter)
        {
            if (inOrStatement)
            {
                this.currentOrQueryObject.startedAfter_Renamed = startedAfter;
            }
            else
            {
                this.startedAfter_Renamed = startedAfter;
            }
            return this;
        }

        public virtual IHistoricProcessInstanceQuery startedBefore(DateTime startedBefore)
        {
            if (inOrStatement)
            {
                this.currentOrQueryObject.startedBefore_Renamed = startedBefore;
            }
            else
            {
                this.startedBefore_Renamed = startedBefore;
            }
            return this;
        }

        public virtual IHistoricProcessInstanceQuery finishedAfter(DateTime finishedAfter)
        {
            if (inOrStatement)
            {
                this.currentOrQueryObject.finishedAfter_Renamed = finishedAfter;
            }
            else
            {
                this.finishedAfter_Renamed = finishedAfter;
                this.finished_Renamed = true;
            }
            return this;
        }

        public virtual IHistoricProcessInstanceQuery finishedBefore(DateTime finishedBefore)
        {
            if (inOrStatement)
            {
                this.currentOrQueryObject.finishedBefore_Renamed = finishedBefore;
            }
            else
            {
                this.finishedBefore_Renamed = finishedBefore;
                this.finished_Renamed = true;
            }
            return this;
        }

        public virtual IHistoricProcessInstanceQuery superProcessInstanceId(string superProcessInstanceId)
        {
            if (inOrStatement)
            {
                this.currentOrQueryObject.superProcessInstanceId_Renamed = superProcessInstanceId;
            }
            else
            {
                this.superProcessInstanceId_Renamed = superProcessInstanceId;
            }
            return this;
        }

        public virtual IHistoricProcessInstanceQuery excludeSubprocesses(bool excludeSubprocesses)
        {
            if (inOrStatement)
            {
                this.currentOrQueryObject.excludeSubprocesses_Renamed = excludeSubprocesses;
            }
            else
            {
                this.excludeSubprocesses_Renamed = excludeSubprocesses;
            }
            return this;
        }

        public virtual IHistoricProcessInstanceQuery involvedUser(string involvedUser)
        {
            if (inOrStatement)
            {
                this.currentOrQueryObject.involvedUser_Renamed = involvedUser;
            }
            else
            {
                this.involvedUser_Renamed = involvedUser;
            }
            return this;
        }

        public virtual IHistoricProcessInstanceQuery includeProcessVariables()
        {
            this.includeProcessVariables_Renamed = true;
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
            this.withJobException_Renamed = true;
            return this;
        }

        public virtual IHistoricProcessInstanceQuery processInstanceTenantId(string tenantId)
        {
            if (string.ReferenceEquals(tenantId, null))
            {
                throw new ActivitiIllegalArgumentException("process instance tenant id is null");
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
            if (string.ReferenceEquals(tenantIdLike, null))
            {
                throw new ActivitiIllegalArgumentException("process instance tenant id is null");
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
            this.locale_Renamed = locale;
            return this;
        }

        public virtual IHistoricProcessInstanceQuery withLocalizationFallback()
        {
            withLocalizationFallback_Renamed = true;
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
                if (!string.ReferenceEquals(specialOrderBy, null) && specialOrderBy.Length > 0)
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
            if (includeProcessVariables_Renamed)
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

            if (!string.ReferenceEquals(locale_Renamed, null) && !string.ReferenceEquals(processInstance.ProcessDefinitionId, null))
            {
                IProcessDefinition processDefinition = commandContext.ProcessEngineConfiguration.DeploymentManager.findDeployedProcessDefinitionById(processInstanceEntity.ProcessDefinitionId);
                JToken languageNode = Context.getLocalizationElementProperties(locale_Renamed, processDefinition.Key, processInstanceEntity.ProcessDefinitionId, withLocalizationFallback_Renamed);

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

            if (includeProcessVariables_Renamed)
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
        }

        public virtual bool Open
        {
            get
            {
                return unfinished_Renamed;
            }
        }

        public virtual string ProcessDefinitionId
        {
            get
            {
                return processDefinitionId_Renamed;
            }
        }

        public virtual string ProcessDefinitionKey
        {
            get
            {
                return processDefinitionKey_Renamed;
            }
        }

        public virtual IList<string> ProcessDefinitionKeyIn
        {
            get
            {
                return processDefinitionKeyIn_Renamed;
            }
        }

        public virtual string ProcessDefinitionIdLike
        {
            get
            {
                return processDefinitionKey_Renamed + ":%:%";
            }
        }

        public virtual string ProcessDefinitionName
        {
            get
            {
                return processDefinitionName_Renamed;
            }
        }
        public virtual string ProcessDefinitionCategory
        {
            get
            {
                return processDefinitionCategory_Renamed;
            }
        }
        public virtual int? ProcessDefinitionVersion
        {
            get
            {
                return processDefinitionVersion_Renamed;
            }
        }

        public virtual string ProcessInstanceId
        {
            get
            {
                return processInstanceId_Renamed;
            }
        }

        public virtual ISet<string> ProcessInstanceIds
        {
            get
            {
                return processInstanceIds_Renamed;
            }
        }

        public virtual string StartedBy
        {
            get
            {
                return startedBy_Renamed;
            }
        }

        public virtual string SuperProcessInstanceId
        {
            get
            {
                return superProcessInstanceId_Renamed;
            }
        }

        public virtual bool ExcludeSubprocesses
        {
            get
            {
                return excludeSubprocesses_Renamed;
            }
        }

        public virtual IList<string> ProcessKeyNotIn
        {
            get
            {
                return processKeyNotIn;
            }
        }

        public virtual DateTime StartedAfter
        {
            get
            {
                return startedAfter_Renamed;
            }
        }

        public virtual DateTime StartedBefore
        {
            get
            {
                return startedBefore_Renamed;
            }
        }

        public virtual DateTime FinishedAfter
        {
            get
            {
                return finishedAfter_Renamed;
            }
        }

        public virtual DateTime FinishedBefore
        {
            get
            {
                return finishedBefore_Renamed;
            }
        }

        public virtual string InvolvedUser
        {
            get
            {
                return involvedUser_Renamed;
            }
        }

        public virtual string Name
        {
            get
            {
                return name;
            }
        }

        public virtual string NameLike
        {
            get
            {
                return nameLike;
            }
        }

        public static long Serialversionuid
        {
            get
            {
                return serialVersionUID;
            }
        }

        public virtual string DeploymentId
        {
            get
            {
                return deploymentId_Renamed;
            }
        }

        public virtual IList<string> DeploymentIds
        {
            get
            {
                return deploymentIds;
            }
        }

        public virtual bool Finished
        {
            get
            {
                return finished_Renamed;
            }
        }

        public virtual bool Unfinished
        {
            get
            {
                return unfinished_Renamed;
            }
        }

        public virtual bool Deleted
        {
            get
            {
                return deleted_Renamed;
            }
        }

        public virtual bool NotDeleted
        {
            get
            {
                return notDeleted_Renamed;
            }
        }

        public virtual bool IncludeProcessVariables
        {
            get
            {
                return includeProcessVariables_Renamed;
            }
        }

        public virtual bool WithException
        {
            get
            {
                return withJobException_Renamed;
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

        public virtual string NameLikeIgnoreCase
        {
            get
            {
                return nameLikeIgnoreCase;
            }
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