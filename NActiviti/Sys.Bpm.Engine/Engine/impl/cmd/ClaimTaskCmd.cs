using System;

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
    using org.activiti.engine.impl.util;

    /// 
    [Serializable]
    public class ClaimTaskCmd : NeedsActiveTaskCmd<object>
    {

        private const long serialVersionUID = 1L;

        protected internal string userId;

        public ClaimTaskCmd(string taskId, string userId) : base(taskId)
        {
            this.userId = userId;
        }

        protected internal override object execute(ICommandContext commandContext, ITaskEntity task)
        {
            if (!string.ReferenceEquals(userId, null))
            {
                task.ClaimTime = commandContext.ProcessEngineConfiguration.Clock.CurrentTime;

                if (!string.ReferenceEquals(task.Assignee, null))
                {
                    if (!task.Assignee.Equals(userId))
                    {
                        // When the task is already claimed by another user, throw
                        // exception. Otherwise, ignore
                        // this, post-conditions of method already met.
                        throw new ActivitiTaskAlreadyClaimedException(task.Id, task.Assignee);
                    }
                }
                else
                {
                    commandContext.TaskEntityManager.changeTaskAssignee(task, userId);
                }
            }
            else
            {
                // Task claim time should be null
                task.ClaimTime = null;

                // Task should be assigned to no one
                commandContext.TaskEntityManager.changeTaskAssignee(task, null);
            }

            // Add claim time to historic task instance
            commandContext.HistoryManager.recordTaskClaim(task);

            return null;
        }

        protected internal override string SuspendedTaskException
        {
            get
            {
                return "Cannot claim a suspended task";
            }
        }

    }

}