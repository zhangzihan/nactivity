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
    using org.activiti.engine.task;
    using System.Collections.Generic;

    /// 
    [Serializable]
    public class SaveAttachmentCmd : ICommand<object>
    {

        private const long serialVersionUID = 1L;
        protected internal IAttachment attachment;

        public SaveAttachmentCmd(IAttachment attachment)
        {
            this.attachment = attachment;
        }

        public  virtual object  execute(ICommandContext commandContext)
        {
            IAttachmentEntity updateAttachment = commandContext.AttachmentEntityManager.findById<IAttachmentEntity>(new KeyValuePair<string, object>("id", attachment.Id));

            string processInstanceId = updateAttachment.ProcessInstanceId;
            string processDefinitionId = null;
            if (!ReferenceEquals(updateAttachment.ProcessInstanceId, null))
            {
                IExecutionEntity process = commandContext.ExecutionEntityManager.findById<IExecutionEntity>(new KeyValuePair<string, object>("id", processInstanceId));
                if (process != null)
                {
                    processDefinitionId = process.ProcessDefinitionId;
                }
            }

            updateAttachment.Name = attachment.Name;
            updateAttachment.Description = attachment.Description;

            if (commandContext.ProcessEngineConfiguration.EventDispatcher.Enabled)
            {
                commandContext.ProcessEngineConfiguration.EventDispatcher.dispatchEvent(ActivitiEventBuilder.createEntityEvent(ActivitiEventType.ENTITY_UPDATED, attachment, processInstanceId, processInstanceId, processDefinitionId));
            }

            return null;
        }
    }

}