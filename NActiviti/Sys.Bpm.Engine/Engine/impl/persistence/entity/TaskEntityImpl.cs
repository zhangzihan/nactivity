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
    using Newtonsoft.Json.Linq;
    using Sys.Workflow.Bpmn.Constants;
    using Sys.Workflow.Bpmn.Models;
    using Sys.Workflow.Engine.Delegate;
    using Sys.Workflow.Engine.Delegate.Events;
    using Sys.Workflow.Engine.Delegate.Events.Impl;
    using Sys.Workflow.Engine.Exceptions;
    using Sys.Workflow.Engine.Impl.Cfg;
    using Sys.Workflow.Engine.Impl.Cmd;
    using Sys.Workflow.Engine.Impl.Contexts;
    using Sys.Workflow.Engine.Impl.DB;
    using Sys.Workflow.Engine.Impl.Interceptor;
    using Sys.Workflow.Engine.Tasks;
    using Sys.Net.Http;
    using Sys.Workflow;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;

    /// 
    /// 
    /// 
    /// 
    [Serializable]
    public class TaskEntityImpl : VariableScopeImpl, ITaskEntity, IBulkDeleteable
    {

        public const string DELETE_REASON_COMPLETED = "completed";
        public const string DELETE_REASON_DELETED = "deleted";

        private const long serialVersionUID = 1L;

        protected internal string owner;
        protected internal int assigneeUpdatedCount; // needed for v5 compatibility
        protected internal string originalAssignee; // needed for v5 compatibility
        protected internal string assignee;
        protected internal DelegationState? delegationState;

        protected internal string parentTaskId;

        protected internal string name;
        protected internal string localizedName;
        protected internal string description;
        protected internal string localizedDescription;
        protected internal int? priority = TaskFields.DEFAULT_PRIORITY;
        protected internal DateTime? createTime; // The time when the task has been created
        protected internal DateTime? dueDate;
        protected internal int suspensionState = SuspensionStateProvider.ACTIVE.StateCode;
        protected internal string category;

        protected internal bool isIdentityLinksInitialized;
        protected internal IList<IIdentityLinkEntity> taskIdentityLinkEntities = new List<IIdentityLinkEntity>();

        protected internal string executionId;
        protected internal IExecutionEntity execution;

        protected internal string processInstanceId;
        protected internal IExecutionEntity processInstance;

        protected internal string processDefinitionId;

        protected internal string taskDefinitionKey;
        protected internal string formKey;

        protected internal new bool isDeleted;
        protected internal bool isCanceled;
        protected internal string eventName;
        protected internal ActivitiListener currentActivitiListener;

        protected internal string tenantId = ProcessEngineConfiguration.NO_TENANT_ID;

        protected internal IList<IVariableInstanceEntity> queryVariables;

        protected internal bool forcedUpdate;

        protected internal DateTime? claimTime;

        public TaskEntityImpl()
        {

        }

        private string businessKey;
        public virtual string BusinessKey
        {
            get
            {
                IExecutionEntity execution = Execution;
                if (string.IsNullOrWhiteSpace(businessKey) && execution is object)
                {
                    businessKey = execution.BusinessKey;
                }

                return businessKey;
            }
        }

        public override PersistentState PersistentState
        {
            get
            {
                PersistentState persistentState = new PersistentState
                {
                    ["assignee"] = this.assignee,
                    ["owner"] = this.owner,
                    ["name"] = this.name,
                    ["priority"] = this.priority,
                    ["canTransfer"] = CanTransfer,
                    ["isTransfer"] = IsTransfer,
                    ["onlyAssignee"] = OnlyAssignee
                };
                if (!string.IsNullOrWhiteSpace(executionId))
                {
                    persistentState["executionId"] = this.executionId;
                }
                if (!string.IsNullOrWhiteSpace(processDefinitionId))
                {
                    persistentState["processDefinitionId"] = this.processDefinitionId;
                }
                if (createTime.HasValue)
                {
                    persistentState["createTime"] = this.createTime;
                }
                if (!string.IsNullOrWhiteSpace(description))
                {
                    persistentState["description"] = this.description;
                }
                if (dueDate.HasValue)
                {
                    persistentState["dueDate"] = this.dueDate;
                }
                if (parentTaskId is object)
                {
                    persistentState["parentTaskId"] = this.parentTaskId;
                }

                persistentState["suspensionState"] = this.suspensionState;

                if (forcedUpdate)
                {
                    persistentState["forcedUpdate"] = true;
                }

                if (claimTime.HasValue)
                {
                    persistentState["claimTime"] = this.claimTime;
                }

                return persistentState;
            }
        }

        public override int RevisionNext
        {
            get
            {
                return revision + 1;
            }
        }

        public virtual void ForceUpdate()
        {
            this.forcedUpdate = true;
        }

        // variables //////////////////////////////////////////////////////////////////

        protected internal override VariableScopeImpl ParentVariableScope
        {
            get
            {
                if (Execution != null)
                {
                    return (ExecutionEntityImpl)execution;
                }
                return null;
            }
        }

        protected internal override void InitializeVariableInstanceBackPointer(IVariableInstanceEntity variableInstance)
        {
            variableInstance.TaskId = id;
            variableInstance.ExecutionId = executionId;
            variableInstance.ProcessInstanceId = processInstanceId;
        }

        protected internal override IList<IVariableInstanceEntity> LoadVariableInstances()
        {
            return Context.CommandContext.VariableInstanceEntityManager.FindVariableInstancesByTaskId(id);
        }

        protected internal override IVariableInstanceEntity CreateVariableInstance(string variableName, object value, IExecutionEntity sourceActivityExecution)
        {
            IVariableInstanceEntity result = base.CreateVariableInstance(variableName, value, sourceActivityExecution);

            // Dispatch event, if needed
            ProcessEngineConfigurationImpl processEngineConfiguration = Context.ProcessEngineConfiguration;
            if (processEngineConfiguration is object && processEngineConfiguration.EventDispatcher.Enabled)
            {
                processEngineConfiguration.EventDispatcher.DispatchEvent(ActivitiEventBuilder.CreateVariableEvent(ActivitiEventType.VARIABLE_CREATED, variableName, value, result.Type, result.TaskId, result.ExecutionId, ProcessInstanceId, ProcessDefinitionId));
            }
            return result;
        }

        protected internal override void UpdateVariableInstance(IVariableInstanceEntity variableInstance, object value, IExecutionEntity sourceActivityExecution)
        {
            base.UpdateVariableInstance(variableInstance, value, sourceActivityExecution);

            // Dispatch event, if needed
            ProcessEngineConfigurationImpl processEngineConfiguration = Context.ProcessEngineConfiguration;
            if (processEngineConfiguration is object && processEngineConfiguration.EventDispatcher.Enabled)
            {
                processEngineConfiguration.EventDispatcher.DispatchEvent(ActivitiEventBuilder.CreateVariableEvent(ActivitiEventType.VARIABLE_UPDATED, variableInstance.Name, value, variableInstance.Type, variableInstance.TaskId, variableInstance.ExecutionId, ProcessInstanceId, ProcessDefinitionId));
            }
        }

        // execution //////////////////////////////////////////////////////////////////

        public virtual IExecutionEntity Execution
        {
            get
            {
                var config = ProcessEngineServiceProvider.Resolve<ProcessEngineConfiguration>() as ProcessEngineConfigurationImpl;
                if (execution == null && !string.IsNullOrWhiteSpace(executionId))
                {
                    this.execution = config.CommandExecutor.Execute(new GetExecutionByIdCmd(executionId));
                }
                return execution;
            }
            set
            {
                this.execution = value;
            }
        }

        // task assignment ////////////////////////////////////////////////////////////

        public virtual void AddCandidateUser(string userId)
        {
            Context.CommandContext.IdentityLinkEntityManager.AddCandidateUser(this, userId);
        }

        public virtual void AddCandidateUsers(IEnumerable<string> candidateUsers)
        {
            Context.CommandContext.IdentityLinkEntityManager.AddCandidateUsers(this, candidateUsers);
        }

        public virtual void AddCandidateGroup(string groupId)
        {
            Context.CommandContext.IdentityLinkEntityManager.AddCandidateGroup(this, groupId);
        }

        public virtual void AddCandidateGroups(IEnumerable<string> candidateGroups)
        {
            Context.CommandContext.IdentityLinkEntityManager.AddCandidateGroups(this, candidateGroups);
        }

        public virtual void AddUserIdentityLink(string userId, string identityLinkType)
        {
            Context.CommandContext.IdentityLinkEntityManager.AddUserIdentityLink(this, userId, identityLinkType);
        }

        public virtual void AddGroupIdentityLink(string groupId, string identityLinkType)
        {
            Context.CommandContext.IdentityLinkEntityManager.AddGroupIdentityLink(this, groupId, identityLinkType);
        }

        public virtual ISet<IIdentityLink> Candidates
        {
            get
            {
                ISet<IIdentityLink> potentialOwners = new HashSet<IIdentityLink>();
                foreach (IIdentityLinkEntity identityLinkEntity in IdentityLinks)
                {
                    if (IdentityLinkType.CANDIDATE.Equals(identityLinkEntity.Type))
                    {
                        potentialOwners.Add(identityLinkEntity);
                    }
                }
                return potentialOwners;
            }
        }

        public virtual void DeleteCandidateGroup(string groupId)
        {
            DeleteGroupIdentityLink(groupId, IdentityLinkType.CANDIDATE);
        }

        public virtual void DeleteCandidateUser(string userId)
        {
            DeleteUserIdentityLink(userId, IdentityLinkType.CANDIDATE);
        }

        public virtual void DeleteGroupIdentityLink(string groupId, string identityLinkType)
        {
            if (groupId is object)
            {
                Context.CommandContext.IdentityLinkEntityManager.DeleteIdentityLink(this, null, groupId, identityLinkType);
            }
        }

        public virtual void DeleteUserIdentityLink(string userId, string identityLinkType)
        {
            if (userId is object)
            {
                Context.CommandContext.IdentityLinkEntityManager.DeleteIdentityLink(this, userId, null, identityLinkType);
            }
        }

        public virtual IList<IIdentityLinkEntity> IdentityLinks
        {
            get
            {
                var ctx = Context.CommandContext;
                if (!isIdentityLinksInitialized && ctx != null)
                {
                    taskIdentityLinkEntities = ctx.IdentityLinkEntityManager.FindIdentityLinksByTaskId(id);
                    isIdentityLinksInitialized = true;
                }

                return taskIdentityLinkEntities;
            }
        }

        public virtual IDictionary<string, object> ExecutionVariables
        {
            set
            {
                if (Execution != null)
                {
                    execution.Variables = value;
                }
            }
        }

        public virtual string Name
        {
            set
            {
                this.name = value;
            }
            get
            {
                if (localizedName is object && localizedName.Length > 0)
                {
                    return localizedName;
                }
                else
                {
                    return name;
                }
            }
        }

        public virtual string Description
        {
            set
            {
                this.description = value;
            }
            get
            {
                if (localizedDescription is object && localizedDescription.Length > 0)
                {
                    return localizedDescription;
                }
                else
                {
                    return description;
                }
            }
        }

        public virtual string Assignee
        {
            set
            {
                this.originalAssignee = this.assignee;
                this.assignee = value;
                assigneeUpdatedCount++;
            }
            get
            {
                return assignee;
            }
        }

        public virtual string AssigneeUser
        {
            get; set;
        }

        internal static IEnumerable<ITask> EnsureAssignerInitialized(IEnumerable<TaskEntityImpl> tasks)
        {
            if (tasks == null || tasks.Count() == 0)
            {
                yield break;
            }

            //foreach (var task in tasks)
            //{
            //    task.EnsureAssignerInitialized();

            //    yield return task;
            //}

            var variableIns = Context.CommandContext.VariableInstanceEntityManager.FindVariableInstancesByTaskIds(tasks.Select(x => x.Id).ToArray());

            var variables = variableIns.Where(x => tasks.Any(y => y.assignee == x.Name))
                .Select(x => new
                {
                    x.TaskId,
                    x.Name,
                    x.Value
                });

            var taskEntities = tasks.Cast<TaskEntityImpl>();
            foreach (TaskEntityImpl task in taskEntities)
            {
                var variable = variables.FirstOrDefault(x => x.TaskId == task.Id && x.Name == task.Assignee);
                if (variable != null)
                {
                    task.assigner = variable.Value as IUserInfo;
                }
                else
                {
                    task.assigner = new UserInfo
                    {
                        Id = task.assignee
                    };
                }

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

            //if (AssigneeUser != null)
            //{
            //    assigner = new UserInfo
            //    {
            //        Id = assignee,
            //        Name = AssigneeUser
            //    };

            //    return assigner;
            //}

            //由于缓存问题，先不要从变量里获取用户
            if (Context.CommandContext != null && (assigner == null || assigner.Id != this.assignee))
            {
                if (this.VariablesLocal.TryGetValue(this.assignee, out var userInfo) && userInfo != null)
                {
                    assigner = JToken.FromObject(userInfo).ToObject<UserInfo>();

                    return assigner;
                }
            }

            assigner = new UserInfo
            {
                Id = assignee,
                FullName = assignee
            };

            return assigner;
        }

        internal IUserInfo assigner = null;

        public virtual IUserInfo Assigner
        {
            get
            {
                if (assigner == null)
                {
                    assigner = EnsureAssignerInitialized();
                }

                return assigner;
            }
        }

        public virtual string Owner
        {
            set
            {
                this.owner = value;
            }
            get
            {
                return owner;
            }
        }

        public virtual DateTime? DueDate
        {
            set
            {
                this.dueDate = value;
            }
            get
            {
                return dueDate;
            }
        }

        public virtual int? Priority
        {
            set
            {
                this.priority = value;
            }
            get
            {
                return priority;
            }
        }

        public virtual string Category
        {
            set
            {
                this.category = value;
            }
            get
            {
                return category;
            }
        }

        public virtual string ParentTaskId
        {
            set
            {
                this.parentTaskId = value;
            }
            get
            {
                return parentTaskId;
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


        // Override from VariableScopeImpl

        protected internal override bool ActivityIdUsedForDetails
        {
            get
            {
                return false;
            }
        }

        // Overridden to avoid fetching *all* variables (as is the case in the super // call)
        protected internal override IVariableInstanceEntity GetSpecificVariable(string variableName)
        {
            ICommandContext commandContext = Context.CommandContext;
            if (commandContext == null)
            {
                throw new ActivitiException("lazy loading outside command context");
            }
            IVariableInstanceEntity variableInstance = commandContext.VariableInstanceEntityManager.FindVariableInstanceByTaskAndName(id, variableName);

            return variableInstance;
        }

        protected internal override IList<IVariableInstanceEntity> GetSpecificVariables(IEnumerable<string> variableNames)
        {
            ICommandContext commandContext = Context.CommandContext;
            if (commandContext == null)
            {
                throw new ActivitiException("lazy loading outside command context");
            }
            return commandContext.VariableInstanceEntityManager.FindVariableInstancesByTaskAndNames(id, variableNames);
        }

        // regular getters and setters ////////////////////////////////////////////////////////

        public override int Revision
        {
            get
            {
                return revision;
            }
            set
            {
                this.revision = value;
            }
        }



        public virtual string LocalizedName
        {
            get
            {
                return localizedName;
            }
            set
            {
                this.localizedName = value;
            }
        }



        public virtual string LocalizedDescription
        {
            get
            {
                return localizedDescription;
            }
            set
            {
                this.localizedDescription = value;
            }
        }




        public virtual DateTime? CreateTime
        {
            get
            {
                return createTime;
            }
            set
            {
                this.createTime = value;
            }
        }


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

        public virtual string ProcessDefinitionId
        {
            get
            {
                return processDefinitionId;
            }
            set
            {
                this.processDefinitionId = value;
            }
        }



        public virtual string OriginalAssignee
        {
            get
            {
                // Don't ask. A stupid hack for v5 compatibility
                if (assigneeUpdatedCount > 1)
                {
                    return originalAssignee;
                }
                else
                {
                    return assignee;
                }
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


        public virtual string EventName
        {
            get
            {
                return eventName;
            }
            set
            {
                this.eventName = value;
            }
        }


        public virtual ActivitiListener CurrentActivitiListener
        {
            get
            {
                return currentActivitiListener;
            }
            set
            {
                this.currentActivitiListener = value;
            }
        }



        public virtual IExecutionEntity ProcessInstance
        {
            get
            {
                var ctx = Context.CommandContext;
                if (processInstance == null && processInstanceId != null && ctx != null)
                {
                    processInstance = ctx.ExecutionEntityManager.FindById<IExecutionEntity>(processInstanceId);
                }
                return processInstance;
            }
            set
            {
                this.processInstance = value;
            }
        }





        public virtual DelegationState? DelegationState
        {
            get
            {
                return delegationState;
            }
            set
            {
                this.delegationState = value;
            }
        }


        public virtual string DelegationStateString
        {
            get
            {
                //Needed for Activiti 5 compatibility, not exposed in terface
                return delegationState.HasValue ? DelegationState.ToString() : null;
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    delegationState = null;
                }

                if (Enum.TryParse<DelegationState>(value, out var state))
                {
                    delegationState = null;
                }
                else
                {
                    delegationState = state;
                }
            }
        }


        public override bool Deleted
        {
            get
            {
                return isDeleted;
            }
            set
            {
                this.isDeleted = value;
            }
        }


        public virtual bool Canceled
        {
            get
            {
                return isCanceled;
            }
            set
            {
                this.isCanceled = value;
            }
        }



        public override IDictionary<string, IVariableInstanceEntity> VariableInstanceEntities
        {
            get
            {
                EnsureVariableInstancesInitialized();
                return variableInstances;
            }
        }

        public virtual int SuspensionState
        {
            get
            {
                return suspensionState;
            }
            set
            {
                this.suspensionState = value;
            }
        }



        public virtual bool Suspended
        {
            get
            {
                return suspensionState == SuspensionStateProvider.SUSPENDED.StateCode;
            }
            set
            {

            }
        }

        public virtual IDictionary<string, object> TaskLocalVariables
        {
            get
            {
                IDictionary<string, object> variables = new Dictionary<string, object>();
                if (queryVariables != null)
                {
                    foreach (IVariableInstanceEntity variableInstance in queryVariables)
                    {
                        if (variableInstance.Id is object && variableInstance.TaskId is object)
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
                    foreach (IVariableInstanceEntity variableInstance in queryVariables)
                    {
                        if (variableInstance.Id is object && variableInstance.TaskId is null)
                        {
                            variables[variableInstance.Name] = variableInstance.Value;
                        }
                    }
                }
                return variables;
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


        public virtual IList<IVariableInstanceEntity> QueryVariables
        {
            get
            {
                if (queryVariables == null && Context.CommandContext != null)
                {
                    queryVariables = new VariableInitializingList();
                }
                return queryVariables;
            }
            set
            {
                this.queryVariables = value;
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

        public bool? IsAppend { get; set; }

        public bool? IsTransfer { get; set; }

        public bool? CanTransfer { get; set; }

        public bool? OnlyAssignee { get; set; }

        public bool? IsRuntime { get; set; }

        public virtual void IsRuntimeAssignee()
        {
            UserTask task = this.Execution.CurrentFlowElement as UserTask;
            if (task.ExtensionElements.TryGetValue(BpmnXMLConstants.ELEMENT_EXTENSIONS_PROPERTY, out IList<ExtensionElement> exts))
            {
                if (bool.TryParse(exts.GetAttributeValue(BpmnXMLConstants.ACTIITI_RUNTIME_ASSIGNEE), out bool result))
                {
                    this.IsRuntime = result;
                    return;
                }
            }

            this.IsRuntime = false;
        }

        public override string ToString()
        {
            return "Task[id=" + id + ", name=" + name + "]";
        }
    }

}