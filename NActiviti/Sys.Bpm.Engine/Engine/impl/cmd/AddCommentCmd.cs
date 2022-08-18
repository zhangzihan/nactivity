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
    using Microsoft.Extensions.Logging;
    using Sys.Workflow.Engine.Impl.Identities;
    using Sys.Workflow.Engine.Impl.Interceptor;
    using Sys.Workflow.Engine.Impl.Persistence.Entity;
    using Sys.Workflow.Engine.Runtime;
    using Sys.Workflow.Engine.Tasks;
    using Sys.Workflow;

    /// 
    public class AddCommentCmd : ICommand<IComment>
    {
        private readonly ILogger<AddCommentCmd> logger = ProcessEngineServiceProvider.LoggerService<AddCommentCmd>();

        protected internal string taskId;
        protected internal string processInstanceId;
        protected internal string type;
        protected internal string message;

        public AddCommentCmd(string taskId, string processInstanceId, string message)
        {
            this.taskId = taskId;
            this.processInstanceId = processInstanceId;
            this.message = message;
        }

        public AddCommentCmd(string taskId, string processInstanceId, string type, string message)
        {
            this.taskId = taskId;
            this.processInstanceId = processInstanceId;
            this.type = type;
            this.message = message;
        }

        public virtual IComment Execute(ICommandContext commandContext)
        {
            // Validate task
            if (taskId is not null)
            {
                ITaskEntity task = commandContext.TaskEntityManager.FindById<ITaskEntity>(taskId);

                if (task is null)
                {
                    logger.LogWarning("Cannot find task with id " + taskId, typeof(ITask));
                    return null;
                }

                if (task.Suspended)
                {
                    throw new ActivitiException(SuspendedTaskException);
                }
            }

            if (processInstanceId is not null)
            {
                IExecutionEntity execution = commandContext.ExecutionEntityManager.FindById<IExecutionEntity>(processInstanceId);

                if (execution is null)
                {
                    logger.LogWarning("Cannot find task with id " + taskId, typeof(ITask));
                    return null;
                    //throw new ActivitiObjectNotFoundException("execution " + processInstanceId + " doesn't exist", typeof(IExecution));
                }

                if (execution.Suspended)
                {
                    throw new ActivitiException(SuspendedExceptionMessage);
                }
            }

            IUserInfo user = Authentication.AuthenticatedUser;
            ICommentEntity comment = commandContext.CommentEntityManager.Create();
            comment.UserId = user.Id;
            comment.Type = (type is null) ? CommentEntityFields.TYPE_COMMENT : type;
            comment.Time = commandContext.ProcessEngineConfiguration.Clock.CurrentTime;
            comment.TaskId = taskId;
            comment.ProcessInstanceId = processInstanceId;
            comment.Action = EventFields.ACTION_ADD_COMMENT;

            //string eventMessage = message.Replace("\\s+", " ");
            //if (eventMessage.Length > 163)
            //{
            //    eventMessage = eventMessage.Substring(0, 160) + "...";
            //}
            comment.Message = message;

            comment.FullMessage = message;

            commandContext.CommentEntityManager.Insert(comment);

            return comment;
        }

        protected internal virtual string SuspendedTaskException
        {
            get
            {
                return "Cannot add a comment to a suspended task";
            }
        }

        protected internal virtual string SuspendedExceptionMessage
        {
            get
            {
                return "Cannot add a comment to a suspended execution";
            }
        }
    }

}