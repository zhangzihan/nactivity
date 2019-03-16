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

    using org.activiti.engine.impl.cfg;
    using org.activiti.engine.impl.cmd;
    using org.activiti.engine.impl.persistence.entity;
    using org.activiti.engine.runtime;
    using org.activiti.engine.task;

    /// 
    /// 
    public class TaskServiceImpl : ServiceImpl, ITaskService
    {

        public TaskServiceImpl()
        {

        }

        public TaskServiceImpl(ProcessEngineConfigurationImpl processEngineConfiguration) : base(processEngineConfiguration)
        {
        }

        public virtual ITask newTask()
        {
            return newTask(null);
        }

        public virtual ITask newTask(string taskId)
        {
            return commandExecutor.execute(new NewTaskCmd(taskId));
        }

        public virtual void saveTask(ITask task)
        {
            commandExecutor.execute(new SaveTaskCmd(task));
        }

        public virtual void terminateTask(string taskId, string terminateReason, bool terminateExecution)
        {
            commandExecutor.execute(new TerminateTaskCmd(taskId, terminateReason, terminateExecution));
        }

        public virtual void deleteTask(string taskId)
        {
            commandExecutor.execute(new DeleteTaskCmd(taskId, null, false));
        }

        public virtual void deleteTasks(ICollection<string> taskIds)
        {
            commandExecutor.execute(new DeleteTaskCmd(taskIds, null, false));
        }

        public virtual void deleteTask(string taskId, bool cascade)
        {
            commandExecutor.execute(new DeleteTaskCmd(taskId, null, cascade));
        }

        public virtual void deleteTasks(ICollection<string> taskIds, bool cascade)
        {
            commandExecutor.execute(new DeleteTaskCmd(taskIds, null, cascade));
        }

        public virtual void deleteTask(string taskId, string deleteReason)
        {
            commandExecutor.execute(new DeleteTaskCmd(taskId, deleteReason, false));
        }

        public virtual void deleteTasks(ICollection<string> taskIds, string deleteReason)
        {
            commandExecutor.execute(new DeleteTaskCmd(taskIds, deleteReason, false));
        }

        public virtual void setAssignee(string taskId, string userId)
        {
            commandExecutor.execute(new AddIdentityLinkCmd(taskId, userId, AddIdentityLinkCmd.IDENTITY_USER, IdentityLinkType.ASSIGNEE));
        }

        public virtual void setOwner(string taskId, string userId)
        {
            commandExecutor.execute(new AddIdentityLinkCmd(taskId, userId, AddIdentityLinkCmd.IDENTITY_USER, IdentityLinkType.OWNER));
        }

        public virtual void addCandidateUser(string taskId, string userId)
        {
            commandExecutor.execute(new AddIdentityLinkCmd(taskId, userId, AddIdentityLinkCmd.IDENTITY_USER, IdentityLinkType.CANDIDATE));
        }

        public virtual void addCandidateGroup(string taskId, string groupId)
        {
            commandExecutor.execute(new AddIdentityLinkCmd(taskId, groupId, AddIdentityLinkCmd.IDENTITY_GROUP, IdentityLinkType.CANDIDATE));
        }

        public virtual void addUserIdentityLink(string taskId, string userId, string identityLinkType)
        {
            commandExecutor.execute(new AddIdentityLinkCmd(taskId, userId, AddIdentityLinkCmd.IDENTITY_USER, identityLinkType));
        }

        public virtual void addGroupIdentityLink(string taskId, string groupId, string identityLinkType)
        {
            commandExecutor.execute(new AddIdentityLinkCmd(taskId, groupId, AddIdentityLinkCmd.IDENTITY_GROUP, identityLinkType));
        }

        public virtual void deleteCandidateGroup(string taskId, string groupId)
        {
            commandExecutor.execute(new DeleteIdentityLinkCmd(taskId, null, groupId, IdentityLinkType.CANDIDATE));
        }

        public virtual void deleteCandidateUser(string taskId, string userId)
        {
            commandExecutor.execute(new DeleteIdentityLinkCmd(taskId, userId, null, IdentityLinkType.CANDIDATE));
        }

        public virtual void deleteGroupIdentityLink(string taskId, string groupId, string identityLinkType)
        {
            commandExecutor.execute(new DeleteIdentityLinkCmd(taskId, null, groupId, identityLinkType));
        }

        public virtual void deleteUserIdentityLink(string taskId, string userId, string identityLinkType)
        {
            commandExecutor.execute(new DeleteIdentityLinkCmd(taskId, userId, null, identityLinkType));
        }

        public virtual IList<IIdentityLink> getIdentityLinksForTask(string taskId)
        {
            return commandExecutor.execute(new GetIdentityLinksForTaskCmd(taskId));
        }

        public virtual void claim(string taskId, string userId)
        {
            commandExecutor.execute(new ClaimTaskCmd(taskId, userId));
        }

        public virtual void unclaim(string taskId)
        {
            commandExecutor.execute(new ClaimTaskCmd(taskId, null));
        }

        public virtual void complete(string taskId)
        {
            commandExecutor.execute(new CompleteTaskCmd(taskId, null));
        }

        public virtual void complete(string taskId, IDictionary<string, object> variables)
        {
            commandExecutor.execute(new CompleteTaskCmd(taskId, variables));
        }

        public virtual void complete(string taskId, IDictionary<string, object> variables, IDictionary<string, object> transientVariables)
        {
            commandExecutor.execute(new CompleteTaskCmd(taskId, variables, transientVariables));
        }

        public virtual void complete(string taskId, IDictionary<string, object> variables, bool localScope)
        {
            commandExecutor.execute(new CompleteTaskCmd(taskId, variables, localScope));
        }

        public virtual void delegateTask(string taskId, string userId)
        {
            commandExecutor.execute(new DelegateTaskCmd(taskId, userId));
        }

        public virtual void resolveTask(string taskId)
        {
            commandExecutor.execute(new ResolveTaskCmd(taskId, null));
        }

        public virtual void resolveTask(string taskId, IDictionary<string, object> variables)
        {
            commandExecutor.execute(new ResolveTaskCmd(taskId, variables));
        }

        public virtual void resolveTask(string taskId, IDictionary<string, object> variables, IDictionary<string, object> transientVariables)
        {
            commandExecutor.execute(new ResolveTaskCmd(taskId, variables, transientVariables));
        }

        public virtual void setPriority(string taskId, int priority)
        {
            commandExecutor.execute(new SetTaskPriorityCmd(taskId, priority));
        }

        public virtual void setDueDate(string taskId, DateTime dueDate)
        {
            commandExecutor.execute(new SetTaskDueDateCmd(taskId, dueDate));
        }

        public virtual ITaskQuery createTaskQuery()
        {
            return new TaskQueryImpl(commandExecutor, processEngineConfiguration.DatabaseType);
        }

        public virtual INativeTaskQuery createNativeTaskQuery()
        {
            return new NativeTaskQueryImpl(commandExecutor);
        }

        public virtual IDictionary<string, object> getVariables(string taskId)
        {
            return commandExecutor.execute(new GetTaskVariablesCmd(taskId, null, false));
        }

        public virtual IDictionary<string, object> getVariablesLocal(string taskId)
        {
            return commandExecutor.execute(new GetTaskVariablesCmd(taskId, null, true));
        }

        public virtual IDictionary<string, object> getVariables(string taskId, ICollection<string> variableNames)
        {
            return commandExecutor.execute(new GetTaskVariablesCmd(taskId, variableNames, false));
        }

        public virtual IDictionary<string, object> getVariablesLocal(string taskId, ICollection<string> variableNames)
        {
            return commandExecutor.execute(new GetTaskVariablesCmd(taskId, variableNames, true));
        }

        public virtual object getVariable(string taskId, string variableName)
        {
            return commandExecutor.execute(new GetTaskVariableCmd(taskId, variableName, false));
        }

        public virtual T getVariable<T>(string taskId, string variableName)
        {
            return (T)getVariable(taskId, variableName);
        }

        public virtual bool hasVariable(string taskId, string variableName)
        {
            return commandExecutor.execute(new HasTaskVariableCmd(taskId, variableName, false));
        }

        public virtual object getVariableLocal(string taskId, string variableName)
        {
            return commandExecutor.execute(new GetTaskVariableCmd(taskId, variableName, true));
        }

        public virtual T getVariableLocal<T>(string taskId, string variableName)
        {
            return (T)getVariableLocal(taskId, variableName);
        }

        public virtual IList<IVariableInstance> getVariableInstancesLocalByTaskIds(ISet<string> taskIds)
        {
            return commandExecutor.execute(new GetTasksLocalVariablesCmd(taskIds));
        }

        public virtual bool hasVariableLocal(string taskId, string variableName)
        {
            return commandExecutor.execute(new HasTaskVariableCmd(taskId, variableName, true));
        }

        public virtual void setVariable(string taskId, string variableName, object value)
        {
            if (ReferenceEquals(variableName, null))
            {
                throw new ActivitiIllegalArgumentException("variableName is null");
            }
            IDictionary<string, object> variables = new Dictionary<string, object>();
            variables[variableName] = value;
            commandExecutor.execute(new SetTaskVariablesCmd<object>(taskId, variables, false));
        }

        public virtual void setVariableLocal(string taskId, string variableName, object value)
        {
            if (ReferenceEquals(variableName, null))
            {
                throw new ActivitiIllegalArgumentException("variableName is null");
            }
            IDictionary<string, object> variables = new Dictionary<string, object>();
            variables[variableName] = value;
            commandExecutor.execute(new SetTaskVariablesCmd<object>(taskId, variables, true));
        }

        public virtual void setVariables<T1>(string taskId, IDictionary<string, T1> variables)
        {
            commandExecutor.execute(new SetTaskVariablesCmd<T1>(taskId, variables, false));
        }

        public virtual void setVariablesLocal<T1>(string taskId, IDictionary<string, T1> variables)
        {
            commandExecutor.execute(new SetTaskVariablesCmd<T1>(taskId, variables, true));
        }

        public virtual void removeVariable(string taskId, string variableName)
        {
            ICollection<string> variableNames = new List<string>();
            variableNames.Add(variableName);
            commandExecutor.execute(new RemoveTaskVariablesCmd(taskId, variableNames, false));
        }

        public virtual void removeVariableLocal(string taskId, string variableName)
        {
            ICollection<string> variableNames = new List<string>(1);
            variableNames.Add(variableName);
            commandExecutor.execute(new RemoveTaskVariablesCmd(taskId, variableNames, true));
        }

        public virtual void removeVariables(string taskId, ICollection<string> variableNames)
        {
            commandExecutor.execute(new RemoveTaskVariablesCmd(taskId, variableNames, false));
        }

        public virtual void removeVariablesLocal(string taskId, ICollection<string> variableNames)
        {
            commandExecutor.execute(new RemoveTaskVariablesCmd(taskId, variableNames, true));
        }

        public virtual IComment addComment(string taskId, string processInstance, string message)
        {
            return commandExecutor.execute(new AddCommentCmd(taskId, processInstance, message));
        }

        public virtual IComment addComment(string taskId, string processInstance, string type, string message)
        {
            return commandExecutor.execute(new AddCommentCmd(taskId, processInstance, type, message));
        }

        public virtual IComment getComment(string commentId)
        {
            return commandExecutor.execute(new GetCommentCmd(commentId));
        }

        public virtual IEvent getEvent(string eventId)
        {
            return commandExecutor.execute(new GetTaskEventCmd(eventId));
        }

        public virtual IList<IComment> getTaskComments(string taskId)
        {
            return commandExecutor.execute(new GetTaskCommentsCmd(taskId));
        }

        public virtual IList<IComment> getTaskComments(string taskId, string type)
        {
            return commandExecutor.execute(new GetTaskCommentsByTypeCmd(taskId, type));
        }

        public virtual IList<IComment> getCommentsByType(string type)
        {
            return commandExecutor.execute(new GetTypeCommentsCmd(type));
        }

        public virtual IList<IEvent> getTaskEvents(string taskId)
        {
            return commandExecutor.execute(new GetTaskEventsCmd(taskId));
        }

        public virtual IList<IComment> getProcessInstanceComments(string processInstanceId)
        {
            return commandExecutor.execute(new GetProcessInstanceCommentsCmd(processInstanceId));
        }

        public virtual IList<IComment> getProcessInstanceComments(string processInstanceId, string type)
        {
            return commandExecutor.execute(new GetProcessInstanceCommentsCmd(processInstanceId, type));
        }

        public virtual IAttachment createAttachment(string attachmentType, string taskId, string processInstanceId, string attachmentName, string attachmentDescription, System.IO.Stream content)
        {
            return commandExecutor.execute(new CreateAttachmentCmd(attachmentType, taskId, processInstanceId, attachmentName, attachmentDescription, content, null));
        }

        public virtual IAttachment createAttachment(string attachmentType, string taskId, string processInstanceId, string attachmentName, string attachmentDescription, string url)
        {
            return commandExecutor.execute(new CreateAttachmentCmd(attachmentType, taskId, processInstanceId, attachmentName, attachmentDescription, null, url));
        }

        public virtual System.IO.Stream getAttachmentContent(string attachmentId)
        {
            return commandExecutor.execute(new GetAttachmentContentCmd(attachmentId));
        }

        public virtual void deleteAttachment(string attachmentId)
        {
            commandExecutor.execute(new DeleteAttachmentCmd(attachmentId));
        }

        public virtual void deleteComments(string taskId, string processInstanceId)
        {
            commandExecutor.execute(new DeleteCommentCmd(taskId, processInstanceId, null));
        }

        public virtual void deleteComment(string commentId)
        {
            commandExecutor.execute(new DeleteCommentCmd(null, null, commentId));
        }

        public virtual IAttachment getAttachment(string attachmentId)
        {
            return commandExecutor.execute(new GetAttachmentCmd(attachmentId));
        }

        public virtual IList<IAttachment> getTaskAttachments(string taskId)
        {
            return (IList<IAttachment>)commandExecutor.execute(new GetTaskAttachmentsCmd(taskId));
        }

        public virtual IList<IAttachment> getProcessInstanceAttachments(string processInstanceId)
        {
            return (IList<IAttachment>)commandExecutor.execute(new GetProcessInstanceAttachmentsCmd(processInstanceId));
        }

        public virtual void saveAttachment(IAttachment attachment)
        {
            commandExecutor.execute(new SaveAttachmentCmd(attachment));
        }

        public virtual IList<ITask> getSubTasks(string parentTaskId)
        {
            return commandExecutor.execute(new GetSubTasksCmd(parentTaskId));
        }

        public virtual IVariableInstance getVariableInstance(string taskId, string variableName)
        {
            return commandExecutor.execute(new GetTaskVariableInstanceCmd(taskId, variableName, false));
        }

        public virtual IVariableInstance getVariableInstanceLocal(string taskId, string variableName)
        {
            return commandExecutor.execute(new GetTaskVariableInstanceCmd(taskId, variableName, true));
        }

        public virtual IDictionary<string, IVariableInstance> getVariableInstances(string taskId)
        {
            return commandExecutor.execute(new GetTaskVariableInstancesCmd(taskId, null, false));
        }

        public virtual IDictionary<string, IVariableInstance> getVariableInstances(string taskId, ICollection<string> variableNames)
        {
            return commandExecutor.execute(new GetTaskVariableInstancesCmd(taskId, variableNames, false));
        }

        public virtual IDictionary<string, IVariableInstance> getVariableInstancesLocal(string taskId)
        {
            return commandExecutor.execute(new GetTaskVariableInstancesCmd(taskId, null, true));
        }

        public virtual IDictionary<string, IVariableInstance> getVariableInstancesLocal(string taskId, ICollection<string> variableNames)
        {
            return commandExecutor.execute(new GetTaskVariableInstancesCmd(taskId, variableNames, true));
        }

        public virtual IDictionary<string, IDataObject> getDataObjects(string taskId)
        {
            return commandExecutor.execute(new GetTaskDataObjectsCmd(taskId, null));
        }

        public virtual IDictionary<string, IDataObject> getDataObjects(string taskId, string locale, bool withLocalizationFallback)
        {
            return commandExecutor.execute(new GetTaskDataObjectsCmd(taskId, null, locale, withLocalizationFallback));
        }

        public virtual IDictionary<string, IDataObject> getDataObjects(string taskId, ICollection<string> dataObjectNames)
        {
            return commandExecutor.execute(new GetTaskDataObjectsCmd(taskId, dataObjectNames));
        }

        public virtual IDictionary<string, IDataObject> getDataObjects(string taskId, ICollection<string> dataObjectNames, string locale, bool withLocalizationFallback)
        {
            return commandExecutor.execute(new GetTaskDataObjectsCmd(taskId, dataObjectNames, locale, withLocalizationFallback));
        }

        public virtual IDataObject getDataObject(string taskId, string dataObject)
        {
            return commandExecutor.execute(new GetTaskDataObjectCmd(taskId, dataObject));
        }

        public virtual IDataObject getDataObject(string taskId, string dataObjectName, string locale, bool withLocalizationFallback)
        {
            return commandExecutor.execute(new GetTaskDataObjectCmd(taskId, dataObjectName, locale, withLocalizationFallback));
        }

    }

}