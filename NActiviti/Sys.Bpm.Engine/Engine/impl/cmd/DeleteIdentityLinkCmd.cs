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
    using org.activiti.engine.task;

    /// 
    /// 
    /// 
    [Serializable]
	public class DeleteIdentityLinkCmd : NeedsActiveTaskCmd<object>
    {

	  private const long serialVersionUID = 1L;

	  public static int IDENTITY_USER = 1;
	  public static int IDENTITY_GROUP = 2;

	  protected internal string userId;

	  protected internal string groupId;

	  protected internal string type;

	  public DeleteIdentityLinkCmd(string taskId, string userId, string groupId, string type) : base(taskId)
	  {
		validateParams(userId, groupId, type, taskId);
		this.taskId = taskId;
		this.userId = userId;
		this.groupId = groupId;
		this.type = type;
	  }

	  protected internal virtual void validateParams(string userId, string groupId, string type, string taskId)
	  {
		if (ReferenceEquals(taskId, null))
		{
		  throw new ActivitiIllegalArgumentException("taskId is null");
		}

		if (ReferenceEquals(type, null))
		{
		  throw new ActivitiIllegalArgumentException("type is required when adding a new task identity link");
		}

		// Special treatment for assignee and owner: group cannot be used and
		// userId may be null
		if (IdentityLinkType.ASSIGNEE.Equals(type) || IdentityLinkType.OWNER.Equals(type))
		{
		  if (!ReferenceEquals(groupId, null))
		  {
			throw new ActivitiIllegalArgumentException("Incompatible usage: cannot use type '" + type + "' together with a groupId");
		  }
		}
		else
		{
		  if (ReferenceEquals(userId, null) && ReferenceEquals(groupId, null))
		  {
			throw new ActivitiIllegalArgumentException("userId and groupId cannot both be null");
		  }
		}
	  }

	  protected internal override object execute(ICommandContext commandContext, ITaskEntity task)
	  {
		if (IdentityLinkType.ASSIGNEE.Equals(type))
		{
		  commandContext.TaskEntityManager.changeTaskAssignee(task, null);
		}
		else if (IdentityLinkType.OWNER.Equals(type))
		{
		  commandContext.TaskEntityManager.changeTaskOwner(task, null);
		}
		else
		{
		  commandContext.IdentityLinkEntityManager.deleteIdentityLink(task, userId, groupId, type);
		}

		commandContext.HistoryManager.createIdentityLinkComment(taskId, userId, groupId, type, false);

		return null;
	  }

	}

}