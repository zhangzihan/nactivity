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
namespace Sys.Workflow.Engine.Impl.Persistence.Entity
{

    using Sys.Workflow.Engine.Impl.DB;

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
                PersistentState persistentState = new PersistentState
                {
                    ["id"] = this.id,
                    ["type"] = this.type
                };

                if (this.userId is object)
                {
                    persistentState["userId"] = this.userId;
                }

                if (this.groupId is object)
                {
                    persistentState["groupId"] = this.groupId;
                }

                if (this.taskId is object)
                {
                    persistentState["taskId"] = this.taskId;
                }

                if (this.processInstanceId is object)
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
                return userId is object;
            }
        }

        public virtual bool Group
        {
            get
            {
                return groupId is object;
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
                if (groupId is object && value is object)
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
                if (userId is object && value is object)
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