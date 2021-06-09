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
    using Sys.Workflow.Engine.History;
    using Sys.Workflow.Engine.Impl.Contexts;
    using Sys.Workflow.Engine.Impl.Interceptor;
    using Sys.Workflow.Engine.Impl.Persistence.Entity;
    using Sys.Workflow.Engine.Impl.Variable;
    using Sys.Workflow;
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
        protected internal IList<string> executionIds;
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
        protected internal IList<IHistoricTaskInstanceQuery> orQueryObjects = new List<IHistoricTaskInstanceQuery>();
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

        public override long ExecuteCount(ICommandContext commandContext)
        {
            EnsureVariablesInitialized();
            CheckQueryOk();
            return commandContext.HistoricTaskInstanceEntityManager.FindHistoricTaskInstanceCountByQueryCriteria(this);
        }

        public override IList<IHistoricTaskInstance> ExecuteList(ICommandContext commandContext, Page page)
        {
            EnsureVariablesInitialized();
            CheckQueryOk();
            IList<IHistoricTaskInstance> tasks;
            if (includeTaskLocalVariables_.GetValueOrDefault() || includeProcessVariables_.GetValueOrDefault())
            {
                tasks = commandContext.HistoricTaskInstanceEntityManager.FindHistoricTaskInstancesAndVariablesByQueryCriteria(this);
            }
            else
            {
                tasks = commandContext.HistoricTaskInstanceEntityManager.FindHistoricTaskInstancesByQueryCriteria(this);
            }

            if (tasks is object && Context.ProcessEngineConfiguration.PerformanceSettings.EnableLocalization)
            {
                foreach (IHistoricTaskInstance task in tasks)
                {
                    Localize(task);
                }
            }

            return tasks;
        }

        public virtual IHistoricTaskInstanceQuery SetProcessInstanceId(string processInstanceId)
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

        public virtual IHistoricTaskInstanceQuery SetProcessInstanceIdIn(string[] processInstanceIds)
        {
            var ids = processInstanceIds is null ? new List<string>() : processInstanceIds.Where(x => x is object).ToList();

            if (inOrStatement)
            {
                this.currentOrQueryObject.processInstanceIds = ids.Count == 0 ? null : ids;
            }
            else
            {
                this.processInstanceIds = ids.Count == 0 ? null : ids;
            }
            return this;
        }

        public virtual IHistoricTaskInstanceQuery SetExecutionIdIn(string[] executionIds)
        {
            var ids = executionIds is null ? new List<string>() : executionIds.Where(x => x is object).ToList();

            if (inOrStatement)
            {
                currentOrQueryObject.executionIds = ids.Count == 0 ? null : ids;
            }
            else
            {
                this.executionIds = ids.Count == 0 ? null : ids;
            }
            return this;
        }

        public virtual IHistoricTaskInstanceQuery SetProcessInstanceBusinessKey(string processInstanceBusinessKey)
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

        public virtual IHistoricTaskInstanceQuery SetProcessInstanceBusinessKeyLike(string processInstanceBusinessKeyLike)
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

        public virtual IHistoricTaskInstanceQuery SetProcessInstanceBusinessKeyLikeIgnoreCase(string processInstanceBusinessKeyLikeIgnoreCase)
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

        public virtual IHistoricTaskInstanceQuery SetExecutionId(string executionId)
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

        public virtual IHistoricTaskInstanceQuery SetProcessDefinitionId(string processDefinitionId)
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

        public virtual IHistoricTaskInstanceQuery SetProcessDefinitionKey(string processDefinitionKey)
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

        public virtual IHistoricTaskInstanceQuery SetProcessDefinitionKeyLike(string processDefinitionKeyLike)
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

        public virtual IHistoricTaskInstanceQuery SetProcessDefinitionKeyLikeIgnoreCase(string processDefinitionKeyLikeIgnoreCase)
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

        public virtual IHistoricTaskInstanceQuery SetProcessDefinitionKeyIn(IList<string> processDefinitionKeys)
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

        public virtual IHistoricTaskInstanceQuery SetProcessDefinitionName(string processDefinitionName)
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

        public virtual IHistoricTaskInstanceQuery SetProcessDefinitionNameLike(string processDefinitionNameLike)
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

        public virtual IHistoricTaskInstanceQuery SetProcessCategoryIn(IList<string> processCategoryInList)
        {
            var cates = processCategoryInList is null ? new List<string>() : processCategoryInList.Where(x => x is object).ToList();

            if (inOrStatement)
            {
                currentOrQueryObject.processCategoryInList = cates.Count == 0 ? null : cates;
            }
            else
            {
                this.processCategoryInList = cates.Count == 0 ? null : cates;
            }
            return this;
        }

        public virtual IHistoricTaskInstanceQuery SetProcessCategoryNotIn(IList<string> processCategoryNotInList)
        {
            var cates = processCategoryNotInList is null ? new List<string>() : processCategoryNotInList.Where(x => x is object).ToList();

            if (inOrStatement)
            {
                currentOrQueryObject.processCategoryNotInList = cates.Count == 0 ? null : cates;
            }
            else
            {
                processCategoryNotInList = cates.Count == 0 ? null : cates;
            }
            return this;
        }

        public virtual IHistoricTaskInstanceQuery SetDeploymentId(string deploymentId)
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

        public virtual IHistoricTaskInstanceQuery SetDeploymentIdIn(IList<string> deploymentIds)
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

        public virtual IHistoricTaskInstanceQuery SetTaskId(string taskId)
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

        public virtual IHistoricTaskInstanceQuery SetTaskName(string taskName)
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

        public virtual IHistoricTaskInstanceQuery SetTaskNameIn(IList<string> taskNameList)
        {
            if (taskName_ is object)
            {
                throw new ActivitiIllegalArgumentException("Invalid query usage: cannot set both taskNameIn and taskName");
            }
            if (taskNameLike_ is object)
            {
                throw new ActivitiIllegalArgumentException("Invalid query usage: cannot set both taskNameIn and taskNameLike");
            }
            if (taskNameLikeIgnoreCase_ is object)
            {
                throw new ActivitiIllegalArgumentException("Invalid query usage: cannot set both taskNameIn and taskNameLikeIgnoreCase");
            }

            var taskNames = taskNameList is null ? new List<string>() : taskNameList.Where(x => x is object).ToList();

            if (inOrStatement)
            {
                currentOrQueryObject.taskNameList = taskNames.Count == 0 ? null : taskNames;
            }
            else
            {
                this.taskNameList = taskNames.Count == 0 ? null : taskNames;
            }
            return this;
        }

        public virtual IHistoricTaskInstanceQuery SetTaskNameInIgnoreCase(IList<string> taskNameList)
        {
            if (taskName_ is object)
            {
                throw new ActivitiIllegalArgumentException("Invalid query usage: cannot set both taskNameInIgnoreCase and name");
            }
            if (taskNameLike_ is object)
            {
                throw new ActivitiIllegalArgumentException("Invalid query usage: cannot set both taskNameInIgnoreCase and nameLike");
            }
            if (taskNameLikeIgnoreCase_ is object)
            {
                throw new ActivitiIllegalArgumentException("Invalid query usage: cannot set both taskNameInIgnoreCase and nameLikeIgnoreCase");
            }

            var taskNames = taskNameList is null ? new List<string>() : taskNameList.Where(x => x is object).Select(x => x.ToLower()).ToList();

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

        public virtual IHistoricTaskInstanceQuery SetTaskNameLike(string taskNameLike)
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

        public virtual IHistoricTaskInstanceQuery SetTaskNameLikeIgnoreCase(string taskNameLikeIgnoreCase)
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

        public virtual IHistoricTaskInstanceQuery SetTaskParentTaskId(string parentTaskId)
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

        public virtual IHistoricTaskInstanceQuery SetTaskDescription(string taskDescription)
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

        public virtual IHistoricTaskInstanceQuery SetTaskDescriptionLike(string taskDescriptionLike)
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

        public virtual IHistoricTaskInstanceQuery SetTaskDescriptionLikeIgnoreCase(string taskDescriptionLikeIgnoreCase)
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

        public virtual IHistoricTaskInstanceQuery SetTaskDeleteReason(string taskDeleteReason)
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

        public virtual IHistoricTaskInstanceQuery SetTaskDeleteReasonLike(string taskDeleteReasonLike)
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

        public virtual IHistoricTaskInstanceQuery SetTaskAssignee(string taskAssignee)
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

        public virtual IHistoricTaskInstanceQuery SetTaskAssigneeLike(string taskAssigneeLike)
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

        public virtual IHistoricTaskInstanceQuery SetTaskAssigneeLikeIgnoreCase(string taskAssigneeLikeIgnoreCase)
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

        public virtual IHistoricTaskInstanceQuery SetTaskAssigneeIds(IList<string> assigneeIds)
        {
            var ids = assigneeIds is null ? new List<string>() : assigneeIds.Where(x => x is object).ToList();

            if (taskAssignee_ is object && ids.Count > 0)
            {
                throw new ActivitiIllegalArgumentException("Invalid query usage: cannot set both taskAssigneeIds and taskAssignee");
            }
            if (taskAssigneeLike_ is object && ids.Count > 0)
            {
                throw new ActivitiIllegalArgumentException("Invalid query usage: cannot set both taskAssigneeIds and taskAssigneeLike");
            }
            if (taskAssigneeLikeIgnoreCase_ is object && ids.Count > 0)
            {
                throw new ActivitiIllegalArgumentException("Invalid query usage: cannot set both taskAssigneeIds and taskAssigneeLikeIgnoreCase");
            }

            if (inOrStatement)
            {
                currentOrQueryObject.taskAssigneeIds_ = ids.Count == 0 ? null : ids;
            }
            else
            {
                this.taskAssigneeIds_ = ids.Count == 0 ? null : ids;
            }

            return this;
        }

        public virtual IHistoricTaskInstanceQuery SetTaskOwner(string taskOwner)
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

        public virtual IHistoricTaskInstanceQuery SetTaskOwnerLike(string taskOwnerLike)
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

        public virtual IHistoricTaskInstanceQuery SetTaskOwnerLikeIgnoreCase(string taskOwnerLikeIgnoreCase)
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

        public virtual IHistoricTaskInstanceQuery SetFinished()
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

        public virtual IHistoricTaskInstanceQuery SetUnfinished()
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

        public virtual IHistoricTaskInstanceQuery SetTaskVariableValueEquals(string variableName, object variableValue)
        {
            if (inOrStatement)
            {
                currentOrQueryObject.VariableValueEquals(variableName, variableValue);
                return this;
            }
            else
            {
                return VariableValueEquals(variableName, variableValue);
            }
        }

        public virtual IHistoricTaskInstanceQuery SetTaskVariableValueEquals(object variableValue)
        {
            if (inOrStatement)
            {
                currentOrQueryObject.VariableValueEquals(variableValue);
                return this;
            }
            else
            {
                return VariableValueEquals(variableValue);
            }
        }

        public virtual IHistoricTaskInstanceQuery SetTaskVariableValueEqualsIgnoreCase(string name, string value)
        {
            if (inOrStatement)
            {
                currentOrQueryObject.VariableValueEqualsIgnoreCase(name, value);
                return this;
            }
            else
            {
                return VariableValueEqualsIgnoreCase(name, value);
            }
        }

        public virtual IHistoricTaskInstanceQuery SetTaskVariableValueNotEqualsIgnoreCase(string name, string value)
        {
            if (inOrStatement)
            {
                currentOrQueryObject.VariableValueNotEqualsIgnoreCase(name, value);
                return this;
            }
            else
            {
                return VariableValueNotEqualsIgnoreCase(name, value);
            }
        }

        public virtual IHistoricTaskInstanceQuery SetTaskVariableValueNotEquals(string variableName, object variableValue)
        {
            if (inOrStatement)
            {
                currentOrQueryObject.VariableValueNotEquals(variableName, variableValue);
                return this;
            }
            else
            {
                return VariableValueNotEquals(variableName, variableValue);
            }
        }

        public virtual IHistoricTaskInstanceQuery SetTaskVariableValueGreaterThan(string name, object value)
        {
            if (inOrStatement)
            {
                currentOrQueryObject.VariableValueGreaterThan(name, value);
                return this;
            }
            else
            {
                return VariableValueGreaterThan(name, value);
            }
        }

        public virtual IHistoricTaskInstanceQuery SetTaskVariableValueGreaterThanOrEqual(string name, object value)
        {
            if (inOrStatement)
            {
                currentOrQueryObject.VariableValueGreaterThanOrEqual(name, value);
                return this;
            }
            else
            {
                return VariableValueGreaterThanOrEqual(name, value);
            }
        }

        public virtual IHistoricTaskInstanceQuery SetTaskVariableValueLessThan(string name, object value)
        {
            if (inOrStatement)
            {
                currentOrQueryObject.VariableValueLessThan(name, value);
                return this;
            }
            else
            {
                return VariableValueLessThan(name, value);
            }
        }

        public virtual IHistoricTaskInstanceQuery SetTaskVariableValueLessThanOrEqual(string name, object value)
        {
            if (inOrStatement)
            {
                currentOrQueryObject.VariableValueLessThanOrEqual(name, value);
                return this;
            }
            else
            {
                return VariableValueLessThanOrEqual(name, value);
            }
        }

        public virtual IHistoricTaskInstanceQuery SetTaskVariableValueLike(string name, string value)
        {
            if (inOrStatement)
            {
                currentOrQueryObject.VariableValueLike(name, value);
                return this;
            }
            else
            {
                return VariableValueLike(name, value);
            }
        }

        public virtual IHistoricTaskInstanceQuery SetTaskVariableValueLikeIgnoreCase(string name, string value)
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

        public virtual IHistoricTaskInstanceQuery SetProcessVariableValueEquals(string variableName, object variableValue)
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

        public virtual IHistoricTaskInstanceQuery SetProcessVariableValueNotEquals(string variableName, object variableValue)
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

        public virtual IHistoricTaskInstanceQuery SetProcessVariableValueEquals(object variableValue)
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

        public virtual IHistoricTaskInstanceQuery SetProcessVariableValueEqualsIgnoreCase(string name, string value)
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

        public virtual IHistoricTaskInstanceQuery SetProcessVariableValueNotEqualsIgnoreCase(string name, string value)
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

        public virtual IHistoricTaskInstanceQuery SetProcessVariableValueGreaterThan(string name, object value)
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

        public virtual IHistoricTaskInstanceQuery SetProcessVariableValueGreaterThanOrEqual(string name, object value)
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

        public virtual IHistoricTaskInstanceQuery SetProcessVariableValueLessThan(string name, object value)
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

        public virtual IHistoricTaskInstanceQuery SetProcessVariableValueLessThanOrEqual(string name, object value)
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

        public virtual IHistoricTaskInstanceQuery SetProcessVariableValueLike(string name, string value)
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

        public virtual IHistoricTaskInstanceQuery SetProcessVariableValueLikeIgnoreCase(string name, string value)
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

        public virtual IHistoricTaskInstanceQuery SetTaskDefinitionKey(string taskDefinitionKey)
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

        public virtual IHistoricTaskInstanceQuery SetTaskDefinitionKeyLike(string taskDefinitionKeyLike)
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

        public virtual IHistoricTaskInstanceQuery SetTaskPriority(int? taskPriority)
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

        public virtual IHistoricTaskInstanceQuery SetTaskMinPriority(int? taskMinPriority)
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

        public virtual IHistoricTaskInstanceQuery SetTaskMaxPriority(int? taskMaxPriority)
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

        public virtual IHistoricTaskInstanceQuery SetProcessFinished()
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

        public virtual IHistoricTaskInstanceQuery SetProcessUnfinished()
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

        protected internal override void EnsureVariablesInitialized()
        {
            IVariableTypes types = Context.ProcessEngineConfiguration.VariableTypes;
            foreach (QueryVariableValue var in queryVariableValues)
            {
                var.Initialize(types);
            }

            foreach (HistoricTaskInstanceQueryImpl orQueryObject in orQueryObjects)
            {
                orQueryObject.EnsureVariablesInitialized();
            }
        }

        public virtual IHistoricTaskInstanceQuery SetTaskDueDate(DateTime? dueDate)
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

        public virtual IHistoricTaskInstanceQuery SetTaskDueAfter(DateTime? dueAfter)
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

        public virtual IHistoricTaskInstanceQuery SetTaskDueBefore(DateTime? dueBefore)
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

        public virtual IHistoricTaskInstanceQuery SetTaskCreatedOn(DateTime? creationDate)
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

        public virtual IHistoricTaskInstanceQuery SetTaskCreatedBefore(DateTime? creationBeforeDate)
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

        public virtual IHistoricTaskInstanceQuery SetTaskCreatedAfter(DateTime? creationAfterDate)
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

        public virtual IHistoricTaskInstanceQuery SetTaskCompletedOn(DateTime? completedDate)
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

        public virtual IHistoricTaskInstanceQuery SetTaskCompletedBefore(DateTime? completedBeforeDate)
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

        public virtual IHistoricTaskInstanceQuery SetTaskCompletedAfter(DateTime? completedAfterDate)
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

        public virtual IHistoricTaskInstanceQuery SetWithoutTaskDueDate()
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

        public virtual IHistoricTaskInstanceQuery SetTaskCategory(string category)
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


        public virtual IHistoricTaskInstanceQuery SetTaskCandidateUser(string candidateUser)
        {
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

        public virtual IHistoricTaskInstanceQuery SetTaskCandidateUser(string candidateUser, IList<string> usersGroups)
        {
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

        public virtual IHistoricTaskInstanceQuery SetTaskCandidateGroup(string candidateGroup)
        {
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

        public virtual IHistoricTaskInstanceQuery SetTaskCandidateGroupIn(IList<string> candidateGroups)
        {
            var groups = candidateGroups is null ? new List<string>() : candidateGroups.Where(x => x is object).ToList();

            if (string.IsNullOrWhiteSpace(candidateGroup) == false && groups.Count > 0)
            {
                throw new ActivitiIllegalArgumentException("Invalid query usage: cannot set both candidateGroupIn and candidateGroup");
            }

            if (inOrStatement)
            {
                this.currentOrQueryObject.candidateGroups = groups.Count == 0 ? null : groups;
            }
            else
            {
                this.candidateGroups = groups.Count == 0 ? null : groups;
            }
            return this;
        }

        public virtual IHistoricTaskInstanceQuery SetTaskInvolvedUser(string involvedUser)
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

        public virtual IHistoricTaskInstanceQuery SetTaskInvolvedGroupsIn(IList<string> involvedGroups)
        {
            var groups = involvedGroups is null ? new List<string>() : involvedGroups.Where(x => x is object).ToList();

            if (inOrStatement)
            {
                this.currentOrQueryObject.involvedGroups = groups.Count == 0 ? null : groups;
            }
            else
            {
                this.involvedGroups = groups.Count == 0 ? null : groups;
            }
            return this;
        }

        public virtual IHistoricTaskInstanceQuery SetTaskTenantId(string tenantId)
        {
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

        public virtual IHistoricTaskInstanceQuery SetTaskTenantIdLike(string tenantIdLike)
        {
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

        public virtual IHistoricTaskInstanceQuery TaskWithoutTenantId()
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

        public virtual IHistoricTaskInstanceQuery SetLocale(string locale)
        {
            this.locale_ = locale;
            return this;
        }

        public virtual IHistoricTaskInstanceQuery WithLocalizationFallback()
        {
            withLocalizationFallback_ = true;
            return this;
        }

        public virtual IHistoricTaskInstanceQuery SetIncludeTaskLocalVariables()
        {
            this.includeTaskLocalVariables_ = true;
            return this;
        }

        public virtual IHistoricTaskInstanceQuery SetIncludeProcessVariables()
        {
            this.includeProcessVariables_ = true;
            return this;
        }

        public virtual IHistoricTaskInstanceQuery SetLimitTaskVariables(int? taskVariablesLimit)
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
            set => SetLimitTaskVariables(value);
        }

        public virtual IHistoricTaskInstanceQuery Or()
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

        public virtual IHistoricTaskInstanceQuery EndOr()
        {
            if (!inOrStatement)
            {
                throw new ActivitiException("endOr() can only be called after calling or()");
            }

            inOrStatement = false;
            currentOrQueryObject = null;
            return this;
        }

        protected internal virtual void Localize(IHistoricTaskInstance task)
        {
            IHistoricTaskInstanceEntity taskEntity = (IHistoricTaskInstanceEntity)task;
            taskEntity.LocalizedName = null;
            taskEntity.LocalizedDescription = null;

            if (locale_ is object)
            {
                string processDefinitionId = task.ProcessDefinitionId;
                if (!string.IsNullOrWhiteSpace(processDefinitionId))
                {
                    JToken languageNode = Context.GetLocalizationElementProperties(locale_, task.TaskDefinitionKey, processDefinitionId, withLocalizationFallback_.GetValueOrDefault());
                    if (languageNode is object)
                    {
                        JToken languageNameNode = languageNode[DynamicBpmnConstants.LOCALIZATION_NAME];
                        if (languageNameNode is object)
                        {
                            taskEntity.LocalizedName = languageNameNode.ToString();
                        }

                        JToken languageDescriptionNode = languageNode[DynamicBpmnConstants.LOCALIZATION_DESCRIPTION];
                        if (languageDescriptionNode is object)
                        {
                            taskEntity.LocalizedDescription = languageDescriptionNode.ToString();
                        }
                    }
                }
            }
        }

        // ordering
        // /////////////////////////////////////////////////////////////////

        public virtual IHistoricTaskInstanceQuery OrderByTaskId()
        {
            SetOrderBy(HistoricTaskInstanceQueryProperty.HISTORIC_TASK_INSTANCE_ID);
            return this;
        }

        public virtual IHistoricTaskInstanceQuery OrderByHistoricActivityInstanceId()
        {
            SetOrderBy(HistoricTaskInstanceQueryProperty.PROCESS_DEFINITION_ID);
            return this;
        }

        public virtual IHistoricTaskInstanceQuery OrderByProcessDefinitionId()
        {
            SetOrderBy(HistoricTaskInstanceQueryProperty.PROCESS_DEFINITION_ID);
            return this;
        }

        public virtual IHistoricTaskInstanceQuery OrderByProcessInstanceId()
        {
            SetOrderBy(HistoricTaskInstanceQueryProperty.PROCESS_INSTANCE_ID);
            return this;
        }

        public virtual IHistoricTaskInstanceQuery OrderByExecutionId()
        {
            SetOrderBy(HistoricTaskInstanceQueryProperty.EXECUTION_ID);
            return this;
        }

        public virtual IHistoricTaskInstanceQuery OrderByHistoricTaskInstanceDuration()
        {
            SetOrderBy(HistoricTaskInstanceQueryProperty.DURATION);
            return this;
        }

        public virtual IHistoricTaskInstanceQuery OrderByHistoricTaskInstanceEndTime()
        {
            SetOrderBy(HistoricTaskInstanceQueryProperty.END);
            return this;
        }

        public virtual IHistoricTaskInstanceQuery OrderByHistoricActivityInstanceStartTime()
        {
            SetOrderBy(HistoricTaskInstanceQueryProperty.START);
            return this;
        }

        public virtual IHistoricTaskInstanceQuery OrderByHistoricTaskInstanceStartTime()
        {
            SetOrderBy(HistoricTaskInstanceQueryProperty.START);
            return this;
        }

        public virtual IHistoricTaskInstanceQuery OrderByTaskCreateTime()
        {
            return OrderByHistoricTaskInstanceStartTime();
        }

        public virtual IHistoricTaskInstanceQuery OrderByTaskName()
        {
            SetOrderBy(HistoricTaskInstanceQueryProperty.TASK_NAME);
            return this;
        }

        public virtual IHistoricTaskInstanceQuery OrderByTaskDescription()
        {
            SetOrderBy(HistoricTaskInstanceQueryProperty.TASK_DESCRIPTION);
            return this;
        }

        public virtual IHistoricTaskInstanceQuery OrderByTaskAssignee()
        {
            SetOrderBy(HistoricTaskInstanceQueryProperty.TASK_ASSIGNEE);
            return this;
        }

        public virtual IHistoricTaskInstanceQuery OrderByTaskOwner()
        {
            SetOrderBy(HistoricTaskInstanceQueryProperty.TASK_OWNER);
            return this;
        }

        public virtual IHistoricTaskInstanceQuery OrderByTaskDueDate()
        {
            SetOrderBy(HistoricTaskInstanceQueryProperty.TASK_DUE_DATE);
            return this;
        }

        public virtual IHistoricTaskInstanceQuery OrderByDueDateNullsFirst()
        {
            return SetOrderBy(HistoricTaskInstanceQueryProperty.TASK_DUE_DATE, NullHandlingOnOrder.NULLS_FIRST);
        }

        public virtual IHistoricTaskInstanceQuery OrderByDueDateNullsLast()
        {
            return SetOrderBy(HistoricTaskInstanceQueryProperty.TASK_DUE_DATE, NullHandlingOnOrder.NULLS_LAST);
        }

        public virtual IHistoricTaskInstanceQuery OrderByDeleteReason()
        {
            SetOrderBy(HistoricTaskInstanceQueryProperty.DELETE_REASON);
            return this;
        }

        public virtual IHistoricTaskInstanceQuery OrderByTaskDefinitionKey()
        {
            SetOrderBy(HistoricTaskInstanceQueryProperty.TASK_DEFINITION_KEY);
            return this;
        }

        public virtual IHistoricTaskInstanceQuery OrderByTaskPriority()
        {
            SetOrderBy(HistoricTaskInstanceQueryProperty.TASK_PRIORITY);
            return this;
        }

        public virtual IHistoricTaskInstanceQuery OrderByTenantId()
        {
            SetOrderBy(HistoricTaskInstanceQueryProperty.TENANT_ID_);
            return this;
        }

        protected internal override void CheckQueryOk()
        {
            base.CheckQueryOk();
            // In case historic query variables are included, an additional order-by
            // clause should be added
            // to ensure the last value of a variable is used
            if (includeProcessVariables_.GetValueOrDefault() || includeTaskLocalVariables_.GetValueOrDefault())
            {
                this.SetOrderBy(HistoricTaskInstanceQueryProperty.INCLUDED_VARIABLE_TIME).Asc();
            }
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

        private string[] taskNotInIds;

        public IHistoricTaskInstanceQuery SetTaskIdNotIn(string[] ids)
        {
            taskNotInIds = ids;
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
            set => SetProcessInstanceId(value);
        }

        public virtual IList<string> ProcessInstanceIds
        {
            get
            {
                return processInstanceIds;
            }
            set => SetProcessInstanceIdIn(value.ToArray());
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
            set => SetExecutionId(value);
        }

        public virtual IList<string> ExecutionIds
        {
            get
            {
                return executionIds;
            }
        }

        public virtual string ProcessDefinitionId
        {
            get
            {
                return processDefinitionId_;
            }
            set => SetProcessDefinitionId(value);
        }

        public virtual string ProcessDefinitionKey
        {
            get
            {
                return processDefinitionKey_;
            }
            set => SetProcessDefinitionKey(value);
        }

        public virtual string ProcessDefinitionKeyLike
        {
            get
            {
                return processDefinitionKeyLike_;
            }
            set => SetProcessDefinitionKeyLike(value);
        }
        public virtual IList<string> ProcessDefinitionKeys
        {
            get
            {
                return processDefinitionKeys;
            }
            set => SetProcessDefinitionKeyIn(value);
        }
        public virtual string ProcessDefinitionName
        {
            get
            {
                return processDefinitionName_;
            }
            set => SetProcessDefinitionName(value);
        }

        public virtual string ProcessDefinitionNameLike
        {
            get
            {
                return processDefinitionNameLike_;
            }
            set => SetProcessDefinitionNameLike(value);
        }

        public virtual IList<string> ProcessCategoryInList
        {
            get
            {
                return processCategoryInList;
            }
            set => SetProcessCategoryIn(value);
        }

        public virtual IList<string> ProcessCategoryNotInList
        {
            get
            {
                return processCategoryNotInList;
            }
            set => SetProcessCategoryNotIn(value);
        }

        public virtual string DeploymentId
        {
            get
            {
                return deploymentId_;
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

        public virtual string ProcessInstanceBusinessKeyLike
        {
            get
            {
                return processInstanceBusinessKeyLike_;
            }
            set => SetProcessInstanceBusinessKeyLike(value);
        }

        public virtual string TaskDefinitionKeyLike
        {
            get
            {
                return taskDefinitionKeyLike_;
            }
            set => SetTaskDefinitionKeyLike(value);
        }

        public virtual int? TaskPriority
        {
            get
            {
                return taskPriority_;
            }
            set => SetTaskPriority(value);
        }

        public virtual int? TaskMinPriority
        {
            get
            {
                return taskMinPriority_;
            }
            set => SetTaskMinPriority(value);
        }

        public virtual int? TaskMaxPriority
        {
            get
            {
                return taskMaxPriority_;
            }
            set => SetTaskMaxPriority(value);
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
                    SetProcessFinished();
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
                    SetProcessUnfinished();
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
            set => SetTaskDueDate(value);
        }

        public virtual DateTime? DueAfter
        {
            get
            {
                return dueAfter;
            }
            set => SetTaskDueAfter(value);
        }

        public virtual DateTime? DueBefore
        {
            get
            {
                return dueBefore;
            }
            set => SetTaskDueBefore(value);
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
                    SetWithoutTaskDueDate();
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
            set => SetTaskCreatedAfter(value);
        }

        public virtual DateTime? CreationBeforeDate
        {
            get
            {
                return creationBeforeDate;
            }
            set => SetTaskCreatedBefore(value);
        }

        public virtual DateTime? CompletedDate
        {
            get
            {
                return completedDate;
            }
            set => SetTaskCompletedOn(value);
        }

        public virtual DateTime? CompletedAfterDate
        {
            get
            {
                return completedAfterDate;
            }
            set => SetTaskCompletedAfter(value);
        }

        public virtual DateTime? CompletedBeforeDate
        {
            get
            {
                return completedBeforeDate;
            }
            set => SetTaskCompletedBefore(value);
        }

        public virtual string Category
        {
            get
            {
                return category;
            }
            set => SetTaskCategory(value);
        }

        public virtual string TenantId
        {
            get
            {
                return tenantId;
            }
            set => SetTaskTenantId(value);
        }

        public virtual string TenantIdLike
        {
            get
            {
                return tenantIdLike;
            }
            set => SetTaskTenantIdLike(value);
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
                    TaskWithoutTenantId();
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
                    SetIncludeProcessVariables();
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
                    SetIncludeProcessVariables();
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
                    SetFinished();
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
                    SetUnfinished();
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
            set => SetTaskName(value);
        }

        public virtual string TaskNameLike
        {
            get
            {
                return taskNameLike_;
            }
            set => SetTaskNameLike(value);
        }

        public virtual IList<string> TaskNameList
        {
            get
            {
                return taskNameList;
            }
            set => SetTaskNameIn(taskNameList);
        }

        public virtual IList<string> TaskNameListIgnoreCase
        {
            get
            {
                return taskNameListIgnoreCase;
            }
            set => SetTaskNameInIgnoreCase(value);
        }

        public virtual string TaskDescription
        {
            get
            {
                return taskDescription_;
            }
            set => SetTaskDescription(value);
        }

        public virtual string TaskDescriptionLike
        {
            get
            {
                return taskDescriptionLike_;
            }
            set => SetTaskDescriptionLike(value);
        }

        public virtual string TaskDeleteReason
        {
            get
            {
                return taskDeleteReason_;
            }
            set => SetTaskDeleteReason(value);
        }

        public virtual string TaskDeleteReasonLike
        {
            get
            {
                return taskDeleteReasonLike_;
            }
            set => SetTaskDeleteReasonLike(value);
        }

        public virtual IList<string> TaskAssigneeIds
        {
            get
            {
                return taskAssigneeIds_;
            }
            set => SetTaskAssigneeIds(value);
        }

        public virtual string TaskAssignee
        {
            get
            {
                return taskAssignee_;
            }
            set => SetTaskAssignee(value);
        }

        public virtual string TaskAssigneeLike
        {
            get
            {
                return taskAssigneeLike_;
            }
            set => SetTaskAssigneeLike(value);
        }

        public virtual string TaskId
        {
            get
            {
                return taskId_;
            }
            set => SetTaskId(value);
        }

        public virtual string TaskDefinitionKey
        {
            get
            {
                return taskDefinitionKey_;
            }
            set => SetTaskDefinitionKey(value);
        }

        public virtual string TaskOwnerLike
        {
            get
            {
                return taskOwnerLike_;
            }
            set => SetTaskOwnerLike(value);
        }

        public virtual string TaskOwner
        {
            get
            {
                return taskOwner_;
            }
            set => SetTaskOwner(value);
        }

        public virtual string TaskParentTaskId
        {
            get
            {
                return taskParentTaskId_;
            }
            set => SetTaskParentTaskId(value);
        }

        public virtual DateTime? CreationDate
        {
            get
            {
                return creationDate;
            }
            set => SetTaskCreatedOn(value);
        }

        public virtual string CandidateUser
        {
            get
            {
                return candidateUser;
            }
            set => SetTaskCandidateUser(value);
        }

        public virtual string CandidateGroup
        {
            get
            {
                return candidateGroup;
            }
            set => SetTaskCandidateGroup(value);
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
            set => SetTaskInvolvedUser(value);
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
            set => SetTaskInvolvedGroupsIn(value);
        }

        public virtual string ProcessDefinitionKeyLikeIgnoreCase
        {
            get
            {
                return processDefinitionKeyLikeIgnoreCase_;
            }
            set => SetProcessDefinitionKeyLikeIgnoreCase(value);
        }

        public virtual string ProcessInstanceBusinessKeyLikeIgnoreCase
        {
            get
            {
                return processInstanceBusinessKeyLikeIgnoreCase_;
            }
            set => SetProcessInstanceBusinessKeyLikeIgnoreCase(value);
        }

        public virtual string TaskNameLikeIgnoreCase
        {
            get
            {
                return taskNameLikeIgnoreCase_;
            }
            set => SetTaskNameLikeIgnoreCase(value);
        }

        public virtual string TaskDescriptionLikeIgnoreCase
        {
            get
            {
                return taskDescriptionLikeIgnoreCase_;
            }
            set => SetTaskDescriptionLikeIgnoreCase(value);
        }

        public virtual string TaskOwnerLikeIgnoreCase
        {
            get
            {
                return taskOwnerLikeIgnoreCase_;
            }
            set => SetTaskOwnerLikeIgnoreCase(value);
        }

        public virtual string TaskAssigneeLikeIgnoreCase
        {
            get
            {
                return taskAssigneeLikeIgnoreCase_;
            }
            set => SetTaskAssigneeLikeIgnoreCase(value);
        }

        public virtual string Locale
        {
            get
            {
                return locale_;
            }
            set => SetLocale(value);
        }

        public virtual IList<IHistoricTaskInstanceQuery> OrQueryObjects
        {
            get
            {
                return orQueryObjects;
            }
            set => orQueryObjects = value;
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
    }

}