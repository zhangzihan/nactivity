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
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json.Linq;
    using org.activiti.engine.history;
    using org.activiti.engine.impl.context;
    using org.activiti.engine.impl.interceptor;
    using org.activiti.engine.impl.persistence.entity;
    using org.activiti.engine.impl.variable;
    using org.activiti.engine.task;
    using Sys;

    /// 
    public class HistoricTaskInstanceQueryImpl : AbstractVariableQueryImpl<IHistoricTaskInstanceQuery, IHistoricTaskInstance>, IHistoricTaskInstanceQuery
    {
        private static readonly ILogger log = ProcessEngineServiceProvider.LoggerService<HistoricTaskInstanceQueryImpl>();

        private const long serialVersionUID = 1L;
        protected internal string processDefinitionId_Renamed;
        protected internal string processDefinitionKey_Renamed;
        protected internal string processDefinitionKeyLike_Renamed;
        protected internal string processDefinitionKeyLikeIgnoreCase_Renamed;
        protected internal IList<string> processDefinitionKeys;
        protected internal string processDefinitionName_Renamed;
        protected internal string processDefinitionNameLike_Renamed;
        protected internal IList<string> processCategoryInList;
        protected internal IList<string> processCategoryNotInList;
        protected internal string deploymentId_Renamed;
        protected internal IList<string> deploymentIds;
        protected internal string processInstanceId_Renamed;
        protected internal IList<string> processInstanceIds;
        protected internal string processInstanceBusinessKey_Renamed;
        protected internal string processInstanceBusinessKeyLike_Renamed;
        protected internal string processInstanceBusinessKeyLikeIgnoreCase_Renamed;
        protected internal string executionId_Renamed;
        protected internal string taskId_Renamed;
        protected internal string taskName_Renamed;
        protected internal string taskNameLike_Renamed;
        protected internal string taskNameLikeIgnoreCase_Renamed;
        protected internal IList<string> taskNameList;
        protected internal IList<string> taskNameListIgnoreCase;
        protected internal string taskParentTaskId_Renamed;
        protected internal string taskDescription_Renamed;
        protected internal string taskDescriptionLike_Renamed;
        protected internal string taskDescriptionLikeIgnoreCase_Renamed;
        protected internal string taskDeleteReason_Renamed;
        protected internal string taskDeleteReasonLike_Renamed;
        protected internal string taskOwner_Renamed;
        protected internal string taskOwnerLike_Renamed;
        protected internal string taskOwnerLikeIgnoreCase_Renamed;
        protected internal string taskAssignee_Renamed;
        protected internal string taskAssigneeLike_Renamed;
        protected internal string taskAssigneeLikeIgnoreCase_Renamed;
        protected internal IList<string> taskAssigneeIds_Renamed;
        protected internal string taskDefinitionKey_Renamed;
        protected internal string taskDefinitionKeyLike_Renamed;
        protected internal string candidateUser;
        protected internal string candidateGroup;
        private IList<string> candidateGroups;
        protected internal string involvedUser;
        protected internal IList<string> involvedGroups;
        protected internal int? taskPriority_Renamed;
        protected internal int? taskMinPriority_Renamed;
        protected internal int? taskMaxPriority_Renamed;
        protected internal bool finished_Renamed;
        protected internal bool unfinished_Renamed;
        protected internal bool processFinished_Renamed;
        protected internal bool processUnfinished_Renamed;
        protected internal DateTime dueDate;
        protected internal DateTime dueAfter;
        protected internal DateTime dueBefore;
        protected internal bool withoutDueDate;
        protected internal DateTime creationDate;
        protected internal DateTime creationAfterDate;
        protected internal DateTime creationBeforeDate;
        protected internal DateTime completedDate;
        protected internal DateTime completedAfterDate;
        protected internal DateTime completedBeforeDate;
        protected internal string category;
        protected internal string tenantId;
        protected internal string tenantIdLike;
        protected internal bool withoutTenantId;
        protected internal string locale_Renamed;
        protected internal bool withLocalizationFallback_Renamed;
        protected internal bool includeTaskLocalVariables_Renamed = false;
        protected internal bool includeProcessVariables_Renamed = false;
        protected internal int? taskVariablesLimit;
        protected internal IList<HistoricTaskInstanceQueryImpl> orQueryObjects = new List<HistoricTaskInstanceQueryImpl>();
        protected internal HistoricTaskInstanceQueryImpl currentOrQueryObject = null;
        protected internal bool inOrStatement = false;

        public HistoricTaskInstanceQueryImpl()
        {
        }

        public HistoricTaskInstanceQueryImpl(ICommandExecutor commandExecutor) : base(commandExecutor)
        {
        }

        public HistoricTaskInstanceQueryImpl(ICommandExecutor commandExecutor, string databaseType) : base(commandExecutor)
        {
            this.databaseType = databaseType;
        }

        public override long executeCount(ICommandContext commandContext)
        {
            ensureVariablesInitialized();
            checkQueryOk();
            return commandContext.HistoricTaskInstanceEntityManager.findHistoricTaskInstanceCountByQueryCriteria(this);
        }

        public override IList<IHistoricTaskInstance> executeList(ICommandContext commandContext, Page page)
        {
            ensureVariablesInitialized();
            checkQueryOk();
            IList<IHistoricTaskInstance> tasks = null;
            if (includeTaskLocalVariables_Renamed || includeProcessVariables_Renamed)
            {
                tasks = commandContext.HistoricTaskInstanceEntityManager.findHistoricTaskInstancesAndVariablesByQueryCriteria(this);
            }
            else
            {
                tasks = commandContext.HistoricTaskInstanceEntityManager.findHistoricTaskInstancesByQueryCriteria(this);
            }

            if (tasks != null && Context.ProcessEngineConfiguration.PerformanceSettings.EnableLocalization)
            {
                foreach (IHistoricTaskInstance task in tasks)
                {
                    localize(task);
                }
            }

            return tasks;
        }

        public virtual IHistoricTaskInstanceQuery processInstanceId(string processInstanceId)
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

        public virtual IHistoricTaskInstanceQuery processInstanceIdIn(IList<string> processInstanceIds)
        {
            if (processInstanceIds == null)
            {
                throw new ActivitiIllegalArgumentException("Process instance id list is null");
            }
            if (processInstanceIds.Count == 0)
            {
                throw new ActivitiIllegalArgumentException("Process instance id list is empty");
            }
            foreach (string processInstanceId in processInstanceIds)
            {
                if (ReferenceEquals(processInstanceId, null))
                {
                    throw new ActivitiIllegalArgumentException("None of the given process instance ids can be null");
                }
            }

            if (inOrStatement)
            {
                this.currentOrQueryObject.processInstanceIds = processInstanceIds;
            }
            else
            {
                this.processInstanceIds = processInstanceIds;
            }
            return this;
        }

        public virtual IHistoricTaskInstanceQuery processInstanceBusinessKey(string processInstanceBusinessKey)
        {
            if (inOrStatement)
            {
                this.currentOrQueryObject.processInstanceBusinessKey_Renamed = processInstanceBusinessKey;
            }
            else
            {
                this.processInstanceBusinessKey_Renamed = processInstanceBusinessKey;
            }
            return this;
        }

        public virtual IHistoricTaskInstanceQuery processInstanceBusinessKeyLike(string processInstanceBusinessKeyLike)
        {
            if (inOrStatement)
            {
                this.currentOrQueryObject.processInstanceBusinessKeyLike_Renamed = processInstanceBusinessKeyLike;
            }
            else
            {
                this.processInstanceBusinessKeyLike_Renamed = processInstanceBusinessKeyLike;
            }
            return this;
        }

        public virtual IHistoricTaskInstanceQuery processInstanceBusinessKeyLikeIgnoreCase(string processInstanceBusinessKeyLikeIgnoreCase)
        {
            if (inOrStatement)
            {
                this.currentOrQueryObject.processInstanceBusinessKeyLikeIgnoreCase_Renamed = processInstanceBusinessKeyLikeIgnoreCase.ToLower();
            }
            else
            {
                this.processInstanceBusinessKeyLikeIgnoreCase_Renamed = processInstanceBusinessKeyLikeIgnoreCase.ToLower();
            }
            return this;
        }

        public virtual IHistoricTaskInstanceQuery executionId(string executionId)
        {
            if (inOrStatement)
            {
                this.currentOrQueryObject.executionId_Renamed = executionId;
            }
            else
            {
                this.executionId_Renamed = executionId;
            }
            return this;
        }

        public virtual IHistoricTaskInstanceQuery processDefinitionId(string processDefinitionId)
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

        public virtual IHistoricTaskInstanceQuery processDefinitionKey(string processDefinitionKey)
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

        public virtual IHistoricTaskInstanceQuery processDefinitionKeyLike(string processDefinitionKeyLike)
        {
            if (inOrStatement)
            {
                this.currentOrQueryObject.processDefinitionKeyLike_Renamed = processDefinitionKeyLike;
            }
            else
            {
                this.processDefinitionKeyLike_Renamed = processDefinitionKeyLike;
            }
            return this;
        }

        public virtual IHistoricTaskInstanceQuery processDefinitionKeyLikeIgnoreCase(string processDefinitionKeyLikeIgnoreCase)
        {
            if (inOrStatement)
            {
                this.currentOrQueryObject.processDefinitionKeyLikeIgnoreCase_Renamed = processDefinitionKeyLikeIgnoreCase.ToLower();
            }
            else
            {
                this.processDefinitionKeyLikeIgnoreCase_Renamed = processDefinitionKeyLikeIgnoreCase.ToLower();
            }
            return this;
        }

        public virtual IHistoricTaskInstanceQuery processDefinitionKeyIn(IList<string> processDefinitionKeys)
        {
            if (inOrStatement)
            {
                this.currentOrQueryObject.processDefinitionKeys = processDefinitionKeys;
            }
            else
            {
                this.processDefinitionKeys = processDefinitionKeys;
            }
            return this;
        }

        public virtual IHistoricTaskInstanceQuery processDefinitionName(string processDefinitionName)
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

        public virtual IHistoricTaskInstanceQuery processDefinitionNameLike(string processDefinitionNameLike)
        {
            if (inOrStatement)
            {
                this.currentOrQueryObject.processDefinitionNameLike_Renamed = processDefinitionNameLike;
            }
            else
            {
                this.processDefinitionNameLike_Renamed = processDefinitionNameLike;
            }
            return this;
        }

        public virtual IHistoricTaskInstanceQuery processCategoryIn(IList<string> processCategoryInList)
        {
            if (processCategoryInList == null)
            {
                throw new ActivitiIllegalArgumentException("Process category list is null");
            }
            if (processCategoryInList.Count == 0)
            {
                throw new ActivitiIllegalArgumentException("Process category list is empty");
            }
            foreach (string processCategory in processCategoryInList)
            {
                if (ReferenceEquals(processCategory, null))
                {
                    throw new ActivitiIllegalArgumentException("None of the given process categories can be null");
                }
            }

            if (inOrStatement)
            {
                currentOrQueryObject.processCategoryInList = processCategoryInList;
            }
            else
            {
                this.processCategoryInList = processCategoryInList;
            }
            return this;
        }

        public virtual IHistoricTaskInstanceQuery processCategoryNotIn(IList<string> processCategoryNotInList)
        {
            if (processCategoryNotInList == null)
            {
                throw new ActivitiIllegalArgumentException("Process category list is null");
            }
            if (processCategoryNotInList.Count == 0)
            {
                throw new ActivitiIllegalArgumentException("Process category list is empty");
            }
            foreach (string processCategory in processCategoryNotInList)
            {
                if (ReferenceEquals(processCategory, null))
                {
                    throw new ActivitiIllegalArgumentException("None of the given process categories can be null");
                }
            }

            if (inOrStatement)
            {
                currentOrQueryObject.processCategoryNotInList = processCategoryNotInList;
            }
            else
            {
                this.processCategoryNotInList = processCategoryNotInList;
            }
            return this;
        }

        public virtual IHistoricTaskInstanceQuery deploymentId(string deploymentId)
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

        public virtual IHistoricTaskInstanceQuery deploymentIdIn(IList<string> deploymentIds)
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

        public virtual IHistoricTaskInstanceQuery taskId(string taskId)
        {
            if (inOrStatement)
            {
                this.currentOrQueryObject.taskId_Renamed = taskId;
            }
            else
            {
                this.taskId_Renamed = taskId;
            }
            return this;
        }

        public virtual IHistoricTaskInstanceQuery taskName(string taskName)
        {
            if (inOrStatement)
            {
                this.currentOrQueryObject.taskName_Renamed = taskName;
            }
            else
            {
                this.taskName_Renamed = taskName;
            }
            return this;
        }

        public virtual IHistoricTaskInstanceQuery taskNameIn(IList<string> taskNameList)
        {
            if (taskNameList == null)
            {
                throw new ActivitiIllegalArgumentException("Task name list is null");
            }
            if (taskNameList.Count == 0)
            {
                throw new ActivitiIllegalArgumentException("Task name list is empty");
            }

            if (!ReferenceEquals(taskName_Renamed, null))
            {
                throw new ActivitiIllegalArgumentException("Invalid query usage: cannot set both taskNameIn and taskName");
            }
            if (!ReferenceEquals(taskNameLike_Renamed, null))
            {
                throw new ActivitiIllegalArgumentException("Invalid query usage: cannot set both taskNameIn and taskNameLike");
            }
            if (!ReferenceEquals(taskNameLikeIgnoreCase_Renamed, null))
            {
                throw new ActivitiIllegalArgumentException("Invalid query usage: cannot set both taskNameIn and taskNameLikeIgnoreCase");
            }

            if (inOrStatement)
            {
                currentOrQueryObject.taskNameList = taskNameList;
            }
            else
            {
                this.taskNameList = taskNameList;
            }
            return this;
        }

        public virtual IHistoricTaskInstanceQuery taskNameInIgnoreCase(IList<string> taskNameList)
        {
            if (taskNameList == null)
            {
                throw new ActivitiIllegalArgumentException("Task name list is null");
            }
            if (taskNameList.Count == 0)
            {
                throw new ActivitiIllegalArgumentException("Task name list is empty");
            }
            foreach (string taskName in taskNameList)
            {
                if (ReferenceEquals(taskName, null))
                {
                    throw new ActivitiIllegalArgumentException("None of the given task names can be null");
                }
            }

            if (!ReferenceEquals(taskName_Renamed, null))
            {
                throw new ActivitiIllegalArgumentException("Invalid query usage: cannot set both taskNameInIgnoreCase and name");
            }
            if (!ReferenceEquals(taskNameLike_Renamed, null))
            {
                throw new ActivitiIllegalArgumentException("Invalid query usage: cannot set both taskNameInIgnoreCase and nameLike");
            }
            if (!ReferenceEquals(taskNameLikeIgnoreCase_Renamed, null))
            {
                throw new ActivitiIllegalArgumentException("Invalid query usage: cannot set both taskNameInIgnoreCase and nameLikeIgnoreCase");
            }

            int nameListSize = taskNameList.Count;
            IList<string> caseIgnoredTaskNameList = new List<string>(nameListSize);
            foreach (string taskName in taskNameList)
            {
                caseIgnoredTaskNameList.Add(taskName.ToLower());
            }

            if (inOrStatement)
            {
                this.currentOrQueryObject.taskNameListIgnoreCase = caseIgnoredTaskNameList;
            }
            else
            {
                this.taskNameListIgnoreCase = caseIgnoredTaskNameList;
            }
            return this;
        }

        public virtual IHistoricTaskInstanceQuery taskNameLike(string taskNameLike)
        {
            if (inOrStatement)
            {
                this.currentOrQueryObject.taskNameLike_Renamed = taskNameLike;
            }
            else
            {
                this.taskNameLike_Renamed = taskNameLike;
            }
            return this;
        }

        public virtual IHistoricTaskInstanceQuery taskNameLikeIgnoreCase(string taskNameLikeIgnoreCase)
        {
            if (inOrStatement)
            {
                this.currentOrQueryObject.taskNameLikeIgnoreCase_Renamed = taskNameLikeIgnoreCase.ToLower();
            }
            else
            {
                this.taskNameLikeIgnoreCase_Renamed = taskNameLikeIgnoreCase.ToLower();
            }
            return this;
        }

        public virtual IHistoricTaskInstanceQuery taskParentTaskId(string parentTaskId)
        {
            if (inOrStatement)
            {
                this.currentOrQueryObject.taskParentTaskId_Renamed = parentTaskId;
            }
            else
            {
                this.taskParentTaskId_Renamed = parentTaskId;
            }
            return this;
        }

        public virtual IHistoricTaskInstanceQuery taskDescription(string taskDescription)
        {
            if (inOrStatement)
            {
                this.currentOrQueryObject.taskDescription_Renamed = taskDescription;
            }
            else
            {
                this.taskDescription_Renamed = taskDescription;
            }
            return this;
        }

        public virtual IHistoricTaskInstanceQuery taskDescriptionLike(string taskDescriptionLike)
        {
            if (inOrStatement)
            {
                this.currentOrQueryObject.taskDescriptionLike_Renamed = taskDescriptionLike;
            }
            else
            {
                this.taskDescriptionLike_Renamed = taskDescriptionLike;
            }
            return this;
        }

        public virtual IHistoricTaskInstanceQuery taskDescriptionLikeIgnoreCase(string taskDescriptionLikeIgnoreCase)
        {
            if (inOrStatement)
            {
                this.currentOrQueryObject.taskDescriptionLikeIgnoreCase_Renamed = taskDescriptionLikeIgnoreCase.ToLower();
            }
            else
            {
                this.taskDescriptionLikeIgnoreCase_Renamed = taskDescriptionLikeIgnoreCase.ToLower();
            }
            return this;
        }

        public virtual IHistoricTaskInstanceQuery taskDeleteReason(string taskDeleteReason)
        {
            if (inOrStatement)
            {
                this.currentOrQueryObject.taskDeleteReason_Renamed = taskDeleteReason;
            }
            else
            {
                this.taskDeleteReason_Renamed = taskDeleteReason;
            }
            return this;
        }

        public virtual IHistoricTaskInstanceQuery taskDeleteReasonLike(string taskDeleteReasonLike)
        {
            if (inOrStatement)
            {
                this.currentOrQueryObject.taskDeleteReasonLike_Renamed = taskDeleteReasonLike;
            }
            else
            {
                this.taskDeleteReasonLike_Renamed = taskDeleteReasonLike;
            }
            return this;
        }

        public virtual IHistoricTaskInstanceQuery taskAssignee(string taskAssignee)
        {
            if (inOrStatement)
            {
                this.currentOrQueryObject.taskAssignee_Renamed = taskAssignee;
            }
            else
            {
                this.taskAssignee_Renamed = taskAssignee;
            }
            return this;
        }

        public virtual IHistoricTaskInstanceQuery taskAssigneeLike(string taskAssigneeLike)
        {
            if (inOrStatement)
            {
                this.currentOrQueryObject.taskAssigneeLike_Renamed = taskAssigneeLike;
            }
            else
            {
                this.taskAssigneeLike_Renamed = taskAssigneeLike;
            }
            return this;
        }

        public virtual IHistoricTaskInstanceQuery taskAssigneeLikeIgnoreCase(string taskAssigneeLikeIgnoreCase)
        {
            if (inOrStatement)
            {
                this.currentOrQueryObject.taskAssigneeLikeIgnoreCase_Renamed = taskAssigneeLikeIgnoreCase.ToLower();
            }
            else
            {
                this.taskAssigneeLikeIgnoreCase_Renamed = taskAssigneeLikeIgnoreCase.ToLower();
            }
            return this;
        }

        public virtual IHistoricTaskInstanceQuery taskAssigneeIds(IList<string> assigneeIds)
        {
            if (assigneeIds == null)
            {
                throw new ActivitiIllegalArgumentException("Task assignee list is null");
            }
            if (assigneeIds.Count == 0)
            {
                throw new ActivitiIllegalArgumentException("Task assignee list is empty");
            }
            foreach (string assignee in assigneeIds)
            {
                if (ReferenceEquals(assignee, null))
                {
                    throw new ActivitiIllegalArgumentException("None of the given task assignees can be null");
                }
            }

            if (!ReferenceEquals(taskAssignee_Renamed, null))
            {
                throw new ActivitiIllegalArgumentException("Invalid query usage: cannot set both taskAssigneeIds and taskAssignee");
            }
            if (!ReferenceEquals(taskAssigneeLike_Renamed, null))
            {
                throw new ActivitiIllegalArgumentException("Invalid query usage: cannot set both taskAssigneeIds and taskAssigneeLike");
            }
            if (!ReferenceEquals(taskAssigneeLikeIgnoreCase_Renamed, null))
            {
                throw new ActivitiIllegalArgumentException("Invalid query usage: cannot set both taskAssigneeIds and taskAssigneeLikeIgnoreCase");
            }

            if (inOrStatement)
            {
                currentOrQueryObject.taskAssigneeIds_Renamed = assigneeIds;
            }
            else
            {
                this.taskAssigneeIds_Renamed = assigneeIds;
            }
            return this;
        }

        public virtual IHistoricTaskInstanceQuery taskOwner(string taskOwner)
        {
            if (inOrStatement)
            {
                this.currentOrQueryObject.taskOwner_Renamed = taskOwner;
            }
            else
            {
                this.taskOwner_Renamed = taskOwner;
            }
            return this;
        }

        public virtual IHistoricTaskInstanceQuery taskOwnerLike(string taskOwnerLike)
        {
            if (inOrStatement)
            {
                this.currentOrQueryObject.taskOwnerLike_Renamed = taskOwnerLike;
            }
            else
            {
                this.taskOwnerLike_Renamed = taskOwnerLike;
            }
            return this;
        }

        public virtual IHistoricTaskInstanceQuery taskOwnerLikeIgnoreCase(string taskOwnerLikeIgnoreCase)
        {
            if (inOrStatement)
            {
                this.currentOrQueryObject.taskOwnerLikeIgnoreCase_Renamed = taskOwnerLikeIgnoreCase.ToLower();
            }
            else
            {
                this.taskOwnerLikeIgnoreCase_Renamed = taskOwnerLikeIgnoreCase.ToLower();
            }
            return this;
        }

        public virtual IHistoricTaskInstanceQuery finished()
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

        public virtual IHistoricTaskInstanceQuery unfinished()
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

        public virtual IHistoricTaskInstanceQuery taskVariableValueEquals(string variableName, object variableValue)
        {
            if (inOrStatement)
            {
                currentOrQueryObject.variableValueEquals(variableName, variableValue);
                return this;
            }
            else
            {
                return variableValueEquals(variableName, variableValue);
            }
        }

        public virtual IHistoricTaskInstanceQuery taskVariableValueEquals(object variableValue)
        {
            if (inOrStatement)
            {
                currentOrQueryObject.variableValueEquals(variableValue);
                return this;
            }
            else
            {
                return variableValueEquals(variableValue);
            }
        }

        public virtual IHistoricTaskInstanceQuery taskVariableValueEqualsIgnoreCase(string name, string value)
        {
            if (inOrStatement)
            {
                currentOrQueryObject.variableValueEqualsIgnoreCase(name, value);
                return this;
            }
            else
            {
                return variableValueEqualsIgnoreCase(name, value);
            }
        }

        public virtual IHistoricTaskInstanceQuery taskVariableValueNotEqualsIgnoreCase(string name, string value)
        {
            if (inOrStatement)
            {
                currentOrQueryObject.variableValueNotEqualsIgnoreCase(name, value);
                return this;
            }
            else
            {
                return variableValueNotEqualsIgnoreCase(name, value);
            }
        }

        public virtual IHistoricTaskInstanceQuery taskVariableValueNotEquals(string variableName, object variableValue)
        {
            if (inOrStatement)
            {
                currentOrQueryObject.variableValueNotEquals(variableName, variableValue);
                return this;
            }
            else
            {
                return variableValueNotEquals(variableName, variableValue);
            }
        }

        public virtual IHistoricTaskInstanceQuery taskVariableValueGreaterThan(string name, object value)
        {
            if (inOrStatement)
            {
                currentOrQueryObject.variableValueGreaterThan(name, value);
                return this;
            }
            else
            {
                return variableValueGreaterThan(name, value);
            }
        }

        public virtual IHistoricTaskInstanceQuery taskVariableValueGreaterThanOrEqual(string name, object value)
        {
            if (inOrStatement)
            {
                currentOrQueryObject.variableValueGreaterThanOrEqual(name, value);
                return this;
            }
            else
            {
                return variableValueGreaterThanOrEqual(name, value);
            }
        }

        public virtual IHistoricTaskInstanceQuery taskVariableValueLessThan(string name, object value)
        {
            if (inOrStatement)
            {
                currentOrQueryObject.variableValueLessThan(name, value);
                return this;
            }
            else
            {
                return variableValueLessThan(name, value);
            }
        }

        public virtual IHistoricTaskInstanceQuery taskVariableValueLessThanOrEqual(string name, object value)
        {
            if (inOrStatement)
            {
                currentOrQueryObject.variableValueLessThanOrEqual(name, value);
                return this;
            }
            else
            {
                return variableValueLessThanOrEqual(name, value);
            }
        }

        public virtual IHistoricTaskInstanceQuery taskVariableValueLike(string name, string value)
        {
            if (inOrStatement)
            {
                currentOrQueryObject.variableValueLike(name, value);
                return this;
            }
            else
            {
                return variableValueLike(name, value);
            }
        }

        public virtual IHistoricTaskInstanceQuery taskVariableValueLikeIgnoreCase(string name, string value)
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

        public virtual IHistoricTaskInstanceQuery processVariableValueEquals(string variableName, object variableValue)
        {
            if (inOrStatement)
            {
                currentOrQueryObject.variableValueEquals(variableName, variableValue, false);
                return this;
            }
            else
            {
                return variableValueEquals(variableName, variableValue, false);
            }
        }

        public virtual IHistoricTaskInstanceQuery processVariableValueNotEquals(string variableName, object variableValue)
        {
            if (inOrStatement)
            {
                currentOrQueryObject.variableValueNotEquals(variableName, variableValue, false);
                return this;
            }
            else
            {
                return variableValueNotEquals(variableName, variableValue, false);
            }
        }

        public virtual IHistoricTaskInstanceQuery processVariableValueEquals(object variableValue)
        {
            if (inOrStatement)
            {
                currentOrQueryObject.variableValueEquals(variableValue, false);
                return this;
            }
            else
            {
                return variableValueEquals(variableValue, false);
            }
        }

        public virtual IHistoricTaskInstanceQuery processVariableValueEqualsIgnoreCase(string name, string value)
        {
            if (inOrStatement)
            {
                currentOrQueryObject.variableValueEqualsIgnoreCase(name, value, false);
                return this;
            }
            else
            {
                return variableValueEqualsIgnoreCase(name, value, false);
            }
        }

        public virtual IHistoricTaskInstanceQuery processVariableValueNotEqualsIgnoreCase(string name, string value)
        {
            if (inOrStatement)
            {
                currentOrQueryObject.variableValueNotEqualsIgnoreCase(name, value, false);
                return this;
            }
            else
            {
                return variableValueNotEqualsIgnoreCase(name, value, false);
            }
        }

        public virtual IHistoricTaskInstanceQuery processVariableValueGreaterThan(string name, object value)
        {
            if (inOrStatement)
            {
                currentOrQueryObject.variableValueGreaterThan(name, value, false);
                return this;
            }
            else
            {
                return variableValueGreaterThan(name, value, false);
            }
        }

        public virtual IHistoricTaskInstanceQuery processVariableValueGreaterThanOrEqual(string name, object value)
        {
            if (inOrStatement)
            {
                currentOrQueryObject.variableValueGreaterThanOrEqual(name, value, false);
                return this;
            }
            else
            {
                return variableValueGreaterThanOrEqual(name, value, false);
            }
        }

        public virtual IHistoricTaskInstanceQuery processVariableValueLessThan(string name, object value)
        {
            if (inOrStatement)
            {
                currentOrQueryObject.variableValueLessThan(name, value, false);
                return this;
            }
            else
            {
                return variableValueLessThan(name, value, false);
            }
        }

        public virtual IHistoricTaskInstanceQuery processVariableValueLessThanOrEqual(string name, object value)
        {
            if (inOrStatement)
            {
                currentOrQueryObject.variableValueLessThanOrEqual(name, value, false);
                return this;
            }
            else
            {
                return variableValueLessThanOrEqual(name, value, false);
            }
        }

        public virtual IHistoricTaskInstanceQuery processVariableValueLike(string name, string value)
        {
            if (inOrStatement)
            {
                currentOrQueryObject.variableValueLike(name, value, false);
                return this;
            }
            else
            {
                return variableValueLike(name, value, false);
            }
        }

        public virtual IHistoricTaskInstanceQuery processVariableValueLikeIgnoreCase(string name, string value)
        {
            if (inOrStatement)
            {
                currentOrQueryObject.variableValueLikeIgnoreCase(name, value, false);
                return this;
            }
            else
            {
                return variableValueLikeIgnoreCase(name, value, false);
            }
        }

        public virtual IHistoricTaskInstanceQuery taskDefinitionKey(string taskDefinitionKey)
        {
            if (inOrStatement)
            {
                this.currentOrQueryObject.taskDefinitionKey_Renamed = taskDefinitionKey;
            }
            else
            {
                this.taskDefinitionKey_Renamed = taskDefinitionKey;
            }
            return this;
        }

        public virtual IHistoricTaskInstanceQuery taskDefinitionKeyLike(string taskDefinitionKeyLike)
        {
            if (inOrStatement)
            {
                this.currentOrQueryObject.taskDefinitionKeyLike_Renamed = taskDefinitionKeyLike;
            }
            else
            {
                this.taskDefinitionKeyLike_Renamed = taskDefinitionKeyLike;
            }
            return this;
        }

        public virtual IHistoricTaskInstanceQuery taskPriority(int? taskPriority)
        {
            if (inOrStatement)
            {
                this.currentOrQueryObject.taskPriority_Renamed = taskPriority;
            }
            else
            {
                this.taskPriority_Renamed = taskPriority;
            }
            return this;
        }

        public virtual IHistoricTaskInstanceQuery taskMinPriority(int? taskMinPriority)
        {
            if (inOrStatement)
            {
                this.currentOrQueryObject.taskMinPriority_Renamed = taskMinPriority;
            }
            else
            {
                this.taskMinPriority_Renamed = taskMinPriority;
            }
            return this;
        }

        public virtual IHistoricTaskInstanceQuery taskMaxPriority(int? taskMaxPriority)
        {
            if (inOrStatement)
            {
                this.currentOrQueryObject.taskMaxPriority_Renamed = taskMaxPriority;
            }
            else
            {
                this.taskMaxPriority_Renamed = taskMaxPriority;
            }
            return this;
        }

        public virtual IHistoricTaskInstanceQuery processFinished()
        {
            if (inOrStatement)
            {
                this.currentOrQueryObject.processFinished_Renamed = true;
            }
            else
            {
                this.processFinished_Renamed = true;
            }
            return this;
        }

        public virtual IHistoricTaskInstanceQuery processUnfinished()
        {
            if (inOrStatement)
            {
                this.currentOrQueryObject.processUnfinished_Renamed = true;
            }
            else
            {
                this.processUnfinished_Renamed = true;
            }
            return this;
        }

        protected internal override void ensureVariablesInitialized()
        {
            IVariableTypes types = Context.ProcessEngineConfiguration.VariableTypes;
            foreach (QueryVariableValue var in queryVariableValues)
            {
                var.initialize(types);
            }

            foreach (HistoricTaskInstanceQueryImpl orQueryObject in orQueryObjects)
            {
                orQueryObject.ensureVariablesInitialized();
            }
        }

        public virtual IHistoricTaskInstanceQuery taskDueDate(DateTime dueDate)
        {
            if (inOrStatement)
            {
                this.currentOrQueryObject.dueDate = dueDate;
            }
            else
            {
                this.dueDate = dueDate;
            }
            return this;
        }

        public virtual IHistoricTaskInstanceQuery taskDueAfter(DateTime dueAfter)
        {
            if (inOrStatement)
            {
                this.currentOrQueryObject.dueAfter = dueAfter;
            }
            else
            {
                this.dueAfter = dueAfter;
            }
            return this;
        }

        public virtual IHistoricTaskInstanceQuery taskDueBefore(DateTime dueBefore)
        {
            if (inOrStatement)
            {
                this.currentOrQueryObject.dueBefore = dueBefore;
            }
            else
            {
                this.dueBefore = dueBefore;
            }
            return this;
        }

        public virtual IHistoricTaskInstanceQuery taskCreatedOn(DateTime creationDate)
        {
            if (inOrStatement)
            {
                this.currentOrQueryObject.creationDate = creationDate;
            }
            else
            {
                this.creationDate = creationDate;
            }
            return this;
        }

        public virtual IHistoricTaskInstanceQuery taskCreatedBefore(DateTime creationBeforeDate)
        {
            if (inOrStatement)
            {
                this.currentOrQueryObject.creationBeforeDate = creationBeforeDate;
            }
            else
            {
                this.creationBeforeDate = creationBeforeDate;
            }
            return this;
        }

        public virtual IHistoricTaskInstanceQuery taskCreatedAfter(DateTime creationAfterDate)
        {
            if (inOrStatement)
            {
                this.currentOrQueryObject.creationAfterDate = creationAfterDate;
            }
            else
            {
                this.creationAfterDate = creationAfterDate;
            }
            return this;
        }

        public virtual IHistoricTaskInstanceQuery taskCompletedOn(DateTime completedDate)
        {
            if (inOrStatement)
            {
                this.currentOrQueryObject.completedDate = completedDate;
            }
            else
            {
                this.completedDate = completedDate;
            }
            return this;
        }

        public virtual IHistoricTaskInstanceQuery taskCompletedBefore(DateTime completedBeforeDate)
        {
            if (inOrStatement)
            {
                this.currentOrQueryObject.completedBeforeDate = completedBeforeDate;
            }
            else
            {
                this.completedBeforeDate = completedBeforeDate;
            }
            return this;
        }

        public virtual IHistoricTaskInstanceQuery taskCompletedAfter(DateTime completedAfterDate)
        {
            if (inOrStatement)
            {
                this.currentOrQueryObject.completedAfterDate = completedAfterDate;
            }
            else
            {
                this.completedAfterDate = completedAfterDate;
            }
            return this;
        }

        public virtual IHistoricTaskInstanceQuery withoutTaskDueDate()
        {
            if (inOrStatement)
            {
                this.currentOrQueryObject.withoutDueDate = true;
            }
            else
            {
                this.withoutDueDate = true;
            }
            return this;
        }

        public virtual IHistoricTaskInstanceQuery taskCategory(string category)
        {
            if (inOrStatement)
            {
                this.currentOrQueryObject.category = category;
            }
            else
            {
                this.category = category;
            }
            return this;
        }


        public virtual IHistoricTaskInstanceQuery taskCandidateUser(string candidateUser)
        {
            if (ReferenceEquals(candidateUser, null))
            {
                throw new ActivitiIllegalArgumentException("Candidate user is null");
            }

            if (inOrStatement)
            {
                this.currentOrQueryObject.candidateUser = candidateUser;
            }
            else
            {
                this.candidateUser = candidateUser;
            }
            return this;
        }

        public virtual IHistoricTaskInstanceQuery taskCandidateUser(string candidateUser, IList<string> usersGroups)
        {
            if (ReferenceEquals(candidateUser, null))
            {
                throw new ActivitiIllegalArgumentException("Candidate user is null");
            }

            if (inOrStatement)
            {
                this.currentOrQueryObject.candidateUser = candidateUser;
                this.currentOrQueryObject.candidateGroups = usersGroups;
            }
            else
            {
                this.candidateUser = candidateUser;
                this.candidateGroups = usersGroups;
            }
            return this;
        }

        public virtual IHistoricTaskInstanceQuery taskCandidateGroup(string candidateGroup)
        {
            if (ReferenceEquals(candidateGroup, null))
            {
                throw new ActivitiIllegalArgumentException("Candidate group is null");
            }

            if (candidateGroups != null)
            {
                throw new ActivitiIllegalArgumentException("Invalid query usage: cannot set both candidateGroup and candidateGroupIn");
            }

            if (inOrStatement)
            {
                this.currentOrQueryObject.candidateGroup = candidateGroup;
            }
            else
            {
                this.candidateGroup = candidateGroup;
            }
            return this;
        }

        public virtual IHistoricTaskInstanceQuery taskCandidateGroupIn(IList<string> candidateGroups)
        {
            if (candidateGroups == null)
            {
                throw new ActivitiIllegalArgumentException("Candidate group list is null");
            }

            if (candidateGroups.Count == 0)
            {
                throw new ActivitiIllegalArgumentException("Candidate group list is empty");
            }

            if (!ReferenceEquals(candidateGroup, null))
            {
                throw new ActivitiIllegalArgumentException("Invalid query usage: cannot set both candidateGroupIn and candidateGroup");
            }

            if (inOrStatement)
            {
                this.currentOrQueryObject.candidateGroups = candidateGroups;
            }
            else
            {
                this.candidateGroups = candidateGroups;
            }
            return this;
        }

        public virtual IHistoricTaskInstanceQuery taskInvolvedUser(string involvedUser)
        {
            if (inOrStatement)
            {
                this.currentOrQueryObject.involvedUser = involvedUser;
            }
            else
            {
                this.involvedUser = involvedUser;
            }
            return this;
        }

        public virtual IHistoricTaskInstanceQuery taskInvolvedGroupsIn(IList<string> involvedGroups)
        {
            if (involvedGroups == null || involvedGroups.Count == 0)
            {
                throw new ActivitiIllegalArgumentException("Involved groups list is null or empty.");
            }

            if (inOrStatement)
            {
                this.currentOrQueryObject.involvedGroups = involvedGroups;
            }
            else
            {
                this.involvedGroups = involvedGroups;
            }
            return this;
        }

        public virtual IHistoricTaskInstanceQuery taskTenantId(string tenantId)
        {
            if (ReferenceEquals(tenantId, null))
            {
                throw new ActivitiIllegalArgumentException("task tenant id is null");
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

        public virtual IHistoricTaskInstanceQuery taskTenantIdLike(string tenantIdLike)
        {
            if (ReferenceEquals(tenantIdLike, null))
            {
                throw new ActivitiIllegalArgumentException("task tenant id is null");
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

        public virtual IHistoricTaskInstanceQuery taskWithoutTenantId()
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

        public virtual IHistoricTaskInstanceQuery locale(string locale)
        {
            this.locale_Renamed = locale;
            return this;
        }

        public virtual IHistoricTaskInstanceQuery withLocalizationFallback()
        {
            withLocalizationFallback_Renamed = true;
            return this;
        }

        public virtual IHistoricTaskInstanceQuery includeTaskLocalVariables()
        {
            this.includeTaskLocalVariables_Renamed = true;
            return this;
        }

        public virtual IHistoricTaskInstanceQuery includeProcessVariables()
        {
            this.includeProcessVariables_Renamed = true;
            return this;
        }

        public virtual IHistoricTaskInstanceQuery limitTaskVariables(int? taskVariablesLimit)
        {
            this.taskVariablesLimit = taskVariablesLimit;
            return this;
        }

        public virtual int? TaskVariablesLimit
        {
            get
            {
                return taskVariablesLimit;
            }
        }

        public virtual IHistoricTaskInstanceQuery or()
        {
            if (inOrStatement)
            {
                throw new ActivitiException("the query is already in an or statement");
            }

            inOrStatement = true;
            currentOrQueryObject = new HistoricTaskInstanceQueryImpl();
            orQueryObjects.Add(currentOrQueryObject);
            return this;
        }

        public virtual IHistoricTaskInstanceQuery endOr()
        {
            if (!inOrStatement)
            {
                throw new ActivitiException("endOr() can only be called after calling or()");
            }

            inOrStatement = false;
            currentOrQueryObject = null;
            return this;
        }

        protected internal virtual void localize(IHistoricTaskInstance task)
        {
            IHistoricTaskInstanceEntity taskEntity = (IHistoricTaskInstanceEntity)task;
            taskEntity.LocalizedName = null;
            taskEntity.LocalizedDescription = null;

            if (!ReferenceEquals(locale_Renamed, null))
            {
                string processDefinitionId = task.ProcessDefinitionId;
                if (!string.IsNullOrWhiteSpace(processDefinitionId))
                {
                    JToken languageNode = Context.getLocalizationElementProperties(locale_Renamed, task.TaskDefinitionKey, processDefinitionId, withLocalizationFallback_Renamed);
                    if (languageNode != null)
                    {
                        JToken languageNameNode = languageNode[DynamicBpmnConstants_Fields.LOCALIZATION_NAME];
                        if (languageNameNode != null)
                        {
                            taskEntity.LocalizedName = languageNameNode.ToString();
                        }

                        JToken languageDescriptionNode = languageNode[DynamicBpmnConstants_Fields.LOCALIZATION_DESCRIPTION];
                        if (languageDescriptionNode != null)
                        {
                            taskEntity.LocalizedDescription = languageDescriptionNode.ToString();
                        }
                    }
                }
            }
        }

        // ordering
        // /////////////////////////////////////////////////////////////////

        public virtual IHistoricTaskInstanceQuery orderByTaskId()
        {
            orderBy(HistoricTaskInstanceQueryProperty.HISTORIC_TASK_INSTANCE_ID);
            return this;
        }

        public virtual IHistoricTaskInstanceQuery orderByHistoricActivityInstanceId()
        {
            orderBy(HistoricTaskInstanceQueryProperty.PROCESS_DEFINITION_ID);
            return this;
        }

        public virtual IHistoricTaskInstanceQuery orderByProcessDefinitionId()
        {
            orderBy(HistoricTaskInstanceQueryProperty.PROCESS_DEFINITION_ID);
            return this;
        }

        public virtual IHistoricTaskInstanceQuery orderByProcessInstanceId()
        {
            orderBy(HistoricTaskInstanceQueryProperty.PROCESS_INSTANCE_ID);
            return this;
        }

        public virtual IHistoricTaskInstanceQuery orderByExecutionId()
        {
            orderBy(HistoricTaskInstanceQueryProperty.EXECUTION_ID);
            return this;
        }

        public virtual IHistoricTaskInstanceQuery orderByHistoricTaskInstanceDuration()
        {
            orderBy(HistoricTaskInstanceQueryProperty.DURATION);
            return this;
        }

        public virtual IHistoricTaskInstanceQuery orderByHistoricTaskInstanceEndTime()
        {
            orderBy(HistoricTaskInstanceQueryProperty.END);
            return this;
        }

        public virtual IHistoricTaskInstanceQuery orderByHistoricActivityInstanceStartTime()
        {
            orderBy(HistoricTaskInstanceQueryProperty.START);
            return this;
        }

        public virtual IHistoricTaskInstanceQuery orderByHistoricTaskInstanceStartTime()
        {
            orderBy(HistoricTaskInstanceQueryProperty.START);
            return this;
        }

        public virtual IHistoricTaskInstanceQuery orderByTaskCreateTime()
        {
            return orderByHistoricTaskInstanceStartTime();
        }

        public virtual IHistoricTaskInstanceQuery orderByTaskName()
        {
            orderBy(HistoricTaskInstanceQueryProperty.TASK_NAME);
            return this;
        }

        public virtual IHistoricTaskInstanceQuery orderByTaskDescription()
        {
            orderBy(HistoricTaskInstanceQueryProperty.TASK_DESCRIPTION);
            return this;
        }

        public virtual IHistoricTaskInstanceQuery orderByTaskAssignee()
        {
            orderBy(HistoricTaskInstanceQueryProperty.TASK_ASSIGNEE);
            return this;
        }

        public virtual IHistoricTaskInstanceQuery orderByTaskOwner()
        {
            orderBy(HistoricTaskInstanceQueryProperty.TASK_OWNER);
            return this;
        }

        public virtual IHistoricTaskInstanceQuery orderByTaskDueDate()
        {
            orderBy(HistoricTaskInstanceQueryProperty.TASK_DUE_DATE);
            return this;
        }

        public virtual IHistoricTaskInstanceQuery orderByDueDateNullsFirst()
        {
            return orderBy(HistoricTaskInstanceQueryProperty.TASK_DUE_DATE, NullHandlingOnOrder.NULLS_FIRST);
        }

        public virtual IHistoricTaskInstanceQuery orderByDueDateNullsLast()
        {
            return orderBy(HistoricTaskInstanceQueryProperty.TASK_DUE_DATE, NullHandlingOnOrder.NULLS_LAST);
        }

        public virtual IHistoricTaskInstanceQuery orderByDeleteReason()
        {
            orderBy(HistoricTaskInstanceQueryProperty.DELETE_REASON);
            return this;
        }

        public virtual IHistoricTaskInstanceQuery orderByTaskDefinitionKey()
        {
            orderBy(HistoricTaskInstanceQueryProperty.TASK_DEFINITION_KEY);
            return this;
        }

        public virtual IHistoricTaskInstanceQuery orderByTaskPriority()
        {
            orderBy(HistoricTaskInstanceQueryProperty.TASK_PRIORITY);
            return this;
        }

        public virtual IHistoricTaskInstanceQuery orderByTenantId()
        {
            orderBy(HistoricTaskInstanceQueryProperty.TENANT_ID_);
            return this;
        }

        protected internal override void checkQueryOk()
        {
            base.checkQueryOk();
            // In case historic query variables are included, an additional order-by
            // clause should be added
            // to ensure the last value of a variable is used
            if (includeProcessVariables_Renamed || includeTaskLocalVariables_Renamed)
            {
                this.orderBy(HistoricTaskInstanceQueryProperty.INCLUDED_VARIABLE_TIME).asc();
            }
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

        public virtual IList<string> CandidateGroups
        {
            get
            {
                if (!ReferenceEquals(candidateGroup, null))
                {
                    IList<string> candidateGroupList = new List<string>(1);
                    candidateGroupList.Add(candidateGroup);
                    return candidateGroupList;

                }
                else if (candidateGroups != null)
                {
                    return candidateGroups;

                }
                else if (!ReferenceEquals(candidateUser, null))
                {
                    return getGroupsForCandidateUser(candidateUser);
                }
                return null;
            }
        }

        protected internal virtual IList<string> getGroupsForCandidateUser(string candidateUser)
        {
            IUserGroupLookupProxy userGroupLookupProxy = Context.ProcessEngineConfiguration.UserGroupLookupProxy;
            if (userGroupLookupProxy != null)
            {
                return userGroupLookupProxy.getGroupsForCandidateUser(candidateUser);
            }
            else
            {
                log.LogWarning("No UserGroupLookupProxy set on ProcessEngineConfiguration. Tasks queried only where user is directly related, not through groups.");
            }
            return null;
        }

        // getters and setters
        // //////////////////////////////////////////////////////

        public virtual string ProcessInstanceId
        {
            get
            {
                return processInstanceId_Renamed;
            }
        }

        public virtual IList<string> ProcessInstanceIds
        {
            get
            {
                return processInstanceIds;
            }
        }

        public virtual string ProcessInstanceBusinessKey
        {
            get
            {
                return processInstanceBusinessKey_Renamed;
            }
        }

        public virtual string ExecutionId
        {
            get
            {
                return executionId_Renamed;
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

        public virtual string ProcessDefinitionKeyLike
        {
            get
            {
                return processDefinitionKeyLike_Renamed;
            }
        }
        public virtual IList<string> ProcessDefinitionKeys
        {
            get
            {
                return processDefinitionKeys;
            }
        }
        public virtual string ProcessDefinitionName
        {
            get
            {
                return processDefinitionName_Renamed;
            }
        }

        public virtual string ProcessDefinitionNameLike
        {
            get
            {
                return processDefinitionNameLike_Renamed;
            }
        }

        public virtual IList<string> ProcessCategoryInList
        {
            get
            {
                return processCategoryInList;
            }
        }

        public virtual IList<string> ProcessCategoryNotInList
        {
            get
            {
                return processCategoryNotInList;
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

        public virtual string ProcessInstanceBusinessKeyLike
        {
            get
            {
                return processInstanceBusinessKeyLike_Renamed;
            }
        }

        public virtual string TaskDefinitionKeyLike
        {
            get
            {
                return taskDefinitionKeyLike_Renamed;
            }
        }

        public virtual int? TaskPriority
        {
            get
            {
                return taskPriority_Renamed;
            }
        }

        public virtual int? TaskMinPriority
        {
            get
            {
                return taskMinPriority_Renamed;
            }
        }

        public virtual int? TaskMaxPriority
        {
            get
            {
                return taskMaxPriority_Renamed;
            }
        }

        public virtual bool ProcessFinished
        {
            get
            {
                return processFinished_Renamed;
            }
        }

        public virtual bool ProcessUnfinished
        {
            get
            {
                return processUnfinished_Renamed;
            }
        }

        public virtual DateTime DueDate
        {
            get
            {
                return dueDate;
            }
        }

        public virtual DateTime DueAfter
        {
            get
            {
                return dueAfter;
            }
        }

        public virtual DateTime DueBefore
        {
            get
            {
                return dueBefore;
            }
        }

        public virtual bool WithoutDueDate
        {
            get
            {
                return withoutDueDate;
            }
        }

        public virtual DateTime CreationAfterDate
        {
            get
            {
                return creationAfterDate;
            }
        }

        public virtual DateTime CreationBeforeDate
        {
            get
            {
                return creationBeforeDate;
            }
        }

        public virtual DateTime CompletedDate
        {
            get
            {
                return completedDate;
            }
        }

        public virtual DateTime CompletedAfterDate
        {
            get
            {
                return completedAfterDate;
            }
        }

        public virtual DateTime CompletedBeforeDate
        {
            get
            {
                return completedBeforeDate;
            }
        }

        public virtual string Category
        {
            get
            {
                return category;
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

        public virtual bool IncludeTaskLocalVariables
        {
            get
            {
                return includeTaskLocalVariables_Renamed;
            }
        }

        public virtual bool IncludeProcessVariables
        {
            get
            {
                return includeProcessVariables_Renamed;
            }
        }

        public virtual bool InOrStatement
        {
            get
            {
                return inOrStatement;
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

        public virtual string TaskName
        {
            get
            {
                return taskName_Renamed;
            }
        }

        public virtual string TaskNameLike
        {
            get
            {
                return taskNameLike_Renamed;
            }
        }

        public virtual IList<string> TaskNameList
        {
            get
            {
                return taskNameList;
            }
        }

        public virtual IList<string> TaskNameListIgnoreCase
        {
            get
            {
                return taskNameListIgnoreCase;
            }
        }

        public virtual string TaskDescription
        {
            get
            {
                return taskDescription_Renamed;
            }
        }

        public virtual string TaskDescriptionLike
        {
            get
            {
                return taskDescriptionLike_Renamed;
            }
        }

        public virtual string TaskDeleteReason
        {
            get
            {
                return taskDeleteReason_Renamed;
            }
        }

        public virtual string TaskDeleteReasonLike
        {
            get
            {
                return taskDeleteReasonLike_Renamed;
            }
        }

        public virtual IList<string> TaskAssigneeIds
        {
            get
            {
                return taskAssigneeIds_Renamed;
            }
        }

        public virtual string TaskAssignee
        {
            get
            {
                return taskAssignee_Renamed;
            }
        }

        public virtual string TaskAssigneeLike
        {
            get
            {
                return taskAssigneeLike_Renamed;
            }
        }

        public virtual string TaskId
        {
            get
            {
                return taskId_Renamed;
            }
        }

        public virtual string TaskDefinitionKey
        {
            get
            {
                return taskDefinitionKey_Renamed;
            }
        }

        public virtual string TaskOwnerLike
        {
            get
            {
                return taskOwnerLike_Renamed;
            }
        }

        public virtual string TaskOwner
        {
            get
            {
                return taskOwner_Renamed;
            }
        }

        public virtual string TaskParentTaskId
        {
            get
            {
                return taskParentTaskId_Renamed;
            }
        }

        public virtual DateTime CreationDate
        {
            get
            {
                return creationDate;
            }
        }

        public virtual string CandidateUser
        {
            get
            {
                return candidateUser;
            }
        }

        public virtual string CandidateGroup
        {
            get
            {
                return candidateGroup;
            }
        }

        public virtual string InvolvedUser
        {
            get
            {
                return involvedUser;
            }
        }
        public virtual IList<string> InvolvedGroups
        {
            get
            {
                return involvedGroups;
            }
        }
        public virtual string ProcessDefinitionKeyLikeIgnoreCase
        {
            get
            {
                return processDefinitionKeyLikeIgnoreCase_Renamed;
            }
        }

        public virtual string ProcessInstanceBusinessKeyLikeIgnoreCase
        {
            get
            {
                return processInstanceBusinessKeyLikeIgnoreCase_Renamed;
            }
        }

        public virtual string TaskNameLikeIgnoreCase
        {
            get
            {
                return taskNameLikeIgnoreCase_Renamed;
            }
        }

        public virtual string TaskDescriptionLikeIgnoreCase
        {
            get
            {
                return taskDescriptionLikeIgnoreCase_Renamed;
            }
        }

        public virtual string TaskOwnerLikeIgnoreCase
        {
            get
            {
                return taskOwnerLikeIgnoreCase_Renamed;
            }
        }

        public virtual string TaskAssigneeLikeIgnoreCase
        {
            get
            {
                return taskAssigneeLikeIgnoreCase_Renamed;
            }
        }

        public virtual string Locale
        {
            get
            {
                return locale_Renamed;
            }
        }

        public virtual IList<HistoricTaskInstanceQueryImpl> OrQueryObjects
        {
            get
            {
                return orQueryObjects;
            }
        }
    }

}