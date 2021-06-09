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

namespace Sys.Workflow.Engine.Impl.Persistence.Entity
{

    using Sys.Workflow.Engine.Delegate.Events;
    using Sys.Workflow.Engine.Delegate.Events.Impl;
    using Sys.Workflow.Engine.Impl.Cfg;
    using Sys.Workflow.Engine.Impl.Histories;
    using Sys.Workflow.Engine.Impl.Persistence.Entity.Data;
    using Sys.Workflow.Engine.Tasks;

    /// 
    /// 
    /// 
    public class IdentityLinkEntityManagerImpl : AbstractEntityManager<IIdentityLinkEntity>, IIdentityLinkEntityManager
    {

        protected internal IIdentityLinkDataManager identityLinkDataManager;

        public IdentityLinkEntityManagerImpl(ProcessEngineConfigurationImpl processEngineConfiguration, IIdentityLinkDataManager identityLinkDataManager) : base(processEngineConfiguration)
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

        public override void Insert(IIdentityLinkEntity entity, bool fireCreateEvent)
        {
            //当审核登记设置为Audit时候，再记录identity link，否则会抛出外键异常
            if (HistoryManager.IsHistoryLevelAtLeast(HistoryLevel.AUDIT))
            {
                base.Insert(entity, fireCreateEvent);
                HistoryManager.RecordIdentityLinkCreated(entity);

                if (entity.ProcessInstanceId is object && ExecutionRelatedEntityCountEnabledGlobally)
                {
                    ICountingExecutionEntity executionEntity = (ICountingExecutionEntity)ExecutionEntityManager.FindById<ICountingExecutionEntity>(entity.ProcessInstanceId);
                    if (IsExecutionRelatedEntityCountEnabled(executionEntity))
                    {
                        executionEntity.IdentityLinkCount += 1;
                    }
                }
            }
        }

        public virtual void DeleteIdentityLink(IIdentityLinkEntity identityLink, bool cascadeHistory)
        {
            Delete(identityLink, false);
            if (cascadeHistory)
            {
                HistoryManager.DeleteHistoricIdentityLink(identityLink.Id);
            }

            if (identityLink.ProcessInstanceId is object && ExecutionRelatedEntityCountEnabledGlobally)
            {
                ICountingExecutionEntity executionEntity = (ICountingExecutionEntity)ExecutionEntityManager.FindById<ICountingExecutionEntity>(identityLink.ProcessInstanceId);
                if (IsExecutionRelatedEntityCountEnabled(executionEntity))
                {
                    executionEntity.IdentityLinkCount -= 1;
                }
            }

            if (EventDispatcher.Enabled)
            {
                EventDispatcher.DispatchEvent(ActivitiEventBuilder.CreateEntityEvent(ActivitiEventType.ENTITY_DELETED, identityLink));
            }
        }

        public virtual IList<IIdentityLinkEntity> FindIdentityLinksByTaskId(string taskId)
        {
            return identityLinkDataManager.FindIdentityLinksByTaskId(taskId);
        }

        public virtual IList<IIdentityLinkEntity> FindIdentityLinksByProcessInstanceId(string processInstanceId)
        {
            return identityLinkDataManager.FindIdentityLinksByProcessInstanceId(processInstanceId);
        }

        public virtual IList<IIdentityLinkEntity> FindIdentityLinksByProcessDefinitionId(string processDefinitionId)
        {
            return identityLinkDataManager.FindIdentityLinksByProcessDefinitionId(processDefinitionId);
        }

        public virtual IList<IIdentityLinkEntity> FindIdentityLinkByTaskUserGroupAndType(string taskId, string userId, string groupId, string type)
        {
            return identityLinkDataManager.FindIdentityLinkByTaskUserGroupAndType(taskId, userId, groupId, type);
        }

        public virtual IList<IIdentityLinkEntity> FindIdentityLinkByProcessInstanceUserGroupAndType(string processInstanceId, string userId, string groupId, string type)
        {
            return identityLinkDataManager.FindIdentityLinkByProcessInstanceUserGroupAndType(processInstanceId, userId, groupId, type);
        }

        public virtual IList<IIdentityLinkEntity> FindIdentityLinkByProcessDefinitionUserAndGroup(string processDefinitionId, string userId, string groupId)
        {
            return identityLinkDataManager.FindIdentityLinkByProcessDefinitionUserAndGroup(processDefinitionId, userId, groupId);
        }

        public virtual IIdentityLinkEntity AddIdentityLink(IExecutionEntity executionEntity, string userId, string groupId, string type)
        {
            IIdentityLinkEntity identityLinkEntity = identityLinkDataManager.Create();
            executionEntity.IdentityLinks.Add(identityLinkEntity);
            identityLinkEntity.ProcessInstance = executionEntity.ProcessInstance ?? executionEntity;
            identityLinkEntity.UserId = userId;
            identityLinkEntity.GroupId = groupId;
            identityLinkEntity.Type = type;
            Insert(identityLinkEntity);
            return identityLinkEntity;
        }

        public virtual IIdentityLinkEntity AddIdentityLink(ITaskEntity taskEntity, string userId, string groupId, string type)
        {
            IIdentityLinkEntity identityLinkEntity = identityLinkDataManager.Create();
            taskEntity.IdentityLinks.Add(identityLinkEntity);
            identityLinkEntity.Task = taskEntity;
            identityLinkEntity.UserId = userId;
            identityLinkEntity.GroupId = groupId;
            identityLinkEntity.Type = type;
            Insert(identityLinkEntity);
            if (userId is object && taskEntity.ProcessInstanceId is object)
            {
                InvolveUser(taskEntity.ProcessInstance, userId, IdentityLinkType.PARTICIPANT);
            }
            return identityLinkEntity;
        }

        public virtual IIdentityLinkEntity AddIdentityLink(IProcessDefinitionEntity processDefinitionEntity, string userId, string groupId)
        {
            IIdentityLinkEntity identityLinkEntity = identityLinkDataManager.Create();
            processDefinitionEntity.IdentityLinks.Add(identityLinkEntity);
            identityLinkEntity.ProcessDef = processDefinitionEntity;
            identityLinkEntity.UserId = userId;
            identityLinkEntity.GroupId = groupId;
            identityLinkEntity.Type = IdentityLinkType.CANDIDATE;
            Insert(identityLinkEntity);
            return identityLinkEntity;
        }

        /// <summary>
        /// Adds an IdentityLink for the given user id with the specified type, 
        /// but only if the user is not associated with the execution entity yet.
        /// 
        /// </summary>
        public virtual IIdentityLinkEntity InvolveUser(IExecutionEntity executionEntity, string userId, string type)
        {
            foreach (IIdentityLinkEntity identityLink in executionEntity.IdentityLinks)
            {
                if (identityLink.User && identityLink.UserId.Equals(userId))
                {
                    return identityLink;
                }
            }
            return AddIdentityLink(executionEntity, userId, null, type);
        }

        public virtual void AddCandidateUser(ITaskEntity taskEntity, string userId)
        {
            AddIdentityLink(taskEntity, userId, null, IdentityLinkType.CANDIDATE);
        }

        public virtual void AddCandidateUsers(ITaskEntity taskEntity, IEnumerable<string> candidateUsers)
        {
            foreach (string candidateUser in candidateUsers)
            {
                AddCandidateUser(taskEntity, candidateUser);
            }
        }

        public virtual void AddCandidateGroup(ITaskEntity taskEntity, string groupId)
        {
            AddIdentityLink(taskEntity, null, groupId, IdentityLinkType.CANDIDATE);
        }

        public virtual void AddCandidateGroups(ITaskEntity taskEntity, IEnumerable<string> candidateGroups)
        {
            foreach (string candidateGroup in candidateGroups)
            {
                AddCandidateGroup(taskEntity, candidateGroup);
            }
        }

        public virtual void AddGroupIdentityLink(ITaskEntity taskEntity, string groupId, string identityLinkType)
        {
            AddIdentityLink(taskEntity, null, groupId, identityLinkType);
        }

        public virtual void AddUserIdentityLink(ITaskEntity taskEntity, string userId, string identityLinkType)
        {
            AddIdentityLink(taskEntity, userId, null, identityLinkType);
        }

        public virtual void DeleteIdentityLink(IExecutionEntity executionEntity, string userId, string groupId, string type)
        {
            string id = executionEntity.ProcessInstanceId is object ? executionEntity.ProcessInstanceId : executionEntity.Id;
            IList<IIdentityLinkEntity> identityLinks = FindIdentityLinkByProcessInstanceUserGroupAndType(id, userId, groupId, type);

            foreach (IIdentityLinkEntity identityLink in identityLinks)
            {
                DeleteIdentityLink(identityLink, true);
            }

            foreach (var il in identityLinks)
            {
                var item = executionEntity.IdentityLinks.FirstOrDefault(x => x.Id == il.Id);
                if (item is object)
                {
                    executionEntity.IdentityLinks.Remove(item);
                }
            }
        }

        public virtual void DeleteIdentityLink(ITaskEntity taskEntity, string userId, string groupId, string type)
        {
            IList<IIdentityLinkEntity> identityLinks = FindIdentityLinkByTaskUserGroupAndType(taskEntity.Id, userId, groupId, type);

            IList<string> identityLinkIds = new List<string>();
            foreach (IIdentityLinkEntity identityLink in identityLinks)
            {
                DeleteIdentityLink(identityLink, true);
                identityLinkIds.Add(identityLink.Id);
            }

            // fix deleteCandidate() in create TaskListener
            IList<IIdentityLinkEntity> removedIdentityLinkEntities = new List<IIdentityLinkEntity>();
            foreach (IIdentityLinkEntity identityLinkEntity in taskEntity.IdentityLinks)
            {
                if (IdentityLinkType.CANDIDATE.Equals(identityLinkEntity.Type) && identityLinkIds.Contains(identityLinkEntity.Id) == false)
                {

                    if ((userId is object && userId.Equals(identityLinkEntity.UserId)) || (groupId is object && groupId.Equals(identityLinkEntity.GroupId)))
                    {

                        DeleteIdentityLink(identityLinkEntity, true);
                        removedIdentityLinkEntities.Add(identityLinkEntity);

                    }
                }
            }

            foreach (var il in identityLinks)
            {
                var item = taskEntity.IdentityLinks.FirstOrDefault(x => x.Id == il.Id);
                if (item is object)
                {
                    taskEntity.IdentityLinks.Remove(item);
                }
            }
        }

        public virtual void DeleteIdentityLink(IProcessDefinitionEntity processDefinitionEntity, string userId, string groupId)
        {
            IList<IIdentityLinkEntity> identityLinks = FindIdentityLinkByProcessDefinitionUserAndGroup(processDefinitionEntity.Id, userId, groupId);
            foreach (IIdentityLinkEntity identityLink in identityLinks)
            {
                DeleteIdentityLink(identityLink, false);
            }
        }

        public virtual void DeleteIdentityLinksByTaskId(string taskId)
        {
            IList<IIdentityLinkEntity> identityLinks = FindIdentityLinksByTaskId(taskId);
            foreach (IIdentityLinkEntity identityLink in identityLinks)
            {
                DeleteIdentityLink(identityLink, false);
            }
        }

        public virtual void DeleteIdentityLinksByProcDef(string processDefId)
        {
            identityLinkDataManager.DeleteIdentityLinksByProcDef(processDefId);
        }

        public virtual IIdentityLinkDataManager IdentityLinkDataManager
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