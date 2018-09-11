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
namespace org.activiti.engine.impl.persistence.entity
{

    using org.activiti.engine.impl.db;

    /// 
    [Serializable]
    public class HistoricIdentityLinkEntityImpl : AbstractEntityNoRevision, IHistoricIdentityLinkEntity, IBulkDeleteable
    {

        private const long serialVersionUID = 1L;

        protected internal string type;
        protected internal string userId;
        protected internal string groupId;
        protected internal string taskId;
        protected internal string processInstanceId;

        public HistoricIdentityLinkEntityImpl()
        {

        }

        public override PersistentState PersistentState
        {
            get
            {
                PersistentState persistentState = new PersistentState();
                persistentState["id"] = this.id;
                persistentState["type"] = this.type;

                if (!string.ReferenceEquals(this.userId, null))
                {
                    persistentState["userId"] = this.userId;
                }

                if (!string.ReferenceEquals(this.groupId, null))
                {
                    persistentState["groupId"] = this.groupId;
                }

                if (!string.ReferenceEquals(this.taskId, null))
                {
                    persistentState["taskId"] = this.taskId;
                }

                if (!string.ReferenceEquals(this.processInstanceId, null))
                {
                    persistentState["processInstanceId"] = this.processInstanceId;
                }

                return persistentState;
            }
        }

        public virtual bool User
        {
            get
            {
                return !string.ReferenceEquals(userId, null);
            }
        }

        public virtual bool Group
        {
            get
            {
                return !string.ReferenceEquals(groupId, null);
            }
        }

        public virtual string Type
        {
            get
            {
                return type;
            }
            set
            {
                this.type = value;
            }
        }


        public virtual string UserId
        {
            get
            {
                return userId;
            }
            set
            {
                if (!string.ReferenceEquals(this.groupId, null) && !string.ReferenceEquals(value, null))
                {
                    throw new ActivitiException("Cannot assign a userId to a task assignment that already has a groupId");
                }
                this.userId = value;
            }
        }


        public virtual string GroupId
        {
            get
            {
                return groupId;
            }
            set
            {
                if (!string.ReferenceEquals(this.userId, null) && !string.ReferenceEquals(value, null))
                {
                    throw new ActivitiException("Cannot assign a groupId to a task assignment that already has a userId");
                }
                this.groupId = value;
            }
        }


        public virtual string TaskId
        {
            get
            {
                return taskId;
            }
            set
            {
                this.taskId = value;
            }
        }


        public virtual string ProcessInstanceId
        {
            get
            {
                return processInstanceId;
            }
            set
            {
                this.processInstanceId = value;
            }
        }

    }

}