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
namespace org.activiti.engine.impl.persistence.entity.data.impl
{

    using org.activiti.engine.impl.cfg;
    using org.activiti.engine.task;
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

        public override ICommentEntity create()
        {
            return new CommentEntityImpl();
        }

        public virtual IList<IComment> findCommentsByTaskId(string taskId)
        {
            return DbSqlSession.selectList<CommentEntityImpl, IComment>("selectCommentsByTaskId", new KeyValuePair<string, object>("taskId", taskId));
        }

        public virtual IList<IComment> findCommentsByTaskIdAndType(string taskId, string type)
        {
            IDictionary<string, object> @params = new Dictionary<string, object>();
            @params["taskId"] = taskId;
            @params["type"] = type;
            return DbSqlSession.selectListWithRawParameter<CommentEntityImpl, IComment>("selectCommentsByTaskIdAndType", @params, 0, int.MaxValue);
        }

        public virtual IList<IComment> findCommentsByType(string type)
        {
            return DbSqlSession.selectList<CommentEntityImpl, IComment>("selectCommentsByType", new KeyValuePair<string, object>("type", type));
        }

        public virtual IList<IEvent> findEventsByTaskId(string taskId)
        {
            return DbSqlSession.selectList<CommentEntityImpl, IEvent>("selectEventsByTaskId", new KeyValuePair<string, object>("taskId", taskId));
        }

        public virtual IList<IEvent> findEventsByProcessInstanceId(string processInstanceId)
        {
            return DbSqlSession.selectList<CommentEntityImpl, IEvent>("selectEventsByProcessInstanceId", new KeyValuePair<string, object>("processInstanceId", processInstanceId));
        }

        public virtual void deleteCommentsByTaskId(string taskId)
        {
            DbSqlSession.delete("deleteCommentsByTaskId", new { taskId }, typeof(CommentEntityImpl));
        }

        public virtual void deleteCommentsByProcessInstanceId(string processInstanceId)
        {
            DbSqlSession.delete("deleteCommentsByProcessInstanceId", new { processInstanceId }, typeof(CommentEntityImpl));
        }

        public virtual IList<IComment> findCommentsByProcessInstanceId(string processInstanceId)
        {
            return DbSqlSession.selectList<CommentEntityImpl, IComment>("selectCommentsByProcessInstanceId", new KeyValuePair<string, object>("processInstanceId", processInstanceId));
        }

        public virtual IList<IComment> findCommentsByProcessInstanceId(string processInstanceId, string type)
        {
            IDictionary<string, object> @params = new Dictionary<string, object>();
            @params["processInstanceId"] = processInstanceId;
            @params["type"] = type;
            return DbSqlSession.selectListWithRawParameter<CommentEntityImpl, IComment>("selectCommentsByProcessInstanceIdAndType", @params, 0, int.MaxValue);
        }

        public virtual IComment findComment(string commentId)
        {
            return findById<IComment>(new KeyValuePair<string, object>("id", commentId));
        }

        public virtual IEvent findEvent(string commentId)
        {
            return findById<IEvent>(new KeyValuePair<string, object>("id", commentId));
        }

    }

}