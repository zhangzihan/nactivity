using System;
using System.Collections.Generic;
using System.Text;

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

    using Sys.Workflow.Engine.Impl.Contexts;
    using Sys.Workflow.Engine.Impl.DB;
    using Sys.Workflow.Engine.Tasks;

    /// 
    [Serializable]
    public class IdentityLinkEntityImpl : AbstractEntityNoRevision, IIdentityLinkEntity, IBulkDeleteable
    {

        private const long serialVersionUID = 1L;

        protected internal string type;
        protected internal string userId;
        protected internal string groupId;
        protected internal string taskId;
        protected internal string processInstanceId;
        protected internal string processDefId;
        protected internal ITaskEntity task;
        protected internal IExecutionEntity processInstance;
        protected internal IProcessDefinitionEntity processDef;

        public IdentityLinkEntityImpl()
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

                if (this.processDefId is object)
                {
                    persistentState["processDefId"] = this.processDefId;
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
                if (this.groupId is object && value is object)
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
                if (this.userId is object && value is object)
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


        public virtual string ProcessDefId
        {
            get
            {
                return processDefId;
            }
            // set
            // {
            //this.processDefId = value;
            // }
        }


        public virtual ITaskEntity Task
        {
            get
            {
                var ctx = Context.CommandContext;
                if (task is null && taskId is object && ctx is object)
                {
                    this.task = ctx.TaskEntityManager.FindById<ITaskEntity>(taskId);
                }
                return task;
            }
            set
            {
                this.task = value;
                this.taskId = value.Id;
            }
        }


        public virtual IExecutionEntity ProcessInstance
        {
            get
            {
                var ctx = Context.CommandContext;
                if (processInstance is null && processInstanceId is object && ctx is object)
                {
                    this.processInstance = ctx.ExecutionEntityManager.FindById<IExecutionEntity>(processInstanceId);
                }
                return processInstance;
            }
            set
            {
                this.processInstance = value;
                this.processInstanceId = value.Id;
            }
        }


        public virtual IProcessDefinitionEntity ProcessDef
        {
            get
            {
                var ctx = Context.CommandContext;
                if (processDef is null && processDefId is object && ctx is object)
                {
                    this.processDef = ctx.ProcessDefinitionEntityManager.FindById<IProcessDefinitionEntity>(processDefId);
                }
                return processDef;
            }
            set
            {
                this.processDef = value;
                this.processDefId = value.Id;
            }
        }


        public virtual string ProcessDefinitionId
        {
            get
            {
                return this.processDefId;
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("IdentityLinkEntity[id=").Append(id);
            sb.Append(", type=").Append(type);
            if (!ReferenceEquals(userId, null))
            {
                sb.Append(", userId=").Append(userId);
            }
            if (groupId is object)
            {
                sb.Append(", groupId=").Append(groupId);
            }
            if (taskId is object)
            {
                sb.Append(", taskId=").Append(taskId);
            }
            if (processInstanceId is object)
            {
                sb.Append(", processInstanceId=").Append(processInstanceId);
            }
            if (processDefId is object)
            {
                sb.Append(", processDefId=").Append(processDefId);
            }
            sb.Append("]");
            return sb.ToString();
        }
    }

}