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
    public class CommentEntityManagerImpl : AbstractEntityManager<ICommentEntity>, ICommentEntityManager
    {

        protected internal ICommentDataManager commentDataManager;

        public CommentEntityManagerImpl(ProcessEngineConfigurationImpl processEngineConfiguration, ICommentDataManager commentDataManager) : base(processEngineConfiguration)
        {
            this.commentDataManager = commentDataManager;
        }

        protected internal override IDataManager<ICommentEntity> DataManager
        {
            get
            {
                return commentDataManager;
            }
        }

        public override void insert(ICommentEntity commentEntity)
        {
            checkHistoryEnabled();

            insert(commentEntity, false);

            IComment comment = (IComment)commentEntity;
            if (EventDispatcher.Enabled)
            {
                // Forced to fetch the process-instance to associate the right
                // process definition
                string processDefinitionId = null;
                string processInstanceId = comment.ProcessInstanceId;
                if (!ReferenceEquals(comment.ProcessInstanceId, null))
                {
                    IExecutionEntity process = ExecutionEntityManager.findById<IExecutionEntity>(new KeyValuePair<string, object>("id", comment.ProcessInstanceId));
                    if (process != null)
                    {
                        processDefinitionId = process.ProcessDefinitionId;
                    }
                }
                EventDispatcher.dispatchEvent(ActivitiEventBuilder.createEntityEvent(ActivitiEventType.ENTITY_CREATED, commentEntity, processInstanceId, processInstanceId, processDefinitionId));
                EventDispatcher.dispatchEvent(ActivitiEventBuilder.createEntityEvent(ActivitiEventType.ENTITY_INITIALIZED, commentEntity, processInstanceId, processInstanceId, processDefinitionId));
            }
        }

        public virtual IList<IComment> findCommentsByTaskId(string taskId)
        {
            checkHistoryEnabled();
            return commentDataManager.findCommentsByTaskId(taskId);
        }

        public virtual IList<IComment> findCommentsByTaskIdAndType(string taskId, string type)
        {
            checkHistoryEnabled();
            return commentDataManager.findCommentsByTaskIdAndType(taskId, type);
        }

        public virtual IList<IComment> findCommentsByType(string type)
        {
            checkHistoryEnabled();
            return commentDataManager.findCommentsByType(type);
        }

        public virtual IList<IEvent> findEventsByTaskId(string taskId)
        {
            checkHistoryEnabled();
            return commentDataManager.findEventsByTaskId(taskId);
        }

        public virtual IList<IEvent> findEventsByProcessInstanceId(string processInstanceId)
        {
            checkHistoryEnabled();
            return commentDataManager.findEventsByProcessInstanceId(processInstanceId);
        }

        public virtual void deleteCommentsByTaskId(string taskId)
        {
            checkHistoryEnabled();
            commentDataManager.deleteCommentsByTaskId(taskId);
        }

        public virtual void deleteCommentsByProcessInstanceId(string processInstanceId)
        {
            checkHistoryEnabled();
            commentDataManager.deleteCommentsByProcessInstanceId(processInstanceId);
        }

        public virtual IList<IComment> findCommentsByProcessInstanceId(string processInstanceId)
        {
            checkHistoryEnabled();
            return commentDataManager.findCommentsByProcessInstanceId(processInstanceId);
        }

        public virtual IList<IComment> findCommentsByProcessInstanceId(string processInstanceId, string type)
        {
            checkHistoryEnabled();
            return commentDataManager.findCommentsByProcessInstanceId(processInstanceId, type);
        }

        public virtual IComment findComment(string commentId)
        {
            return commentDataManager.findComment(commentId);
        }

        public virtual IEvent findEvent(string commentId)
        {
            return commentDataManager.findEvent(commentId);
        }

        public override void delete(ICommentEntity commentEntity)
        {
            checkHistoryEnabled();

            delete(commentEntity, false);

            IComment comment = (IComment)commentEntity;
            if (EventDispatcher.Enabled)
            {
                // Forced to fetch the process-instance to associate the right
                // process definition
                string processDefinitionId = null;
                string processInstanceId = comment.ProcessInstanceId;
                if (!ReferenceEquals(comment.ProcessInstanceId, null))
                {
                    IExecutionEntity process = ExecutionEntityManager.findById<IExecutionEntity>(new KeyValuePair<string, object>("id", comment.ProcessInstanceId));
                    if (process != null)
                    {
                        processDefinitionId = process.ProcessDefinitionId;
                    }
                }
                EventDispatcher.dispatchEvent(ActivitiEventBuilder.createEntityEvent(ActivitiEventType.ENTITY_DELETED, commentEntity, processInstanceId, processInstanceId, processDefinitionId));
            }
        }

        protected internal virtual void checkHistoryEnabled()
        {
            if (!HistoryManager.HistoryEnabled)
            {
                throw new ActivitiException("In order to use comments, history should be enabled");
            }
        }

        public virtual ICommentDataManager CommentDataManager
        {
            get
            {
                return commentDataManager;
            }
            set
            {
                this.commentDataManager = value;
            }
        }


    }

}