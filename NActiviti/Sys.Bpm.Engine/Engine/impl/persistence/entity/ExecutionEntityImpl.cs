using Newtonsoft.Json.Linq;
using Sys.Net.Http;
using Sys.Workflow.Bpmn.Models;
using Sys.Workflow.Engine.Delegate.Events;
using Sys.Workflow.Engine.Delegate.Events.Impl;
using Sys.Workflow.Engine.Impl.Cfg;
using Sys.Workflow.Engine.Impl.Contexts;
using Sys.Workflow.Engine.Impl.Interceptor;
using Sys.Workflow.Engine.Impl.Util;
using Sys.Workflow.Engine.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
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
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class ExecutionEntityImpl : VariableScopeImpl, IExecutionEntity, ICountingExecutionEntity
    {

        private const long serialVersionUID = 1L;

        // current position /////////////////////////////////////////////////////////

        private FlowElement currentFlowElement;
        private ActivitiListener currentActivitiListener; // Only set when executing an execution listener

        /// <summary>
        /// the process instance. this is the root of the execution tree. the processInstance of a process instance is a self reference.
        /// </summary>
        private IExecutionEntity processInstance;

        /// <summary>
        /// the parent execution </summary>
        private IExecutionEntity parent;

        /// <summary>
        /// nested executions representing scopes or concurrent paths </summary>
        private IList<IExecutionEntity> executions;

        /// <summary>
        /// super execution, not-null if this execution is part of a subprocess </summary>
        private IExecutionEntity superExecution;

        /// <summary>
        /// reference to a subprocessinstance, not-null if currently subprocess is started from this execution </summary>
        private IExecutionEntity subProcessInstance;

        /// <summary>
        /// The tenant identifier (if any) </summary>
        private string tenantId = ProcessEngineConfiguration.NO_TENANT_ID;
        private string name;
        private string description;
        private string localizedName;
        private string localizedDescription;

        private DateTime? lockTime;

        // state/type of execution //////////////////////////////////////////////////

        private bool isActive = true;
        private bool isScope = true;
        private bool isConcurrent;
        private bool isEnded;
        private bool isEventScope;
        private bool isMultiInstanceRoot;
        private bool isCountEnabled;

        // events ///////////////////////////////////////////////////////////////////

        // TODO: still needed in v6?

        private string eventName;

        // associated entities /////////////////////////////////////////////////////

        // (we cache associated entities here to minimize db queries)
        private IList<IEventSubscriptionEntity> eventSubscriptions;
        private IList<IJobEntity> jobs;
        private IList<ITimerJobEntity> timerJobs;
        private IList<ITaskEntity> tasks;
        private IList<IIdentityLinkEntity> identityLinks;

        // cascade deletion ////////////////////////////////////////////////////////

        private string deleteReason;

        private int suspensionState = SuspensionStateProvider.ACTIVE.StateCode;

        private string startUserId;
        private DateTime startTime;

        // CountingExecutionEntity 
        private int eventSubscriptionCount;
        private int taskCount;
        private int jobCount;
        private int timerJobCount;
        private int suspendedJobCount;
        private int deadLetterJobCount;
        private int variableCount;
        private int identityLinkCount;

        /// <summary>
        /// persisted reference to the processDefinition.
        /// </summary>
        /// <seealso cref= #processDefinition </seealso>
        /// <seealso cref= #setProcessDefinition(ProcessDefinitionImpl) </seealso>
        /// <seealso cref= #getProcessDefinition() </seealso>
        private string processDefinitionId;

        /// <summary>
        /// persisted reference to the process definition key.
        /// </summary>
        private string processDefinitionKey;

        /// <summary>
        /// persisted reference to the process definition name.
        /// </summary>
        private string processDefinitionName;

        /// <summary>
        /// persisted reference to the process definition version.
        /// </summary>
        private int? processDefinitionVersion;

        /// <summary>
        /// persisted reference to the deployment id.
        /// </summary>
        private string deploymentId;

        /// <summary>
        /// persisted reference to the current position in the diagram within the <seealso cref="#processDefinition"/>.
        /// </summary>
        /// <seealso cref= #activity </seealso>
        /// <seealso cref= #setActivity(ActivityImpl) </seealso>
        /// <seealso cref= #getActivity() </seealso>
        private string activityId;

        /// <summary>
        /// The name of the current activity position
        /// </summary>
        private string activityName;

        /// <summary>
        /// persisted reference to the process instance.
        /// </summary>
        /// <seealso cref= #getProcessInstance() </seealso>
        private string processInstanceId;

        /// <summary>
        /// persisted reference to the business key.
        /// </summary>
        private string businessKey;

        /// <summary>
        /// persisted reference to the parent of this execution.
        /// </summary>
        /// <seealso cref= #getParent() </seealso>
        /// <seealso cref= #setParentId(String) </seealso>
        private string parentId;

        /// <summary>
        /// persisted reference to the super execution of this execution
        /// 
        /// @See <seealso cref="#getSuperExecution()"/> </summary>
        /// <seealso cref= #setSuperExecution(ExecutionEntityImpl) </seealso>
        private string superExecutionId;

        private string rootProcessInstanceId;
        private ExecutionEntityImpl rootProcessInstance;

        private bool forcedUpdate;

        private IList<IVariableInstanceEntity> queryVariables;
        /// <summary>
        /// 
        /// </summary>
        public ExecutionEntityImpl()
        {

        }

        /// <summary>
        /// Static factory method: to be used when a new execution is created for the very first time/
        /// Calling this will make sure no extra db fetches are needed later on, as all collections
        /// will be populated with empty collections. If they would be null, it would trigger
        /// a database fetch for those relationship entities.
        /// </summary>
        public static ExecutionEntityImpl CreateWithEmptyRelationshipCollections()
        {
            ExecutionEntityImpl execution = new ExecutionEntityImpl
            {
                executions = new List<IExecutionEntity>(1),
                tasks = new List<ITaskEntity>(1),
                variableInstances = new Dictionary<string, IVariableInstanceEntity>(1),
                jobs = new List<IJobEntity>(1),
                timerJobs = new List<ITimerJobEntity>(1),
                eventSubscriptions = new List<IEventSubscriptionEntity>(1),
                identityLinks = new List<IIdentityLinkEntity>(1)
            };
            return execution;
        }


        //persistent state /////////////////////////////////////////////////////////
        /// <summary>
        /// 
        /// </summary>
        public override PersistentState PersistentState
        {
            get
            {
                PersistentState persistentState = new PersistentState
                {
                    ["processDefinitionId"] = this.processDefinitionId,
                    ["businessKey"] = this.businessKey,
                    ["activityId"] = this.activityId,
                    ["isActive"] = this.isActive,
                    ["isConcurrent"] = this.isConcurrent,
                    ["isScope"] = this.isScope,
                    ["isEventScope"] = this.isEventScope,
                    ["parentId"] = parentId,
                    ["name"] = name,
                    ["lockTime"] = lockTime,
                    ["superExecution"] = this.superExecutionId,
                    ["rootProcessInstanceId"] = this.rootProcessInstanceId
                };
                if (forcedUpdate)
                {
                    persistentState["forcedUpdate"] = true;
                }
                persistentState["suspensionState"] = this.suspensionState;
                persistentState["startTime"] = this.startTime;
                persistentState["startUserId"] = this.startUserId;
                persistentState["eventSubscriptionCount"] = eventSubscriptionCount;
                persistentState["taskCount"] = taskCount;
                persistentState["jobCount"] = jobCount;
                persistentState["timerJobCount"] = timerJobCount;
                persistentState["suspendedJobCount"] = suspendedJobCount;
                persistentState["deadLetterJobCount"] = deadLetterJobCount;
                persistentState["variableCount"] = variableCount;
                persistentState["identityLinkCount"] = identityLinkCount;
                return persistentState;
            }
        }

        // The current flow element, will be filled during operation execution
        /// <summary>
        /// 
        /// </summary>
        public virtual FlowElement CurrentFlowElement
        {
            get
            {
                if (currentFlowElement is null)
                {
                    string processDefinitionId = ProcessDefinitionId;
                    if (!string.IsNullOrWhiteSpace(processDefinitionId))
                    {
                        Process process = ProcessDefinitionUtil.GetProcess(processDefinitionId);
                        currentFlowElement = process.GetFlowElement(CurrentActivityId, true);
                    }
                }
                return currentFlowElement;
            }
            set
            {
                this.currentFlowElement = value;
                if (value != null)
                {
                    this.activityId = value.Id;
                }
                else
                {
                    this.activityId = null;
                }
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


        // executions ///////////////////////////////////////////////////////////////

        /// <summary>
        /// ensures initialization and returns the non-null executions list </summary>
        public virtual IList<IExecutionEntity> Executions
        {
            get
            {
                EnsureExecutionsInitialized();
                return executions;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="executionEntity"></param>
        public virtual void AddChildExecution(IExecutionEntity executionEntity)
        {
            EnsureExecutionsInitialized();
            executions.Add((ExecutionEntityImpl)executionEntity);
        }

        /// <summary>
        /// 
        /// </summary>
        protected internal virtual void EnsureExecutionsInitialized()
        {
            var ctx = Context.CommandContext;
            if (executions is null && ctx != null)
            {
                this.executions = ctx.ExecutionEntityManager.FindChildExecutionsByParentExecutionId(Id);
            }
        }

        // business key ////////////////////////////////////////////////////////////
        /// <summary>
        /// 
        /// </summary>
        public virtual string BusinessKey
        {
            get
            {
                return businessKey;
            }
            set
            {
                this.businessKey = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual string ProcessInstanceBusinessKey
        {
            get
            {
                return ProcessInstance?.BusinessKey;
            }
        }

        // process definition ///////////////////////////////////////////////////////
        /// <summary>
        /// 
        /// </summary>
        public virtual string ProcessDefinitionId
        {
            set
            {
                this.processDefinitionId = value;
            }
            get
            {
                return processDefinitionId;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual string ProcessDefinitionKey
        {
            get
            {
                return processDefinitionKey;
            }
            set
            {
                this.processDefinitionKey = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual string ProcessDefinitionName
        {
            get
            {
                return processDefinitionName;
            }
            set
            {
                this.processDefinitionName = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual int? ProcessDefinitionVersion
        {
            get
            {
                return processDefinitionVersion;
            }
            set
            {
                this.processDefinitionVersion = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual string DeploymentId
        {
            get
            {
                return deploymentId;
            }
            set
            {
                this.deploymentId = value;
            }
        }


        // process instance /////////////////////////////////////////////////////////

        /// <summary>
        /// ensures initialization and returns the process instance. </summary>
        public virtual IExecutionEntity ProcessInstance
        {
            get
            {
                EnsureProcessInstanceInitialized();
                return processInstance;
            }
            set
            {
                this.processInstance = (ExecutionEntityImpl)processInstance;
                if (processInstance != null)
                {
                    this.processInstanceId = this.processInstance.Id;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected internal virtual void EnsureProcessInstanceInitialized()
        {
            var ctx = Context.CommandContext;
            if (processInstance is null && processInstanceId != null && ctx != null)
            {
                processInstance = ctx.ExecutionEntityManager.FindById<ExecutionEntityImpl>(processInstanceId);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public virtual bool ProcessInstanceType
        {
            get
            {
                return parentId is null;
            }
        }

        // parent ///////////////////////////////////////////////////////////////////

        /// <summary>
        /// ensures initialization and returns the parent </summary>
        public virtual IExecutionEntity Parent
        {
            get
            {
                EnsureParentInitialized();
                return parent;
            }
            set
            {
                this.parent = value;
                if (this.parentId != value?.Id)
                {
                    this.parentId = value?.Id;
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        protected internal virtual void EnsureParentInitialized()
        {
            var ctx = Context.CommandContext;
            if (parent is null && parentId != null && ctx != null)
            {
                parent = ctx.ExecutionEntityManager.FindById<ExecutionEntityImpl>(parentId);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="parent"></param>
        public virtual void SetParent(IExecutionEntity parent)
        {
            this.parent = (ExecutionEntityImpl)parent;

            if (parent != null)
            {
                this.parentId = parent.Id;
            }
            else
            {
                this.parentId = null;
            }
        }

        // super- and subprocess executions /////////////////////////////////////////
        /// <summary>
        /// 
        /// </summary>
        public virtual string SuperExecutionId
        {
            get
            {
                return superExecutionId;
            }
            set
            {
                superExecutionId = value;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public virtual IExecutionEntity SuperExecution
        {
            get
            {
                EnsureSuperExecutionInitialized();
                return superExecution;
            }
            set
            {
                this.superExecution = value;

                if (superExecution != null)
                {
                    superExecution.SubProcessInstance = null;
                }

                if (superExecution != null)
                {
                    this.superExecutionId = ((ExecutionEntityImpl)superExecution).Id;
                }
                else
                {
                    this.superExecutionId = null;
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        protected internal virtual void EnsureSuperExecutionInitialized()
        {
            var ctx = Context.CommandContext;
            if (superExecution is null && superExecutionId != null && ctx != null)
            {
                superExecution = ctx.ExecutionEntityManager.FindById<ExecutionEntityImpl>(superExecutionId);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public virtual IExecutionEntity SubProcessInstance
        {
            get
            {
                EnsureSubProcessInstanceInitialized();
                return subProcessInstance;
            }
            set
            {
                this.subProcessInstance = value;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        protected internal virtual void EnsureSubProcessInstanceInitialized()
        {
            var ctx = Context.CommandContext;
            if (subProcessInstance is null && ctx != null)
            {
                subProcessInstance = ctx.ExecutionEntityManager.FindSubProcessInstanceBySuperExecutionId(Id);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public virtual IExecutionEntity RootProcessInstance
        {
            get
            {
                EnsureRootProcessInstanceInitialized();
                return rootProcessInstance;
            }
            set
            {
                this.rootProcessInstance = (ExecutionEntityImpl)value;

                if (value != null)
                {
                    this.rootProcessInstanceId = value.Id;
                }
                else
                {
                    this.rootProcessInstanceId = null;
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        protected internal virtual void EnsureRootProcessInstanceInitialized()
        {
            var ctx = Context.CommandContext;
            if (rootProcessInstanceId is null && ctx != null)
            {
                rootProcessInstance = ctx.ExecutionEntityManager.FindById<ExecutionEntityImpl>(rootProcessInstanceId);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual string RootProcessInstanceId
        {
            get
            {
                return rootProcessInstanceId;
            }
            set
            {
                this.rootProcessInstanceId = value;
            }
        }


        // scopes ///////////////////////////////////////////////////////////////////
        /// <summary>
        /// 
        /// </summary>
        public virtual bool IsScope
        {
            get
            {
                return isScope;
            }
            set
            {
                this.isScope = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void ForceUpdate()
        {
            this.forcedUpdate = true;
        }

        // VariableScopeImpl methods //////////////////////////////////////////////////////////////////


        /// <summary>
        /// 
        /// </summary>
        // TODO: this should ideally move to another place  
        protected internal override void InitializeVariableInstanceBackPointer(IVariableInstanceEntity variableInstance)
        {
            if (processInstanceId is object)
            {
                variableInstance.ProcessInstanceId = processInstanceId;
            }
            else
            {
                variableInstance.ProcessInstanceId = Id;
            }
            variableInstance.ExecutionId = Id;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected internal override IList<IVariableInstanceEntity> LoadVariableInstances()
        {
            return Context.CommandContext.VariableInstanceEntityManager.FindVariableInstancesByExecutionId(Id) as IList<IVariableInstanceEntity>;
        }
        /// <summary>
        /// 
        /// </summary>
        protected internal override VariableScopeImpl ParentVariableScope
        {
            get
            {
                return Parent as VariableScopeImpl;
            }
        }

        /// <summary>
        /// used to calculate the sourceActivityExecution for method <seealso cref="#updateActivityInstanceIdInHistoricVariableUpdate(HistoricDetailVariableInstanceUpdateEntity, ExecutionEntityImpl)"/>
        /// </summary>
        protected internal override IExecutionEntity SourceActivityExecution
        {
            get
            {
                return this;
            }
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="variableName"></param>
        /// <returns></returns>
        protected internal override IVariableInstanceEntity GetSpecificVariable(string variableName)
        {

            ICommandContext commandContext = Context.CommandContext;
            if (commandContext is null)
            {
                throw new ActivitiException("lazy loading outside command context");
            }
            IVariableInstanceEntity variableInstance = commandContext.VariableInstanceEntityManager.FindVariableInstanceByExecutionAndName(Id, variableName);

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
            return commandContext.VariableInstanceEntityManager.FindVariableInstancesByExecutionAndNames(Id, variableNames);
        }

        // event subscription support //////////////////////////////////////////////
        /// <summary>
        /// 
        /// </summary>
        public virtual IList<IEventSubscriptionEntity> EventSubscriptions
        {
            get
            {
                EnsureEventSubscriptionsInitialized();
                return eventSubscriptions;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        protected internal virtual void EnsureEventSubscriptionsInitialized()
        {
            var ctx = Context.CommandContext;
            if (eventSubscriptions is null && ctx != null)
            {
                eventSubscriptions = ctx.EventSubscriptionEntityManager.FindEventSubscriptionsByExecution(Id);
            }
        }

        // referenced job entities //////////////////////////////////////////////////
        /// <summary>
        /// 
        /// </summary>
        public virtual IList<IJobEntity> Jobs
        {
            get
            {
                EnsureJobsInitialized();
                return jobs;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        protected internal virtual void EnsureJobsInitialized()
        {
            var ctx = Context.CommandContext;
            if (jobs is null && ctx != null)
            {
                jobs = ctx.JobEntityManager.FindJobsByExecutionId(Id);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public virtual IList<ITimerJobEntity> TimerJobs
        {
            get
            {
                EnsureTimerJobsInitialized();
                return timerJobs;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        protected internal virtual void EnsureTimerJobsInitialized()
        {
            var ctx = Context.CommandContext;
            if (timerJobs is null && ctx != null)
            {
                timerJobs = ctx.TimerJobEntityManager.FindJobsByExecutionId(Id);
            }
        }

        // referenced task entities ///////////////////////////////////////////////////
        /// <summary>
        /// 
        /// </summary>
        protected internal virtual void EnsureTasksInitialized()
        {
            var ctx = Context.CommandContext;
            if (tasks is null && ctx != null)
            {
                tasks = ctx.TaskEntityManager.FindTasksByExecutionId(Id);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public virtual IList<ITaskEntity> Tasks
        {
            get
            {
                EnsureTasksInitialized();
                return tasks;
            }
        }

        // identity links ///////////////////////////////////////////////////////////
        /// <summary>
        /// 
        /// </summary>
        public virtual IList<IIdentityLinkEntity> IdentityLinks
        {
            get
            {
                EnsureIdentityLinksInitialized();
                return identityLinks;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        protected internal virtual void EnsureIdentityLinksInitialized()
        {
            var ctx = Context.CommandContext;
            if (identityLinks is null && ctx != null)
            {
                identityLinks = ctx.IdentityLinkEntityManager.FindIdentityLinksByProcessInstanceId(Id);
            }
        }

        // getters and setters //////////////////////////////////////////////////////
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

        public virtual string ParentId
        {
            get
            {
                return parentId;
            }
            set
            {
                this.parentId = value;
            }
        }
        /// <summary>
        /// 
        /// </summary>

        public virtual string ActivityId
        {
            get
            {
                return activityId;
            }
            set
            {
                if (activityId is null)
                {
                    activityId = value;
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public virtual bool IsConcurrent
        {
            get
            {
                return isConcurrent;
            }
            set
            {
                this.isConcurrent = value;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public virtual bool IsActive
        {
            get
            {
                return isActive;
            }
            set
            {
                this.isActive = value;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public virtual void Inactivate()
        {
            this.isActive = false;
        }
        /// <summary>
        /// 
        /// </summary>
        public virtual bool Ended
        {
            get
            {
                return isEnded;
            }
            set
            {
                this.isEnded = value;
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
        public virtual string DeleteReason
        {
            get
            {
                return deleteReason;
            }
            set
            {
                this.deleteReason = value;
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
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual bool IsEventScope
        {
            get
            {
                return isEventScope;
            }
            set
            {
                this.isEventScope = value;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        public virtual bool IsMultiInstanceRoot
        {
            get
            {
                return isMultiInstanceRoot;
            }
            set
            {
                this.isMultiInstanceRoot = value;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        public virtual bool IsCountEnabled
        {
            get
            {
                return isCountEnabled;
            }
            set
            {
                this.isCountEnabled = value;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        public virtual string CurrentActivityId
        {
            get
            {
                return activityId;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual string Name
        {
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
            set
            {
                this.name = value;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        public virtual string Description
        {
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
            set
            {
                this.description = value;
            }
        }


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
        public virtual DateTime? LockTime
        {
            get
            {
                return lockTime;
            }
            set
            {
                this.lockTime = value;
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

        /// <summary>
        /// 
        /// </summary>
        public virtual IList<IVariableInstanceEntity> QueryVariables
        {
            get
            {
                if (queryVariables is null && Context.CommandContext != null)
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
        public virtual string ActivityName
        {
            get
            {
                return activityName;
            }
            set
            {
                if (activityName is null)
                {
                    activityName = value;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual string StartUserId
        {
            get
            {
                return startUserId;
            }
            set
            {
                this.startUserId = value;
            }
        }

        internal static IEnumerable<IProcessInstance> EnsureStarterInitialized(IEnumerable<ExecutionEntityImpl> insts)
        {
            if (insts is null || insts.Count() == 0)
            {
                yield break;
            }

            //foreach (var inst in insts)
            //{
            //    inst.EnsureStarterInitialized();

            //    yield return inst;
            //}

            IList<IVariableInstanceEntity> vars = Context.ProcessEngineConfiguration.VariableInstanceEntityManager.FindVariableInstancesByExecutionIds(insts.Select(x => x.Id).ToArray());

            var variables = vars.Where(x => insts.Any(y => y.startUserId == x.Name))
                .Select(x => new
                {
                    x.ExecutionId,
                    x.Name,
                    x.Value
                }).ToList();

            var executions = insts.Cast<ExecutionEntityImpl>();
            foreach (ExecutionEntityImpl inst in executions)
            {
                var variable = variables.FirstOrDefault(x => x.ExecutionId == inst.Id && x.Name == inst.StartUserId);
                if (variable != null)
                {
                    inst.starter = variable.Value as UserInfo;
                }
                else
                {
                    inst.starter = new UserInfo
                    {
                        Id = inst.startUserId
                    };
                }

                yield return inst;
            }

            yield break;
        }

        private IUserInfo EnsureStarterInitialized()
        {
            if (startUserId is null)
            {
                starter = null;
                return null;
            }

            //由于缓存问题，先不要从变量里获取用户
            if (Context.CommandContext != null && (starter is null || starter.Id != this.startUserId))
            {
                if (this.VariablesLocal.TryGetValue(this.startUserId, out var userInfo) && userInfo != null)
                {
                    starter = JToken.FromObject(userInfo).ToObject<UserInfo>();

                    return starter;
                }
            }

            starter = new UserInfo
            {
                Id = startUserId,
                FullName = startUserId,
                TenantId = this.tenantId
            };

            return starter;
        }

        private IUserInfo starter = null;
        /// <summary>
        /// 
        /// </summary>
        public virtual IUserInfo Starter
        {
            get
            {
                if (starter is null)
                {
                    starter = EnsureStarterInitialized();
                }

                return starter;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public virtual string StartUser
        {
            get; set;
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual DateTime StartTime
        {
            get
            {
                return startTime;
            }
            set
            {
                this.startTime = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual int EventSubscriptionCount
        {
            get
            {
                return eventSubscriptionCount;
            }
            set
            {
                this.eventSubscriptionCount = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual int TaskCount
        {
            get
            {
                return taskCount;
            }
            set
            {
                this.taskCount = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual int JobCount
        {
            get
            {
                return jobCount;
            }
            set
            {
                this.jobCount = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual int TimerJobCount
        {
            get
            {
                return timerJobCount;
            }
            set
            {
                this.timerJobCount = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual int SuspendedJobCount
        {
            get
            {
                return suspendedJobCount;
            }
            set
            {
                this.suspendedJobCount = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual int DeadLetterJobCount
        {
            get
            {
                return deadLetterJobCount;
            }
            set
            {
                this.deadLetterJobCount = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual int VariableCount
        {
            get
            {
                return variableCount;
            }
            set
            {
                this.variableCount = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual int IdentityLinkCount
        {
            get
            {
                return identityLinkCount;
            }
            set
            {
                this.identityLinkCount = value;
            }
        }

        //toString /////////////////////////////////////////////////////////////////
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (ProcessInstanceType)
            {
                return "ProcessInstance[" + Id + "]";
            }
            else
            {
                StringBuilder strb = new StringBuilder();
                if (isScope)
                {
                    strb.Append("Scoped execution[ id '" + Id + "' ]");
                }
                else if (isMultiInstanceRoot)
                {
                    strb.Append("Multi instance root execution[ id '" + Id + "' ]");
                }
                else
                {
                    strb.Append("Execution[ id '" + Id + "' ]");
                }
                if (activityId is object)
                {
                    strb.Append(" - activity '" + activityId);
                }
                if (parentId is object)
                {
                    strb.Append(" - parent '" + parentId + "'");
                }
                return strb.ToString();
            }
        }
    }

}