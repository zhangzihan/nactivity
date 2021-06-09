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
namespace Sys.Workflow.Engine.Impl.Cmd
{
    using Sys.Workflow.Engine.Impl.Interceptor;
    using Sys.Workflow.Engine.Impl.Persistence.Entity;
    using Sys.Workflow.Engine.Tasks;

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

        public virtual object Execute(ICommandContext commandContext)
        {
            ICommentEntityManager commentManager = commandContext.CommentEntityManager;

            if (commentId is object)
            {
                // Delete for an individual comment
                IComment comment = commentManager.FindComment(commentId);
                if (comment is null)
                {
                    throw new ActivitiObjectNotFoundException("Comment with id '" + commentId + "' doesn't exists.", typeof(IComment));
                }

                if (comment.ProcessInstanceId is object)
                {
                    commandContext.ExecutionEntityManager.FindById<IExecutionEntity>(comment.ProcessInstanceId);

                }
                else if (comment.TaskId is object)
                {
                    commandContext.TaskEntityManager.FindById<ITask>(new KeyValuePair<string, object>("id", comment.TaskId));
                }

                commentManager.Delete((ICommentEntity)comment);
            }
            else
            {
                // Delete all comments on a task of process
                List<IComment> comments = new List<IComment>();
                if (processInstanceId is object)
                {
                    commandContext.ExecutionEntityManager.FindById<IExecutionEntity>(processInstanceId);

                    comments.AddRange(commentManager.FindCommentsByProcessInstanceId(processInstanceId));
                }
                if (taskId is object)
                {
                    commandContext.TaskEntityManager.FindById<ITask>(new KeyValuePair<string, object>("id", taskId));
                    comments.AddRange(commentManager.FindCommentsByTaskId(taskId));
                }

                foreach (IComment comment in comments)
                {
                    commentManager.Delete((ICommentEntity)comment);
                }
            }
            return null;
        }
    }

}