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
namespace org.activiti.engine.impl.bpmn.behavior
{
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json.Linq;
    using org.activiti.bpmn.model;
    using org.activiti.engine.@delegate;
    using org.activiti.engine.@delegate.@event;
    using org.activiti.engine.@delegate.@event.impl;
    using org.activiti.engine.impl.bpmn.helper;
    using org.activiti.engine.impl.calendar;
    using org.activiti.engine.impl.cfg;
    using org.activiti.engine.impl.context;
    using org.activiti.engine.impl.el;
    using org.activiti.engine.impl.interceptor;
    using org.activiti.engine.impl.persistence.entity;
    using Sys;

    /// 
    [Serializable]
    public class UserTaskActivityBehavior : TaskActivityBehavior
    {
        private static readonly ILogger<MultiInstanceActivityBehavior> log = ProcessEngineServiceProvider.LoggerService<MultiInstanceActivityBehavior>();

        private const long serialVersionUID = 1L;

        protected internal UserTask userTask;

        public UserTaskActivityBehavior(UserTask userTask)
        {
            this.userTask = userTask;
        }

        public override void execute(IExecutionEntity execution)
        {
            ICommandContext commandContext = Context.CommandContext;
            ITaskEntityManager taskEntityManager = commandContext.TaskEntityManager;

            ITaskEntity task = taskEntityManager.create();
            task.Execution = execution;
            task.TaskDefinitionKey = userTask.Id;

            string activeTaskName = null;
            string activeTaskDescription = null;
            string activeTaskDueDate = null;
            string activeTaskPriority = null;
            string activeTaskCategory = null;
            string activeTaskFormKey = null;
            string activeTaskSkipExpression = null;
            string activeTaskAssignee = null;
            string activeTaskOwner = null;
            IList<string> activeTaskCandidateUsers = null;
            IList<string> activeTaskCandidateGroups = null;

            ProcessEngineConfigurationImpl processEngineConfiguration = Context.ProcessEngineConfiguration;
            ExpressionManager expressionManager = processEngineConfiguration.ExpressionManager;

            if (Context.ProcessEngineConfiguration.EnableProcessDefinitionInfoCache)
            {
                JToken taskElementProperties = Context.getBpmnOverrideElementProperties(userTask.Id, execution.ProcessDefinitionId);
                activeTaskName = getActiveValue(userTask.Name, DynamicBpmnConstants_Fields.USER_TASK_NAME, taskElementProperties);
                activeTaskDescription = getActiveValue(userTask.Documentation, DynamicBpmnConstants_Fields.USER_TASK_DESCRIPTION, taskElementProperties);
                activeTaskDueDate = getActiveValue(userTask.DueDate, DynamicBpmnConstants_Fields.USER_TASK_DUEDATE, taskElementProperties);
                activeTaskPriority = getActiveValue(userTask.Priority, DynamicBpmnConstants_Fields.USER_TASK_PRIORITY, taskElementProperties);
                activeTaskCategory = getActiveValue(userTask.Category, DynamicBpmnConstants_Fields.USER_TASK_CATEGORY, taskElementProperties);
                activeTaskFormKey = getActiveValue(userTask.FormKey, DynamicBpmnConstants_Fields.USER_TASK_FORM_KEY, taskElementProperties);
                activeTaskSkipExpression = getActiveValue(userTask.SkipExpression, DynamicBpmnConstants_Fields.TASK_SKIP_EXPRESSION, taskElementProperties);
                activeTaskAssignee = getActiveValue(userTask.Assignee, DynamicBpmnConstants_Fields.USER_TASK_ASSIGNEE, taskElementProperties);
                activeTaskOwner = getActiveValue(userTask.Owner, DynamicBpmnConstants_Fields.USER_TASK_OWNER, taskElementProperties);
                activeTaskCandidateUsers = getActiveValueList(userTask.CandidateUsers, DynamicBpmnConstants_Fields.USER_TASK_CANDIDATE_USERS, taskElementProperties);
                activeTaskCandidateGroups = getActiveValueList(userTask.CandidateGroups, DynamicBpmnConstants_Fields.USER_TASK_CANDIDATE_GROUPS, taskElementProperties);

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
                string name = null;
                try
                {
                    name = (string)expressionManager.createExpression(activeTaskName).getValue(execution);
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
                string description = null;
                try
                {
                    description = (string)expressionManager.createExpression(activeTaskDescription).getValue(execution);
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
                object dueDate = expressionManager.createExpression(activeTaskDueDate).getValue(execution);
                if (dueDate != null)
                {
                    if (dueDate is DateTime)
                    {
                        ((task.ITask)task).DueDate = (DateTime)dueDate;
                    }
                    else if (dueDate is string)
                    {
                        string businessCalendarName = null;
                        if (!string.IsNullOrWhiteSpace(userTask.BusinessCalendarName))
                        {
                            businessCalendarName = expressionManager.createExpression(userTask.BusinessCalendarName).getValue(execution).ToString();
                        }
                        else
                        {
                            businessCalendarName = DueDateBusinessCalendar.NAME;
                        }

                        IBusinessCalendar businessCalendar = Context.ProcessEngineConfiguration.BusinessCalendarManager.getBusinessCalendar(businessCalendarName);
                        ((task.ITask)task).DueDate = businessCalendar.resolveDuedate((string)dueDate);

                    }
                    else
                    {
                        throw new ActivitiIllegalArgumentException("Due date expression does not resolve to a Date or Date string: " + activeTaskDueDate);
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(activeTaskPriority))
            {
                object priority = expressionManager.createExpression(activeTaskPriority).getValue(execution);
                if (priority != null)
                {
                    if (priority is string)
                    {
                        try
                        {
                            ((task.ITask)task).Priority = Convert.ToInt32((string)priority);
                        }
                        catch (System.FormatException e)
                        {
                            throw new ActivitiIllegalArgumentException("Priority does not resolve to a number: " + priority, e);
                        }
                    }
                    else if (priority is Int32 || priority is Int64)
                    {
                        ((task.ITask)task).Priority = ((int)priority);
                    }
                    else
                    {
                        throw new ActivitiIllegalArgumentException("Priority expression does not resolve to a number: " + activeTaskPriority);
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(activeTaskCategory))
            {
                object category = expressionManager.createExpression(activeTaskCategory).getValue(execution);
                if (category != null)
                {
                    if (category is string)
                    {
                        ((task.ITask)task).Category = category.ToString();
                    }
                    else
                    {
                        throw new ActivitiIllegalArgumentException("Category expression does not resolve to a string: " + activeTaskCategory);
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(activeTaskFormKey))
            {
                object formKey = expressionManager.createExpression(activeTaskFormKey).getValue(execution);
                if (formKey != null)
                {
                    if (formKey is string)
                    {
                        ((task.ITask)task).FormKey = formKey.ToString();
                    }
                    else
                    {
                        throw new ActivitiIllegalArgumentException("FormKey expression does not resolve to a string: " + activeTaskFormKey);
                    }
                }
            }

            taskEntityManager.insert(task, execution);

            bool skipUserTask = false;
            if (!string.IsNullOrWhiteSpace(activeTaskSkipExpression))
            {
                IExpression skipExpression = expressionManager.createExpression(activeTaskSkipExpression);
                skipUserTask = SkipExpressionUtil.isSkipExpressionEnabled(execution, skipExpression) && SkipExpressionUtil.shouldSkipFlowElement(execution, skipExpression);
            }

            // Handling assignments need to be done after the task is inserted, to have an id
            if (!skipUserTask)
            {
                handleAssignments(taskEntityManager, activeTaskAssignee, activeTaskOwner, activeTaskCandidateUsers, activeTaskCandidateGroups, task, expressionManager, execution);
            }

            processEngineConfiguration.ListenerNotificationHelper.executeTaskListeners(task, BaseTaskListener_Fields.EVENTNAME_CREATE);

            // All properties set, now fire events
            if (Context.ProcessEngineConfiguration.EventDispatcher.Enabled)
            {
                IActivitiEventDispatcher eventDispatcher = Context.ProcessEngineConfiguration.EventDispatcher;
                eventDispatcher.dispatchEvent(ActivitiEventBuilder.createEntityEvent(ActivitiEventType.TASK_CREATED, task));
                if (!ReferenceEquals(((task.ITask)task).Assignee, null))
                {
                    eventDispatcher.dispatchEvent(ActivitiEventBuilder.createEntityEvent(ActivitiEventType.TASK_ASSIGNED, task));
                }
            }

            if (skipUserTask)
            {
                taskEntityManager.deleteTask(task, null, false, false);
                leave(execution);
            }
        }

        public override void trigger(IExecutionEntity execution, string signalName, object signalData)
        {
            ICommandContext commandContext = Context.CommandContext;
            ITaskEntityManager taskEntityManager = commandContext.TaskEntityManager;
            IList<ITaskEntity> taskEntities = taskEntityManager.findTasksByExecutionId(execution.Id); // Should be only one
            foreach (ITaskEntity taskEntity in taskEntities)
            {
                if (!taskEntity.Deleted)
                {
                    throw new ActivitiException("UserTask should not be signalled before complete");
                }
            }

            leave(execution);
        }

        protected internal virtual void handleAssignments(ITaskEntityManager taskEntityManager, string assignee, string owner, IList<string> candidateUsers, IList<string> candidateGroups, ITaskEntity task, ExpressionManager expressionManager, IExecutionEntity execution)
        {

            if (!string.IsNullOrWhiteSpace(assignee))
            {
                object assigneeExpressionValue = expressionManager.createExpression(assignee).getValue(execution);
                string assigneeValue = null;
                if (assigneeExpressionValue != null)
                {
                    assigneeValue = assigneeExpressionValue.ToString();
                }

                taskEntityManager.changeTaskAssigneeNoEvents(task, assigneeValue);
            }

            if (!string.IsNullOrWhiteSpace(owner))
            {
                object ownerExpressionValue = expressionManager.createExpression(owner).getValue(execution);
                string ownerValue = null;
                if (ownerExpressionValue != null)
                {
                    ownerValue = ownerExpressionValue.ToString();
                }

                taskEntityManager.changeTaskOwner(task, ownerValue);
            }

            if (candidateGroups != null && candidateGroups.Count > 0)
            {
                foreach (string candidateGroup in candidateGroups)
                {
                    IExpression groupIdExpr = expressionManager.createExpression(candidateGroup);
                    object value = groupIdExpr.getValue(execution);
                    if (value is string)
                    {
                        IList<string> candidates = extractCandidates((string)value);
                        task.addCandidateGroups(candidates);
                    }
                    else if (value is System.Collections.ICollection)
                    {
                        task.addCandidateGroups((ICollection<string>)value);
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
                    IExpression userIdExpr = expressionManager.createExpression(candidateUser);
                    object value = userIdExpr.getValue(execution);
                    if (value is string)
                    {
                        IList<string> candidates = extractCandidates((string)value);
                        task.addCandidateUsers(candidates);
                    }
                    else if (value is System.Collections.ICollection)
                    {
                        task.addCandidateUsers((ICollection<string>)value);
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
                        IExpression idExpression = expressionManager.createExpression(userIdentityLink);
                        object value = idExpression.getValue(execution);
                        if (value is string)
                        {
                            IList<string> userIds = extractCandidates((string)value);
                            foreach (string userId in userIds)
                            {
                                task.addUserIdentityLink(userId, customUserIdentityLinkType);
                            }
                        }
                        else if (value is System.Collections.ICollection)
                        {
                            System.Collections.IEnumerator userIdSet = ((System.Collections.ICollection)value).GetEnumerator();
                            while (userIdSet.MoveNext())
                            {
                                task.addUserIdentityLink((string)userIdSet.Current, customUserIdentityLinkType);
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

                        IExpression idExpression = expressionManager.createExpression(groupIdentityLink);
                        object value = idExpression.getValue(execution);
                        if (value is string)
                        {
                            IList<string> groupIds = extractCandidates((string)value);
                            foreach (string groupId in groupIds)
                            {
                                task.addGroupIdentityLink(groupId, customGroupIdentityLinkType);
                            }
                        }
                        else if (value is System.Collections.ICollection)
                        {
                            System.Collections.IEnumerator groupIdSet = ((System.Collections.ICollection)value).GetEnumerator();
                            while (groupIdSet.MoveNext())
                            {
                                task.addGroupIdentityLink((string)groupIdSet.Current, customGroupIdentityLinkType);
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
        protected internal virtual IList<string> extractCandidates(string str)
        {
            return new List<string>(str.Split(new string[] { "[\\s]*,[\\s]*" }, StringSplitOptions.RemoveEmptyEntries));
        }
    }

}