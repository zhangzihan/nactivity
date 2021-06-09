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
using Newtonsoft.Json.Linq;
using Sys.Net.Http;
using Sys.Workflow.Bpmn.Constants;
using Sys.Workflow.Bpmn.Models;
using Sys.Workflow.Engine.Delegate.Events;
using Sys.Workflow.Engine.Delegate.Events.Impl;
using Sys.Workflow.Engine.Impl.Cfg;
using Sys.Workflow.Engine.Impl.Cmd;
using Sys.Workflow.Engine.Impl.Contexts;
using Sys.Workflow.Engine.Impl.DB;
using Sys.Workflow.Engine.Impl.Interceptor;
using Sys.Workflow.Engine.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sys.Workflow.Engine.Impl.Persistence.Entity
{

    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class TaskEntityImpl : VariableScopeImpl, ITaskEntity, IBulkDeleteable
    {
        /// <summary>
        /// 
        /// </summary>
        public const string DELETE_REASON_COMPLETED = "completed";
        /// <summary>
        /// 
        /// </summary>
        public const string DELETE_REASON_DELETED = "deleted";

        private const long serialVersionUID = 1L;
        /// <summary>
        /// 
        /// </summary>
        private string owner;
        /// <summary>
        /// 
        /// </summary>
        private int assigneeUpdatedCount; // needed for v5 compatibility
        /// <summary>
        /// 
        /// </summary>
        private string originalAssignee; // needed for v5 compatibility
        /// <summary>
        /// 
        /// </summary>
        private string assignee;
        /// <summary>
        /// 
        /// </summary>
        private DelegationState? delegationState;
        /// <summary>
        /// 
        /// </summary>
        private string parentTaskId;
        /// <summary>
        /// 
        /// </summary>
        private string name;
        /// <summary>
        /// 
        /// </summary>
        private string localizedName;
        /// <summary>
        /// 
        /// </summary>
        private string description;
        /// <summary>
        /// 
        /// </summary>
        private string localizedDescription;
        /// <summary>
        /// 
        /// </summary>
        private int? priority = TaskFields.DEFAULT_PRIORITY;
        /// <summary>
        /// 
        /// </summary>
        private DateTime? createTime; // The time when the task has been created
        /// <summary>
        /// 
        /// </summary>
        private DateTime? dueDate;
        /// <summary>
        /// 
        /// </summary>
        private int suspensionState = SuspensionStateProvider.ACTIVE.StateCode;
        /// <summary>
        /// 
        /// </summary>
        private string category;
        /// <summary>
        /// 
        /// </summary>
        private bool isIdentityLinksInitialized;
        /// <summary>
        /// 
        /// </summary>
        private IList<IIdentityLinkEntity> taskIdentityLinkEntities = new List<IIdentityLinkEntity>();
        /// <summary>
        /// 
        /// </summary>
        private string executionId;
        /// <summary>
        /// 
        /// </summary>
        private IExecutionEntity execution;
        /// <summary>
        /// 
        /// </summary>
        private string processInstanceId;
        /// <summary>
        /// 
        /// </summary>
        private IExecutionEntity processInstance;
        /// <summary>
        /// 
        /// </summary>
        private string processDefinitionId;
        /// <summary>
        /// 
        /// </summary>
        private string taskDefinitionKey;
        /// <summary>
        /// 
        /// </summary>
        private string formKey;
        /// <summary>
        /// 
        /// </summary>
        private bool isCanceled;
        /// <summary>
        /// 
        /// </summary>
        private string eventName;
        /// <summary>
        /// 
        /// </summary>
        private ActivitiListener currentActivitiListener;
        /// <summary>
        /// 
        /// </summary>
        private string tenantId = ProcessEngineConfiguration.NO_TENANT_ID;
        /// <summary>
        /// 
        /// </summary>
        private IList<IVariableInstanceEntity> queryVariables;
        /// <summary>
        /// 
        /// </summary>
        private bool forcedUpdate;
        /// <summary>
        /// 
        /// </summary>
        private DateTime? claimTime;
        /// <summary>
        /// 
        /// </summary>
        public TaskEntityImpl()
        {

        }

        private string businessKey;
        /// <summary>
        /// 
        /// </summary>
        public virtual string BusinessKey
        {
            get
            {
                IExecutionEntity execution = Execution;
                businessKey = execution?.BusinessKey;

                return businessKey;
            }
        }
        /// <summary>
        /// 
        /// </summary>
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
        /// <summary>
        /// 
        /// </summary>
        public override int RevisionNext
        {
            get
            {
                return Revision + 1;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public virtual void ForceUpdate()
        {
            this.forcedUpdate = true;
        }

        // variables //////////////////////////////////////////////////////////////////
        /// <summary>
        /// 
        /// </summary>
        protected internal override VariableScopeImpl ParentVariableScope
        {
            get
            {
                if (Execution is object)
                {
                    return (ExecutionEntityImpl)execution;
                }
                return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="variableInstance"></param>
        protected internal override void InitializeVariableInstanceBackPointer(IVariableInstanceEntity variableInstance)
        {
            variableInstance.TaskId = Id;
            variableInstance.ExecutionId = executionId;
            variableInstance.ProcessInstanceId = processInstanceId;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected internal override IList<IVariableInstanceEntity> LoadVariableInstances()
        {
            return Context.CommandContext.VariableInstanceEntityManager.FindVariableInstancesByTaskId(Id);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="variableName"></param>
        /// <param name="value"></param>
        /// <param name="sourceActivityExecution"></param>
        /// <returns></returns>
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="variableInstance"></param>
        /// <param name="value"></param>
        /// <param name="sourceActivityExecution"></param>
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
        /// <summary>
        /// 
        /// </summary>
        public virtual IExecutionEntity Execution
        {
            get
            {
                var config = ProcessEngineServiceProvider.Resolve<ProcessEngineConfiguration>() as ProcessEngineConfigurationImpl;
                if (execution is null && !string.IsNullOrWhiteSpace(executionId))
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="userId"></param>
        public virtual void AddCandidateUser(string userId)
        {
            Context.CommandContext.IdentityLinkEntityManager.AddCandidateUser(this, userId);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="candidateUsers"></param>
        public virtual void AddCandidateUsers(IEnumerable<string> candidateUsers)
        {
            Context.CommandContext.IdentityLinkEntityManager.AddCandidateUsers(this, candidateUsers);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="groupId"></param>
        public virtual void AddCandidateGroup(string groupId)
        {
            Context.CommandContext.IdentityLinkEntityManager.AddCandidateGroup(this, groupId);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="candidateGroups"></param>
        public virtual void AddCandidateGroups(IEnumerable<string> candidateGroups)
        {
            Context.CommandContext.IdentityLinkEntityManager.AddCandidateGroups(this, candidateGroups);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="identityLinkType"></param>
        public virtual void AddUserIdentityLink(string userId, string identityLinkType)
        {
            Context.CommandContext.IdentityLinkEntityManager.AddUserIdentityLink(this, userId, identityLinkType);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="identityLinkType"></param>
        public virtual void AddGroupIdentityLink(string groupId, string identityLinkType)
        {
            Context.CommandContext.IdentityLinkEntityManager.AddGroupIdentityLink(this, groupId, identityLinkType);
        }
        /// <summary>
        /// 
        /// </summary>
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="groupId"></param>
        public virtual void DeleteCandidateGroup(string groupId)
        {
            DeleteGroupIdentityLink(groupId, IdentityLinkType.CANDIDATE);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="userId"></param>
        public virtual void DeleteCandidateUser(string userId)
        {
            DeleteUserIdentityLink(userId, IdentityLinkType.CANDIDATE);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="identityLinkType"></param>
        public virtual void DeleteGroupIdentityLink(string groupId, string identityLinkType)
        {
            if (groupId is object)
            {
                Context.CommandContext.IdentityLinkEntityManager.DeleteIdentityLink(this, null, groupId, identityLinkType);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="identityLinkType"></param>
        public virtual void DeleteUserIdentityLink(string userId, string identityLinkType)
        {
            if (userId is object)
            {
                Context.CommandContext.IdentityLinkEntityManager.DeleteIdentityLink(this, userId, null, identityLinkType);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public virtual IList<IIdentityLinkEntity> IdentityLinks
        {
            get
            {
                var ctx = Context.CommandContext;
                if (!isIdentityLinksInitialized && ctx is object)
                {
                    taskIdentityLinkEntities = ctx.IdentityLinkEntityManager.FindIdentityLinksByTaskId(Id);
                    isIdentityLinksInitialized = true;
                }

                return taskIdentityLinkEntities;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public virtual IDictionary<string, object> ExecutionVariables
        {
            set
            {
                if (Execution is object)
                {
                    execution.Variables = value;
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
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
        /// <summary>
        /// 
        /// </summary>
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
        /// <summary>
        /// 
        /// </summary>
        public virtual string Assignee
        {
            set
            {
                this.originalAssignee = this.assignee;
                this.assignee = string.Empty.Equals(value?.Trim()) ? null : value?.Trim();
                assigneeUpdatedCount++;
            }
            get
            {
                return assignee;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public virtual string AssigneeUser
        {
            get; set;
        }

        internal static IEnumerable<ITask> EnsureAssignerInitialized(IEnumerable<TaskEntityImpl> tasks)
        {
            if (tasks is null || tasks.Count() == 0)
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
                if (variable is object)
                {
                    task.assigner = variable.Value as IUserInfo;
                }
                else
                {
                    task.assigner = new UserInfo
                    {
                        Id = task.assignee,
                        FullName = task.assignee,
                        TenantId = task.tenantId
                    };
                }

                yield return task;
            }

            yield break;
        }

        private IUserInfo EnsureAssignerInitialized()
        {
            if (assignee is null)
            {
                assigner = null;

                return null;
            }

            if (Context.CommandContext is object && (assigner is null || assigner.Id != this.assignee))
            {
                if (this.VariablesLocal.TryGetValue(this.assignee, out var userInfo) && userInfo is object)
                {
                    assigner = JToken.FromObject(userInfo).ToObject<UserInfo>();

                    return assigner;
                }
            }

            assigner = new UserInfo
            {
                Id = assignee,
                FullName = assignee,
                TenantId = this.tenantId
            };

            return assigner;
        }

        internal IUserInfo assigner = null;
        /// <summary>
        /// 
        /// </summary>
        public virtual IUserInfo Assigner
        {
            get
            {
                if (assigner is null)
                {
                    assigner = EnsureAssignerInitialized();
                }

                return assigner;
            }
        }
        /// <summary>
        /// 
        /// </summary>
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
        /// <summary>
        /// 
        /// </summary>
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
        /// <summary>
        /// 
        /// </summary>
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
        /// <summary>
        /// 
        /// </summary>
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
        /// <summary>
        /// 
        /// </summary>
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
        /// <summary>
        /// 
        /// </summary>
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
        /// <summary>
        /// 
        /// </summary>
        protected internal override bool ActivityIdUsedForDetails
        {
            get
            {
                return false;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="variableName"></param>
        /// <returns></returns>
        // Overridden to avoid fetching *all* variables (as is the case in the super // call)
        protected internal override IVariableInstanceEntity GetSpecificVariable(string variableName)
        {
            ICommandContext commandContext = Context.CommandContext;
            if (commandContext is null)
            {
                throw new ActivitiException("lazy loading outside command context");
            }
            IVariableInstanceEntity variableInstance = commandContext.VariableInstanceEntityManager.FindVariableInstanceByTaskAndName(Id, variableName);

            return variableInstance;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="variableNames"></param>
        /// <returns></returns>
        protected internal override IList<IVariableInstanceEntity> GetSpecificVariables(IEnumerable<string> variableNames)
        {
            ICommandContext commandContext = Context.CommandContext;
            if (commandContext is null)
            {
                throw new ActivitiException("lazy loading outside command context");
            }
            return commandContext.VariableInstanceEntityManager.FindVariableInstancesByTaskAndNames(Id, variableNames);
        }

        // regular getters and setters ////////////////////////////////////////////////////////
        /// <summary>
        /// 
        /// </summary>
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
        /// <summary>
        /// 
        /// </summary>
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

        /// <summary>
        /// 
        /// </summary>
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

        /// <summary>
        /// 
        /// </summary>
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

        /// <summary>
        /// 
        /// </summary>
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
        /// <summary>
        /// 
        /// </summary>
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


        /// <summary>
        /// 
        /// </summary>
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
        /// <summary>
        /// 
        /// </summary>
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

        /// <summary>
        /// 
        /// </summary>
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

        /// <summary>
        /// 
        /// </summary>
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


        /// <summary>
        /// 
        /// </summary>
        public virtual IExecutionEntity ProcessInstance
        {
            get
            {
                var ctx = Context.CommandContext;
                if (processInstance is null && processInstanceId is object && ctx is object)
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

        /// <summary>
        /// 
        /// </summary>
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

        /// <summary>
        /// 
        /// </summary>
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

        /// <summary>
        /// 
        /// </summary>
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

        /// <summary>
        /// 
        /// </summary>
        public override IDictionary<string, IVariableInstanceEntity> VariableInstanceEntities
        {
            get
            {
                EnsureVariableInstancesInitialized();
                return variableInstances;
            }
        }
        /// <summary>
        /// 
        /// </summary>
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
        /// <summary>
        /// 
        /// </summary>
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
        /// <summary>
        /// 
        /// </summary>
        public virtual IDictionary<string, object> TaskLocalVariables
        {
            get
            {
                IDictionary<string, object> variables = new Dictionary<string, object>();
                if (queryVariables is object)
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

        /// <summary>
        /// 
        /// </summary>
        public virtual IDictionary<string, object> ProcessVariables
        {
            get
            {
                IDictionary<string, object> variables = new Dictionary<string, object>();
                if (queryVariables is object)
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
        /// <summary>
        /// 
        /// </summary>
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
        /// <summary>
        /// 
        /// </summary>
        public virtual IList<IVariableInstanceEntity> QueryVariables
        {
            get
            {
                if (queryVariables is null && Context.CommandContext is object)
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

        /// <summary>
        /// 
        /// </summary>
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
        /// <summary>
        /// 
        /// </summary>
        public bool? IsAppend { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool? IsTransfer { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool? CanTransfer { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool? OnlyAssignee { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool? IsRuntime { get; set; }
        /// <summary>
        /// 
        /// </summary>
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
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "Task[id=" + Id + ", name=" + name + "]";
        }
    }

}