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
namespace org.activiti.engine.impl.cmd
{
    using org.activiti.engine.impl.interceptor;
    using org.activiti.engine.impl.persistence.entity;
    using org.activiti.engine.task;

    /// 
    [Serializable]
    public class DeleteCommentCmd : ICommand<object>
    {

        private const long serialVersionUID = 1L;
        protected internal string taskId;
        protected internal string processInstanceId;
        protected internal string commentId;

        public DeleteCommentCmd(string taskId, string processInstanceId, string commentId)
        {
            this.taskId = taskId;
            this.processInstanceId = processInstanceId;
            this.commentId = commentId;
        }

        public virtual object execute(ICommandContext commandContext)
        {
            ICommentEntityManager commentManager = commandContext.CommentEntityManager;

            if (!ReferenceEquals(commentId, null))
            {
                // Delete for an individual comment
                IComment comment = commentManager.findComment(commentId);
                if (comment == null)
                {
                    throw new ActivitiObjectNotFoundException("Comment with id '" + commentId + "' doesn't exists.", typeof(IComment));
                }

                if (!ReferenceEquals(comment.ProcessInstanceId, null))
                {
                    IExecutionEntity execution = commandContext.ExecutionEntityManager.findById<IExecutionEntity>(comment.ProcessInstanceId);

                }
                else if (!ReferenceEquals(comment.TaskId, null))
                {
                    ITask task = commandContext.TaskEntityManager.findById<ITask>(new KeyValuePair<string, object>("id", comment.TaskId));
                }

                commentManager.delete((ICommentEntity)comment);

            }
            else
            {
                // Delete all comments on a task of process
                List<IComment> comments = new List<IComment>();
                if (!ReferenceEquals(processInstanceId, null))
                {

                    IExecutionEntity execution = commandContext.ExecutionEntityManager.findById<IExecutionEntity>(processInstanceId);

                    comments.AddRange(commentManager.findCommentsByProcessInstanceId(processInstanceId));
                }
                if (!ReferenceEquals(taskId, null))
                {

                    ITask task = commandContext.TaskEntityManager.findById<ITask>(new KeyValuePair<string, object>("id", taskId));
                    comments.AddRange(commentManager.findCommentsByTaskId(taskId));
                }

                foreach (IComment comment in comments)
                {
                    commentManager.delete((ICommentEntity)comment);
                }
            }
            return null;
        }
    }

}