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
    using System.Linq;

    /// 
    public class HistoricTaskInstanceQueryImpl : AbstractVariableQueryImpl<IHistoricTaskInstanceQuery, IHistoricTaskInstance>, IHistoricTaskInstanceQuery
    {
        private static readonly ILogger log = ProcessEngineServiceProvider.LoggerService<HistoricTaskInstanceQueryImpl>();

        private const long serialVersionUID = 1L;
        protected internal string processDefinitionId_;
        protected internal string processDefinitionKey_;
        protected internal string processDefinitionKeyLike_;
        protected internal string processDefinitionKeyLikeIgnoreCase_;
        protected internal IList<string> processDefinitionKeys;
        protected internal string processDefinitionName_;
        protected internal string processDefinitionNameLike_;
        protected internal IList<string> processCategoryInList;
        protected internal IList<string> processCategoryNotInList;
        protected internal string deploymentId_;
        protected internal IList<string> deploymentIds;
        protected internal string processInstanceId_;
        protected internal IList<string> processInstanceIds;
        protected internal string processInstanceBusinessKey_;
        protected internal string processInstanceBusinessKeyLike_;
        protected internal string processInstanceBusinessKeyLikeIgnoreCase_;
        protected internal string executionId_;
        protected internal string taskId_;
        protected internal string taskName_;
        protected internal string taskNameLike_;
        protected internal string taskNameLikeIgnoreCase_;
        protected internal IList<string> taskNameList;
        protected internal IList<string> taskNameListIgnoreCase;
        protected internal string taskParentTaskId_;
        protected internal string taskDescription_;
        protected internal string taskDescriptionLike_;
        protected internal string taskDescriptionLikeIgnoreCase_;
        protected internal string taskDeleteReason_;
        protected internal string taskDeleteReasonLike_;
        protected internal string taskOwner_;
        protected internal string taskOwnerLike_;
        protected internal string taskOwnerLikeIgnoreCase_;
        protected internal string taskAssignee_;
        protected internal string taskAssigneeLike_;
        protected internal string taskAssigneeLikeIgnoreCase_;
        protected internal IList<string> taskAssigneeIds_;
        protected internal string taskDefinitionKey_;
        protected internal string taskDefinitionKeyLike_;
        protected internal string candidateUser;
        protected internal string candidateGroup;
        private IList<string> candidateGroups;
        protected internal string involvedUser;
        protected internal IList<string> involvedGroups;
        protected internal int? taskPriority_;
        protected internal int? taskMinPriority_;
        protected internal int? taskMaxPriority_;
        protected internal bool? finished_;
        protected internal bool? unfinished_;
        protected internal bool? processFinished_;
        protected internal bool? processUnfinished_;
        protected internal DateTime? dueDate;
        protected internal DateTime? dueAfter;
        protected internal DateTime? dueBefore;
        protected internal bool? withoutDueDate;
        protected internal DateTime? creationDate;
        protected internal DateTime? creationAfterDate;
        protected internal DateTime? creationBeforeDate;
        protected internal DateTime? completedDate;
        protected internal DateTime? completedAfterDate;
        protected internal DateTime? completedBeforeDate;
        protected internal string category;
        protected internal string tenantId;
        protected internal string tenantIdLike;
        protected internal bool? withoutTenantId;
        protected internal string locale_;
        protected internal bool? withLocalizationFallback_;
        protected internal bool? includeTaskLocalVariables_ = false;
        protected internal bool? includeProcessVariables_ = false;
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
            if (includeTaskLocalVariables_.GetValueOrDefault() || includeProcessVariables_.GetValueOrDefault())
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
                this.currentOrQueryObject.processInstanceId_ = processInstanceId;
            }
            else
            {
                this.processInstanceId_ = processInstanceId;
            }
            return this;
        }

        public virtual IHistoricTaskInstanceQuery processInstanceIdIn(IList<string> processInstanceIds)
        {
            //if (processInstanceIds?.Count == 0)
            //{
            //    throw new ActivitiIllegalArgumentException("Process instance id list is empty");
            //}
            //foreach (string processInstanceId in processInstanceIds)
            //{
            //    if (ReferenceEquals(processInstanceId, null))
            //    {
            //        throw new ActivitiIllegalArgumentException("None of the given process instance ids can be null");
            //    }
            //}

            var ids = processInstanceIds?.Where(x => x != null) ?? new List<string>();

            if (inOrStatement)
            {
                this.currentOrQueryObject.processInstanceIds = ids.Count() == 0 ? null : ids.ToList();
            }
            else
            {
                this.processInstanceIds = processInstanceIds = ids.Count() == 0 ? null : ids.ToList();
            }
            return this;
        }

        public virtual IHistoricTaskInstanceQuery processInstanceBusinessKey(string processInstanceBusinessKey)
        {
            if (inOrStatement)
            {
                this.currentOrQueryObject.processInstanceBusinessKey_ = processInstanceBusinessKey;
            }
            else
            {
                this.processInstanceBusinessKey_ = processInstanceBusinessKey;
            }
            return this;
        }

        public virtual IHistoricTaskInstanceQuery processInstanceBusinessKeyLike(string processInstanceBusinessKeyLike)
        {
            if (inOrStatement)
            {
                this.currentOrQueryObject.processInstanceBusinessKeyLike_ = processInstanceBusinessKeyLike;
            }
            else
            {
                this.processInstanceBusinessKeyLike_ = processInstanceBusinessKeyLike;
            }
            return this;
        }

        public virtual IHistoricTaskInstanceQuery processInstanceBusinessKeyLikeIgnoreCase(string processInstanceBusinessKeyLikeIgnoreCase)
        {
            if (inOrStatement)
            {
                this.currentOrQueryObject.processInstanceBusinessKeyLikeIgnoreCase_ = processInstanceBusinessKeyLikeIgnoreCase.ToLower();
            }
            else
            {
                this.processInstanceBusinessKeyLikeIgnoreCase_ = processInstanceBusinessKeyLikeIgnoreCase.ToLower();
            }
            return this;
        }

        public virtual IHistoricTaskInstanceQuery executionId(string executionId)
        {
            if (inOrStatement)
            {
                this.currentOrQueryObject.executionId_ = executionId;
            }
            else
            {
                this.executionId_ = executionId;
            }
            return this;
        }

        public virtual IHistoricTaskInstanceQuery processDefinitionId(string processDefinitionId)
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

        public virtual IHistoricTaskInstanceQuery processDefinitionKey(string processDefinitionKey)
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

        public virtual IHistoricTaskInstanceQuery processDefinitionKeyLike(string processDefinitionKeyLike)
        {
            if (inOrStatement)
            {
                this.currentOrQueryObject.processDefinitionKeyLike_ = processDefinitionKeyLike;
            }
            else
            {
                this.processDefinitionKeyLike_ = processDefinitionKeyLike;
            }
            return this;
        }

        public virtual IHistoricTaskInstanceQuery processDefinitionKeyLikeIgnoreCase(string processDefinitionKeyLikeIgnoreCase)
        {
            if (inOrStatement)
            {
                this.currentOrQueryObject.processDefinitionKeyLikeIgnoreCase_ = processDefinitionKeyLikeIgnoreCase.ToLower();
            }
            else
            {
                this.processDefinitionKeyLikeIgnoreCase_ = processDefinitionKeyLikeIgnoreCase.ToLower();
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
                this.currentOrQueryObject.processDefinitionName_ = processDefinitionName;
            }
            else
            {
                this.processDefinitionName_ = processDefinitionName;
            }
            return this;
        }

        public virtual IHistoricTaskInstanceQuery processDefinitionNameLike(string processDefinitionNameLike)
        {
            if (inOrStatement)
            {
                this.currentOrQueryObject.processDefinitionNameLike_ = processDefinitionNameLike;
            }
            else
            {
                this.processDefinitionNameLike_ = processDefinitionNameLike;
            }
            return this;
        }

        public virtual IHistoricTaskInstanceQuery processCategoryIn(IList<string> processCategoryInList)
        {
            //if (processCategoryInList == null)
            //{
            //    throw new ActivitiIllegalArgumentException("Process category list is null");
            //}
            //if (processCategoryInList.Count == 0)
            //{
            //    throw new ActivitiIllegalArgumentException("Process category list is empty");
            //}
            //foreach (string processCategory in processCategoryInList)
            //{
            //    if (ReferenceEquals(processCategory, null))
            //    {
            //        throw new ActivitiIllegalArgumentException("None of the given process categories can be null");
            //    }
            //}

            var cates = processCategoryInList?.Where(x => x != null) ?? new List<string>();

            if (inOrStatement)
            {
                currentOrQueryObject.processCategoryInList = cates.Count() == 0 ? null : cates.ToList();
            }
            else
            {
                this.processCategoryInList = cates.Count() == 0 ? null : cates.ToList();
            }
            return this;
        }

        public virtual IHistoricTaskInstanceQuery processCategoryNotIn(IList<string> processCategoryNotInList)
        {
            //if (processCategoryNotInList == null)
            //{
            //    throw new ActivitiIllegalArgumentException("Process category list is null");
            //}
            //if (processCategoryNotInList.Count == 0)
            //{
            //    throw new ActivitiIllegalArgumentException("Process category list is empty");
            //}
            //foreach (string processCategory in processCategoryNotInList)
            //{
            //    if (ReferenceEquals(processCategory, null))
            //    {
            //        throw new ActivitiIllegalArgumentException("None of the given process categories can be null");
            //    }
            //}

            var cates = processCategoryNotInList?.Where(x => x != null) ?? new List<string>();

            if (inOrStatement)
            {
                currentOrQueryObject.processCategoryNotInList = cates.Count() == 0 ? null : cates.ToList();
            }
            else
            {
                this.processCategoryNotInList = cates.Count() == 0 ? null : cates.ToList();
            }
            return this;
        }

        public virtual IHistoricTaskInstanceQuery deploymentId(string deploymentId)
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
                this.currentOrQueryObject.taskId_ = taskId;
            }
            else
            {
                this.taskId_ = taskId;
            }
            return this;
        }

        public virtual IHistoricTaskInstanceQuery taskName(string taskName)
        {
            if (inOrStatement)
            {
                this.currentOrQueryObject.taskName_ = taskName;
            }
            else
            {
                this.taskName_ = taskName;
            }
            return this;
        }

        public virtual IHistoricTaskInstanceQuery taskNameIn(IList<string> taskNameList)
        {
            //if (taskNameList == null)
            //{
            //    throw new ActivitiIllegalArgumentException("Task name list is null");
            //}
            //if (taskNameList.Count == 0)
            //{
            //    throw new ActivitiIllegalArgumentException("Task name list is empty");
            //}

            var taskNames = taskNameList?.Where(x => x != null) ?? new List<string>();

            if (taskName_ != null)
            {
                throw new ActivitiIllegalArgumentException("Invalid query usage: cannot set both taskNameIn and taskName");
            }
            if (taskNameLike_ != null)
            {
                throw new ActivitiIllegalArgumentException("Invalid query usage: cannot set both taskNameIn and taskNameLike");
            }
            if (taskNameLikeIgnoreCase_ != null)
            {
                throw new ActivitiIllegalArgumentException("Invalid query usage: cannot set both taskNameIn and taskNameLikeIgnoreCase");
            }

            if (inOrStatement)
            {
                currentOrQueryObject.taskNameList = taskNames.Count() == 0 ? null : taskNames.ToList();
            }
            else
            {
                this.taskNameList = taskNames.Count() == 0 ? null : taskNames.ToList();
            }
            return this;
        }

        public virtual IHistoricTaskInstanceQuery taskNameInIgnoreCase(IList<string> taskNameList)
        {
            //if (taskNameList == null)
            //{
            //    throw new ActivitiIllegalArgumentException("Task name list is null");
            //}
            //if (taskNameList.Count == 0)
            //{
            //    throw new ActivitiIllegalArgumentException("Task name list is empty");
            //}
            //foreach (string taskName in taskNameList)
            //{
            //    if (ReferenceEquals(taskName, null))
            //    {
            //        throw new ActivitiIllegalArgumentException("None of the given task names can be null");
            //    }
            //}

            var taskNames = (taskNameList?.Where(x => x != null) ?? new List<string>())
                .Select(x => x.ToLower())
                .ToList();

            if (taskName_ != null)
            {
                throw new ActivitiIllegalArgumentException("Invalid query usage: cannot set both taskNameInIgnoreCase and name");
            }
            if (taskNameLike_ != null)
            {
                throw new ActivitiIllegalArgumentException("Invalid query usage: cannot set both taskNameInIgnoreCase and nameLike");
            }
            if (taskNameLikeIgnoreCase_ != null)
            {
                throw new ActivitiIllegalArgumentException("Invalid query usage: cannot set both taskNameInIgnoreCase and nameLikeIgnoreCase");
            }

            //int nameListSize = taskNames.Count();
            //IList<string> caseIgnoredTaskNameList = new List<string>(nameListSize);
            //foreach (string taskName in taskNameList)
            //{
            //    caseIgnoredTaskNameList.Add(taskName.ToLower());
            //}

            if (inOrStatement)
            {
                this.currentOrQueryObject.taskNameListIgnoreCase = taskNames.Count == 0 ? null : taskNames;
            }
            else
            {
                this.taskNameListIgnoreCase = taskNames.Count == 0 ? null : taskNames;
            }
            return this;
        }

        public virtual IHistoricTaskInstanceQuery taskNameLike(string taskNameLike)
        {
            if (inOrStatement)
            {
                this.currentOrQueryObject.taskNameLike_ = taskNameLike;
            }
            else
            {
                this.taskNameLike_ = taskNameLike;
            }
            return this;
        }

        public virtual IHistoricTaskInstanceQuery taskNameLikeIgnoreCase(string taskNameLikeIgnoreCase)
        {
            if (inOrStatement)
            {
                this.currentOrQueryObject.taskNameLikeIgnoreCase_ = taskNameLikeIgnoreCase?.ToLower();
            }
            else
            {
                this.taskNameLikeIgnoreCase_ = taskNameLikeIgnoreCase?.ToLower();
            }
            return this;
        }

        public virtual IHistoricTaskInstanceQuery taskParentTaskId(string parentTaskId)
        {
            if (inOrStatement)
            {
                this.currentOrQueryObject.taskParentTaskId_ = parentTaskId;
            }
            else
            {
                this.taskParentTaskId_ = parentTaskId;
            }
            return this;
        }

        public virtual IHistoricTaskInstanceQuery taskDescription(string taskDescription)
        {
            if (inOrStatement)
            {
                this.currentOrQueryObject.taskDescription_ = taskDescription;
            }
            else
            {
                this.taskDescription_ = taskDescription;
            }
            return this;
        }

        public virtual IHistoricTaskInstanceQuery taskDescriptionLike(string taskDescriptionLike)
        {
            if (inOrStatement)
            {
                this.currentOrQueryObject.taskDescriptionLike_ = taskDescriptionLike;
            }
            else
            {
                this.taskDescriptionLike_ = taskDescriptionLike;
            }
            return this;
        }

        public virtual IHistoricTaskInstanceQuery taskDescriptionLikeIgnoreCase(string taskDescriptionLikeIgnoreCase)
        {
            if (inOrStatement)
            {
                this.currentOrQueryObject.taskDescriptionLikeIgnoreCase_ = taskDescriptionLikeIgnoreCase?.ToLower();
            }
            else
            {
                this.taskDescriptionLikeIgnoreCase_ = taskDescriptionLikeIgnoreCase?.ToLower();
            }
            return this;
        }

        public virtual IHistoricTaskInstanceQuery taskDeleteReason(string taskDeleteReason)
        {
            if (inOrStatement)
            {
                this.currentOrQueryObject.taskDeleteReason_ = taskDeleteReason;
            }
            else
            {
                this.taskDeleteReason_ = taskDeleteReason;
            }
            return this;
        }

        public virtual IHistoricTaskInstanceQuery taskDeleteReasonLike(string taskDeleteReasonLike)
        {
            if (inOrStatement)
            {
                this.currentOrQueryObject.taskDeleteReasonLike_ = taskDeleteReasonLike;
            }
            else
            {
                this.taskDeleteReasonLike_ = taskDeleteReasonLike;
            }
            return this;
        }

        public virtual IHistoricTaskInstanceQuery taskAssignee(string taskAssignee)
        {
            if (inOrStatement)
            {
                this.currentOrQueryObject.taskAssignee_ = taskAssignee;
            }
            else
            {
                this.taskAssignee_ = taskAssignee;
            }
            return this;
        }

        public virtual IHistoricTaskInstanceQuery taskAssigneeLike(string taskAssigneeLike)
        {
            if (inOrStatement)
            {
                this.currentOrQueryObject.taskAssigneeLike_ = taskAssigneeLike;
            }
            else
            {
                this.taskAssigneeLike_ = taskAssigneeLike;
            }
            return this;
        }

        public virtual IHistoricTaskInstanceQuery taskAssigneeLikeIgnoreCase(string taskAssigneeLikeIgnoreCase)
        {
            if (inOrStatement)
            {
                this.currentOrQueryObject.taskAssigneeLikeIgnoreCase_ = taskAssigneeLikeIgnoreCase?.ToLower();
            }
            else
            {
                this.taskAssigneeLikeIgnoreCase_ = taskAssigneeLikeIgnoreCase?.ToLower();
            }
            return this;
        }

        public virtual IHistoricTaskInstanceQuery taskAssigneeIds(IList<string> assigneeIds)
        {
            //if (assigneeIds == null)
            //{
            //    throw new ActivitiIllegalArgumentException("Task assignee list is null");
            //}
            //if (assigneeIds.Count == 0)
            //{
            //    throw new ActivitiIllegalArgumentException("Task assignee list is empty");
            //}
            //foreach (string assignee in assigneeIds)
            //{
            //    if (ReferenceEquals(assignee, null))
            //    {
            //        throw new ActivitiIllegalArgumentException("None of the given task assignees can be null");
            //    }
            //}

            var ids = assigneeIds.Where(x => x != null) ?? new List<string>();

            if (taskAssignee_ != null && ids.Count() > 0)
            {
                throw new ActivitiIllegalArgumentException("Invalid query usage: cannot set both taskAssigneeIds and taskAssignee");
            }
            if (taskAssigneeLike_ != null && ids.Count() > 0)
            {
                throw new ActivitiIllegalArgumentException("Invalid query usage: cannot set both taskAssigneeIds and taskAssigneeLike");
            }
            if (taskAssigneeLikeIgnoreCase_ != null && ids.Count() > 0)
            {
                throw new ActivitiIllegalArgumentException("Invalid query usage: cannot set both taskAssigneeIds and taskAssigneeLikeIgnoreCase");
            }

            if (inOrStatement)
            {
                currentOrQueryObject.taskAssigneeIds_ = ids.Count() == 0 ? null : ids.ToList();
            }
            else
            {
                this.taskAssigneeIds_ = ids.Count() == 0 ? null : ids.ToList();
            }
            return this;
        }

        public virtual IHistoricTaskInstanceQuery taskOwner(string taskOwner)
        {
            if (inOrStatement)
            {
                this.currentOrQueryObject.taskOwner_ = taskOwner;
            }
            else
            {
                this.taskOwner_ = taskOwner;
            }
            return this;
        }

        public virtual IHistoricTaskInstanceQuery taskOwnerLike(string taskOwnerLike)
        {
            if (inOrStatement)
            {
                this.currentOrQueryObject.taskOwnerLike_ = taskOwnerLike;
            }
            else
            {
                this.taskOwnerLike_ = taskOwnerLike;
            }
            return this;
        }

        public virtual IHistoricTaskInstanceQuery taskOwnerLikeIgnoreCase(string taskOwnerLikeIgnoreCase)
        {
            if (inOrStatement)
            {
                this.currentOrQueryObject.taskOwnerLikeIgnoreCase_ = taskOwnerLikeIgnoreCase?.ToLower();
            }
            else
            {
                this.taskOwnerLikeIgnoreCase_ = taskOwnerLikeIgnoreCase?.ToLower();
            }
            return this;
        }

        public virtual IHistoricTaskInstanceQuery finished()
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

        public virtual IHistoricTaskInstanceQuery unfinished()
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
                this.currentOrQueryObject.taskDefinitionKey_ = taskDefinitionKey;
            }
            else
            {
                this.taskDefinitionKey_ = taskDefinitionKey;
            }
            return this;
        }

        public virtual IHistoricTaskInstanceQuery taskDefinitionKeyLike(string taskDefinitionKeyLike)
        {
            if (inOrStatement)
            {
                this.currentOrQueryObject.taskDefinitionKeyLike_ = taskDefinitionKeyLike;
            }
            else
            {
                this.taskDefinitionKeyLike_ = taskDefinitionKeyLike;
            }
            return this;
        }

        public virtual IHistoricTaskInstanceQuery taskPriority(int? taskPriority)
        {
            if (inOrStatement)
            {
                this.currentOrQueryObject.taskPriority_ = taskPriority;
            }
            else
            {
                this.taskPriority_ = taskPriority;
            }
            return this;
        }

        public virtual IHistoricTaskInstanceQuery taskMinPriority(int? taskMinPriority)
        {
            if (inOrStatement)
            {
                this.currentOrQueryObject.taskMinPriority_ = taskMinPriority;
            }
            else
            {
                this.taskMinPriority_ = taskMinPriority;
            }
            return this;
        }

        public virtual IHistoricTaskInstanceQuery taskMaxPriority(int? taskMaxPriority)
        {
            if (inOrStatement)
            {
                this.currentOrQueryObject.taskMaxPriority_ = taskMaxPriority;
            }
            else
            {
                this.taskMaxPriority_ = taskMaxPriority;
            }
            return this;
        }

        public virtual IHistoricTaskInstanceQuery processFinished()
        {
            if (inOrStatement)
            {
                this.currentOrQueryObject.processFinished_ = true;
            }
            else
            {
                this.processFinished_ = true;
            }
            return this;
        }

        public virtual IHistoricTaskInstanceQuery processUnfinished()
        {
            if (inOrStatement)
            {
                this.currentOrQueryObject.processUnfinished_ = true;
            }
            else
            {
                this.processUnfinished_ = true;
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

        public virtual IHistoricTaskInstanceQuery taskDueDate(DateTime? dueDate)
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

        public virtual IHistoricTaskInstanceQuery taskDueAfter(DateTime? dueAfter)
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

        public virtual IHistoricTaskInstanceQuery taskDueBefore(DateTime? dueBefore)
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

        public virtual IHistoricTaskInstanceQuery taskCreatedOn(DateTime? creationDate)
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

        public virtual IHistoricTaskInstanceQuery taskCreatedBefore(DateTime? creationBeforeDate)
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

        public virtual IHistoricTaskInstanceQuery taskCreatedAfter(DateTime? creationAfterDate)
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

        public virtual IHistoricTaskInstanceQuery taskCompletedOn(DateTime? completedDate)
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

        public virtual IHistoricTaskInstanceQuery taskCompletedBefore(DateTime? completedBeforeDate)
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

        public virtual IHistoricTaskInstanceQuery taskCompletedAfter(DateTime? completedAfterDate)
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
            //if (ReferenceEquals(candidateUser, null))
            //{
            //    throw new ActivitiIllegalArgumentException("Candidate user is null");
            //}

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
            //if (ReferenceEquals(candidateGroup, null))
            //{
            //    throw new ActivitiIllegalArgumentException("Candidate group is null");
            //}

            //if (candidateGroups != null)
            //{
            //    throw new ActivitiIllegalArgumentException("Invalid query usage: cannot set both candidateGroup and candidateGroupIn");
            //}

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
            //if (candidateGroups == null)
            //{
            //    throw new ActivitiIllegalArgumentException("Candidate group list is null");
            //}

            //if (candidateGroups.Count == 0)
            //{
            //    throw new ActivitiIllegalArgumentException("Candidate group list is empty");
            //}

            var groups = candidateGroups?.Where(x => x != null) ?? new List<string>();

            if (string.IsNullOrWhiteSpace(candidateGroup) == false && groups.Count() > 0)
            {
                throw new ActivitiIllegalArgumentException("Invalid query usage: cannot set both candidateGroupIn and candidateGroup");
            }

            if (inOrStatement)
            {
                this.currentOrQueryObject.candidateGroups = groups.Count() == 0 ? null : groups.ToList();
            }
            else
            {
                this.candidateGroups = groups.Count() == 0 ? null : groups.ToList();
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
            //if (involvedGroups == null || involvedGroups.Count == 0)
            //{
            //    throw new ActivitiIllegalArgumentException("Involved groups list is null or empty.");
            //}

            var groups = involvedGroups?.Where(x => x != null) ?? new List<string>();

            if (inOrStatement)
            {
                this.currentOrQueryObject.involvedGroups = groups.Count() == 0 ? null : groups.ToList();
            }
            else
            {
                this.involvedGroups = groups.Count() == 0 ? null : groups.ToList();
            }
            return this;
        }

        public virtual IHistoricTaskInstanceQuery taskTenantId(string tenantId)
        {
            //if (ReferenceEquals(tenantId, null))
            //{
            //    throw new ActivitiIllegalArgumentException("task tenant id is null");
            //}
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
            //if (ReferenceEquals(tenantIdLike, null))
            //{
            //    throw new ActivitiIllegalArgumentException("task tenant id is null");
            //}
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
            this.locale_ = locale;
            return this;
        }

        public virtual IHistoricTaskInstanceQuery withLocalizationFallback()
        {
            withLocalizationFallback_ = true;
            return this;
        }

        public virtual IHistoricTaskInstanceQuery includeTaskLocalVariables()
        {
            this.includeTaskLocalVariables_ = true;
            return this;
        }

        public virtual IHistoricTaskInstanceQuery includeProcessVariables()
        {
            this.includeProcessVariables_ = true;
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
            set => limitTaskVariables(value);
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

            if (!ReferenceEquals(locale_, null))
            {
                string processDefinitionId = task.ProcessDefinitionId;
                if (!string.IsNullOrWhiteSpace(processDefinitionId))
                {
                    JToken languageNode = Context.getLocalizationElementProperties(locale_, task.TaskDefinitionKey, processDefinitionId, withLocalizationFallback_.GetValueOrDefault());
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
            if (includeProcessVariables_.GetValueOrDefault() || includeTaskLocalVariables_.GetValueOrDefault())
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

        private string[] taskNotInIds;

        public IHistoricTaskInstanceQuery taskIdNotIn(string[] ids)
        {
            this.taskNotInIds = ids;
            return this;
        }

        public string[] TaskNotInIds
        {
            get => taskNotInIds;
            set => taskNotInIds = value;
        }

        // getters and setters
        // //////////////////////////////////////////////////////

        public virtual string ProcessInstanceId
        {
            get
            {
                return processInstanceId_;
            }
            set => processInstanceId(value);
        }

        public virtual IList<string> ProcessInstanceIds
        {
            get
            {
                return processInstanceIds;
            }
            set => processInstanceIdIn(value);
        }

        public virtual string ProcessInstanceBusinessKey
        {
            get
            {
                return processInstanceBusinessKey_;
            }
        }

        public virtual string ExecutionId
        {
            get
            {
                return executionId_;
            }
            set => executionId(value);
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

        public virtual string ProcessDefinitionKeyLike
        {
            get
            {
                return processDefinitionKeyLike_;
            }
            set => processDefinitionKeyLike(value);
        }
        public virtual IList<string> ProcessDefinitionKeys
        {
            get
            {
                return processDefinitionKeys;
            }
            set => processDefinitionKeyIn(value);
        }
        public virtual string ProcessDefinitionName
        {
            get
            {
                return processDefinitionName_;
            }
            set => processDefinitionName(value);
        }

        public virtual string ProcessDefinitionNameLike
        {
            get
            {
                return processDefinitionNameLike_;
            }
            set => processDefinitionNameLike(value);
        }

        public virtual IList<string> ProcessCategoryInList
        {
            get
            {
                return processCategoryInList;
            }
            set => processCategoryIn(value);
        }

        public virtual IList<string> ProcessCategoryNotInList
        {
            get
            {
                return processCategoryNotInList;
            }
            set => processCategoryNotIn(value);
        }

        public virtual string DeploymentId
        {
            get
            {
                return deploymentId_;
            }
            set => deploymentId(value);
        }

        public virtual IList<string> DeploymentIds
        {
            get
            {
                return deploymentIds;
            }
            set => deploymentIdIn(value);
        }

        public virtual string ProcessInstanceBusinessKeyLike
        {
            get
            {
                return processInstanceBusinessKeyLike_;
            }
            set => processInstanceBusinessKeyLike(value);
        }

        public virtual string TaskDefinitionKeyLike
        {
            get
            {
                return taskDefinitionKeyLike_;
            }
            set => taskDefinitionKeyLike(value);
        }

        public virtual int? TaskPriority
        {
            get
            {
                return taskPriority_;
            }
            set => taskPriority(value);
        }

        public virtual int? TaskMinPriority
        {
            get
            {
                return taskMinPriority_;
            }
            set => taskMinPriority(value);
        }

        public virtual int? TaskMaxPriority
        {
            get
            {
                return taskMaxPriority_;
            }
            set => taskMaxPriority(value);
        }

        public virtual bool? ProcessFinished
        {
            get
            {
                return processFinished_;
            }
            set
            {
                if (value.GetValueOrDefault())
                {
                    processFinished();
                }
                else
                {
                    processFinished_ = false;
                }
            }
        }

        public virtual bool? ProcessUnfinished
        {
            get
            {
                return processUnfinished_;
            }
            set
            {
                if (value.GetValueOrDefault())
                {
                    processUnfinished();
                }
                else
                {
                    processUnfinished_ = value;
                }
            }
        }

        public virtual DateTime? DueDate
        {
            get
            {
                return dueDate;
            }
            set => taskDueDate(value);
        }

        public virtual DateTime? DueAfter
        {
            get
            {
                return dueAfter;
            }
            set => taskDueAfter(value);
        }

        public virtual DateTime? DueBefore
        {
            get
            {
                return dueBefore;
            }
            set => taskDueBefore(value);
        }

        public virtual bool? WithoutDueDate
        {
            get
            {
                return withoutDueDate;
            }
            set
            {
                if (value.GetValueOrDefault())
                {
                    withoutTaskDueDate();
                }
                else
                {
                    withoutDueDate = false;
                }
            }
        }

        public virtual DateTime? CreationAfterDate
        {
            get
            {
                return creationAfterDate;
            }
            set => taskCreatedAfter(value);
        }

        public virtual DateTime? CreationBeforeDate
        {
            get
            {
                return creationBeforeDate;
            }
            set => taskCreatedBefore(value);
        }

        public virtual DateTime? CompletedDate
        {
            get
            {
                return completedDate;
            }
            set => taskCompletedOn(value);
        }

        public virtual DateTime? CompletedAfterDate
        {
            get
            {
                return completedAfterDate;
            }
            set => taskCompletedAfter(value);
        }

        public virtual DateTime? CompletedBeforeDate
        {
            get
            {
                return completedBeforeDate;
            }
            set => taskCompletedBefore(value);
        }

        public virtual string Category
        {
            get
            {
                return category;
            }
            set => taskCategory(value);
        }

        public virtual string TenantId
        {
            get
            {
                return tenantId;
            }
            set => taskTenantId(value);
        }

        public virtual string TenantIdLike
        {
            get
            {
                return tenantIdLike;
            }
            set => taskTenantIdLike(value);
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
                    taskWithoutTenantId();
                }
                else
                {
                    withoutTenantId = false;
                }
            }
        }

        public virtual bool? IncludeTaskLocalVariables
        {
            get
            {
                return includeTaskLocalVariables_;
            }
            set
            {
                if (value.GetValueOrDefault())
                {
                    includeProcessVariables();
                }
                else
                {
                    includeProcessVariables_ = false;
                }
            }
        }

        public virtual bool? IncludeProcessVariables
        {
            get
            {
                return includeProcessVariables_;
            }
            set
            {
                if (value.GetValueOrDefault())
                {
                    includeProcessVariables();
                }
                else
                {
                    includeProcessVariables_ = false;
                }
            }
        }

        public virtual bool InOrStatement
        {
            get
            {
                return inOrStatement;
            }
            set => inOrStatement = value;
        }

        public virtual bool? Finished
        {
            get
            {
                return finished_;
            }
            set
            {
                if (value.GetValueOrDefault())
                {
                    finished();
                }
                else
                {
                    finished_ = value;
                }
            }
        }

        public virtual bool? Unfinished
        {
            get
            {
                return unfinished_;
            }
            set
            {
                if (value.GetValueOrDefault())
                {
                    unfinished();
                }
                else
                {
                    unfinished_ = false;
                }
            }
        }

        public virtual string TaskName
        {
            get
            {
                return taskName_;
            }
            set => taskName(value);
        }

        public virtual string TaskNameLike
        {
            get
            {
                return taskNameLike_;
            }
            set => taskNameLike(value);
        }

        public virtual IList<string> TaskNameList
        {
            get
            {
                return taskNameList;
            }
            set => taskNameIn(taskNameList);
        }

        public virtual IList<string> TaskNameListIgnoreCase
        {
            get
            {
                return taskNameListIgnoreCase;
            }
            set => taskNameInIgnoreCase(value);
        }

        public virtual string TaskDescription
        {
            get
            {
                return taskDescription_;
            }
            set => taskDescription(value);
        }

        public virtual string TaskDescriptionLike
        {
            get
            {
                return taskDescriptionLike_;
            }
            set => taskDescriptionLike(value);
        }

        public virtual string TaskDeleteReason
        {
            get
            {
                return taskDeleteReason_;
            }
            set => taskDeleteReason(value);
        }

        public virtual string TaskDeleteReasonLike
        {
            get
            {
                return taskDeleteReasonLike_;
            }
            set => taskDeleteReasonLike(value);
        }

        public virtual IList<string> TaskAssigneeIds
        {
            get
            {
                return taskAssigneeIds_;
            }
            set => taskAssigneeIds(value);
        }

        public virtual string TaskAssignee
        {
            get
            {
                return taskAssignee_;
            }
            set => taskAssignee(value);
        }

        public virtual string TaskAssigneeLike
        {
            get
            {
                return taskAssigneeLike_;
            }
            set => taskAssigneeLike(value);
        }

        public virtual string TaskId
        {
            get
            {
                return taskId_;
            }
            set => taskId(value);
        }

        public virtual string TaskDefinitionKey
        {
            get
            {
                return taskDefinitionKey_;
            }
            set => taskDefinitionKey(value);
        }

        public virtual string TaskOwnerLike
        {
            get
            {
                return taskOwnerLike_;
            }
            set => taskOwnerLike(value);
        }

        public virtual string TaskOwner
        {
            get
            {
                return taskOwner_;
            }
            set => taskOwner(value);
        }

        public virtual string TaskParentTaskId
        {
            get
            {
                return taskParentTaskId_;
            }
            set => taskParentTaskId(value);
        }

        public virtual DateTime? CreationDate
        {
            get
            {
                return creationDate;
            }
            set => taskCreatedOn(value);
        }

        public virtual string CandidateUser
        {
            get
            {
                return candidateUser;
            }
            set => taskCandidateUser(value);
        }

        public virtual string CandidateGroup
        {
            get
            {
                return candidateGroup;
            }
            set => taskCandidateGroup(value);
        }

        /// <summary>
        /// 参与用户
        /// </summary>
        public virtual string InvolvedUser
        {
            get
            {
                return involvedUser;
            }
            set => taskInvolvedUser(value);
        }

        /// <summary>
        /// 参与用户组
        /// </summary>
        public virtual IList<string> InvolvedGroups
        {
            get
            {
                return involvedGroups;
            }
            set => taskInvolvedGroupsIn(value);
        }

        public virtual string ProcessDefinitionKeyLikeIgnoreCase
        {
            get
            {
                return processDefinitionKeyLikeIgnoreCase_;
            }
            set => processDefinitionKeyLikeIgnoreCase(value);
        }

        public virtual string ProcessInstanceBusinessKeyLikeIgnoreCase
        {
            get
            {
                return processInstanceBusinessKeyLikeIgnoreCase_;
            }
            set => processInstanceBusinessKeyLikeIgnoreCase(value);
        }

        public virtual string TaskNameLikeIgnoreCase
        {
            get
            {
                return taskNameLikeIgnoreCase_;
            }
            set => taskNameLikeIgnoreCase(value);
        }

        public virtual string TaskDescriptionLikeIgnoreCase
        {
            get
            {
                return taskDescriptionLikeIgnoreCase_;
            }
            set => taskDescriptionLikeIgnoreCase(value);
        }

        public virtual string TaskOwnerLikeIgnoreCase
        {
            get
            {
                return taskOwnerLikeIgnoreCase_;
            }
            set => taskOwnerLikeIgnoreCase(value);
        }

        public virtual string TaskAssigneeLikeIgnoreCase
        {
            get
            {
                return taskAssigneeLikeIgnoreCase_;
            }
            set => taskAssigneeLikeIgnoreCase(value);
        }

        public virtual string Locale
        {
            get
            {
                return locale_;
            }
            set => locale(value);
        }

        public virtual IList<HistoricTaskInstanceQueryImpl> OrQueryObjects
        {
            get
            {
                return orQueryObjects;
            }
            set => orQueryObjects = value;
        }
    }

}