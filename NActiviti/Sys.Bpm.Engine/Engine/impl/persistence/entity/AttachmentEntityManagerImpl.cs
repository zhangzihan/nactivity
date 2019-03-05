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

namespace org.activiti.engine.impl.persistence.entity
{

    using org.activiti.engine.@delegate.@event;
    using org.activiti.engine.@delegate.@event.impl;
    using org.activiti.engine.impl.cfg;
    using org.activiti.engine.impl.persistence.entity.data;
    using org.activiti.engine.task;

    /// 
    /// 
    public class AttachmentEntityManagerImpl : AbstractEntityManager<IAttachmentEntity>, IAttachmentEntityManager
    {

        protected internal IAttachmentDataManager attachmentDataManager;

        public AttachmentEntityManagerImpl(ProcessEngineConfigurationImpl processEngineConfiguration, IAttachmentDataManager attachmentDataManager) : base(processEngineConfiguration)
        {
            this.attachmentDataManager = attachmentDataManager;
        }

        protected internal override IDataManager<IAttachmentEntity> DataManager
        {
            get
            {
                return attachmentDataManager;
            }
        }

        public virtual IList<IAttachmentEntity> findAttachmentsByProcessInstanceId(string processInstanceId)
        {
            checkHistoryEnabled();
            return attachmentDataManager.findAttachmentsByProcessInstanceId(processInstanceId);
        }

        public virtual IList<IAttachmentEntity> findAttachmentsByTaskId(string taskId)
        {
            checkHistoryEnabled();
            return attachmentDataManager.findAttachmentsByTaskId(taskId);
        }

        public virtual void deleteAttachmentsByTaskId(string taskId)
        {
            checkHistoryEnabled();
            IList<IAttachmentEntity> attachments = findAttachmentsByTaskId(taskId);
            bool dispatchEvents = EventDispatcher.Enabled;

            string processInstanceId = null;
            string processDefinitionId = null;
            string executionId = null;

            if (dispatchEvents && attachments != null && attachments.Count > 0)
            {
                // Forced to fetch the task to get hold of the process definition
                // for event-dispatching, if available
                ITask task = TaskEntityManager.findById<ITask>(new KeyValuePair<string, object>("id", taskId));
                if (task != null)
                {
                    processDefinitionId = task.ProcessDefinitionId;
                    processInstanceId = task.ProcessInstanceId;
                    executionId = task.ExecutionId;
                }
            }

            foreach (IAttachment attachment in attachments)
            {
                string contentId = attachment.ContentId;
                if (!ReferenceEquals(contentId, null))
                {
                    ByteArrayEntityManager.deleteByteArrayById(contentId);
                }

                attachmentDataManager.delete((IAttachmentEntity)attachment);

                if (dispatchEvents)
                {
                    EventDispatcher.dispatchEvent(ActivitiEventBuilder.createEntityEvent(ActivitiEventType.ENTITY_DELETED, attachment, executionId, processInstanceId, processDefinitionId));
                }
            }
        }

        protected internal virtual void checkHistoryEnabled()
        {
            if (!HistoryManager.HistoryEnabled)
            {
                throw new ActivitiException("In order to use attachments, history should be enabled");
            }
        }

        public virtual IAttachmentDataManager AttachmentDataManager
        {
            get
            {
                return attachmentDataManager;
            }
            set
            {
                this.attachmentDataManager = value;
            }
        }


    }

}