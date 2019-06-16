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
namespace Sys.Workflow.engine.impl.persistence.entity
{

	/// 
	public interface IIdentityLinkEntityManager : IEntityManager<IIdentityLinkEntity>
	{

	  IList<IIdentityLinkEntity> FindIdentityLinksByTaskId(string taskId);

	  IList<IIdentityLinkEntity> FindIdentityLinksByProcessInstanceId(string processInstanceId);

	  IList<IIdentityLinkEntity> FindIdentityLinksByProcessDefinitionId(string processDefinitionId);

	  IList<IIdentityLinkEntity> FindIdentityLinkByTaskUserGroupAndType(string taskId, string userId, string groupId, string type);

	  IList<IIdentityLinkEntity> FindIdentityLinkByProcessInstanceUserGroupAndType(string processInstanceId, string userId, string groupId, string type);

	  IList<IIdentityLinkEntity> FindIdentityLinkByProcessDefinitionUserAndGroup(string processDefinitionId, string userId, string groupId);


	  IIdentityLinkEntity AddIdentityLink(IExecutionEntity executionEntity, string userId, string groupId, string type);

	  IIdentityLinkEntity AddIdentityLink(ITaskEntity taskEntity, string userId, string groupId, string type);

	  IIdentityLinkEntity AddIdentityLink(IProcessDefinitionEntity processDefinitionEntity, string userId, string groupId);

	  /// <summary>
	  /// Adds an IdentityLink for the given user id with the specified type, 
	  /// but only if the user is not associated with the execution entity yet.
	  /// 
	  /// </summary>
	  IIdentityLinkEntity InvolveUser(IExecutionEntity executionEntity, string userId, string type);

	  void AddCandidateUser(ITaskEntity taskEntity, string userId);

	  void AddCandidateUsers(ITaskEntity taskEntity, IEnumerable<string> candidateUsers);

	  void AddCandidateGroup(ITaskEntity taskEntity, string groupId);

	  void AddCandidateGroups(ITaskEntity taskEntity, IEnumerable<string> candidateGroups);

	  void AddGroupIdentityLink(ITaskEntity taskEntity, string groupId, string identityLinkType);

	  void AddUserIdentityLink(ITaskEntity taskEntity, string userId, string identityLinkType);


	  void DeleteIdentityLink(IIdentityLinkEntity identityLink, bool cascadeHistory);

	  void DeleteIdentityLink(IExecutionEntity executionEntity, string userId, string groupId, string type);

	  void DeleteIdentityLink(ITaskEntity taskEntity, string userId, string groupId, string type);

	  void DeleteIdentityLink(IProcessDefinitionEntity processDefinitionEntity, string userId, string groupId);

	  void DeleteIdentityLinksByTaskId(string taskId);

	  void DeleteIdentityLinksByProcDef(string processDefId);
	}
}