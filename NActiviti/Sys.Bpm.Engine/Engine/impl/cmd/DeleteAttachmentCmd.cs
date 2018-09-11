using System;

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
    using org.activiti.engine.impl.interceptor;
    using org.activiti.engine.impl.persistence.entity;
    using org.activiti.engine.impl.util;
    using System.Collections.Generic;

    /// 
    /// 
    [Serializable]
    public class DeleteAttachmentCmd : ICommand<object>
    {

        private const long serialVersionUID = 1L;
        protected internal string attachmentId;

        public DeleteAttachmentCmd(string attachmentId)
        {
            this.attachmentId = attachmentId;
        }

        public virtual object execute(ICommandContext commandContext)
        {
            IAttachmentEntity attachment = commandContext.AttachmentEntityManager.findById<IAttachmentEntity>(new KeyValuePair<string, object>("id", attachmentId));

            string processInstanceId = attachment.ProcessInstanceId;
            string processDefinitionId = null;
            if (!string.ReferenceEquals(attachment.ProcessInstanceId, null))
            {
                IExecutionEntity process = commandContext.ExecutionEntityManager.findById<IExecutionEntity>(new KeyValuePair<string, object>("id", processInstanceId));
                if (process != null)
                {
                    processDefinitionId = process.ProcessDefinitionId;
                }
            }

            commandContext.AttachmentEntityManager.delete(attachment, false);

            if (!string.ReferenceEquals(attachment.ContentId, null))
            {
                commandContext.ByteArrayEntityManager.deleteByteArrayById(attachment.ContentId);
            }

            if (!string.ReferenceEquals(attachment.TaskId, null))
            {
                commandContext.HistoryManager.createAttachmentComment(attachment.TaskId, attachment.ProcessInstanceId, attachment.Name, false);
            }

            if (commandContext.ProcessEngineConfiguration.EventDispatcher.Enabled)
            {
                commandContext.ProcessEngineConfiguration.EventDispatcher.dispatchEvent(ActivitiEventBuilder.createEntityEvent(ActivitiEventType.ENTITY_DELETED, attachment, processInstanceId, processInstanceId, processDefinitionId));
            }
            return null;
        }

    }

}