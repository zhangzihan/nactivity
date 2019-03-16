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

namespace org.activiti.engine.impl.persistence.entity
{

    using org.activiti.bpmn.model;
    using org.activiti.engine.@delegate;
    using org.activiti.engine.@delegate.@event;
    using org.activiti.engine.@delegate.@event.impl;
    using org.activiti.engine.impl.context;
    using org.activiti.engine.impl.interceptor;
    using org.activiti.engine.impl.util;

    /// 
    /// 
    /// 
    /// 
    /// 

    [Serializable]
    public class ExecutionEntityImpl : VariableScopeImpl, IExecutionEntity, ICountingExecutionEntity
    {

        private const long serialVersionUID = 1L;

        // current position /////////////////////////////////////////////////////////

        protected internal FlowElement currentFlowElement;
        protected internal ActivitiListener currentActivitiListener; // Only set when executing an execution listener

        /// <summary>
        /// the process instance. this is the root of the execution tree. the processInstance of a process instance is a self reference.
        /// </summary>
        protected internal IExecutionEntity processInstance;

        /// <summary>
        /// the parent execution </summary>
        protected internal IExecutionEntity parent;

        /// <summary>
        /// nested executions representing scopes or concurrent paths </summary>
        protected internal IList<IExecutionEntity> executions;

        /// <summary>
        /// super execution, not-null if this execution is part of a subprocess </summary>
        protected internal IExecutionEntity superExecution;

        /// <summary>
        /// reference to a subprocessinstance, not-null if currently subprocess is started from this execution </summary>
        protected internal IExecutionEntity subProcessInstance;

        /// <summary>
        /// The tenant identifier (if any) </summary>
        protected internal string tenantId = ProcessEngineConfiguration.NO_TENANT_ID;
        protected internal string name;
        protected internal string description;
        protected internal string localizedName;
        protected internal string localizedDescription;

        protected internal DateTime? lockTime;

        // state/type of execution //////////////////////////////////////////////////

        protected internal bool isActive = true;
        protected internal bool isScope = true;
        protected internal bool isConcurrent;
        protected internal bool isEnded;
        protected internal bool isEventScope;
        protected internal bool isMultiInstanceRoot;
        protected internal bool isCountEnabled;

        // events ///////////////////////////////////////////////////////////////////

        // TODO: still needed in v6?

        protected internal string eventName;

        // associated entities /////////////////////////////////////////////////////

        // (we cache associated entities here to minimize db queries)
        protected internal IList<IEventSubscriptionEntity> eventSubscriptions;
        protected internal IList<IJobEntity> jobs;
        protected internal IList<ITimerJobEntity> timerJobs;
        protected internal IList<ITaskEntity> tasks;
        protected internal IList<IIdentityLinkEntity> identityLinks;

        // cascade deletion ////////////////////////////////////////////////////////

        protected internal string deleteReason;

        protected internal int suspensionState = SuspensionStateProvider.ACTIVE.StateCode;

        protected internal string startUserId;
        protected internal DateTime startTime;

        // CountingExecutionEntity 
        protected internal int eventSubscriptionCount;
        protected internal int taskCount;
        protected internal int jobCount;
        protected internal int timerJobCount;
        protected internal int suspendedJobCount;
        protected internal int deadLetterJobCount;
        protected internal int variableCount;
        protected internal int identityLinkCount;

        /// <summary>
        /// persisted reference to the processDefinition.
        /// </summary>
        /// <seealso cref= #processDefinition </seealso>
        /// <seealso cref= #setProcessDefinition(ProcessDefinitionImpl) </seealso>
        /// <seealso cref= #getProcessDefinition() </seealso>
        protected internal string processDefinitionId;

        /// <summary>
        /// persisted reference to the process definition key.
        /// </summary>
        protected internal string processDefinitionKey;

        /// <summary>
        /// persisted reference to the process definition name.
        /// </summary>
        protected internal string processDefinitionName;

        /// <summary>
        /// persisted reference to the process definition version.
        /// </summary>
        protected internal int? processDefinitionVersion;

        /// <summary>
        /// persisted reference to the deployment id.
        /// </summary>
        protected internal string deploymentId;

        /// <summary>
        /// persisted reference to the current position in the diagram within the <seealso cref="#processDefinition"/>.
        /// </summary>
        /// <seealso cref= #activity </seealso>
        /// <seealso cref= #setActivity(ActivityImpl) </seealso>
        /// <seealso cref= #getActivity() </seealso>
        protected internal string activityId;

        /// <summary>
        /// The name of the current activity position
        /// </summary>
        protected internal string activityName;

        /// <summary>
        /// persisted reference to the process instance.
        /// </summary>
        /// <seealso cref= #getProcessInstance() </seealso>
        protected internal string processInstanceId;

        /// <summary>
        /// persisted reference to the business key.
        /// </summary>
        protected internal string businessKey;

        /// <summary>
        /// persisted reference to the parent of this execution.
        /// </summary>
        /// <seealso cref= #getParent() </seealso>
        /// <seealso cref= #setParentId(String) </seealso>
        protected internal string parentId;

        /// <summary>
        /// persisted reference to the super execution of this execution
        /// 
        /// @See <seealso cref="#getSuperExecution()"/> </summary>
        /// <seealso cref= #setSuperExecution(ExecutionEntityImpl) </seealso>
        protected internal string superExecutionId;

        protected internal string rootProcessInstanceId;
        protected internal ExecutionEntityImpl rootProcessInstance;

        protected internal bool forcedUpdate;

        protected internal IList<IVariableInstanceEntity> queryVariables;

        protected internal new bool isDeleted; // TODO: should be in entity superclass probably

        public ExecutionEntityImpl()
        {

        }

        /// <summary>
        /// Static factory method: to be used when a new execution is created for the very first time/
        /// Calling this will make sure no extra db fetches are needed later on, as all collections
        /// will be populated with empty collections. If they would be null, it would trigger
        /// a database fetch for those relationship entities.
        /// </summary>
        public static ExecutionEntityImpl createWithEmptyRelationshipCollections()
        {
            ExecutionEntityImpl execution = new ExecutionEntityImpl();
            execution.executions = new List<IExecutionEntity>(1);
            execution.tasks = new List<ITaskEntity>(1);
            execution.variableInstances = new Dictionary<string, IVariableInstanceEntity>(1);
            execution.jobs = new List<IJobEntity>(1);
            execution.timerJobs = new List<ITimerJobEntity>(1);
            execution.eventSubscriptions = new List<IEventSubscriptionEntity>(1);
            execution.identityLinks = new List<IIdentityLinkEntity>(1);
            return execution;
        }


        //persistent state /////////////////////////////////////////////////////////

        public override PersistentState PersistentState
        {
            get
            {
                PersistentState persistentState = new PersistentState();
                persistentState["processDefinitionId"] = this.processDefinitionId;
                persistentState["businessKey"] = this.businessKey;
                persistentState["activityId"] = this.activityId;
                persistentState["isActive"] = this.isActive;
                persistentState["isConcurrent"] = this.isConcurrent;
                persistentState["isScope"] = this.isScope;
                persistentState["isEventScope"] = this.isEventScope;
                persistentState["parentId"] = parentId;
                persistentState["name"] = name;
                persistentState["lockTime"] = lockTime;
                persistentState["superExecution"] = this.superExecutionId;
                persistentState["rootProcessInstanceId"] = this.rootProcessInstanceId;
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

        public virtual FlowElement CurrentFlowElement
        {
            get
            {
                if (currentFlowElement == null)
                {
                    string processDefinitionId = ProcessDefinitionId;
                    if (!string.IsNullOrWhiteSpace(processDefinitionId))
                    {
                        org.activiti.bpmn.model.Process process = ProcessDefinitionUtil.getProcess(processDefinitionId);
                        currentFlowElement = process.getFlowElement(CurrentActivityId, true);
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
                ensureExecutionsInitialized();
                return executions;
            }
        }

        public virtual void addChildExecution(IExecutionEntity executionEntity)
        {
            ensureExecutionsInitialized();
            executions.Add((ExecutionEntityImpl)executionEntity);
        }

        protected internal virtual void ensureExecutionsInitialized()
        {
            var ctx = Context.CommandContext;
            if (executions == null && ctx != null)
            {
                this.executions = ctx.ExecutionEntityManager.findChildExecutionsByParentExecutionId(id);
            }
        }

        // business key ////////////////////////////////////////////////////////////

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


        public virtual string ProcessInstanceBusinessKey
        {
            get
            {
                return ProcessInstance?.BusinessKey;
            }
        }

        // process definition ///////////////////////////////////////////////////////

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
                ensureProcessInstanceInitialized();
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

        protected internal virtual void ensureProcessInstanceInitialized()
        {
            var ctx = Context.CommandContext;
            if (processInstance == null && processInstanceId != null && ctx != null)
            {
                processInstance = ctx.ExecutionEntityManager.findById<ExecutionEntityImpl>(processInstanceId);
            }
        }

        public virtual bool ProcessInstanceType
        {
            get
            {
                return ReferenceEquals(parentId, null);
            }
        }

        // parent ///////////////////////////////////////////////////////////////////

        /// <summary>
        /// ensures initialization and returns the parent </summary>
        public virtual IExecutionEntity Parent
        {
            get
            {
                ensureParentInitialized();
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

        protected internal virtual void ensureParentInitialized()
        {
            var ctx = Context.CommandContext;
            if (parent == null && parentId != null && ctx != null)
            {
                parent = ctx.ExecutionEntityManager.findById<ExecutionEntityImpl>(parentId);
            }
        }

        public virtual void setParent(IExecutionEntity parent)
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

        public virtual string SuperExecutionId
        {
            get
            {
                return superExecutionId;
            }
        }

        public virtual IExecutionEntity SuperExecution
        {
            get
            {
                ensureSuperExecutionInitialized();
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

        protected internal virtual void ensureSuperExecutionInitialized()
        {
            var ctx = Context.CommandContext;
            if (superExecution == null && superExecutionId != null && ctx != null)
            {
                superExecution = ctx.ExecutionEntityManager.findById<ExecutionEntityImpl>(superExecutionId);
            }
        }

        public virtual IExecutionEntity SubProcessInstance
        {
            get
            {
                ensureSubProcessInstanceInitialized();
                return subProcessInstance;
            }
            set
            {
                this.subProcessInstance = value;
            }
        }

        protected internal virtual void ensureSubProcessInstanceInitialized()
        {
            var ctx = Context.CommandContext;
            if (subProcessInstance == null && ctx != null)
            {
                subProcessInstance = ctx.ExecutionEntityManager.findSubProcessInstanceBySuperExecutionId(id);
            }
        }

        public virtual IExecutionEntity RootProcessInstance
        {
            get
            {
                ensureRootProcessInstanceInitialized();
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

        protected internal virtual void ensureRootProcessInstanceInitialized()
        {
            var ctx = Context.CommandContext;
            if (rootProcessInstanceId == null && ctx != null)
            {
                rootProcessInstance = ctx.ExecutionEntityManager.findById<ExecutionEntityImpl>(rootProcessInstanceId);
            }
        }


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


        public virtual void forceUpdate()
        {
            this.forcedUpdate = true;
        }

        // VariableScopeImpl methods //////////////////////////////////////////////////////////////////


        // TODO: this should ideally move to another place  
        protected internal override void initializeVariableInstanceBackPointer(IVariableInstanceEntity variableInstance)
        {
            if (!ReferenceEquals(processInstanceId, null))
            {
                variableInstance.ProcessInstanceId = processInstanceId;
            }
            else
            {
                variableInstance.ProcessInstanceId = id;
            }
            variableInstance.ExecutionId = id;
        }

        protected internal override IList<IVariableInstanceEntity> loadVariableInstances()
        {
            return Context.CommandContext.VariableInstanceEntityManager.findVariableInstancesByExecutionId(id) as IList<IVariableInstanceEntity>;
        }

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

        protected internal override IVariableInstanceEntity createVariableInstance(string variableName, object value, IExecutionEntity sourceActivityExecution)
        {
            IVariableInstanceEntity result = base.createVariableInstance(variableName, value, sourceActivityExecution);

            // Dispatch event, if needed
            if (Context.ProcessEngineConfiguration != null && Context.ProcessEngineConfiguration.EventDispatcher.Enabled)
            {
                Context.ProcessEngineConfiguration.EventDispatcher.dispatchEvent(ActivitiEventBuilder.createVariableEvent(ActivitiEventType.VARIABLE_CREATED, variableName, value, result.Type, result.TaskId, result.ExecutionId, ProcessInstanceId, ProcessDefinitionId));
            }
            return result;
        }

        protected internal override void updateVariableInstance(IVariableInstanceEntity variableInstance, object value, IExecutionEntity sourceActivityExecution)
        {
            base.updateVariableInstance(variableInstance, value, sourceActivityExecution);

            // Dispatch event, if needed
            if (Context.ProcessEngineConfiguration != null && Context.ProcessEngineConfiguration.EventDispatcher.Enabled)
            {
                Context.ProcessEngineConfiguration.EventDispatcher.dispatchEvent(ActivitiEventBuilder.createVariableEvent(ActivitiEventType.VARIABLE_UPDATED, variableInstance.Name, value, variableInstance.Type, variableInstance.TaskId, variableInstance.ExecutionId, ProcessInstanceId, ProcessDefinitionId));
            }
        }

        protected internal override IVariableInstanceEntity getSpecificVariable(string variableName)
        {

            ICommandContext commandContext = Context.CommandContext;
            if (commandContext == null)
            {
                throw new ActivitiException("lazy loading outside command context");
            }
            IVariableInstanceEntity variableInstance = commandContext.VariableInstanceEntityManager.findVariableInstanceByExecutionAndName(id, variableName);

            return variableInstance;
        }

        protected internal override IList<IVariableInstanceEntity> getSpecificVariables(ICollection<string> variableNames)
        {
            ICommandContext commandContext = Context.CommandContext;
            if (commandContext == null)
            {
                throw new ActivitiException("lazy loading outside command context");
            }
            return commandContext.VariableInstanceEntityManager.findVariableInstancesByExecutionAndNames(id, variableNames);
        }

        // event subscription support //////////////////////////////////////////////

        public virtual IList<IEventSubscriptionEntity> EventSubscriptions
        {
            get
            {
                ensureEventSubscriptionsInitialized();
                return eventSubscriptions;
            }
        }

        protected internal virtual void ensureEventSubscriptionsInitialized()
        {
            var ctx = Context.CommandContext;
            if (eventSubscriptions == null && ctx != null)
            {
                eventSubscriptions = ctx.EventSubscriptionEntityManager.findEventSubscriptionsByExecution(id);
            }
        }

        // referenced job entities //////////////////////////////////////////////////

        public virtual IList<IJobEntity> Jobs
        {
            get
            {
                ensureJobsInitialized();
                return jobs;
            }
        }

        protected internal virtual void ensureJobsInitialized()
        {
            var ctx = Context.CommandContext;
            if (jobs == null && ctx != null)
            {
                jobs = ctx.JobEntityManager.findJobsByExecutionId(id);
            }
        }

        public virtual IList<ITimerJobEntity> TimerJobs
        {
            get
            {
                ensureTimerJobsInitialized();
                return timerJobs;
            }
        }

        protected internal virtual void ensureTimerJobsInitialized()
        {
            var ctx = Context.CommandContext;
            if (timerJobs == null && ctx != null)
            {
                timerJobs = ctx.TimerJobEntityManager.findJobsByExecutionId(id);
            }
        }

        // referenced task entities ///////////////////////////////////////////////////

        protected internal virtual void ensureTasksInitialized()
        {
            var ctx = Context.CommandContext;
            if (tasks == null && ctx != null)
            {
                tasks = ctx.TaskEntityManager.findTasksByExecutionId(id);
            }
        }

        public virtual IList<ITaskEntity> Tasks
        {
            get
            {
                ensureTasksInitialized();
                return tasks;
            }
        }

        // identity links ///////////////////////////////////////////////////////////

        public virtual IList<IIdentityLinkEntity> IdentityLinks
        {
            get
            {
                ensureIdentityLinksInitialized();
                return identityLinks;
            }
        }

        protected internal virtual void ensureIdentityLinksInitialized()
        {
            var ctx = Context.CommandContext;
            if (identityLinks == null && ctx != null)
            {
                identityLinks = ctx.IdentityLinkEntityManager.findIdentityLinksByProcessInstanceId(id);
            }
        }

        // getters and setters //////////////////////////////////////////////////////

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


        public virtual string ActivityId
        {
            get
            {
                return activityId;
            }
            set
            {
                if (activityId == null)
                {
                    activityId = value;
                }
            }
        }

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


        public virtual void inactivate()
        {
            this.isActive = false;
        }

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
        }

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


        public virtual string CurrentActivityId
        {
            get
            {
                return activityId;
            }
        }

        public virtual string Name
        {
            get
            {
                if (!ReferenceEquals(localizedName, null) && localizedName.Length > 0)
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


        public virtual string Description
        {
            get
            {
                if (!ReferenceEquals(localizedDescription, null) && localizedDescription.Length > 0)
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


        public virtual IDictionary<string, object> ProcessVariables
        {
            get
            {
                IDictionary<string, object> variables = new Dictionary<string, object>();
                if (queryVariables != null)
                {
                    foreach (IVariableInstanceEntity variableInstance in queryVariables)
                    {
                        if (!ReferenceEquals(variableInstance.Id, null) && ReferenceEquals(variableInstance.TaskId, null))
                        {
                            variables[variableInstance.Name] = variableInstance.Value;
                        }
                    }
                }
                return variables;
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


        public virtual string ActivityName
        {
            get
            {
                return activityName;
            }
            set
            {
                if (activityName == null)
                {
                    activityName = value;
                }
            }
        }

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
                if (!ReferenceEquals(activityId, null))
                {
                    strb.Append(" - activity '" + activityId);
                }
                if (!ReferenceEquals(parentId, null))
                {
                    strb.Append(" - parent '" + parentId + "'");
                }
                return strb.ToString();
            }
        }
    }

}