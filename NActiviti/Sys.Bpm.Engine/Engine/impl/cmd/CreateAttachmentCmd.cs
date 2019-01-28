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

namespace org.activiti.engine.impl.cmd
{

    
    using org.activiti.engine.@delegate.@event;
    using org.activiti.engine.@delegate.@event.impl;
    using org.activiti.engine.impl.identity;
    using org.activiti.engine.impl.interceptor;
    using org.activiti.engine.impl.persistence.entity;
    using org.activiti.engine.impl.util;
    using org.activiti.engine.runtime;
    using org.activiti.engine.task;
    using System.Collections.Generic;

    /// 
    /// 
    // Not Serializable
    public class CreateAttachmentCmd : ICommand<IAttachment>
    {

        protected internal string attachmentType;
        protected internal string taskId;
        protected internal string processInstanceId;
        protected internal string attachmentName;
        protected internal string attachmentDescription;
        protected internal System.IO.Stream content;
        protected internal string url;

        public CreateAttachmentCmd(string attachmentType, string taskId, string processInstanceId, string attachmentName, string attachmentDescription, System.IO.Stream content, string url)
        {
            this.attachmentType = attachmentType;
            this.taskId = taskId;
            this.processInstanceId = processInstanceId;
            this.attachmentName = attachmentName;
            this.attachmentDescription = attachmentDescription;
            this.content = content;
            this.url = url;
        }

        public virtual IAttachment execute(ICommandContext commandContext)
        {
            if (!ReferenceEquals(processInstanceId, null))
            {
                IExecutionEntity execution = verifyExecutionParameters(commandContext);
            }

            IAttachmentEntity attachment = commandContext.AttachmentEntityManager.create();
            attachment.Name = attachmentName;
            attachment.ProcessInstanceId = processInstanceId;
            attachment.TaskId = taskId;
            attachment.Description = attachmentDescription;
            attachment.Type = attachmentType;
            attachment.Url = url;
            attachment.UserId = Authentication.AuthenticatedUserId;
            attachment.Time = commandContext.ProcessEngineConfiguration.Clock.CurrentTime;

            commandContext.AttachmentEntityManager.insert(attachment, false);

            if (content != null)
            {
                byte[] bytes = IoUtil.readInputStream(content, attachmentName);
                IByteArrayEntity byteArray = commandContext.ByteArrayEntityManager.create();
                byteArray.Bytes = bytes;
                commandContext.ByteArrayEntityManager.insert(byteArray);
                attachment.ContentId = byteArray.Id;
                attachment.Content = byteArray;
            }

            commandContext.HistoryManager.createAttachmentComment(taskId, processInstanceId, attachmentName, true);

            if (commandContext.ProcessEngineConfiguration.EventDispatcher.Enabled)
            {
                // Forced to fetch the process-instance to associate the right
                // process definition
                string processDefinitionId = null;
                if (!ReferenceEquals(attachment.ProcessInstanceId, null))
                {
                    IExecutionEntity process = commandContext.ExecutionEntityManager.findById<IExecutionEntity>(new KeyValuePair<string, object>("id", processInstanceId));
                    if (process != null)
                    {
                        processDefinitionId = process.ProcessDefinitionId;
                    }
                }

                commandContext.ProcessEngineConfiguration.EventDispatcher.dispatchEvent(ActivitiEventBuilder.createEntityEvent(ActivitiEventType.ENTITY_CREATED, attachment, processInstanceId, processInstanceId, processDefinitionId));
                commandContext.ProcessEngineConfiguration.EventDispatcher.dispatchEvent(ActivitiEventBuilder.createEntityEvent(ActivitiEventType.ENTITY_INITIALIZED, attachment, processInstanceId, processInstanceId, processDefinitionId));
            }

            return attachment;
        }

        protected internal virtual ITaskEntity verifyTaskParameters(ICommandContext commandContext)
        {
            ITaskEntity task = commandContext.TaskEntityManager.findById<ITaskEntity>(new KeyValuePair<string, object>("id", taskId));

            if (task == null)
            {
                throw new ActivitiObjectNotFoundException("Cannot find task with id " + taskId, typeof(ITask));
            }

            if (task.Suspended)
            {
                throw new ActivitiException("It is not allowed to add an attachment to a suspended task");
            }

            return task;
        }

        protected internal virtual IExecutionEntity verifyExecutionParameters(ICommandContext commandContext)
        {
            IExecutionEntity execution = commandContext.ExecutionEntityManager.findById<IExecutionEntity>(new KeyValuePair<string, object>("id", processInstanceId));

            if (execution == null)
            {
                throw new ActivitiObjectNotFoundException("Process instance " + processInstanceId + " doesn't exist", typeof(IProcessInstance));
            }

            if (execution.Suspended)
            {
                throw new ActivitiException("It is not allowed to add an attachment to a suspended process instance");
            }

            return execution;
        }

    }

}