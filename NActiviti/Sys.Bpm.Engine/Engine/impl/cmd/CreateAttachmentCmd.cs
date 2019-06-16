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

namespace Sys.Workflow.engine.impl.cmd
{


    using Sys.Workflow.engine.@delegate.@event;
    using Sys.Workflow.engine.@delegate.@event.impl;
    using Sys.Workflow.engine.impl.identity;
    using Sys.Workflow.engine.impl.interceptor;
    using Sys.Workflow.engine.impl.persistence.entity;
    using Sys.Workflow.engine.impl.util;
    using Sys.Workflow.engine.runtime;
    using Sys.Workflow.engine.task;
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

        public virtual IAttachment Execute(ICommandContext commandContext)
        {
            if (!(processInstanceId is null))
            {
                VerifyExecutionParameters(commandContext);
            }

            IAttachmentEntity attachment = commandContext.AttachmentEntityManager.Create();
            attachment.Name = attachmentName;
            attachment.ProcessInstanceId = processInstanceId;
            attachment.TaskId = taskId;
            attachment.Description = attachmentDescription;
            attachment.Type = attachmentType;
            attachment.Url = url;
            attachment.UserId = Authentication.AuthenticatedUser.Id;
            attachment.Time = commandContext.ProcessEngineConfiguration.Clock.CurrentTime;

            commandContext.AttachmentEntityManager.Insert(attachment, false);

            if (content != null)
            {
                byte[] bytes = IoUtil.ReadInputStream(content, attachmentName);
                IByteArrayEntity byteArray = commandContext.ByteArrayEntityManager.Create();
                byteArray.Bytes = bytes;
                commandContext.ByteArrayEntityManager.Insert(byteArray);
                attachment.ContentId = byteArray.Id;
                attachment.Content = byteArray;
            }

            commandContext.HistoryManager.CreateAttachmentComment(taskId, processInstanceId, attachmentName, true);

            if (commandContext.ProcessEngineConfiguration.EventDispatcher.Enabled)
            {
                // Forced to fetch the process-instance to associate the right
                // process definition
                string processDefinitionId = null;
                if (!(attachment.ProcessInstanceId is null))
                {
                    IExecutionEntity process = commandContext.ExecutionEntityManager.FindById<IExecutionEntity>(processInstanceId);
                    if (process != null)
                    {
                        processDefinitionId = process.ProcessDefinitionId;
                    }
                }

                commandContext.ProcessEngineConfiguration.EventDispatcher.DispatchEvent(ActivitiEventBuilder.CreateEntityEvent(ActivitiEventType.ENTITY_CREATED, attachment, processInstanceId, processInstanceId, processDefinitionId));
                commandContext.ProcessEngineConfiguration.EventDispatcher.DispatchEvent(ActivitiEventBuilder.CreateEntityEvent(ActivitiEventType.ENTITY_INITIALIZED, attachment, processInstanceId, processInstanceId, processDefinitionId));
            }

            return attachment;
        }

        protected internal virtual ITaskEntity VerifyTaskParameters(ICommandContext commandContext)
        {
            ITaskEntity task = commandContext.TaskEntityManager.FindById<ITaskEntity>(new KeyValuePair<string, object>("id", taskId));

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

        protected internal virtual IExecutionEntity VerifyExecutionParameters(ICommandContext commandContext)
        {
            IExecutionEntity execution = commandContext.ExecutionEntityManager.FindById<IExecutionEntity>(processInstanceId);

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