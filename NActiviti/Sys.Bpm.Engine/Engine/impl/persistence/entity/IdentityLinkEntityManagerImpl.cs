using System.Collections.Generic;
using System.Linq;

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

    using org.activiti.engine.@delegate.@event;
    using org.activiti.engine.@delegate.@event.impl;
    using org.activiti.engine.impl.cfg;
    using org.activiti.engine.impl.persistence.entity.data;
    using org.activiti.engine.task;

    /// 
    /// 
    /// 
    public class IdentityLinkEntityManagerImpl : AbstractEntityManager<IIdentityLinkEntity>, IdentityLinkEntityManager
    {

        protected internal IdentityLinkDataManager identityLinkDataManager;

        public IdentityLinkEntityManagerImpl(ProcessEngineConfigurationImpl processEngineConfiguration, IdentityLinkDataManager identityLinkDataManager) : base(processEngineConfiguration)
        {
            this.identityLinkDataManager = identityLinkDataManager;
        }

        protected internal override IDataManager<IIdentityLinkEntity> DataManager
        {
            get
            {
                return identityLinkDataManager;
            }
        }

        public override void insert(IIdentityLinkEntity entity, bool fireCreateEvent)
        {
            base.insert(entity, fireCreateEvent);
            HistoryManager.recordIdentityLinkCreated(entity);

            if (!ReferenceEquals(entity.ProcessInstanceId, null) && ExecutionRelatedEntityCountEnabledGlobally)
            {
                ICountingExecutionEntity executionEntity = (ICountingExecutionEntity)ExecutionEntityManager.findById<ICountingExecutionEntity>(entity.ProcessInstanceId);
                if (isExecutionRelatedEntityCountEnabled(executionEntity))
                {
                    executionEntity.IdentityLinkCount = executionEntity.IdentityLinkCount + 1;
                }
            }
        }

        public virtual void deleteIdentityLink(IIdentityLinkEntity identityLink, bool cascadeHistory)
        {
            delete(identityLink, false);
            if (cascadeHistory)
            {
                HistoryManager.deleteHistoricIdentityLink(identityLink.Id);
            }

            if (!ReferenceEquals(identityLink.ProcessInstanceId, null) && ExecutionRelatedEntityCountEnabledGlobally)
            {
                ICountingExecutionEntity executionEntity = (ICountingExecutionEntity)ExecutionEntityManager.findById<ICountingExecutionEntity>(identityLink.ProcessInstanceId);
                if (isExecutionRelatedEntityCountEnabled(executionEntity))
                {
                    executionEntity.IdentityLinkCount = executionEntity.IdentityLinkCount - 1;
                }
            }

            if (EventDispatcher.Enabled)
            {
                EventDispatcher.dispatchEvent(ActivitiEventBuilder.createEntityEvent(ActivitiEventType.ENTITY_DELETED, identityLink));
            }
        }

        public virtual IList<IIdentityLinkEntity> findIdentityLinksByTaskId(string taskId)
        {
            return identityLinkDataManager.findIdentityLinksByTaskId(taskId);
        }

        public virtual IList<IIdentityLinkEntity> findIdentityLinksByProcessInstanceId(string processInstanceId)
        {
            return identityLinkDataManager.findIdentityLinksByProcessInstanceId(processInstanceId);
        }

        public virtual IList<IIdentityLinkEntity> findIdentityLinksByProcessDefinitionId(string processDefinitionId)
        {
            return identityLinkDataManager.findIdentityLinksByProcessDefinitionId(processDefinitionId);
        }

        public virtual IList<IIdentityLinkEntity> findIdentityLinkByTaskUserGroupAndType(string taskId, string userId, string groupId, string type)
        {
            return identityLinkDataManager.findIdentityLinkByTaskUserGroupAndType(taskId, userId, groupId, type);
        }

        public virtual IList<IIdentityLinkEntity> findIdentityLinkByProcessInstanceUserGroupAndType(string processInstanceId, string userId, string groupId, string type)
        {
            return identityLinkDataManager.findIdentityLinkByProcessInstanceUserGroupAndType(processInstanceId, userId, groupId, type);
        }

        public virtual IList<IIdentityLinkEntity> findIdentityLinkByProcessDefinitionUserAndGroup(string processDefinitionId, string userId, string groupId)
        {
            return identityLinkDataManager.findIdentityLinkByProcessDefinitionUserAndGroup(processDefinitionId, userId, groupId);
        }

        public virtual IIdentityLinkEntity addIdentityLink(IExecutionEntity executionEntity, string userId, string groupId, string type)
        {
            IIdentityLinkEntity identityLinkEntity = identityLinkDataManager.create();
            executionEntity.IdentityLinks.Add(identityLinkEntity);
            identityLinkEntity.ProcessInstance = executionEntity.ProcessInstance != null ? executionEntity.ProcessInstance : executionEntity;
            identityLinkEntity.UserId = userId;
            identityLinkEntity.GroupId = groupId;
            identityLinkEntity.Type = type;
            insert(identityLinkEntity);
            return identityLinkEntity;
        }

        public virtual IIdentityLinkEntity addIdentityLink(ITaskEntity taskEntity, string userId, string groupId, string type)
        {
            IIdentityLinkEntity identityLinkEntity = identityLinkDataManager.create();
            taskEntity.IdentityLinks.Add(identityLinkEntity);
            identityLinkEntity.Task = taskEntity;
            identityLinkEntity.UserId = userId;
            identityLinkEntity.GroupId = groupId;
            identityLinkEntity.Type = type;
            insert(identityLinkEntity);
            if (!ReferenceEquals(userId, null) && !ReferenceEquals(taskEntity.ProcessInstanceId, null))
            {
                involveUser(taskEntity.ProcessInstance, userId, IdentityLinkType.PARTICIPANT);
            }
            return identityLinkEntity;
        }

        public virtual IIdentityLinkEntity addIdentityLink(IProcessDefinitionEntity processDefinitionEntity, string userId, string groupId)
        {
            IIdentityLinkEntity identityLinkEntity = identityLinkDataManager.create();
            processDefinitionEntity.IdentityLinks.Add(identityLinkEntity);
            identityLinkEntity.ProcessDef = processDefinitionEntity;
            identityLinkEntity.UserId = userId;
            identityLinkEntity.GroupId = groupId;
            identityLinkEntity.Type = IdentityLinkType.CANDIDATE;
            insert(identityLinkEntity);
            return identityLinkEntity;
        }

        /// <summary>
        /// Adds an IdentityLink for the given user id with the specified type, 
        /// but only if the user is not associated with the execution entity yet.
        /// 
        /// </summary>
        public virtual IIdentityLinkEntity involveUser(IExecutionEntity executionEntity, string userId, string type)
        {
            foreach (IIdentityLinkEntity identityLink in executionEntity.IdentityLinks)
            {
                if (identityLink.User && identityLink.UserId.Equals(userId))
                {
                    return identityLink;
                }
            }
            return addIdentityLink(executionEntity, userId, null, type);
        }

        public virtual void addCandidateUser(ITaskEntity taskEntity, string userId)
        {
            addIdentityLink(taskEntity, userId, null, IdentityLinkType.CANDIDATE);
        }

        public virtual void addCandidateUsers(ITaskEntity taskEntity, ICollection<string> candidateUsers)
        {
            foreach (string candidateUser in candidateUsers)
            {
                addCandidateUser(taskEntity, candidateUser);
            }
        }

        public virtual void addCandidateGroup(ITaskEntity taskEntity, string groupId)
        {
            addIdentityLink(taskEntity, null, groupId, IdentityLinkType.CANDIDATE);
        }

        public virtual void addCandidateGroups(ITaskEntity taskEntity, ICollection<string> candidateGroups)
        {
            foreach (string candidateGroup in candidateGroups)
            {
                addCandidateGroup(taskEntity, candidateGroup);
            }
        }

        public virtual void addGroupIdentityLink(ITaskEntity taskEntity, string groupId, string identityLinkType)
        {
            addIdentityLink(taskEntity, null, groupId, identityLinkType);
        }

        public virtual void addUserIdentityLink(ITaskEntity taskEntity, string userId, string identityLinkType)
        {
            addIdentityLink(taskEntity, userId, null, identityLinkType);
        }

        public virtual void deleteIdentityLink(IExecutionEntity executionEntity, string userId, string groupId, string type)
        {
            string id = !ReferenceEquals(executionEntity.ProcessInstanceId, null) ? executionEntity.ProcessInstanceId : executionEntity.Id;
            IList<IIdentityLinkEntity> identityLinks = findIdentityLinkByProcessInstanceUserGroupAndType(id, userId, groupId, type);

            foreach (IIdentityLinkEntity identityLink in identityLinks)
            {
                deleteIdentityLink(identityLink, true);
            }

            foreach (var il in identityLinks)
            {
                var item = executionEntity.IdentityLinks.FirstOrDefault(x => x.Id == il.Id);
                if (item != null)
                {
                    executionEntity.IdentityLinks.Remove(item);
                }
            }
        }

        public virtual void deleteIdentityLink(ITaskEntity taskEntity, string userId, string groupId, string type)
        {
            IList<IIdentityLinkEntity> identityLinks = findIdentityLinkByTaskUserGroupAndType(taskEntity.Id, userId, groupId, type);

            IList<string> identityLinkIds = new List<string>();
            foreach (IIdentityLinkEntity identityLink in identityLinks)
            {
                deleteIdentityLink(identityLink, true);
                identityLinkIds.Add(identityLink.Id);
            }

            // fix deleteCandidate() in create TaskListener
            IList<IIdentityLinkEntity> removedIdentityLinkEntities = new List<IIdentityLinkEntity>();
            foreach (IIdentityLinkEntity identityLinkEntity in taskEntity.IdentityLinks)
            {
                if (IdentityLinkType.CANDIDATE.Equals(identityLinkEntity.Type) && identityLinkIds.Contains(identityLinkEntity.Id) == false)
                {

                    if ((!ReferenceEquals(userId, null) && userId.Equals(identityLinkEntity.UserId)) || (!ReferenceEquals(groupId, null) && groupId.Equals(identityLinkEntity.GroupId)))
                    {

                        deleteIdentityLink(identityLinkEntity, true);
                        removedIdentityLinkEntities.Add(identityLinkEntity);

                    }
                }
            }

            foreach (var il in identityLinks)
            {
                var item = taskEntity.IdentityLinks.FirstOrDefault(x => x.Id == il.Id);
                if (item != null)
                {
                    taskEntity.IdentityLinks.Remove(item);
                }
            }
        }

        public virtual void deleteIdentityLink(IProcessDefinitionEntity processDefinitionEntity, string userId, string groupId)
        {
            IList<IIdentityLinkEntity> identityLinks = findIdentityLinkByProcessDefinitionUserAndGroup(processDefinitionEntity.Id, userId, groupId);
            foreach (IIdentityLinkEntity identityLink in identityLinks)
            {
                deleteIdentityLink(identityLink, false);
            }
        }

        public virtual void deleteIdentityLinksByTaskId(string taskId)
        {
            IList<IIdentityLinkEntity> identityLinks = findIdentityLinksByTaskId(taskId);
            foreach (IIdentityLinkEntity identityLink in identityLinks)
            {
                deleteIdentityLink(identityLink, false);
            }
        }

        public virtual void deleteIdentityLinksByProcDef(string processDefId)
        {
            identityLinkDataManager.deleteIdentityLinksByProcDef(processDefId);
        }

        public virtual IdentityLinkDataManager IdentityLinkDataManager
        {
            get
            {
                return identityLinkDataManager;
            }
            set
            {
                this.identityLinkDataManager = value;
            }
        }


    }

}