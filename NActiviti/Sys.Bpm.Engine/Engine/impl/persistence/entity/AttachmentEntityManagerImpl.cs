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

namespace Sys.Workflow.engine.impl.persistence.entity
{

    using Sys.Workflow.engine.@delegate.@event;
    using Sys.Workflow.engine.@delegate.@event.impl;
    using Sys.Workflow.engine.impl.cfg;
    using Sys.Workflow.engine.impl.persistence.entity.data;
    using Sys.Workflow.engine.task;

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

        public virtual IList<IAttachmentEntity> FindAttachmentsByProcessInstanceId(string processInstanceId)
        {
            CheckHistoryEnabled();
            return attachmentDataManager.FindAttachmentsByProcessInstanceId(processInstanceId);
        }

        public virtual IList<IAttachmentEntity> FindAttachmentsByTaskId(string taskId)
        {
            CheckHistoryEnabled();
            return attachmentDataManager.FindAttachmentsByTaskId(taskId);
        }

        public virtual void DeleteAttachmentsByTaskId(string taskId)
        {
            CheckHistoryEnabled();
            IList<IAttachmentEntity> attachments = FindAttachmentsByTaskId(taskId);
            bool dispatchEvents = EventDispatcher.Enabled;

            string processInstanceId = null;
            string processDefinitionId = null;
            string executionId = null;

            if (dispatchEvents && attachments != null && attachments.Count > 0)
            {
                // Forced to fetch the task to get hold of the process definition
                // for event-dispatching, if available
                ITask task = TaskEntityManager.FindById<ITask>(new KeyValuePair<string, object>("id", taskId));
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
                if (!(contentId is null))
                {
                    ByteArrayEntityManager.DeleteByteArrayById(contentId);
                }

                attachmentDataManager.Delete((IAttachmentEntity)attachment);

                if (dispatchEvents)
                {
                    EventDispatcher.DispatchEvent(ActivitiEventBuilder.CreateEntityEvent(ActivitiEventType.ENTITY_DELETED, attachment, executionId, processInstanceId, processDefinitionId));
                }
            }
        }

        protected internal virtual void CheckHistoryEnabled()
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