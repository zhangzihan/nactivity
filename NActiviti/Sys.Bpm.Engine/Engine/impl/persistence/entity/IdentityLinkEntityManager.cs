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
namespace org.activiti.engine.impl.persistence.entity
{

	/// 
	public interface IdentityLinkEntityManager : IEntityManager<IIdentityLinkEntity>
	{

	  IList<IIdentityLinkEntity> findIdentityLinksByTaskId(string taskId);

	  IList<IIdentityLinkEntity> findIdentityLinksByProcessInstanceId(string processInstanceId);

	  IList<IIdentityLinkEntity> findIdentityLinksByProcessDefinitionId(string processDefinitionId);

	  IList<IIdentityLinkEntity> findIdentityLinkByTaskUserGroupAndType(string taskId, string userId, string groupId, string type);

	  IList<IIdentityLinkEntity> findIdentityLinkByProcessInstanceUserGroupAndType(string processInstanceId, string userId, string groupId, string type);

	  IList<IIdentityLinkEntity> findIdentityLinkByProcessDefinitionUserAndGroup(string processDefinitionId, string userId, string groupId);


	  IIdentityLinkEntity addIdentityLink(IExecutionEntity executionEntity, string userId, string groupId, string type);

	  IIdentityLinkEntity addIdentityLink(ITaskEntity taskEntity, string userId, string groupId, string type);

	  IIdentityLinkEntity addIdentityLink(IProcessDefinitionEntity processDefinitionEntity, string userId, string groupId);

	  /// <summary>
	  /// Adds an IdentityLink for the given user id with the specified type, 
	  /// but only if the user is not associated with the execution entity yet.
	  /// 
	  /// </summary>
	  IIdentityLinkEntity involveUser(IExecutionEntity executionEntity, string userId, string type);

	  void addCandidateUser(ITaskEntity taskEntity, string userId);

	  void addCandidateUsers(ITaskEntity taskEntity, ICollection<string> candidateUsers);

	  void addCandidateGroup(ITaskEntity taskEntity, string groupId);

	  void addCandidateGroups(ITaskEntity taskEntity, ICollection<string> candidateGroups);

	  void addGroupIdentityLink(ITaskEntity taskEntity, string groupId, string identityLinkType);

	  void addUserIdentityLink(ITaskEntity taskEntity, string userId, string identityLinkType);


	  void deleteIdentityLink(IIdentityLinkEntity identityLink, bool cascadeHistory);

	  void deleteIdentityLink(IExecutionEntity executionEntity, string userId, string groupId, string type);

	  void deleteIdentityLink(ITaskEntity taskEntity, string userId, string groupId, string type);

	  void deleteIdentityLink(IProcessDefinitionEntity processDefinitionEntity, string userId, string groupId);

	  void deleteIdentityLinksByTaskId(string taskId);

	  void deleteIdentityLinksByProcDef(string processDefId);

	}
}