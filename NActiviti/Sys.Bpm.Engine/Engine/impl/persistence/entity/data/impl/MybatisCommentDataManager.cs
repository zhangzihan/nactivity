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
namespace Sys.Workflow.engine.impl.persistence.entity.data.impl
{

    using Sys.Workflow.engine.impl.cfg;
    using Sys.Workflow.engine.task;
    using Sys.Bpm.Engine;

    /// 
    public class MybatisCommentDataManager : AbstractDataManager<ICommentEntity>, ICommentDataManager
    {

        public MybatisCommentDataManager(ProcessEngineConfigurationImpl processEngineConfiguration) : base(processEngineConfiguration)
        {
        }

        public override Type ManagedEntityClass
        {
            get
            {
                return typeof(CommentEntityImpl);
            }
        }

        public override ICommentEntity Create()
        {
            return new CommentEntityImpl();
        }

        public virtual IList<IComment> FindCommentsByTaskId(string taskId)
        {
            return DbSqlSession.SelectList<CommentEntityImpl, IComment>("selectCommentsByTaskId", new { taskId });
        }

        public virtual IList<IComment> FindCommentsByTaskIdAndType(string taskId, string type)
        {
            return DbSqlSession.SelectListWithRawParameter<CommentEntityImpl, IComment>("selectCommentsByTaskIdAndType", new { taskId, type }, 0, int.MaxValue);
        }

        public virtual IList<IComment> FindCommentsByType(string type)
        {
            return DbSqlSession.SelectList<CommentEntityImpl, IComment>("selectCommentsByType", new { type });
        }

        public virtual IList<IEvent> FindEventsByTaskId(string taskId)
        {
            return DbSqlSession.SelectList<CommentEntityImpl, IEvent>("selectEventsByTaskId", new { taskId });
        }

        public virtual IList<IEvent> FindEventsByProcessInstanceId(string processInstanceId)
        {
            return DbSqlSession.SelectList<CommentEntityImpl, IEvent>("selectEventsByProcessInstanceId", new { processInstanceId });
        }

        public virtual void DeleteCommentsByTaskId(string taskId)
        {
            DbSqlSession.Delete("deleteCommentsByTaskId", new { taskId }, typeof(CommentEntityImpl));
        }

        public virtual void DeleteCommentsByProcessInstanceId(string processInstanceId)
        {
            DbSqlSession.Delete("deleteCommentsByProcessInstanceId", new { processInstanceId }, typeof(CommentEntityImpl));
        }

        public virtual IList<IComment> FindCommentsByProcessInstanceId(string processInstanceId)
        {
            return DbSqlSession.SelectList<CommentEntityImpl, IComment>("selectCommentsByProcessInstanceId", new { processInstanceId });
        }

        public virtual IList<IComment> FindCommentsByProcessInstanceId(string processInstanceId, string type)
        {
            return DbSqlSession.SelectListWithRawParameter<CommentEntityImpl, IComment>("selectCommentsByProcessInstanceIdAndType", new { processInstanceId, type }, 0, int.MaxValue);
        }

        public virtual IComment FindComment(string commentId)
        {
            return FindById<IComment>(new KeyValuePair<string, object>("id", commentId));
        }

        public virtual IEvent FindEvent(string commentId)
        {
            return FindById<IEvent>(new KeyValuePair<string, object>("id", commentId));
        }

    }

}