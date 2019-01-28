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
    
    using org.activiti.engine.impl.identity;
    using org.activiti.engine.impl.interceptor;
    using org.activiti.engine.impl.persistence.entity;
    using org.activiti.engine.impl.util;
    using org.activiti.engine.runtime;
    using org.activiti.engine.task;
    using System.Collections.Generic;

    /// 
    public class AddCommentCmd : ICommand<IComment>
    {

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

        public virtual IComment execute(ICommandContext commandContext)
        {

            ITaskEntity task = null;
            // Validate task
            if (!ReferenceEquals(taskId, null))
            {
                task = commandContext.TaskEntityManager.findById<ITaskEntity>(new KeyValuePair<string, object>("id", taskId));

                if (task == null)
                {
                    throw new ActivitiObjectNotFoundException("Cannot find task with id " + taskId, typeof(ITask));
                }

                if (task.Suspended)
                {
                    throw new ActivitiException(SuspendedTaskException);
                }
            }

            IExecutionEntity execution = null;
            if (!ReferenceEquals(processInstanceId, null))
            {
                execution = commandContext.ExecutionEntityManager.findById<IExecutionEntity>(new KeyValuePair<string, object>("id", processInstanceId));

                if (execution == null)
                {
                    throw new ActivitiObjectNotFoundException("execution " + processInstanceId + " doesn't exist", typeof(IExecution));
                }

                if (execution.Suspended)
                {
                    throw new ActivitiException(SuspendedExceptionMessage);
                }
            }

            string processDefinitionId = null;
            if (execution != null)
            {
                processDefinitionId = execution.ProcessDefinitionId;
            }
            else if (task != null)
            {
                processDefinitionId = task.ProcessDefinitionId;
            }

            string userId = Authentication.AuthenticatedUserId;
            ICommentEntity comment = commandContext.CommentEntityManager.create();
            comment.UserId = userId;
            comment.Type = (ReferenceEquals(type, null)) ? CommentEntity_Fields.TYPE_COMMENT : type;
            comment.Time = commandContext.ProcessEngineConfiguration.Clock.CurrentTime;
            comment.TaskId = taskId;
            comment.ProcessInstanceId = processInstanceId;
            comment.Action = Event_Fields.ACTION_ADD_COMMENT;

            string eventMessage = message.Replace("\\s+", " ");
            if (eventMessage.Length > 163)
            {
                eventMessage = eventMessage.Substring(0, 160) + "...";
            }
            comment.Message = eventMessage;

            comment.FullMessage = message;

            commandContext.CommentEntityManager.insert(comment);

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