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

namespace Sys.Workflow.Engine.Impl.Persistence.Entity
{

    using Sys.Workflow.Engine.Delegate.Events;
    using Sys.Workflow.Engine.Delegate.Events.Impl;
    using Sys.Workflow.Engine.Impl.Cfg;
    using Sys.Workflow.Engine.Impl.Persistence.Entity.Data;
    using Sys.Workflow.Engine.Tasks;

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

        public override void Insert(ICommentEntity commentEntity)
        {
            CheckHistoryEnabled();

            Insert(commentEntity, false);

            IComment comment = commentEntity;
            if (EventDispatcher.Enabled)
            {
                // Forced to fetch the process-instance to associate the right
                // process definition
                string processDefinitionId = null;
                string processInstanceId = comment.ProcessInstanceId;
                if (!(comment.ProcessInstanceId is null))
                {
                    IExecutionEntity process = ExecutionEntityManager.FindById<IExecutionEntity>(comment.ProcessInstanceId);
                    if (process != null)
                    {
                        processDefinitionId = process.ProcessDefinitionId;
                    }
                }
                EventDispatcher.DispatchEvent(ActivitiEventBuilder.CreateEntityEvent(ActivitiEventType.ENTITY_CREATED, commentEntity, processInstanceId, processInstanceId, processDefinitionId));
                EventDispatcher.DispatchEvent(ActivitiEventBuilder.CreateEntityEvent(ActivitiEventType.ENTITY_INITIALIZED, commentEntity, processInstanceId, processInstanceId, processDefinitionId));
            }
        }

        public virtual IList<IComment> FindCommentsByTaskId(string taskId)
        {
            CheckHistoryEnabled();
            return commentDataManager.FindCommentsByTaskId(taskId);
        }

        public virtual IList<IComment> FindCommentsByTaskIdAndType(string taskId, string type)
        {
            CheckHistoryEnabled();
            return commentDataManager.FindCommentsByTaskIdAndType(taskId, type);
        }

        public virtual IList<IComment> FindCommentsByType(string type)
        {
            CheckHistoryEnabled();
            return commentDataManager.FindCommentsByType(type);
        }

        public virtual IList<IEvent> FindEventsByTaskId(string taskId)
        {
            CheckHistoryEnabled();
            return commentDataManager.FindEventsByTaskId(taskId);
        }

        public virtual IList<IEvent> FindEventsByProcessInstanceId(string processInstanceId)
        {
            CheckHistoryEnabled();
            return commentDataManager.FindEventsByProcessInstanceId(processInstanceId);
        }

        public virtual void DeleteCommentsByTaskId(string taskId)
        {
            CheckHistoryEnabled();
            commentDataManager.DeleteCommentsByTaskId(taskId);
        }

        public virtual void DeleteCommentsByProcessInstanceId(string processInstanceId)
        {
            CheckHistoryEnabled();
            commentDataManager.DeleteCommentsByProcessInstanceId(processInstanceId);
        }

        public virtual IList<IComment> FindCommentsByProcessInstanceId(string processInstanceId)
        {
            CheckHistoryEnabled();
            return commentDataManager.FindCommentsByProcessInstanceId(processInstanceId);
        }

        public virtual IList<IComment> FindCommentsByProcessInstanceId(string processInstanceId, string type)
        {
            CheckHistoryEnabled();
            return commentDataManager.FindCommentsByProcessInstanceId(processInstanceId, type);
        }

        public virtual IComment FindComment(string commentId)
        {
            return commentDataManager.FindComment(commentId);
        }

        public virtual IEvent FindEvent(string commentId)
        {
            return commentDataManager.FindEvent(commentId);
        }

        public override void Delete(ICommentEntity commentEntity)
        {
            CheckHistoryEnabled();

            Delete(commentEntity, false);

            IComment comment = commentEntity;
            if (EventDispatcher.Enabled)
            {
                // Forced to fetch the process-instance to associate the right
                // process definition
                string processDefinitionId = null;
                string processInstanceId = comment.ProcessInstanceId;
                if (!(comment.ProcessInstanceId is null))
                {
                    IExecutionEntity process = ExecutionEntityManager.FindById<IExecutionEntity>(comment.ProcessInstanceId);
                    if (process != null)
                    {
                        processDefinitionId = process.ProcessDefinitionId;
                    }
                }
                EventDispatcher.DispatchEvent(ActivitiEventBuilder.CreateEntityEvent(ActivitiEventType.ENTITY_DELETED, commentEntity, processInstanceId, processInstanceId, processDefinitionId));
            }
        }

        protected internal virtual void CheckHistoryEnabled()
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