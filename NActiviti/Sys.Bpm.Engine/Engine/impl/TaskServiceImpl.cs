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
    using org.activiti.engine.impl.cfg;
    using org.activiti.engine.impl.cmd;
    using org.activiti.engine.impl.interceptor;
    using org.activiti.engine.impl.persistence.entity;
    using org.activiti.engine.runtime;
    using org.activiti.engine.task;
    using org.activiti.services.api.commands;
    using Sys.Workflow;
    using System.IO;
    using System.Linq;

    /// 
    /// 
    public class TaskServiceImpl : ServiceImpl, ITaskService
    {
        private readonly object syncRoot = new object();

        private readonly ILogger<TaskServiceImpl> logger = ProcessEngineServiceProvider.LoggerService<TaskServiceImpl>();

        public TaskServiceImpl()
        {

        }

        public TaskServiceImpl(ProcessEngineConfigurationImpl processEngineConfiguration) : base(processEngineConfiguration)
        {
        }

        private TOut ExecuteCommand<TOut>(ICommand<TOut> command)
        {
            return commandExecutor.Execute(command);
        }

        public virtual ITask NewTask()
        {
            return NewTask(null);
        }

        public virtual ITask NewTask(string taskId)
        {
            return ExecuteCommand(new NewTaskCmd(taskId));
        }

        public virtual void SaveTask(ITask task)
        {
            ExecuteCommand(new SaveTaskCmd(task));
        }

        public virtual void TerminateTask(string taskId, string terminateReason, bool terminateExecution)
        {
            ExecuteCommand(new TerminateTaskCmd(taskId, terminateReason, terminateExecution));
        }

        public virtual void DeleteTasks(ICollection<string> taskIds)
        {
            ExecuteCommand(new DeleteTaskCmd(taskIds, null, false));
        }

        public virtual void DeleteTasks(ICollection<string> taskIds, bool cascade)
        {
            ExecuteCommand(new DeleteTaskCmd(taskIds, null, cascade));
        }

        public virtual void DeleteTask(string taskId)
        {
            DeleteTask(taskId, "Canceled");
        }

        public virtual void DeleteTask(string taskId, string deleteReason, bool cascade = false)
        {
            ExecuteCommand(new DeleteTaskCmd(taskId, deleteReason, cascade));
        }

        public virtual void DeleteTasks(ICollection<string> taskIds, string deleteReason)
        {
            ExecuteCommand(new DeleteTaskCmd(taskIds, deleteReason, false));
        }

        public virtual void SetAssignee(string taskId, string userId)
        {
            ExecuteCommand(new AddIdentityLinkCmd(taskId, userId, AddIdentityLinkCmd.IDENTITY_USER, IdentityLinkType.ASSIGNEE));
        }

        public virtual void SetOwner(string taskId, string userId)
        {
            ExecuteCommand(new AddIdentityLinkCmd(taskId, userId, AddIdentityLinkCmd.IDENTITY_USER, IdentityLinkType.OWNER));
        }

        public virtual void AddCandidateUser(string taskId, string userId)
        {
            ExecuteCommand(new AddIdentityLinkCmd(taskId, userId, AddIdentityLinkCmd.IDENTITY_USER, IdentityLinkType.CANDIDATE));
        }

        public virtual void AddCandidateGroup(string taskId, string groupId)
        {
            ExecuteCommand(new AddIdentityLinkCmd(taskId, groupId, AddIdentityLinkCmd.IDENTITY_GROUP, IdentityLinkType.CANDIDATE));
        }

        public virtual void AddUserIdentityLink(string taskId, string userId, string identityLinkType)
        {
            ExecuteCommand(new AddIdentityLinkCmd(taskId, userId, AddIdentityLinkCmd.IDENTITY_USER, identityLinkType));
        }

        public virtual void AddGroupIdentityLink(string taskId, string groupId, string identityLinkType)
        {
            ExecuteCommand(new AddIdentityLinkCmd(taskId, groupId, AddIdentityLinkCmd.IDENTITY_GROUP, identityLinkType));
        }

        public virtual void DeleteCandidateGroup(string taskId, string groupId)
        {
            ExecuteCommand(new DeleteIdentityLinkCmd(taskId, null, groupId, IdentityLinkType.CANDIDATE));
        }

        public virtual void DeleteCandidateUser(string taskId, string userId)
        {
            ExecuteCommand(new DeleteIdentityLinkCmd(taskId, userId, null, IdentityLinkType.CANDIDATE));
        }

        public virtual void DeleteGroupIdentityLink(string taskId, string groupId, string identityLinkType)
        {
            ExecuteCommand(new DeleteIdentityLinkCmd(taskId, null, groupId, identityLinkType));
        }

        public virtual void DeleteUserIdentityLink(string taskId, string userId, string identityLinkType)
        {
            ExecuteCommand(new DeleteIdentityLinkCmd(taskId, userId, null, identityLinkType));
        }

        public virtual IList<IIdentityLink> GetIdentityLinksForTask(string taskId)
        {
            return ExecuteCommand(new GetIdentityLinksForTaskCmd(taskId));
        }

        public virtual void Claim(string taskId, string userId)
        {
            ExecuteCommand(new ClaimTaskCmd(taskId, userId));
        }

        public virtual void Unclaim(string taskId)
        {
            ExecuteCommand(new ClaimTaskCmd(taskId, null));
        }

        public virtual void Complete(string taskId)
        {
            lock (syncRoot)
            {
                try
                {
                    if (TryGetTask(taskId, out var task) == false)
                    {
                        return;
                    }
                    Complete(taskId, null);
                }
                catch (ActivitiObjectNotFoundException)
                {
                    if (logger.IsEnabled(LogLevel.Debug))
                    {
                        logger.LogDebug("任务可能已经终止或无效.");
                    }
                }
            }
        }

        public virtual void Complete(string taskId, IDictionary<string, object> variables)
        {
            lock (syncRoot)
            {
                try
                {
                    if (TryGetTask(taskId, out var task) == false)
                    {
                        return;
                    }
                    Complete(taskId, variables, null);
                }
                catch (ActivitiObjectNotFoundException)
                {
                    if (logger.IsEnabled(LogLevel.Debug))
                    {
                        logger.LogDebug("任务可能已经终止或无效.");
                    }
                }
            }
        }

        public virtual void Complete(string taskId, IDictionary<string, object> variables, IDictionary<string, object> transientVariables)
        {
            lock (syncRoot)
            {
                try
                {
                    if (TryGetTask(taskId, out var task) == false)
                    {
                        return;
                    }
                    ExecuteCommand(new CompleteTaskCmd(taskId, variables, transientVariables));
                }
                catch (ActivitiObjectNotFoundException)
                {
                    if (logger.IsEnabled(LogLevel.Debug))
                    {
                        logger.LogDebug("任务可能已经终止或无效.");
                    }
                }
            }
        }

        public virtual void Complete(string taskId, IDictionary<string, object> variables, bool localScope)
        {
            lock (syncRoot)
            {
                try
                {
                    if (TryGetTask(taskId, out var task) == false)
                    {
                        return;
                    }
                    ExecuteCommand(new CompleteTaskCmd(taskId, variables, localScope));
                }
                catch (ActivitiObjectNotFoundException)
                {
                    if (logger.IsEnabled(LogLevel.Debug))
                    {
                        logger.LogDebug("任务可能已经终止或无效.");
                    }
                }
            }
        }

        public virtual void Complete(string taskId, string comment, IDictionary<string, object> variables, bool localScope, IDictionary<string, object> transientVariables)
        {
            lock (syncRoot)
            {
                try
                {
                    if (TryGetTask(taskId, out var task) == false)
                    {
                        return;
                    }
                    if (string.IsNullOrWhiteSpace(comment) == false)
                    {
                        AddComment(taskId, task.ProcessInstanceId, comment);
                    }

                    ExecuteCommand(new CompleteTaskCmd(taskId, variables, transientVariables, localScope));
                }
                catch (ActivitiObjectNotFoundException)
                {
                    if (logger.IsEnabled(LogLevel.Debug))
                    {
                        logger.LogDebug("任务可能已经终止或无效.");
                    }
                }
            }
        }

        public void Complete(string businessKey, string assignee, string comment, IDictionary<string, object> variables, bool localScope, IDictionary<string, object> transientVariables = null)
        {
            lock (syncRoot)
            {
                try
                {
                    if (TryGetTask(businessKey, assignee, out var task) == false)
                    {
                        return;
                    }

                    ExecuteCommand(new CompleteTaskCmd(task.Id, variables, transientVariables, localScope));
                }
                catch (ActivitiObjectNotFoundException)
                {
                    if (logger.IsEnabled(LogLevel.Debug))
                    {
                        logger.LogDebug("任务可能已经终止或无效.");
                    }
                }
            }
        }

        public virtual void DelegateTask(string taskId, string userId)
        {
            ExecuteCommand(new DelegateTaskCmd(taskId, userId));
        }

        public virtual void ResolveTask(string taskId)
        {
            ExecuteCommand(new ResolveTaskCmd(taskId, null));
        }

        public virtual void ResolveTask(string taskId, IDictionary<string, object> variables)
        {
            ExecuteCommand(new ResolveTaskCmd(taskId, variables));
        }

        public virtual void ResolveTask(string taskId, IDictionary<string, object> variables, IDictionary<string, object> transientVariables)
        {
            ExecuteCommand(new ResolveTaskCmd(taskId, variables, transientVariables));
        }

        public virtual void SetPriority(string taskId, int priority)
        {
            ExecuteCommand(new SetTaskPriorityCmd(taskId, priority));
        }

        public virtual void SetDueDate(string taskId, DateTime dueDate)
        {
            ExecuteCommand(new SetTaskDueDateCmd(taskId, dueDate));
        }

        public virtual ITaskQuery CreateTaskQuery()
        {
            return new TaskQueryImpl(commandExecutor, processEngineConfiguration.DatabaseType);
        }

        public virtual INativeTaskQuery CreateNativeTaskQuery()
        {
            return new NativeTaskQueryImpl(commandExecutor);
        }

        public virtual IDictionary<string, object> GetVariables(string taskId)
        {
            return ExecuteCommand(new GetTaskVariablesCmd(taskId, null, false));
        }

        public virtual IDictionary<string, object> GetVariablesLocal(string taskId)
        {
            return ExecuteCommand(new GetTaskVariablesCmd(taskId, null, true));
        }

        public virtual IDictionary<string, object> GetVariables(string taskId, ICollection<string> variableNames)
        {
            return ExecuteCommand(new GetTaskVariablesCmd(taskId, variableNames, false));
        }

        public virtual IDictionary<string, object> GetVariablesLocal(string taskId, ICollection<string> variableNames)
        {
            return ExecuteCommand(new GetTaskVariablesCmd(taskId, variableNames, true));
        }

        public virtual object GetVariable(string taskId, string variableName)
        {
            return ExecuteCommand(new GetTaskVariableCmd(taskId, variableName, false));
        }

        public virtual T GetVariable<T>(string taskId, string variableName)
        {
            return (T)GetVariable(taskId, variableName);
        }

        public virtual bool HasVariable(string taskId, string variableName)
        {
            return ExecuteCommand(new HasTaskVariableCmd(taskId, variableName, false));
        }

        public virtual object GetVariableLocal(string taskId, string variableName)
        {
            return ExecuteCommand(new GetTaskVariableCmd(taskId, variableName, true));
        }

        public virtual T GetVariableLocal<T>(string taskId, string variableName)
        {
            return (T)GetVariableLocal(taskId, variableName);
        }

        public virtual IList<IVariableInstance> GetVariableInstancesLocalByTaskIds(string[] taskIds)
        {
            return ExecuteCommand(new GetTasksLocalVariablesCmd(taskIds));
        }

        public virtual bool HasVariableLocal(string taskId, string variableName)
        {
            return ExecuteCommand(new HasTaskVariableCmd(taskId, variableName, true));
        }

        public virtual bool TryGetTask(string businessKey, string assignee, out ITask task)
        {
            if (string.IsNullOrWhiteSpace(businessKey) || string.IsNullOrWhiteSpace(assignee))
            {
                task = null;
            }
            else
            {
                task = CreateTaskQuery().SetProcessInstanceBusinessKey(businessKey)
                    .SetTaskInvolvedUser(assignee)
                    .List()
                    .FirstOrDefault();
            }

            return task != null;
        }

        public virtual bool TryGetTask(string taskId, out ITask task)
        {
            if (string.IsNullOrWhiteSpace(taskId))
            {
                task = null;
            }
            else
            {
                task = CreateTaskQuery().SetTaskId(taskId).SingleResult();
            }

            return task != null;
        }

        public virtual void SetVariable(string taskId, string variableName, object value)
        {
            if (variableName is null)
            {
                throw new ActivitiIllegalArgumentException("variableName is null");
            }
            IDictionary<string, object> variables = new Dictionary<string, object>
            {
                [variableName] = value
            };
            ExecuteCommand(new SetTaskVariablesCmd<object>(taskId, variables, false));
        }

        public virtual void SetVariableLocal(string taskId, string variableName, object value)
        {
            if (variableName is null)
            {
                throw new ActivitiIllegalArgumentException("variableName is null");
            }
            IDictionary<string, object> variables = new Dictionary<string, object>
            {
                [variableName] = value
            };
            ExecuteCommand(new SetTaskVariablesCmd<object>(taskId, variables, true));
        }

        public virtual void SetVariables<T1>(string taskId, IDictionary<string, T1> variables)
        {
            ExecuteCommand(new SetTaskVariablesCmd<T1>(taskId, variables, false));
        }

        public virtual void SetVariablesLocal<T1>(string taskId, IDictionary<string, T1> variables)
        {
            ExecuteCommand(new SetTaskVariablesCmd<T1>(taskId, variables, true));
        }

        public virtual void RemoveVariable(string taskId, string variableName)
        {
            ICollection<string> variableNames = new List<string>
            {
                variableName
            };
            ExecuteCommand(new RemoveTaskVariablesCmd(taskId, variableNames, false));
        }

        public virtual void RemoveVariableLocal(string taskId, string variableName)
        {
            ICollection<string> variableNames = new List<string>(1)
            {
                variableName
            };
            ExecuteCommand(new RemoveTaskVariablesCmd(taskId, variableNames, true));
        }

        public virtual void RemoveVariables(string taskId, ICollection<string> variableNames)
        {
            ExecuteCommand(new RemoveTaskVariablesCmd(taskId, variableNames, false));
        }

        public virtual void RemoveVariablesLocal(string taskId, ICollection<string> variableNames)
        {
            ExecuteCommand(new RemoveTaskVariablesCmd(taskId, variableNames, true));
        }

        public virtual IComment AddComment(string taskId, string processInstance, string message)
        {
            return ExecuteCommand(new AddCommentCmd(taskId, processInstance, message));
        }

        public virtual IComment AddComment(string taskId, string processInstance, string type, string message)
        {
            return ExecuteCommand(new AddCommentCmd(taskId, processInstance, type, message));
        }

        public virtual IComment GetComment(string commentId)
        {
            return ExecuteCommand(new GetCommentCmd(commentId));
        }

        public virtual IEvent GetEvent(string eventId)
        {
            return ExecuteCommand(new GetTaskEventCmd(eventId));
        }

        public virtual IList<IComment> GetTaskComments(string taskId)
        {
            return ExecuteCommand(new GetTaskCommentsCmd(taskId));
        }

        public virtual IList<IComment> GetTaskComments(string taskId, string type)
        {
            return ExecuteCommand(new GetTaskCommentsByTypeCmd(taskId, type));
        }

        public virtual IList<IComment> GetCommentsByType(string type)
        {
            return ExecuteCommand(new GetTypeCommentsCmd(type));
        }

        public virtual IList<IEvent> GetTaskEvents(string taskId)
        {
            return ExecuteCommand(new GetTaskEventsCmd(taskId));
        }

        public virtual IList<IComment> GetProcessInstanceComments(string processInstanceId)
        {
            return ExecuteCommand(new GetProcessInstanceCommentsCmd(processInstanceId));
        }

        public virtual IList<IComment> GetProcessInstanceComments(string processInstanceId, string type)
        {
            return ExecuteCommand(new GetProcessInstanceCommentsCmd(processInstanceId, type));
        }

        public virtual IAttachment CreateAttachment(string attachmentType, string taskId, string processInstanceId, string attachmentName, string attachmentDescription, System.IO.Stream content)
        {
            return ExecuteCommand(new CreateAttachmentCmd(attachmentType, taskId, processInstanceId, attachmentName, attachmentDescription, content, null));
        }

        public virtual IAttachment CreateAttachment(string attachmentType, string taskId, string processInstanceId, string attachmentName, string attachmentDescription, string url)
        {
            return ExecuteCommand(new CreateAttachmentCmd(attachmentType, taskId, processInstanceId, attachmentName, attachmentDescription, null, url));
        }

        public virtual Stream GetAttachmentContent(string attachmentId)
        {
            return ExecuteCommand(new GetAttachmentContentCmd(attachmentId));
        }

        public virtual void DeleteAttachment(string attachmentId)
        {
            ExecuteCommand(new DeleteAttachmentCmd(attachmentId));
        }

        public virtual void DeleteComments(string taskId, string processInstanceId)
        {
            ExecuteCommand(new DeleteCommentCmd(taskId, processInstanceId, null));
        }

        public virtual void DeleteComment(string commentId)
        {
            ExecuteCommand(new DeleteCommentCmd(null, null, commentId));
        }

        public virtual IAttachment GetAttachment(string attachmentId)
        {
            return ExecuteCommand(new GetAttachmentCmd(attachmentId));
        }

        public virtual IList<IAttachment> GetTaskAttachments(string taskId)
        {
            return (IList<IAttachment>)ExecuteCommand(new GetTaskAttachmentsCmd(taskId));
        }

        public virtual IList<IAttachment> GetProcessInstanceAttachments(string processInstanceId)
        {
            return (IList<IAttachment>)ExecuteCommand(new GetProcessInstanceAttachmentsCmd(processInstanceId));
        }

        public virtual void SaveAttachment(IAttachment attachment)
        {
            ExecuteCommand(new SaveAttachmentCmd(attachment));
        }

        public virtual IList<ITask> GetSubTasks(string parentTaskId)
        {
            return ExecuteCommand(new GetSubTasksCmd(parentTaskId));
        }

        public virtual IVariableInstance GetVariableInstance(string taskId, string variableName)
        {
            return ExecuteCommand(new GetTaskVariableInstanceCmd(taskId, variableName, false));
        }

        public virtual IVariableInstance GetVariableInstanceLocal(string taskId, string variableName)
        {
            return ExecuteCommand(new GetTaskVariableInstanceCmd(taskId, variableName, true));
        }

        public virtual IDictionary<string, IVariableInstance> GetVariableInstances(string taskId)
        {
            return ExecuteCommand(new GetTaskVariableInstancesCmd(taskId, null, false));
        }

        public virtual IDictionary<string, IVariableInstance> GetVariableInstances(string taskId, ICollection<string> variableNames)
        {
            return ExecuteCommand(new GetTaskVariableInstancesCmd(taskId, variableNames, false));
        }

        public virtual IDictionary<string, IVariableInstance> GetVariableInstancesLocal(string taskId)
        {
            return ExecuteCommand(new GetTaskVariableInstancesCmd(taskId, null, true));
        }

        public virtual IDictionary<string, IVariableInstance> GetVariableInstancesLocal(string taskId, ICollection<string> variableNames)
        {
            return ExecuteCommand(new GetTaskVariableInstancesCmd(taskId, variableNames, true));
        }

        public virtual IDictionary<string, IDataObject> GetDataObjects(string taskId)
        {
            return ExecuteCommand(new GetTaskDataObjectsCmd(taskId, null));
        }

        public virtual IDictionary<string, IDataObject> GetDataObjects(string taskId, string locale, bool withLocalizationFallback)
        {
            return ExecuteCommand(new GetTaskDataObjectsCmd(taskId, null, locale, withLocalizationFallback));
        }

        public virtual IDictionary<string, IDataObject> GetDataObjects(string taskId, ICollection<string> dataObjectNames)
        {
            return ExecuteCommand(new GetTaskDataObjectsCmd(taskId, dataObjectNames));
        }

        public virtual IDictionary<string, IDataObject> GetDataObjects(string taskId, ICollection<string> dataObjectNames, string locale, bool withLocalizationFallback)
        {
            return ExecuteCommand(new GetTaskDataObjectsCmd(taskId, dataObjectNames, locale, withLocalizationFallback));
        }

        public virtual IDataObject GetDataObject(string taskId, string dataObject)
        {
            return ExecuteCommand(new GetTaskDataObjectCmd(taskId, dataObject));
        }

        public virtual IDataObject GetDataObject(string taskId, string dataObjectName, string locale, bool withLocalizationFallback)
        {
            return ExecuteCommand(new GetTaskDataObjectCmd(taskId, dataObjectName, locale, withLocalizationFallback));
        }

        public ITask UpdateTask(IUpdateTaskCmd updateTaskCmd)
        {
            return ExecuteCommand(new UpdateTaskCmd(updateTaskCmd));
        }

        public ITask CreateNewSubtask(string taskName, string description, DateTime? dueDate, int? priority, string parentTaskId, string assignee, string tenantId)
        {
            var cmd = new CreateNewSubtaskCmd(taskName, description, dueDate, priority, parentTaskId, assignee, tenantId);

            return ExecuteCommand(cmd);
        }

        /// <inheritdoc />
        public ITask[] Transfer(ITransferTaskCmd cmd)
        {
            lock (syncRoot)
            {
                return ExecuteCommand(new TransferTaskCmd(cmd)) as ITask[];
            }
        }

        /// <inheritdoc />
        public ITask[] AddCountersign(string taskId, string[] assignees, string tenantId)
        {
            lock (syncRoot)
            {
                var cmd = new AddCountersignCmd(taskId, assignees, tenantId);

                return ExecuteCommand(cmd);
            }
        }

        /// <inheritdoc />
        public ITask CreateNewTask(string name, string description, DateTime? dueDate, int? priority, string parentTaskId, string assignee, string tenantId)
        {
            var cmd = new CreateNewTaskCmd(name, description, dueDate, priority, parentTaskId, assignee, tenantId);

            return ExecuteCommand(cmd);
        }

        public IList<ITask> GetMyTasks(string assignee)
        {
            var cmd = new GetMyTasksCmd(assignee);

            return ExecuteCommand(cmd);
        }

        public ITask[] ReassignTaskUsers(ReassignUser[] users)
        {
            var cmd = new ReassignTaskUsersCmd(users);

            return ExecuteCommand(cmd);
        }
    }
}