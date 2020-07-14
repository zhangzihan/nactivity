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
namespace Sys.Workflow.Engine.Impl.Bpmn.Behavior
{
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json.Linq;
    using Sys.Workflow.Bpmn.Constants;
    using Sys.Workflow.Bpmn.Models;
    using Sys.Workflow.Engine.Delegate;
    using Sys.Workflow.Engine.Delegate.Events;
    using Sys.Workflow.Engine.Delegate.Events.Impl;
    using Sys.Workflow.Engine.Impl.Bpmn.Helper;
    using Sys.Workflow.Engine.Impl.Calendars;
    using Sys.Workflow.Engine.Impl.Cfg;
    using Sys.Workflow.Engine.Impl.Contexts;
    using Sys.Workflow.Engine.Impl.EL;
    using Sys.Workflow.Engine.Impl.Interceptor;
    using Sys.Workflow.Engine.Impl.Persistence.Entity;
    using Sys;
    using Sys.Workflow;
    using Sys.Workflow.Engine.Bpmn.Rules;
    using System.Collections;

    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class UserTaskActivityBehavior : TaskActivityBehavior
    {
        private static readonly ILogger<UserTaskActivityBehavior> log = ProcessEngineServiceProvider.LoggerService<UserTaskActivityBehavior>();

        private const long serialVersionUID = 1L;

        protected internal UserTask userTask;

        private IUserServiceProxy UserService => ProcessEngineServiceProvider.Resolve<IUserServiceProxy>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userTask"></param>
        public UserTaskActivityBehavior(UserTask userTask)
        {
            this.userTask = userTask;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="execution"></param>
        public override void Execute(IExecutionEntity execution)
        {
            ICommandContext commandContext = Context.CommandContext;
            ITaskEntityManager taskEntityManager = commandContext.TaskEntityManager;

            //如果当前任务为补偿任务，则修改任务的父级为流程实例
            if ((execution.CurrentFlowElement as UserTask).ForCompensation)
            {
                execution.Parent = execution.ProcessInstance;
            }

            ITaskEntity task = taskEntityManager.Create();
            task.Execution = execution;
            task.TaskDefinitionKey = userTask.Id;
            task.IsRuntimeAssignee();

            task.CanTransfer = userTask.CanTransfer;
            task.OnlyAssignee = task.OnlyAssignee;

            ProcessEngineConfigurationImpl processEngineConfiguration = Context.ProcessEngineConfiguration;
            ExpressionManager expressionManager = processEngineConfiguration.ExpressionManager;

            string activeTaskName;
            string activeTaskDescription;
            string activeTaskDueDate;
            string activeTaskCategory;
            string activeTaskSkipExpression;
            string activeTaskPriority;
            string activeTaskFormKey;
            string activeTaskAssignee;
            string activeTaskOwner;
            IList<string> activeTaskCandidateUsers;
            IList<string> activeTaskCandidateGroups;

            if (Context.ProcessEngineConfiguration.EnableProcessDefinitionInfoCache)
            {
                JToken taskElementProperties = Context.GetBpmnOverrideElementProperties(userTask.Id, execution.ProcessDefinitionId);
                activeTaskName = GetActiveValue(userTask.Name, DynamicBpmnConstants.USER_TASK_NAME, taskElementProperties);
                activeTaskDescription = GetActiveValue(userTask.Documentation, DynamicBpmnConstants.USER_TASK_DESCRIPTION, taskElementProperties);
                activeTaskDueDate = GetActiveValue(userTask.DueDate, DynamicBpmnConstants.USER_TASK_DUEDATE, taskElementProperties);
                activeTaskPriority = GetActiveValue(userTask.Priority, DynamicBpmnConstants.USER_TASK_PRIORITY, taskElementProperties);
                activeTaskCategory = GetActiveValue(userTask.Category, DynamicBpmnConstants.USER_TASK_CATEGORY, taskElementProperties);
                activeTaskFormKey = GetActiveValue(userTask.FormKey, DynamicBpmnConstants.USER_TASK_FORM_KEY, taskElementProperties);
                activeTaskSkipExpression = GetActiveValue(userTask.SkipExpression, DynamicBpmnConstants.TASK_SKIP_EXPRESSION, taskElementProperties);
                activeTaskAssignee = GetActiveValue(userTask.Assignee, DynamicBpmnConstants.USER_TASK_ASSIGNEE, taskElementProperties);
                activeTaskOwner = GetActiveValue(userTask.Owner, DynamicBpmnConstants.USER_TASK_OWNER, taskElementProperties);
                activeTaskCandidateUsers = GetActiveValueList(userTask.CandidateUsers, DynamicBpmnConstants.USER_TASK_CANDIDATE_USERS, taskElementProperties);
                activeTaskCandidateGroups = GetActiveValueList(userTask.CandidateGroups, DynamicBpmnConstants.USER_TASK_CANDIDATE_GROUPS, taskElementProperties);

            }
            else
            {
                activeTaskName = userTask.Name;
                activeTaskDescription = userTask.Documentation;
                activeTaskDueDate = userTask.DueDate;
                activeTaskPriority = userTask.Priority;
                activeTaskCategory = userTask.Category;
                activeTaskFormKey = userTask.FormKey;
                activeTaskSkipExpression = userTask.SkipExpression;
                activeTaskAssignee = userTask.Assignee;
                activeTaskOwner = userTask.Owner;
                activeTaskCandidateUsers = userTask.CandidateUsers;
                activeTaskCandidateGroups = userTask.CandidateGroups;
            }

            if (!string.IsNullOrWhiteSpace(activeTaskName))
            {
                string name;
                try
                {
                    name = (string)expressionManager.CreateExpression(activeTaskName).GetValue(execution);
                }
                catch (ActivitiException e)
                {
                    name = activeTaskName;
                    log.LogWarning("property not found in task name expression " + e.Message);
                }
                task.Name = name;
            }

            if (!string.IsNullOrWhiteSpace(activeTaskDescription))
            {
                string description;
                try
                {
                    description = (string)expressionManager.CreateExpression(activeTaskDescription).GetValue(execution);
                }
                catch (ActivitiException e)
                {
                    description = activeTaskDescription;
                    log.LogWarning("property not found in task description expression " + e.Message);
                }
                task.Description = description;
            }

            if (!string.IsNullOrWhiteSpace(activeTaskDueDate))
            {
                object dueDate = expressionManager.CreateExpression(activeTaskDueDate).GetValue(execution);
                if (dueDate != null)
                {
                    if (dueDate is DateTime time)
                    {
                        task.DueDate = time;
                    }
                    else if (dueDate is string @string)
                    {
                        string businessCalendarName;
                        if (!string.IsNullOrWhiteSpace(userTask.BusinessCalendarName))
                        {
                            businessCalendarName = expressionManager.CreateExpression(userTask.BusinessCalendarName).GetValue(execution).ToString();
                        }
                        else
                        {
                            businessCalendarName = DueDateBusinessCalendar.NAME;
                        }

                        IBusinessCalendar businessCalendar = Context.ProcessEngineConfiguration.BusinessCalendarManager.GetBusinessCalendar(businessCalendarName);
                        task.DueDate = businessCalendar.ResolveDuedate(@string);

                    }
                    else
                    {
                        throw new ActivitiIllegalArgumentException("Due date expression does not resolve to a Date or Date string: " + activeTaskDueDate);
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(activeTaskPriority))
            {
                object priority = expressionManager.CreateExpression(activeTaskPriority).GetValue(execution);
                if (priority != null)
                {
                    if (priority is string @string)
                    {
                        try
                        {
                            task.Priority = Convert.ToInt32(@string);
                        }
                        catch (FormatException e)
                        {
                            throw new ActivitiIllegalArgumentException("Priority does not resolve to a number: " + priority, e);
                        }
                    }
                    else if (priority is int || priority is long)
                    {
                        task.Priority = (int)priority;
                    }
                    else
                    {
                        throw new ActivitiIllegalArgumentException("Priority expression does not resolve to a number: " + activeTaskPriority);
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(activeTaskCategory))
            {
                object category = expressionManager.CreateExpression(activeTaskCategory).GetValue(execution);
                if (category != null)
                {
                    if (category is string)
                    {
                        task.Category = category.ToString();
                    }
                    else
                    {
                        throw new ActivitiIllegalArgumentException("Category expression does not resolve to a string: " + activeTaskCategory);
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(activeTaskFormKey))
            {
                object formKey = expressionManager.CreateExpression(activeTaskFormKey).GetValue(execution);
                if (formKey != null)
                {
                    if (formKey is string)
                    {
                        task.FormKey = formKey.ToString();
                    }
                    else
                    {
                        throw new ActivitiIllegalArgumentException("FormKey expression does not resolve to a string: " + activeTaskFormKey);
                    }
                }
            }

            taskEntityManager.Insert(task, execution);

            bool skipUserTask = false;
            if (!string.IsNullOrWhiteSpace(activeTaskSkipExpression))
            {
                IExpression skipExpression = expressionManager.CreateExpression(activeTaskSkipExpression);
                skipUserTask = SkipExpressionUtil.IsSkipExpressionEnabled(execution, skipExpression) && SkipExpressionUtil.ShouldSkipFlowElement(execution, skipExpression);
            }

            // Handling assignments need to be done after the task is inserted, to have an id
            if (!skipUserTask)
            {
                HandleAssignments(taskEntityManager, activeTaskAssignee, activeTaskOwner, activeTaskCandidateUsers, activeTaskCandidateGroups, task, expressionManager, execution);
            }

            processEngineConfiguration.ListenerNotificationHelper.ExecuteTaskListeners(task, BaseTaskListenerFields.EVENTNAME_CREATE);

            // All properties set, now fire events
            if (Context.ProcessEngineConfiguration.EventDispatcher.Enabled)
            {
                IActivitiEventDispatcher eventDispatcher = Context.ProcessEngineConfiguration.EventDispatcher;
                eventDispatcher.DispatchEvent(ActivitiEventBuilder.CreateEntityEvent(ActivitiEventType.TASK_CREATED, task));
                if (string.IsNullOrWhiteSpace(task.Assignee) == false)
                {
                    eventDispatcher.DispatchEvent(ActivitiEventBuilder.CreateEntityEvent(ActivitiEventType.TASK_ASSIGNED, task));
                }
            }

            if (skipUserTask)
            {
                taskEntityManager.DeleteTask(task, null, false, false);
                Leave(execution);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="execution"></param>
        /// <param name="signalName"></param>
        /// <param name="signalData"></param>
        /// <param name="throwError"></param>
        public override void Trigger(IExecutionEntity execution, string signalName, object signalData, bool throwError = true)
        {
            ICommandContext commandContext = Context.CommandContext;
            ITaskEntityManager taskEntityManager = commandContext.TaskEntityManager;
            IList<ITaskEntity> taskEntities = taskEntityManager.FindTasksByExecutionId(execution.Id); // Should be only one
            foreach (ITaskEntity taskEntity in taskEntities)
            {
                if (!taskEntity.Deleted)
                {
                    if (throwError)
                    {
                        throw new ActivitiException("UserTask should not be signalled before complete");
                    }
                }
            }

            Leave(execution, signalData);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="taskEntityManager"></param>
        /// <param name="assignee"></param>
        /// <param name="owner"></param>
        /// <param name="candidateUsers"></param>
        /// <param name="candidateGroups"></param>
        /// <param name="task"></param>
        /// <param name="expressionManager"></param>
        /// <param name="execution"></param>
        protected internal virtual void HandleAssignments(ITaskEntityManager taskEntityManager, string assignee, string owner, IList<string> candidateUsers, IList<string> candidateGroups, ITaskEntity task, ExpressionManager expressionManager, IExecutionEntity execution)
        {

            if (!string.IsNullOrWhiteSpace(assignee))
            {
                object assigneeExpressionValue = expressionManager.CreateExpression(assignee).GetValue(execution);
                string assigneeValue = null;
                if (assigneeExpressionValue != null)
                {
                    assigneeValue = assigneeExpressionValue.ToString();
                }
                string assigneeUser = null;

                if (string.IsNullOrWhiteSpace(assigneeValue) == false)
                {
                    IUserServiceProxy userService = ProcessEngineServiceProvider.Resolve<IUserServiceProxy>();

                    var user = userService.GetUser(assigneeValue)
                         .ConfigureAwait(false)
                         .GetAwaiter()
                         .GetResult();

                    assigneeUser = user?.FullName;

                    task.SetVariableLocal(assigneeValue, user);
                }
                taskEntityManager.ChangeTaskAssigneeNoEvents(task, assigneeValue, assigneeUser);
            }

            if (!string.IsNullOrWhiteSpace(owner))
            {
                object ownerExpressionValue = expressionManager.CreateExpression(owner).GetValue(execution);
                string ownerValue = null;
                if (ownerExpressionValue != null)
                {
                    ownerValue = ownerExpressionValue.ToString();
                }

                taskEntityManager.ChangeTaskOwner(task, ownerValue);
            }

            if (candidateGroups != null && candidateGroups.Count > 0)
            {
                foreach (string candidateGroup in candidateGroups)
                {
                    IExpression groupIdExpr = expressionManager.CreateExpression(candidateGroup);
                    object value = groupIdExpr.GetValue(execution);
                    if (value is string @string)
                    {
                        IList<string> candidates = ExtractCandidates(@string);
                        task.AddCandidateGroups(candidates);
                    }
                    else if (value is ICollection)
                    {
                        task.AddCandidateGroups((ICollection<string>)value);
                    }
                    else
                    {
                        throw new ActivitiIllegalArgumentException("Expression did not resolve to a string or collection of strings");
                    }
                }
            }

            if (candidateUsers != null && candidateUsers.Count > 0)
            {
                foreach (string candidateUser in candidateUsers)
                {
                    IExpression userIdExpr = expressionManager.CreateExpression(candidateUser);
                    object value = userIdExpr.GetValue(execution);
                    if (value is string @string)
                    {
                        IList<string> candidates = ExtractCandidates(@string);
                        task.AddCandidateUsers(candidates);
                    }
                    else if (value is ICollection)
                    {
                        task.AddCandidateUsers((ICollection<string>)value);
                    }
                    else
                    {
                        throw new ActivitiException("Expression did not resolve to a string or collection of strings");
                    }
                }
            }

            if (userTask.CustomUserIdentityLinks != null && userTask.CustomUserIdentityLinks.Count > 0)
            {

                foreach (string customUserIdentityLinkType in userTask.CustomUserIdentityLinks.Keys)
                {
                    foreach (string userIdentityLink in userTask.CustomUserIdentityLinks[customUserIdentityLinkType])
                    {
                        IExpression idExpression = expressionManager.CreateExpression(userIdentityLink);
                        object value = idExpression.GetValue(execution);
                        if (value is string @string)
                        {
                            IList<string> userIds = ExtractCandidates(@string);
                            foreach (string userId in userIds)
                            {
                                task.AddUserIdentityLink(userId, customUserIdentityLinkType);
                            }
                        }
                        else if (value is ICollection collection)
                        {
                            IEnumerator userIdSet = collection.GetEnumerator();
                            while (userIdSet.MoveNext())
                            {
                                task.AddUserIdentityLink((string)userIdSet.Current, customUserIdentityLinkType);
                            }
                        }
                        else
                        {
                            throw new ActivitiException("Expression did not resolve to a string or collection of strings");
                        }

                    }
                }

            }

            if (userTask.CustomGroupIdentityLinks != null && userTask.CustomGroupIdentityLinks.Count > 0)
            {
                foreach (string customGroupIdentityLinkType in userTask.CustomGroupIdentityLinks.Keys)
                {
                    foreach (string groupIdentityLink in userTask.CustomGroupIdentityLinks[customGroupIdentityLinkType])
                    {
                        IExpression idExpression = expressionManager.CreateExpression(groupIdentityLink);
                        object value = idExpression.GetValue(execution);
                        if (value is string @string)
                        {
                            IList<string> groupIds = ExtractCandidates(@string);
                            foreach (string groupId in groupIds)
                            {
                                task.AddGroupIdentityLink(groupId, customGroupIdentityLinkType);
                            }
                        }
                        else if (value is ICollection collection)
                        {
                            IEnumerator groupIdSet = collection.GetEnumerator();
                            while (groupIdSet.MoveNext())
                            {
                                task.AddGroupIdentityLink((string)groupIdSet.Current, customGroupIdentityLinkType);
                            }
                        }
                        else
                        {
                            throw new ActivitiException("Expression did not resolve to a string or collection of strings");
                        }

                    }
                }
            }
        }

        /// <summary>
        /// Extract a candidate list from a string.
        /// </summary>
        /// <param name="str">
        /// @return </param>
        protected internal virtual IList<string> ExtractCandidates(string str)
        {
            return new List<string>(str.Split(new string[] { "[\\s]*,[\\s]*" }, StringSplitOptions.RemoveEmptyEntries));
        }
    }

}