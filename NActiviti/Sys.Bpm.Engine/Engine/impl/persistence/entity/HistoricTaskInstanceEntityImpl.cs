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
    using Newtonsoft.Json.Linq;
    using org.activiti.engine.history;
    using org.activiti.engine.impl.context;
    using org.activiti.engine.impl.db;
    using org.activiti.engine.task;
    using Sys.Net.Http;
    using Sys.Workflow;
    using System.Linq;

    /// 
    /// 
    [Serializable]
    public class HistoricTaskInstanceEntityImpl : HistoricScopeInstanceEntityImpl, IHistoricTaskInstanceEntity, IBulkDeleteable
    {

        private const long serialVersionUID = 1L;

        protected internal string executionId;
        protected internal string name;
        protected internal string localizedName;
        protected internal string parentTaskId;
        protected internal string description;
        protected internal string localizedDescription;
        protected internal string owner;
        protected internal string assignee;
        protected internal string taskDefinitionKey;
        protected internal string formKey;
        protected internal int? priority;
        protected internal DateTime? dueDate;
        protected internal DateTime? claimTime;
        protected internal string category;
        protected internal string tenantId = ProcessEngineConfiguration.NO_TENANT_ID;
        protected internal IList<IHistoricVariableInstanceEntity> queryVariables;

        public HistoricTaskInstanceEntityImpl()
        {

        }

        public HistoricTaskInstanceEntityImpl(ITaskEntity task, IExecutionEntity execution)
        {
            this.id = task.Id;
            if (execution != null)
            {
                this.processDefinitionId = execution.ProcessDefinitionId;
                this.processInstanceId = execution.ProcessInstanceId;
                this.executionId = execution.Id;
            }
            else
            {
                this.processDefinitionId = task.ProcessDefinitionId;
                this.ProcessInstanceId = task.ProcessInstanceId;
                this.executionId = task.ExecutionId;
            }
            this.name = task.Name;
            this.parentTaskId = task.ParentTaskId;
            this.description = task.Description;
            this.owner = task.Owner;
            this.assignee = task.Assignee;
            this.AssigneeUser = task.AssigneeUser;
            this.startTime = Context.ProcessEngineConfiguration.Clock.CurrentTime;
            this.taskDefinitionKey = task.TaskDefinitionKey;

            this.Priority = task.Priority;
            this.DueDate = task.DueDate;
            this.Category = task.Category;
            this.CanTransfer = task.CanTransfer;
            this.OnlyAssignee = task.OnlyAssignee;
            this.IsTransfer = task.IsTransfer;

            // Inherit tenant id (if applicable)
            if (!(task.TenantId is null))
            {
                tenantId = task.TenantId;
            }
        }

        // persistence //////////////////////////////////////////////////////////////

        public override PersistentState PersistentState
        {
            get
            {
                PersistentState persistentState = new PersistentState
                {
                    ["name"] = name,
                    ["owner"] = owner,
                    ["assignee"] = assignee,
                    ["endTime"] = endTime,
                    ["durationInMillis"] = durationInMillis,
                    ["description"] = description,
                    ["deleteReason"] = deleteReason,
                    ["taskDefinitionKey"] = taskDefinitionKey,
                    ["formKey"] = formKey,
                    ["priority"] = priority,
                    ["category"] = category,
                    ["processDefinitionId"] = processDefinitionId,
                    ["canTransfer"] = CanTransfer,
                    ["isTransfer"] = IsTransfer,
                    ["onlyAssignee"] = OnlyAssignee
                };
                if (!(parentTaskId is null))
                {
                    persistentState["parentTaskId"] = parentTaskId;
                }
                if (dueDate != null)
                {
                    persistentState["dueDate"] = dueDate;
                }
                if (claimTime != null)
                {
                    persistentState["claimTime"] = claimTime;
                }
                return persistentState;
            }
        }

        // getters and setters ////////////////////////////////////////////////////////

        public virtual string ExecutionId
        {
            get
            {
                return executionId;
            }
            set
            {
                this.executionId = value;
            }
        }

        private string businessKey;
        public virtual string BusinessKey
        {
            get
            {
                return businessKey;
            }
            set
            {
                businessKey = value;
            }
        }

        public virtual string Name
        {
            get
            {
                if (!(localizedName is null) && localizedName.Length > 0)
                {
                    return localizedName;
                }
                else
                {
                    return name;
                }
            }
            set
            {
                this.name = value;
            }
        }

        public virtual string LocalizedName
        {
            get
            {
                return this.localizedName;
            }
            set
            {
                this.localizedName = value;
            }
        }

        public virtual string Description
        {
            get
            {
                if (!(localizedDescription is null) && localizedDescription.Length > 0)
                {
                    return localizedDescription;
                }
                else
                {
                    return description;
                }
            }
            set
            {
                this.description = value;
            }
        }


        public virtual string LocalizedDescription
        {
            get
            {
                return this.localizedDescription;
            }
            set
            {
                this.localizedDescription = value;
            }
        }

        public virtual string Assignee
        {
            get
            {
                return assignee;
            }
            set
            {
                this.assignee = value;
            }
        }

        public virtual string AssigneeUser
        {
            get; set;
        }

        internal static IEnumerable<IHistoricTaskInstance> EnsureAssignerInitialized(IEnumerable<HistoricTaskInstanceEntityImpl> tasks)
        {
            foreach (var task in tasks ?? new HistoricTaskInstanceEntityImpl[0])
            {
                task.EnsureAssignerInitialized();

                yield return task;
            }

            yield break;
        }

        private IUserInfo EnsureAssignerInitialized()
        {
            if (assignee == null)
            {
                assigner = null;
                return null;
            }

            if (AssigneeUser != null)
            {
                assigner = new UserInfo
                {
                    Id = assignee,
                    FullName = AssigneeUser
                };

                return assigner;
            }

            if (Context.CommandContext != null && (assigner == null || assigner.Id != this.assignee))
            {
                var hisTask = this.QueryVariables.FirstOrDefault(x => x.VariableName == this.assignee);

                if (hisTask != null)
                {
                    assigner = JToken.FromObject(hisTask.Value).ToObject<UserInfo>();

                    return assigner;
                }
            }

            assigner = new UserInfo
            {
                Id = assignee
            };

            return assigner;
        }

        private IUserInfo assigner = null;

        public virtual IUserInfo Assigner
        {
            get
            {
                if (assigner == null)
                {
                    assigner = EnsureAssignerInitialized();
                }

                return assigner;
                //                throw new org.activiti.engine.exceptions.NotFoundAssigneeException(this.assignee);  
                //#endif
            }
        }

        public virtual string TaskDefinitionKey
        {
            get
            {
                return taskDefinitionKey;
            }
            set
            {
                this.taskDefinitionKey = value;
            }
        }


        public virtual DateTime? CreateTime
        {
            get
            {
                return StartTime; // For backwards compatible reason implemented with createTime and startTime
            }
        }

        public virtual string FormKey
        {
            get
            {
                return formKey;
            }
            set
            {
                this.formKey = value;
            }
        }


        public virtual int? Priority
        {
            get
            {
                return priority;
            }
            set
            {
                this.priority = value;
            }
        }


        public virtual DateTime? DueDate
        {
            get
            {
                return dueDate;
            }
            set
            {
                this.dueDate = value;
            }
        }


        public virtual string Category
        {
            get
            {
                return category;
            }
            set
            {
                this.category = value;
            }
        }


        public virtual string Owner
        {
            get
            {
                return owner;
            }
            set
            {
                this.owner = value;
            }
        }


        public virtual string ParentTaskId
        {
            get
            {
                return parentTaskId;
            }
            set
            {
                this.parentTaskId = value;
            }
        }


        public virtual DateTime? ClaimTime
        {
            get
            {
                return claimTime;
            }
            set
            {
                this.claimTime = value;
            }
        }


        public virtual string TenantId
        {
            get
            {
                return tenantId;
            }
            set
            {
                this.tenantId = value;
            }
        }


        public virtual DateTime? Time
        {
            get
            {
                return StartTime;
            }
        }

        public virtual long? WorkTimeInMillis
        {
            get
            {
                if (!endTime.HasValue || !claimTime.HasValue)
                {
                    return null;
                }
                return endTime.Value.Ticks - claimTime.Value.Ticks;
            }
        }

        public virtual IDictionary<string, object> TaskLocalVariables
        {
            get
            {
                IDictionary<string, object> variables = new Dictionary<string, object>();
                if (queryVariables != null)
                {
                    foreach (IHistoricVariableInstanceEntity variableInstance in queryVariables)
                    {
                        if (!(variableInstance.Id is null) && !(variableInstance.TaskId is null))
                        {
                            variables[variableInstance.Name] = variableInstance.Value;
                        }
                    }
                }
                return variables;
            }
        }

        public virtual IDictionary<string, object> ProcessVariables
        {
            get
            {
                IDictionary<string, object> variables = new Dictionary<string, object>();
                if (queryVariables != null)
                {
                    foreach (IHistoricVariableInstanceEntity variableInstance in queryVariables)
                    {
                        if (!(variableInstance.Id is null) && variableInstance.TaskId is null)
                        {
                            variables[variableInstance.Name] = variableInstance.Value;
                        }
                    }
                }
                return variables;
            }
        }

        public virtual IList<IHistoricVariableInstanceEntity> QueryVariables
        {
            get
            {
                if (queryVariables == null && Context.CommandContext != null)
                {
                    queryVariables = new HistoricVariableInitializingList();
                }
                return queryVariables;
            }
            set
            {
                this.queryVariables = value;
            }
        }

        /// <inheritdoc />
        public bool? IsAppend { get; set; }

        /// <inheritdoc />
        public bool? IsTransfer { get; set; }

        public bool? CanTransfer { get; set; }

        public bool? OnlyAssignee { get; set; }

        /// <inheritdoc />
        public bool? IsRuntime { get; set; }
    }

}