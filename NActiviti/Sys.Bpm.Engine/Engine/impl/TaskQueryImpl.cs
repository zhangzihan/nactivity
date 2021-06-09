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
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json.Linq;
    using Sys.Workflow.Engine.Impl.Contexts;
    using Sys.Workflow.Engine.Impl.Interceptor;
    using Sys.Workflow.Engine.Impl.Persistence.Entity;
    using Sys.Workflow.Engine.Impl.Variable;
    using Sys.Workflow.Engine.Tasks;
    using Sys.Workflow;
    using System.Linq;

    /// 
    /// 
    /// 
    /// 
    public class TaskQueryImpl : AbstractVariableQueryImpl<ITaskQuery, ITask>, ITaskQuery
    {
        private static readonly ILogger log = ProcessEngineServiceProvider.LoggerService<TaskQueryImpl>();

        private const long serialVersionUID = 1L;
        protected internal string _taskId;
        protected internal string name;
        protected internal string nameLike;
        protected internal string nameLikeIgnoreCase;
        protected internal IList<string> nameList;
        protected internal IList<string> nameListIgnoreCase;
        protected internal string description;
        protected internal string descriptionLike;
        protected internal string descriptionLikeIgnoreCase;
        protected internal int? priority;
        protected internal int? minPriority;
        protected internal int? maxPriority;
        protected internal string assignee;
        protected internal string assigneeLike;
        protected internal string assigneeLikeIgnoreCase;
        protected internal IList<string> assigneeIds;
        protected internal string involvedUser;
        protected internal IList<string> involvedGroups;
        protected internal string owner;
        protected internal string ownerLike;
        protected internal string ownerLikeIgnoreCase;
        protected internal bool unassigned;
        protected internal bool noDelegationState;
        protected internal DelegationState? delegationState;
        protected internal string candidateUser;
        protected internal string candidateGroup;
        protected internal IList<string> candidateGroups;
        protected internal string tenantId;
        protected internal string tenantIdLike;
        protected internal bool withoutTenantId;

        protected internal string _processInstanceId;
        protected internal IList<string> processInstanceIds;

        protected internal string _executionId;
        protected internal IList<string> executionIds;
        protected internal DateTime? createTime;
        protected internal DateTime? createTimeBefore;
        protected internal DateTime? createTimeAfter;
        protected internal string category;
        protected internal string key;
        protected internal string keyLike;

        protected internal string _processDefinitionKey;

        protected internal string _processDefinitionKeyLike;

        protected internal string _processDefinitionKeyLikeIgnoreCase;
        protected internal IList<string> processDefinitionKeys;

        protected internal string _processDefinitionId;

        protected internal string _processDefinitionName;

        protected internal string _processDefinitionNameLike;
        protected internal IList<string> processCategoryInList;
        protected internal IList<string> processCategoryNotInList;

        protected internal string _deploymentId;
        protected internal IList<string> deploymentIds;

        protected internal string _processInstanceBusinessKey;

        protected internal string _processInstanceBusinessKeyLike;

        protected internal string _processInstanceBusinessKeyLikeIgnoreCase;

        protected internal DateTime? _dueDate;

        protected internal DateTime? _dueBefore;

        protected internal DateTime? _dueAfter;

        protected internal bool _withoutDueDate;
        protected internal ISuspensionState suspensionState;

        protected internal bool _excludeSubtasks;

        protected internal bool _includeTaskLocalVariables;

        protected internal bool _includeProcessVariables;
        protected internal int? taskVariablesLimit;
        protected internal string userIdForCandidateAndAssignee;
        protected internal bool bothCandidateAndAssigned;

        protected internal string _locale;

        protected internal bool _withLocalizationFallback;
        protected internal bool orActive;
        protected internal IList<TaskQueryImpl> orQueryObjects = new List<TaskQueryImpl>();
        protected internal TaskQueryImpl currentOrQueryObject = null;

        public TaskQueryImpl()
        {
        }

        public TaskQueryImpl(ICommandContext commandContext) : base(commandContext)
        {
        }

        public TaskQueryImpl(ICommandExecutor commandExecutor) : base(commandExecutor)
        {
        }

        public TaskQueryImpl(ICommandExecutor commandExecutor, string databaseType) : base(commandExecutor)
        {
            this.databaseType = databaseType;
        }

        public virtual ITaskQuery SetTaskId(string taskId)
        {
            if (orActive)
            {
                currentOrQueryObject._taskId = taskId;
            }
            else
            {
                this._taskId = taskId;
            }
            return this;
        }

        public virtual ITaskQuery SetTaskName(string name)
        {
            if (orActive)
            {
                currentOrQueryObject.name = name;
            }
            else
            {
                this.name = name;
            }
            return this;
        }

        public virtual ITaskQuery SetTaskNameIn(IList<string> nameList)
        {
            if (nameList is null)
            {
                throw new ActivitiIllegalArgumentException("Task name list is null");
            }
            if (nameList.Count == 0)
            {
                throw new ActivitiIllegalArgumentException("Task name list is empty");
            }
            foreach (string name in nameList)
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    throw new ActivitiIllegalArgumentException("None of the given task names can be null");
                }
            }

            if (!string.IsNullOrWhiteSpace(name))
            {
                throw new ActivitiIllegalArgumentException("Invalid query usage: cannot set both taskNameIn and name");
            }
            if (!string.IsNullOrWhiteSpace(nameLike))
            {
                throw new ActivitiIllegalArgumentException("Invalid query usage: cannot set both taskNameIn and nameLike");
            }
            if (!string.IsNullOrWhiteSpace(nameLikeIgnoreCase))
            {
                throw new ActivitiIllegalArgumentException("Invalid query usage: cannot set both taskNameIn and nameLikeIgnoreCase");
            }

            if (orActive)
            {
                currentOrQueryObject.nameList = nameList;
            }
            else
            {
                this.nameList = nameList;
            }
            return this;
        }

        public virtual ITaskQuery SetTaskNameInIgnoreCase(IList<string> nameList)
        {
            if (nameList is null)
            {
                throw new ActivitiIllegalArgumentException("Task name list is null");
            }
            if (nameList.Count == 0)
            {
                throw new ActivitiIllegalArgumentException("Task name list is empty");
            }
            foreach (string name in nameList)
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    throw new ActivitiIllegalArgumentException("None of the given task names can be null");
                }
            }

            if (name is object)
            {
                throw new ActivitiIllegalArgumentException("Invalid query usage: cannot set both taskNameInIgnoreCase and name");
            }
            if (nameLike is object)
            {
                throw new ActivitiIllegalArgumentException("Invalid query usage: cannot set both taskNameInIgnoreCase and nameLike");
            }
            if (nameLikeIgnoreCase is object)
            {
                throw new ActivitiIllegalArgumentException("Invalid query usage: cannot set both taskNameInIgnoreCase and nameLikeIgnoreCase");
            }


            int nameListSize = nameList.Count;

            IList<string> caseIgnoredNameList = new List<string>(nameListSize);
            foreach (string name in nameList)
            {
                caseIgnoredNameList.Add(name.ToLower());
            }

            if (orActive)
            {
                this.currentOrQueryObject.nameListIgnoreCase = caseIgnoredNameList;
            }
            else
            {
                this.nameListIgnoreCase = caseIgnoredNameList;
            }
            return this;
        }

        public virtual ITaskQuery SetTaskNameLike(string nameLike)
        {
            if (orActive)
            {
                currentOrQueryObject.nameLike = nameLike;
            }
            else
            {
                this.nameLike = nameLike;
            }
            return this;
        }

        public virtual ITaskQuery SetTaskNameLikeIgnoreCase(string nameLikeIgnoreCase)
        {
            if (orActive)
            {
                currentOrQueryObject.nameLikeIgnoreCase = nameLikeIgnoreCase.ToLower();
            }
            else
            {
                this.nameLikeIgnoreCase = nameLikeIgnoreCase.ToLower();
            }
            return this;
        }

        public virtual ITaskQuery SetTaskDescription(string description)
        {
            if (orActive)
            {
                currentOrQueryObject.description = description;
            }
            else
            {
                this.description = description;
            }
            return this;
        }

        public virtual ITaskQuery SetTaskDescriptionLike(string descriptionLike)
        {
            if (orActive)
            {
                currentOrQueryObject.descriptionLike = descriptionLike;
            }
            else
            {
                this.descriptionLike = descriptionLike;
            }
            return this;
        }

        public virtual ITaskQuery SetIsTaskBusinessKey(bool isTaskBusinessKey)
        {
            IsTaskBusinessKey = isTaskBusinessKey;

            return this;
        }

        public virtual ITaskQuery SetTaskDescriptionLikeIgnoreCase(string descriptionLikeIgnoreCase)
        {
            if (orActive)
            {
                currentOrQueryObject.descriptionLikeIgnoreCase = descriptionLikeIgnoreCase.ToLower();
            }
            else
            {
                this.descriptionLikeIgnoreCase = descriptionLikeIgnoreCase.ToLower();
            }
            return this;
        }

        public virtual ITaskQuery SetTaskPriority(int? priority)
        {
            if (orActive)
            {
                currentOrQueryObject.priority = priority;
            }
            else
            {
                this.priority = priority;
            }
            return this;
        }

        public virtual ITaskQuery SetTaskMinPriority(int? minPriority)
        {
            if (orActive)
            {
                currentOrQueryObject.minPriority = minPriority;
            }
            else
            {
                this.minPriority = minPriority;
            }
            return this;
        }

        public virtual ITaskQuery SetTaskMaxPriority(int? maxPriority)
        {
            if (orActive)
            {
                currentOrQueryObject.maxPriority = maxPriority;
            }
            else
            {
                this.maxPriority = maxPriority;
            }
            return this;
        }

        public virtual ITaskQuery SetTaskAssignee(string assignee)
        {
            if (orActive)
            {
                currentOrQueryObject.assignee = assignee;
            }
            else
            {
                this.assignee = assignee;
            }
            return this;
        }

        public virtual ITaskQuery SetTaskAssigneeLike(string assigneeLike)
        {
            if (orActive)
            {
                currentOrQueryObject.assigneeLike = assignee;
            }
            else
            {
                this.assigneeLike = assigneeLike;
            }
            return this;
        }

        public virtual ITaskQuery SetTaskAssigneeLikeIgnoreCase(string assigneeLikeIgnoreCase)
        {
            if (orActive)
            {
                currentOrQueryObject.assigneeLikeIgnoreCase = assigneeLikeIgnoreCase.ToLower();
            }
            else
            {
                this.assigneeLikeIgnoreCase = assigneeLikeIgnoreCase.ToLower();
            }
            return this;
        }

        public virtual ITaskQuery SetTaskAssigneeIds(IList<string> assigneeIds)
        {
            if (assigneeIds is null)
            {
                throw new ActivitiIllegalArgumentException("Task assignee list is null");
            }
            if (assigneeIds.Count == 0)
            {
                throw new ActivitiIllegalArgumentException("Task assignee list is empty");
            }
            foreach (string assignee in assigneeIds)
            {
                if (assignee is null)
                {
                    throw new ActivitiIllegalArgumentException("None of the given task assignees can be null");
                }
            }

            if (assignee is object)
            {
                throw new ActivitiIllegalArgumentException("Invalid query usage: cannot set both taskAssigneeIds and taskAssignee");
            }
            if (assigneeLike is object)
            {
                throw new ActivitiIllegalArgumentException("Invalid query usage: cannot set both taskAssigneeIds and taskAssigneeLike");
            }
            if (assigneeLikeIgnoreCase is object)
            {
                throw new ActivitiIllegalArgumentException("Invalid query usage: cannot set both taskAssigneeIds and taskAssigneeLikeIgnoreCase");
            }

            if (orActive)
            {
                currentOrQueryObject.assigneeIds = assigneeIds;
            }
            else
            {
                this.assigneeIds = assigneeIds;
            }
            return this;
        }

        public virtual ITaskQuery SetTaskOwner(string owner)
        {
            if (orActive)
            {
                currentOrQueryObject.owner = owner;
            }
            else
            {
                this.owner = owner;
            }
            return this;
        }

        public virtual ITaskQuery SetTaskOwnerLike(string ownerLike)
        {
            if (orActive)
            {
                currentOrQueryObject.ownerLike = ownerLike;
            }
            else
            {
                this.ownerLike = ownerLike;
            }
            return this;
        }

        public virtual ITaskQuery SetTaskOwnerLikeIgnoreCase(string ownerLikeIgnoreCase)
        {
            if (orActive)
            {
                currentOrQueryObject.ownerLikeIgnoreCase = ownerLikeIgnoreCase.ToLower();
            }
            else
            {
                this.ownerLikeIgnoreCase = ownerLikeIgnoreCase.ToLower();
            }
            return this;
        }

        public virtual ITaskQuery SetTaskUnassigned()
        {
            if (orActive)
            {
                currentOrQueryObject.unassigned = true;
            }
            else
            {
                this.unassigned = true;
            }
            return this;
        }

        public virtual ITaskQuery SetTaskDelegationState(DelegationState? delegationState)
        {
            if (orActive)
            {
                if (!delegationState.HasValue)
                {
                    currentOrQueryObject.noDelegationState = true;
                }
                else
                {
                    currentOrQueryObject.delegationState = delegationState.Value;
                }
            }
            else
            {
                if (!delegationState.HasValue)
                {
                    this.noDelegationState = true;
                }
                else
                {
                    this.delegationState = delegationState.Value;
                }
            }
            return this;
        }

        public virtual ITaskQuery SetTaskCandidateUser(string candidateUser)
        {
            if (orActive)
            {
                currentOrQueryObject.candidateUser = candidateUser;
            }
            else
            {
                this.candidateUser = candidateUser;
            }

            return this;
        }

        public virtual ITaskQuery SetTaskCandidateUser(string candidateUser, IList<string> usersGroups)
        {
            if (orActive)
            {
                currentOrQueryObject.candidateUser = candidateUser;
                currentOrQueryObject.candidateGroups = usersGroups;
            }
            else
            {
                this.candidateUser = candidateUser;
                this.candidateGroups = usersGroups;
            }

            return this;
        }

        public virtual ITaskQuery SetTaskInvolvedUser(string involvedUser)
        {
            if (orActive)
            {
                currentOrQueryObject.involvedUser = involvedUser;
            }
            else
            {
                this.involvedUser = involvedUser;
            }
            return this;
        }

        public virtual ITaskQuery SetTaskInvolvedGroupsIn(IList<string> involvedGroups)
        {
            if (involvedGroups is null || involvedGroups.Count == 0)
            {
                throw new ActivitiIllegalArgumentException("Involved groups list is null or empty.");
            }

            if (orActive)
            {
                currentOrQueryObject.involvedGroups = involvedGroups;
            }
            else
            {
                this.involvedGroups = involvedGroups;
            }

            return this;
        }

        public virtual ITaskQuery SetTaskCandidateGroup(string candidateGroup)
        {
            if (candidateGroup is null)
            {
                throw new ActivitiIllegalArgumentException("Candidate group is null");
            }

            if (candidateGroups is object)
            {
                throw new ActivitiIllegalArgumentException("Invalid query usage: cannot set both candidateGroup and candidateGroupIn");
            }

            if (orActive)
            {
                currentOrQueryObject.candidateGroup = candidateGroup;
            }
            else
            {
                this.candidateGroup = candidateGroup;
            }
            return this;
        }

        public virtual ITaskQuery SetTaskCandidateOrAssigned(string userIdForCandidateAndAssignee)
        {
            if (candidateGroup is object)
            {
                throw new ActivitiIllegalArgumentException("Invalid query usage: cannot set candidateGroup");
            }
            if (candidateUser is object)
            {
                throw new ActivitiIllegalArgumentException("Invalid query usage: cannot set both candidateGroup and candidateUser");
            }

            if (orActive)
            {
                currentOrQueryObject.bothCandidateAndAssigned = true;
                currentOrQueryObject.userIdForCandidateAndAssignee = userIdForCandidateAndAssignee;
            }
            else
            {
                this.bothCandidateAndAssigned = true;
                this.userIdForCandidateAndAssignee = userIdForCandidateAndAssignee;
            }

            return this;
        }


        public virtual ITaskQuery SetTaskCandidateOrAssigned(string userIdForCandidateAndAssignee, IList<string> usersGroups)
        {
            if (candidateGroup is object)
            {
                throw new ActivitiIllegalArgumentException("Invalid query usage: cannot set candidateGroup");
            }
            if (candidateUser is object)
            {
                throw new ActivitiIllegalArgumentException("Invalid query usage: cannot set both candidateGroup and candidateUser");
            }

            if (orActive)
            {
                currentOrQueryObject.bothCandidateAndAssigned = true;
                currentOrQueryObject.userIdForCandidateAndAssignee = userIdForCandidateAndAssignee;
                currentOrQueryObject.candidateGroups = usersGroups;
            }
            else
            {
                this.bothCandidateAndAssigned = true;
                this.userIdForCandidateAndAssignee = userIdForCandidateAndAssignee;
                this.candidateGroups = usersGroups;
            }

            return this;
        }

        public virtual ITaskQuery SetTaskCandidateGroupIn(IList<string> candidateGroups)
        {
            if (candidateGroups is null)
            {
                throw new ActivitiIllegalArgumentException("Candidate group list is null");
            }

            if (candidateGroups.Count == 0)
            {
                throw new ActivitiIllegalArgumentException("Candidate group list is empty");
            }

            if (candidateGroup is object)
            {
                throw new ActivitiIllegalArgumentException("Invalid query usage: cannot set both candidateGroupIn and candidateGroup");
            }

            if (orActive)
            {
                currentOrQueryObject.candidateGroups = candidateGroups;
            }
            else
            {
                this.candidateGroups = candidateGroups;
            }
            return this;
        }

        public virtual ITaskQuery SetTaskTenantId(string tenantId)
        {
            if (tenantId is null)
            {
                throw new ActivitiIllegalArgumentException("task tenant id is null");
            }
            if (orActive)
            {
                currentOrQueryObject.tenantId = tenantId;
            }
            else
            {
                this.tenantId = tenantId;
            }
            return this;
        }

        public virtual ITaskQuery SetTaskTenantIdLike(string tenantIdLike)
        {
            if (tenantIdLike is null)
            {
                throw new ActivitiIllegalArgumentException("task tenant id is null");
            }
            if (orActive)
            {
                currentOrQueryObject.tenantIdLike = tenantIdLike;
            }
            else
            {
                this.tenantIdLike = tenantIdLike;
            }
            return this;
        }

        public virtual ITaskQuery TaskWithoutTenantId()
        {
            if (orActive)
            {
                currentOrQueryObject.withoutTenantId = true;
            }
            else
            {
                this.withoutTenantId = true;
            }
            return this;
        }

        public virtual ITaskQuery SetProcessInstanceId(string processInstanceId)
        {
            if (orActive)
            {
                currentOrQueryObject._processInstanceId = processInstanceId;
            }
            else
            {
                this._processInstanceId = processInstanceId;
            }
            return this;
        }

        public virtual ITaskQuery SetProcessInstanceIdIn(string[] processInstanceIds)
        {
            var ids = processInstanceIds is null ? new List<string>() : processInstanceIds.Where(x => x is object).ToList();

            if (orActive)
            {
                currentOrQueryObject.processInstanceIds = ids.Count == 0 ? null : ids;
            }
            else
            {
                this.processInstanceIds = ids.Count == 0 ? null : ids;
            }
            return this;
        }

        public virtual ITaskQuery SetExecutionIdIn(string[] executionIds)
        {
            var ids = executionIds is null ? new List<string>() : executionIds.Where(x => x is object).ToList();

            if (orActive)
            {
                currentOrQueryObject.executionIds = ids.Count == 0 ? null : ids;
            }
            else
            {
                this.executionIds = ids.Count == 0 ? null : ids;
            }
            return this;
        }

        public virtual ITaskQuery SetProcessInstanceBusinessKey(string processInstanceBusinessKey)
        {
            if (orActive)
            {
                currentOrQueryObject._processInstanceBusinessKey = processInstanceBusinessKey;
            }
            else
            {
                this._processInstanceBusinessKey = processInstanceBusinessKey;
            }
            return this;
        }

        public virtual ITaskQuery SetProcessInstanceBusinessKeyLike(string processInstanceBusinessKeyLike)
        {
            if (orActive)
            {
                currentOrQueryObject._processInstanceBusinessKeyLike = processInstanceBusinessKeyLike;
            }
            else
            {
                this._processInstanceBusinessKeyLike = processInstanceBusinessKeyLike;
            }
            return this;
        }

        public virtual ITaskQuery SetProcessInstanceBusinessKeyLikeIgnoreCase(string processInstanceBusinessKeyLikeIgnoreCase)
        {
            if (orActive)
            {
                currentOrQueryObject._processInstanceBusinessKeyLikeIgnoreCase = processInstanceBusinessKeyLikeIgnoreCase.ToLower();
            }
            else
            {
                this._processInstanceBusinessKeyLikeIgnoreCase = processInstanceBusinessKeyLikeIgnoreCase.ToLower();
            }
            return this;
        }

        public virtual ITaskQuery SetExecutionId(string executionId)
        {
            if (orActive)
            {
                currentOrQueryObject._executionId = executionId;
            }
            else
            {
                this._executionId = executionId;
            }
            return this;
        }

        public virtual ITaskQuery SetTaskCreatedOn(DateTime? createTime)
        {
            if (orActive)
            {
                currentOrQueryObject.createTime = createTime;
            }
            else
            {
                this.createTime = createTime;
            }
            return this;
        }

        public virtual ITaskQuery SetTaskCreatedBefore(DateTime? before)
        {
            if (orActive)
            {
                currentOrQueryObject.createTimeBefore = before;
            }
            else
            {
                this.createTimeBefore = before;
            }
            return this;
        }

        public virtual ITaskQuery SetTaskCreatedAfter(DateTime? after)
        {
            if (orActive)
            {
                currentOrQueryObject.createTimeAfter = after;
            }
            else
            {
                this.createTimeAfter = after;
            }
            return this;
        }

        public virtual ITaskQuery SetTaskCategory(string category)
        {
            if (orActive)
            {
                currentOrQueryObject.category = category;
            }
            else
            {
                this.category = category;
            }
            return this;
        }

        public virtual ITaskQuery SetTaskDefinitionKey(string key)
        {
            if (orActive)
            {
                currentOrQueryObject.key = key;
            }
            else
            {
                this.key = key;
            }
            return this;
        }

        public virtual ITaskQuery SetTaskDefinitionKeyLike(string keyLike)
        {
            if (orActive)
            {
                currentOrQueryObject.keyLike = keyLike;
            }
            else
            {
                this.keyLike = keyLike;
            }
            return this;
        }

        public virtual ITaskQuery SetTaskVariableValueEquals(string variableName, object variableValue)
        {
            if (orActive)
            {
                currentOrQueryObject.VariableValueEquals(variableName, variableValue);
            }
            else
            {
                this.VariableValueEquals(variableName, variableValue);
            }
            return this;
        }

        public virtual ITaskQuery SetTaskVariableValueEquals(object variableValue)
        {
            if (orActive)
            {
                currentOrQueryObject.VariableValueEquals(variableValue);
            }
            else
            {
                this.VariableValueEquals(variableValue);
            }
            return this;
        }

        public virtual ITaskQuery SetTaskVariableValueEqualsIgnoreCase(string name, string value)
        {
            if (orActive)
            {
                currentOrQueryObject.VariableValueEqualsIgnoreCase(name, value);
            }
            else
            {
                this.VariableValueEqualsIgnoreCase(name, value);
            }
            return this;
        }

        public virtual ITaskQuery SetTaskVariableValueNotEqualsIgnoreCase(string name, string value)
        {
            if (orActive)
            {
                currentOrQueryObject.VariableValueNotEqualsIgnoreCase(name, value);
            }
            else
            {
                this.VariableValueNotEqualsIgnoreCase(name, value);
            }
            return this;
        }

        public virtual ITaskQuery SetTaskVariableValueNotEquals(string variableName, object variableValue)
        {
            if (orActive)
            {
                currentOrQueryObject.VariableValueNotEquals(variableName, variableValue);
            }
            else
            {
                this.VariableValueNotEquals(variableName, variableValue);
            }
            return this;
        }

        public virtual ITaskQuery SetTaskVariableValueGreaterThan(string name, object value)
        {
            if (orActive)
            {
                currentOrQueryObject.VariableValueGreaterThan(name, value);
            }
            else
            {
                this.VariableValueGreaterThan(name, value);
            }
            return this;
        }

        public virtual ITaskQuery SetTaskVariableValueGreaterThanOrEqual(string name, object value)
        {
            if (orActive)
            {
                currentOrQueryObject.VariableValueGreaterThanOrEqual(name, value);
            }
            else
            {
                this.VariableValueGreaterThanOrEqual(name, value);
            }
            return this;
        }

        public virtual ITaskQuery SetTaskVariableValueLessThan(string name, object value)
        {
            if (orActive)
            {
                currentOrQueryObject.VariableValueLessThan(name, value);
            }
            else
            {
                this.VariableValueLessThan(name, value);
            }
            return this;
        }

        public virtual ITaskQuery SetTaskVariableValueLessThanOrEqual(string name, object value)
        {
            if (orActive)
            {
                currentOrQueryObject.VariableValueLessThanOrEqual(name, value);
            }
            else
            {
                this.VariableValueLessThanOrEqual(name, value);
            }
            return this;
        }

        public virtual ITaskQuery SetTaskVariableValueLike(string name, string value)
        {
            if (orActive)
            {
                currentOrQueryObject.VariableValueLike(name, value);
            }
            else
            {
                this.VariableValueLike(name, value);
            }
            return this;
        }

        public virtual ITaskQuery SetTaskVariableValueLikeIgnoreCase(string name, string value)
        {
            if (orActive)
            {
                currentOrQueryObject.VariableValueLikeIgnoreCase(name, value);
            }
            else
            {
                this.VariableValueLikeIgnoreCase(name, value);
            }
            return this;
        }

        public virtual ITaskQuery SetProcessVariableValueEquals(string variableName, object variableValue)
        {
            if (orActive)
            {
                currentOrQueryObject.VariableValueEquals(variableName, variableValue, false);
            }
            else
            {
                this.VariableValueEquals(variableName, variableValue, false);
            }
            return this;
        }

        public virtual ITaskQuery SetProcessVariableValueNotEquals(string variableName, object variableValue)
        {
            if (orActive)
            {
                currentOrQueryObject.VariableValueNotEquals(variableName, variableValue, false);
            }
            else
            {
                this.VariableValueNotEquals(variableName, variableValue, false);
            }
            return this;
        }

        public virtual ITaskQuery SetProcessVariableValueEquals(object variableValue)
        {
            if (orActive)
            {
                currentOrQueryObject.VariableValueEquals(variableValue, false);
            }
            else
            {
                this.VariableValueEquals(variableValue, false);
            }
            return this;
        }

        public virtual ITaskQuery SetProcessVariableValueEqualsIgnoreCase(string name, string value)
        {
            if (orActive)
            {
                currentOrQueryObject.VariableValueEqualsIgnoreCase(name, value, false);
            }
            else
            {
                this.VariableValueEqualsIgnoreCase(name, value, false);
            }
            return this;
        }

        public virtual ITaskQuery SetProcessVariableValueNotEqualsIgnoreCase(string name, string value)
        {
            if (orActive)
            {
                currentOrQueryObject.VariableValueNotEqualsIgnoreCase(name, value, false);
            }
            else
            {
                this.VariableValueNotEqualsIgnoreCase(name, value, false);
            }
            return this;
        }

        public virtual ITaskQuery SetProcessVariableValueGreaterThan(string name, object value)
        {
            if (orActive)
            {
                currentOrQueryObject.VariableValueGreaterThan(name, value, false);
            }
            else
            {
                this.VariableValueGreaterThan(name, value, false);
            }
            return this;
        }

        public virtual ITaskQuery SetProcessVariableValueGreaterThanOrEqual(string name, object value)
        {
            if (orActive)
            {
                currentOrQueryObject.VariableValueGreaterThanOrEqual(name, value, false);
            }
            else
            {
                this.VariableValueGreaterThanOrEqual(name, value, false);
            }
            return this;
        }

        public virtual ITaskQuery SetProcessVariableValueLessThan(string name, object value)
        {
            if (orActive)
            {
                currentOrQueryObject.VariableValueLessThan(name, value, false);
            }
            else
            {
                this.VariableValueLessThan(name, value, false);
            }
            return this;
        }

        public virtual ITaskQuery SetProcessVariableValueLessThanOrEqual(string name, object value)
        {
            if (orActive)
            {
                currentOrQueryObject.VariableValueLessThanOrEqual(name, value, false);
            }
            else
            {
                this.VariableValueLessThanOrEqual(name, value, false);
            }
            return this;
        }

        public virtual ITaskQuery SetProcessVariableValueLike(string name, string value)
        {
            if (orActive)
            {
                currentOrQueryObject.VariableValueLike(name, value, false);
            }
            else
            {
                this.VariableValueLike(name, value, false);
            }
            return this;
        }

        public virtual ITaskQuery SetProcessVariableValueLikeIgnoreCase(string name, string value)
        {
            if (orActive)
            {
                currentOrQueryObject.VariableValueLikeIgnoreCase(name, value, false);
            }
            else
            {
                this.VariableValueLikeIgnoreCase(name, value, false);
            }
            return this;
        }

        public virtual ITaskQuery SetProcessDefinitionKey(string processDefinitionKey)
        {
            if (orActive)
            {
                currentOrQueryObject._processDefinitionKey = processDefinitionKey;
            }
            else
            {
                this._processDefinitionKey = processDefinitionKey;
            }
            return this;
        }

        public virtual ITaskQuery SetProcessDefinitionKeyLike(string processDefinitionKeyLike)
        {
            if (orActive)
            {
                currentOrQueryObject._processDefinitionKeyLike = processDefinitionKeyLike;
            }
            else
            {
                this._processDefinitionKeyLike = processDefinitionKeyLike;
            }
            return this;
        }

        public virtual ITaskQuery SetProcessDefinitionKeyLikeIgnoreCase(string processDefinitionKeyLikeIgnoreCase)
        {
            if (orActive)
            {
                currentOrQueryObject._processDefinitionKeyLikeIgnoreCase = processDefinitionKeyLikeIgnoreCase.ToLower();
            }
            else
            {
                this._processDefinitionKeyLikeIgnoreCase = processDefinitionKeyLikeIgnoreCase.ToLower();
            }
            return this;
        }

        public virtual ITaskQuery SetProcessDefinitionKeyIn(IList<string> processDefinitionKeys)
        {
            if (orActive)
            {
                this.currentOrQueryObject.processDefinitionKeys = processDefinitionKeys;
            }
            else
            {
                this.processDefinitionKeys = processDefinitionKeys;
            }
            return this;
        }

        public virtual ITaskQuery SetProcessDefinitionId(string processDefinitionId)
        {
            if (orActive)
            {
                currentOrQueryObject._processDefinitionId = processDefinitionId;
            }
            else
            {
                this._processDefinitionId = processDefinitionId;
            }
            return this;
        }

        public virtual ITaskQuery SetProcessDefinitionName(string processDefinitionName)
        {
            if (orActive)
            {
                currentOrQueryObject._processDefinitionName = processDefinitionName;
            }
            else
            {
                this._processDefinitionName = processDefinitionName;
            }
            return this;
        }

        public virtual ITaskQuery SetProcessDefinitionNameLike(string processDefinitionNameLike)
        {
            if (orActive)
            {
                currentOrQueryObject._processDefinitionNameLike = processDefinitionNameLike;
            }
            else
            {
                this._processDefinitionNameLike = processDefinitionNameLike;
            }
            return this;
        }

        public virtual ITaskQuery SetProcessCategoryIn(IList<string> processCategoryInList)
        {
            if (processCategoryInList is null)
            {
                throw new ActivitiIllegalArgumentException("Process category list is null");
            }
            if (processCategoryInList.Count == 0)
            {
                throw new ActivitiIllegalArgumentException("Process category list is empty");
            }
            foreach (string processCategory in processCategoryInList)
            {
                if (processCategory is null)
                {
                    throw new ActivitiIllegalArgumentException("None of the given process categories can be null");
                }
            }

            if (orActive)
            {
                currentOrQueryObject.processCategoryInList = processCategoryInList;
            }
            else
            {
                this.processCategoryInList = processCategoryInList;
            }
            return this;
        }

        public virtual ITaskQuery SetProcessCategoryNotIn(IList<string> processCategoryNotInList)
        {
            if (processCategoryNotInList is null)
            {
                throw new ActivitiIllegalArgumentException("Process category list is null");
            }
            if (processCategoryNotInList.Count == 0)
            {
                throw new ActivitiIllegalArgumentException("Process category list is empty");
            }
            foreach (string processCategory in processCategoryNotInList)
            {
                if (processCategory is null)
                {
                    throw new ActivitiIllegalArgumentException("None of the given process categories can be null");
                }
            }

            if (orActive)
            {
                currentOrQueryObject.processCategoryNotInList = processCategoryNotInList;
            }
            else
            {
                this.processCategoryNotInList = processCategoryNotInList;
            }
            return this;
        }

        public virtual ITaskQuery SetDeploymentId(string deploymentId)
        {
            if (orActive)
            {
                currentOrQueryObject._deploymentId = deploymentId;
            }
            else
            {
                this._deploymentId = deploymentId;
            }
            return this;
        }

        public virtual ITaskQuery SetDeploymentIdIn(IList<string> deploymentIds)
        {
            if (orActive)
            {
                currentOrQueryObject.deploymentIds = deploymentIds;
            }
            else
            {
                this.deploymentIds = deploymentIds;
            }
            return this;
        }

        public virtual ITaskQuery SetDueDate(DateTime? dueDate)
        {
            if (orActive)
            {
                currentOrQueryObject._dueDate = dueDate;
                currentOrQueryObject._withoutDueDate = false;
            }
            else
            {
                this._dueDate = dueDate;
                this._withoutDueDate = false;
            }
            return this;
        }

        public virtual ITaskQuery SetTaskDueDate(DateTime? dueDate)
        {
            return this.SetDueDate(dueDate);
        }

        public virtual ITaskQuery SetDueBefore(DateTime? dueBefore)
        {
            if (orActive)
            {
                currentOrQueryObject._dueBefore = dueBefore;
                currentOrQueryObject._withoutDueDate = false;
            }
            else
            {
                this._dueBefore = dueBefore;
                this._withoutDueDate = false;
            }
            return this;
        }

        public virtual ITaskQuery SetTaskDueBefore(DateTime? dueDate)
        {
            return SetDueBefore(dueDate);
        }

        public virtual ITaskQuery SetDueAfter(DateTime? dueAfter)
        {
            if (orActive)
            {
                currentOrQueryObject._dueAfter = dueAfter;
                currentOrQueryObject._withoutDueDate = false;
            }
            else
            {
                this._dueAfter = dueAfter;
                this._withoutDueDate = false;
            }
            return this;
        }

        public virtual ITaskQuery SetTaskDueAfter(DateTime? dueDate)
        {
            return SetDueAfter(dueDate);
        }

        public virtual ITaskQuery SetWithoutDueDate()
        {
            if (orActive)
            {
                currentOrQueryObject._withoutDueDate = true;
            }
            else
            {
                this._withoutDueDate = true;
            }
            return this;
        }

        public virtual ITaskQuery SetWithoutTaskDueDate()
        {
            return SetWithoutDueDate();
        }

        public virtual ITaskQuery SetExcludeSubtasks()
        {
            if (orActive)
            {
                currentOrQueryObject._excludeSubtasks = true;
            }
            else
            {
                this._excludeSubtasks = true;
            }
            return this;
        }

        public virtual ITaskQuery SetSuspended()
        {
            if (orActive)
            {
                currentOrQueryObject.suspensionState = SuspensionStateProvider.SUSPENDED;
            }
            else
            {
                this.suspensionState = SuspensionStateProvider.SUSPENDED;
            }
            return this;
        }

        public virtual ITaskQuery SetActive()
        {
            if (orActive)
            {
                currentOrQueryObject.suspensionState = SuspensionStateProvider.ACTIVE;
            }
            else
            {
                this.suspensionState = SuspensionStateProvider.ACTIVE;
            }
            return this;
        }

        public virtual ITaskQuery SetLocale(string locale)
        {
            this._locale = locale;
            return this;
        }

        public override IList<ITask> List()
        {
            IList<ITask> tasks = base.List();

            return tasks;
        }

        public override IList<ITask> ListPage(int firstResult, int maxResults)
        {
            IList<ITask> tasks = base.ListPage(firstResult, maxResults);

            return tasks;
        }

        public virtual ITaskQuery WithLocalizationFallback()
        {
            _withLocalizationFallback = true;
            return this;
        }

        public virtual ITaskQuery SetIncludeTaskLocalVariables()
        {
            this._includeTaskLocalVariables = true;
            return this;
        }

        public virtual ITaskQuery SetIncludeProcessVariables()
        {
            this._includeProcessVariables = true;
            return this;
        }

        public virtual ITaskQuery SetLimitTaskVariables(int? taskVariablesLimit)
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

        public virtual IList<string> CandidateGroups
        {
            get
            {
                if (candidateGroup is object)
                {
                    IList<string> candidateGroupList = new List<string>(1)
                    {
                        candidateGroup
                    };
                    return candidateGroupList;

                }
                else if (candidateGroups is object)
                {
                    return candidateGroups;

                }
                else if (candidateUser is object)
                {
                    return GetGroupsForCandidateUser(candidateUser);

                }
                else if (userIdForCandidateAndAssignee is object)
                {
                    return GetGroupsForCandidateUser(userIdForCandidateAndAssignee);
                }
                return null;
            }
        }

        protected internal virtual IList<string> GetGroupsForCandidateUser(string candidateUser)
        {
            IUserGroupLookupProxy userGroupLookupProxy = Context.ProcessEngineConfiguration.UserGroupLookupProxy;
            if (userGroupLookupProxy is object)
            {
                return userGroupLookupProxy.GetGroupsForCandidateUser(candidateUser);
            }
            else
            {
                log.LogWarning("No UserGroupLookupProxy set on ProcessEngineConfiguration. Tasks queried only where user is directly related, not through groups.");
            }
            return null;
        }

        protected internal override void EnsureVariablesInitialized()
        {
            IVariableTypes types = Context.ProcessEngineConfiguration.VariableTypes;
            foreach (QueryVariableValue var in queryVariableValues)
            {
                var.Initialize(types);
            }

            foreach (TaskQueryImpl orQueryObject in orQueryObjects)
            {
                orQueryObject.EnsureVariablesInitialized();
            }
        }

        // or query ////////////////////////////////////////////////////////////////

        public virtual ITaskQuery Or()
        {
            if (orActive)
            {
                throw new ActivitiException("the query is already in an or statement");
            }

            // Create instance of the orQuery
            orActive = true;
            currentOrQueryObject = new TaskQueryImpl();
            orQueryObjects.Add(currentOrQueryObject);
            return this;
        }

        public virtual ITaskQuery EndOr()
        {
            if (!orActive)
            {
                throw new ActivitiException("endOr() can only be called after calling or()");
            }

            orActive = false;
            currentOrQueryObject = null;
            return this;
        }

        // ordering ////////////////////////////////////////////////////////////////

        public virtual ITaskQuery OrderByTaskId()
        {
            return SetOrderBy(TaskQueryProperty.TASK_ID);
        }

        public virtual ITaskQuery OrderByTaskName()
        {
            return SetOrderBy(TaskQueryProperty.NAME);
        }

        public virtual ITaskQuery OrderByTaskDescription()
        {
            return SetOrderBy(TaskQueryProperty.DESCRIPTION);
        }

        public virtual ITaskQuery OrderByTaskPriority()
        {
            return SetOrderBy(TaskQueryProperty.PRIORITY);
        }

        public virtual ITaskQuery OrderByProcessInstanceId()
        {
            return SetOrderBy(TaskQueryProperty.PROCESS_INSTANCE_ID);
        }

        public virtual ITaskQuery OrderByExecutionId()
        {
            return SetOrderBy(TaskQueryProperty.EXECUTION_ID);
        }

        public virtual ITaskQuery OrderByProcessDefinitionId()
        {
            return SetOrderBy(TaskQueryProperty.PROCESS_DEFINITION_ID);
        }

        public virtual ITaskQuery OrderByTaskAssignee()
        {
            return SetOrderBy(TaskQueryProperty.ASSIGNEE);
        }

        public virtual ITaskQuery OrderByTaskOwner()
        {
            return SetOrderBy(TaskQueryProperty.OWNER);
        }

        public virtual ITaskQuery OrderByTaskCreateTime()
        {
            return SetOrderBy(TaskQueryProperty.CREATE_TIME);
        }

        public virtual ITaskQuery OrderByDueDate()
        {
            return SetOrderBy(TaskQueryProperty.DUE_DATE);
        }

        public virtual ITaskQuery OrderByTaskDueDate()
        {
            return OrderByDueDate();
        }

        public virtual ITaskQuery OrderByTaskDefinitionKey()
        {
            return SetOrderBy(TaskQueryProperty.TASK_DEFINITION_KEY);
        }

        public virtual ITaskQuery OrderByDueDateNullsFirst()
        {
            return SetOrderBy(TaskQueryProperty.DUE_DATE, NullHandlingOnOrder.NULLS_FIRST);
        }

        public virtual ITaskQuery OrderByDueDateNullsLast()
        {
            return SetOrderBy(TaskQueryProperty.DUE_DATE, NullHandlingOnOrder.NULLS_LAST);
        }

        public virtual ITaskQuery OrderByTenantId()
        {
            return SetOrderBy(TaskQueryProperty.TENANT_ID);
        }

        public virtual string MssqlOrDB2OrderBy
        {
            get
            {
                string specialOrderBy = base.OrderBy;
                if (specialOrderBy is object && specialOrderBy.Length > 0)
                {
                    specialOrderBy = specialOrderBy.Replace("RES.", "TEMPRES_");
                }
                return specialOrderBy;
            }
        }

        // results ////////////////////////////////////////////////////////////////

        public override IList<ITask> ExecuteList(ICommandContext commandContext, Page page)
        {
            EnsureVariablesInitialized();
            CheckQueryOk();
            IList<ITask> tasks;
            if (_includeTaskLocalVariables || _includeProcessVariables)
            {
                tasks = commandContext.TaskEntityManager.FindTasksAndVariablesByQueryCriteria(this);
            }
            else
            {
                tasks = commandContext.TaskEntityManager.FindTasksByQueryCriteria(this);
            }

            if (tasks is object && Context.ProcessEngineConfiguration.PerformanceSettings.EnableLocalization)
            {
                foreach (ITask task in tasks)
                {
                    Localize(task);
                }
            }

            return tasks;
        }

        public override long ExecuteCount(ICommandContext commandContext)
        {
            EnsureVariablesInitialized();
            CheckQueryOk();
            return commandContext.TaskEntityManager.FindTaskCountByQueryCriteria(this);
        }

        protected internal virtual void Localize(ITask task)
        {
            task.LocalizedName = null;
            task.LocalizedDescription = null;

            if (!string.IsNullOrWhiteSpace(_locale))
            {
                string processDefinitionId = task.ProcessDefinitionId;
                if (!string.IsNullOrWhiteSpace(processDefinitionId))
                {
                    JToken languageNode = Context.GetLocalizationElementProperties(_locale, task.TaskDefinitionKey, processDefinitionId, _withLocalizationFallback);
                    if (languageNode is object)
                    {
                        JToken languageNameNode = languageNode[DynamicBpmnConstants.LOCALIZATION_NAME];
                        if (languageNameNode is object)
                        {
                            task.LocalizedName = languageNameNode.ToString();
                        }

                        JToken languageDescriptionNode = languageNode[DynamicBpmnConstants.LOCALIZATION_DESCRIPTION];
                        if (languageDescriptionNode is object)
                        {
                            task.LocalizedDescription = languageDescriptionNode.ToString();
                        }
                    }
                }
            }
        }

        private string[] taskNotInIds;

        public ITaskQuery SetTaskIdNotIn(string[] ids)
        {
            this.taskNotInIds = ids;
            return this;
        }

        public string[] TaskNotInIds
        {
            get => taskNotInIds;
            set => taskNotInIds = value;
        }

        // getters ////////////////////////////////////////////////////////////////

        public virtual string Name
        {
            get
            {
                return name;
            }
            set
            {
                SetTaskName(value);
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
                SetTaskNameLike(value);
            }
        }

        public virtual IList<string> NameList
        {
            get
            {
                return nameList;
            }
            set
            {
                SetTaskNameIn(value);
            }
        }

        public virtual IList<string> NameListIgnoreCase
        {
            get
            {
                return nameListIgnoreCase;
            }
            set
            {
                SetTaskNameInIgnoreCase(value);
            }
        }

        public virtual string Assignee
        {
            get
            {
                return assignee;
            }
            set
            {
                SetTaskAssignee(value);
            }
        }

        public virtual bool Unassigned
        {
            get
            {
                return unassigned;
            }
            set
            {
                if (value)
                {
                    SetTaskUnassigned();
                }
                else
                {
                    unassigned = false;
                }
            }
        }

        public virtual DelegationState? DelegationState
        {
            get
            {
                return delegationState;
            }
        }

        public virtual bool NoDelegationState
        {
            get
            {
                return noDelegationState;
            }
        }

        public virtual string DelegationStateString
        {
            get
            {
                return delegationState.ToString();
            }
            set
            {
                if (Enum.TryParse<DelegationState>(value, out var s))
                {
                    SetTaskDelegationState(s);
                }
            }
        }

        public virtual string CandidateUser
        {
            get
            {
                return candidateUser;
            }
            set
            {
                SetTaskCandidateUser(value);
            }
        }

        public virtual string CandidateGroup
        {
            get
            {
                return candidateGroup;
            }
            set
            {
                SetTaskCandidateGroup(value);
            }
        }

        public virtual string ProcessInstanceId
        {
            get
            {
                return _processInstanceId;
            }
            set
            {
                SetProcessInstanceId(value);
            }
        }

        public virtual IList<string> ProcessInstanceIds
        {
            get
            {
                return processInstanceIds;
            }
            set
            {
                SetProcessInstanceIdIn(value?.ToArray());
            }
        }

        public virtual IList<string> ExecutionIds
        {
            get
            {
                return executionIds;
            }
            set
            {
                SetExecutionIdIn(value?.ToArray());
            }
        }

        public virtual string ExecutionId
        {
            get
            {
                return _executionId;
            }
            set
            {
                SetExecutionId(value);
            }
        }

        public virtual string TaskId
        {
            get
            {
                return _taskId;
            }
            set
            {
                SetTaskId(value);
            }
        }

        public virtual string Description
        {
            get
            {
                return description;
            }
            set
            {
                SetTaskDescription(value);
            }
        }

        public virtual string DescriptionLike
        {
            get
            {
                return descriptionLike;
            }
            set
            {
                SetTaskDescriptionLike(value);
            }
        }

        public virtual int? Priority
        {
            get
            {
                return priority;
            }
            set
            {
                SetTaskPriority(value);
            }
        }

        public virtual DateTime? CreateTime
        {
            get
            {
                return createTime;
            }
            set
            {
                SetTaskCreatedOn(value);
            }
        }

        public virtual DateTime? CreateTimeBefore
        {
            get
            {
                return createTimeBefore;
            }
            set
            {
                SetTaskCreatedBefore(value);
            }
        }

        public virtual DateTime? CreateTimeAfter
        {
            get
            {
                return createTimeAfter;
            }
            set
            {
                SetTaskCreatedAfter(value);
            }
        }

        public virtual string Key
        {
            get
            {
                return key;
            }
            set
            {
                SetTaskDefinitionKey(value);
            }
        }

        public virtual string KeyLike
        {
            get
            {
                return keyLike;
            }
            set
            {
                SetTaskDefinitionKeyLike(value);
            }
        }

        public virtual string ProcessDefinitionKey
        {
            get
            {
                return _processDefinitionKey;
            }
            set
            {
                SetProcessDefinitionKey(value);
            }
        }

        public virtual string ProcessDefinitionId
        {
            get
            {
                return _processDefinitionId;
            }
            set
            {
                SetProcessDefinitionId(value);
            }
        }

        public virtual string ProcessDefinitionName
        {
            get
            {
                return _processDefinitionName;
            }
            set
            {
                SetProcessDefinitionName(value);
            }
        }

        public virtual string ProcessInstanceBusinessKey
        {
            get
            {
                return _processInstanceBusinessKey;
            }
            set
            {
                SetProcessInstanceBusinessKey(value);
            }
        }

        public virtual bool ExcludeSubtasks
        {
            get
            {
                return _excludeSubtasks;
            }
            set
            {
                if (value)
                {
                    SetExcludeSubtasks();
                }
                else
                {
                    _excludeSubtasks = false;
                }
            }
        }

        public virtual string TenantId
        {
            get
            {
                return tenantId;
            }
            set
            {
                SetTaskTenantId(value);
            }
        }

        public virtual string TenantIdLike
        {
            get
            {
                return tenantIdLike;
            }
            set
            {
                SetTaskTenantIdLike(value);
            }
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
                    TaskWithoutTenantId();
                }
                else
                {
                    withoutTenantId = false;
                }
            }
        }

        public virtual string UserIdForCandidateAndAssignee
        {
            get
            {
                return userIdForCandidateAndAssignee;
            }
            set
            {
                SetTaskCandidateOrAssigned(value);
            }
        }

        public virtual IList<TaskQueryImpl> OrQueryObjects
        {
            get
            {
                return orQueryObjects;
            }
            set
            {
                this.orQueryObjects = value;
            }
        }


        public virtual int? MinPriority
        {
            get
            {
                return minPriority;
            }
            set
            {
                SetTaskMinPriority(value);
            }
        }

        public virtual int? MaxPriority
        {
            get
            {
                return maxPriority;
            }
            set
            {
                SetTaskMaxPriority(value);
            }
        }

        public virtual string AssigneeLike
        {
            get
            {
                return assigneeLike;
            }
            set
            {
                SetTaskAssigneeLike(value);
            }
        }

        public virtual IList<string> AssigneeIds
        {
            get
            {
                return assigneeIds;
            }
            set
            {
                SetTaskAssigneeIds(value);
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
                SetTaskInvolvedUser(value);
            }
        }

        public virtual IList<string> InvolvedGroups
        {
            get
            {
                return involvedGroups;
            }
            set
            {
                SetTaskInvolvedGroupsIn(value);
            }
        }

        public virtual string Owner
        {
            get
            {
                return owner;
            }
            set
            {
                SetTaskOwner(value);
            }
        }

        public virtual string OwnerLike
        {
            get
            {
                return ownerLike;
            }
            set
            {
                SetTaskOwnerLike(value);
            }
        }

        public virtual string Category
        {
            get
            {
                return category;
            }
            set
            {
                SetTaskCategory(value);
            }
        }

        public virtual string ProcessDefinitionKeyLike
        {
            get
            {
                return _processDefinitionKeyLike;
            }
            set
            {
                SetProcessDefinitionKeyLike(value);
            }
        }

        public virtual IList<string> ProcessDefinitionKeys
        {
            get
            {
                return processDefinitionKeys;
            }
            set
            {
                SetProcessDefinitionKeyIn(value);
            }
        }

        public virtual string ProcessDefinitionNameLike
        {
            get
            {
                return _processDefinitionNameLike;
            }
            set
            {
                SetProcessDefinitionNameLike(value);
            }
        }

        public virtual IList<string> ProcessCategoryInList
        {
            get
            {
                return processCategoryInList;
            }
            set
            {
                SetProcessCategoryIn(value);
            }
        }

        public virtual IList<string> ProcessCategoryNotInList
        {
            get
            {
                return processCategoryNotInList;
            }

            set
            {
                SetProcessCategoryNotIn(value);
            }
        }

        public virtual string DeploymentId
        {
            get
            {
                return _deploymentId;
            }
            set
            {
                SetDeploymentId(value);
            }
        }

        public virtual IList<string> DeploymentIds
        {
            get
            {
                return deploymentIds;
            }
            set
            {
                SetDeploymentIdIn(value);
            }
        }

        public virtual string ProcessInstanceBusinessKeyLike
        {
            get
            {
                return _processInstanceBusinessKeyLike;
            }
            set
            {
                SetProcessInstanceBusinessKeyLike(value);
            }
        }

        public virtual DateTime? DueDate
        {
            get
            {
                return _dueDate;
            }
            set
            {
                SetTaskDueDate(value);
            }
        }

        public virtual DateTime? DueBefore
        {
            get
            {
                return _dueBefore;
            }
            set
            {
                SetTaskDueBefore(value);
            }
        }

        public virtual DateTime? DueAfter
        {
            get
            {
                return _dueAfter;
            }
            set
            {
                SetTaskDueAfter(value);
            }
        }

        public virtual bool WithoutDueDate
        {
            get
            {
                return _withoutDueDate;
            }
            set
            {
                if (value)
                {
                    SetWithoutDueDate();
                }
                else
                {
                    _withoutDueDate = false;
                }
            }
        }

        public virtual ISuspensionState SuspensionState
        {
            get
            {
                return suspensionState;
            }
        }

        public virtual bool IncludeTaskLocalVariables
        {
            get
            {
                return _includeTaskLocalVariables;
            }
            set
            {
                if (value)
                {
                    SetIncludeTaskLocalVariables();
                }
                else
                {
                    _includeTaskLocalVariables = false;
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
                    SetIncludeProcessVariables();
                }
                else
                {
                    _includeProcessVariables = false;
                }
            }
        }

        public virtual bool BothCandidateAndAssigned
        {
            get
            {
                return bothCandidateAndAssigned;
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
                SetTaskNameLikeIgnoreCase(value);
            }
        }

        public virtual string DescriptionLikeIgnoreCase
        {
            get
            {
                return descriptionLikeIgnoreCase;
            }
            set
            {
                SetTaskDescriptionLikeIgnoreCase(value);
            }
        }

        public virtual string AssigneeLikeIgnoreCase
        {
            get
            {
                return assigneeLikeIgnoreCase;
            }
            set
            {
                SetTaskAssigneeLikeIgnoreCase(value);
            }
        }

        public virtual string OwnerLikeIgnoreCase
        {
            get
            {
                return ownerLikeIgnoreCase;
            }
            set
            {
                SetTaskOwnerLikeIgnoreCase(value);
            }
        }

        public virtual string ProcessInstanceBusinessKeyLikeIgnoreCase
        {
            get
            {
                return _processInstanceBusinessKeyLikeIgnoreCase;
            }
            set
            {
                SetProcessInstanceBusinessKeyLikeIgnoreCase(value);
            }
        }

        public virtual string ProcessDefinitionKeyLikeIgnoreCase
        {
            get
            {
                return _processDefinitionKeyLikeIgnoreCase;
            }
            set
            {
                SetProcessDefinitionKeyLikeIgnoreCase(value);
            }
        }

        public virtual string Locale
        {
            get
            {
                return _locale;
            }
            set
            {
                SetLocale(value);
            }
        }

        public virtual bool OrActive
        {
            get
            {
                return orActive;
            }
            set
            {
                if (value)
                {
                    Or();
                }
                else
                {
                    orActive = false;
                }
            }
        }

        public virtual bool? IsAppend
        {
            get; set;
        }

        public virtual bool? IsTransfer
        {
            get; set;
        }

        public virtual bool? IsRuntime
        {
            get; set;
        }

        public virtual bool IsTaskBusinessKey
        {
            get; set;
        }
    }

}