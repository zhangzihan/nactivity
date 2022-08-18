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

namespace Sys.Workflow.Engine.Impl.Bpmn.Behavior
{
    using Microsoft.Extensions.Logging;
    using Sys.Workflow.Bpmn.Models;
    using Sys.Workflow.Engine.Delegate;
    using Sys.Workflow.Engine.Impl.Bpmn.Helper;
    using Sys.Workflow.Engine.Impl.Cmd;
    using Sys.Workflow.Engine.Impl.Contexts;
    using Sys.Workflow.Engine.Impl.Delegate;
    using Sys.Workflow.Engine.Impl.Persistence.Entity;
    using Sys.Workflow.Engine.Impl.Util;
    using Sys.Workflow;
    using System.Collections;
    using Sys.Workflow.Services.Api.Commands;

    /// <summary>
    /// Implementation of the multi-instance functionality as described in the BPMN 2.0 spec.
    /// 
    /// Multi instance functionality is implemented as an <seealso cref="IActivityBehavior"/> that wraps the original <seealso cref="IActivityBehavior"/> of the activity.
    /// 
    /// Only subclasses of <seealso cref="AbstractBpmnActivityBehavior"/> can have multi-instance behavior. As such, special logic is contained in the <seealso cref="AbstractBpmnActivityBehavior"/> to delegate to the
    /// <seealso cref="MultiInstanceActivityBehavior"/> if needed.
    /// 
    /// 
    /// 
    /// </summary>
    [Serializable]
    public abstract class MultiInstanceActivityBehavior : SubProcessActivityBehavior, ISubProcessActivityBehavior
    {
        private static readonly ILogger<MultiInstanceActivityBehavior> log = ProcessEngineServiceProvider.LoggerService<MultiInstanceActivityBehavior>();

        private const long serialVersionUID = 1L;
        /// <summary>
        /// 
        /// </summary>
        protected IMultiinstanceCompletedPolicy completedPolicy;

        // Variable names for outer instance(as described in spec)
        internal static readonly string NUMBER_OF_INSTANCES = "nrOfInstances";
        internal static readonly string NUMBER_OF_ACTIVE_INSTANCES = "nrOfActiveInstances";
        internal static readonly string NUMBER_OF_COMPLETED_INSTANCES = "nrOfCompletedInstances";

        // Instance members
        /// <summary>
        /// 
        /// </summary>
        protected internal Activity activity;
        /// <summary>
        /// 
        /// </summary>
        protected internal AbstractBpmnActivityBehavior innerActivityBehavior;
        /// <summary>
        /// 
        /// </summary>
        protected internal IExpression loopCardinalityExpression;
        /// <summary>
        /// 
        /// </summary>
        protected internal IExpression completionConditionExpression;
        /// <summary>
        /// 
        /// </summary>
        protected internal IExpression collectionExpression;
        /// <summary>
        /// 
        /// </summary>
        protected internal string collectionVariable;
        /// <summary>
        /// 
        /// </summary>
        protected internal string collectionElementVariable;
        // default variable name for loop counter for inner instances (as described in the spec)
        /// <summary>
        /// 
        /// </summary>
        protected internal string collectionElementIndexVariable = "loopCounter";

        /// <param name="innerActivityBehavior">
        ///          The original <seealso cref="IActivityBehavior"/> of the activity that will be wrapped inside this behavior. </param>
        /// <param name="isSequential">
        ///          Indicates whether the multi instance behavior must be sequential or parallel </param>
        public MultiInstanceActivityBehavior(Activity activity, AbstractBpmnActivityBehavior innerActivityBehavior)
        {
            this.activity = activity;
            InnerActivityBehavior = innerActivityBehavior;
            completedPolicy = new DefaultMultiInstanceCompletedPolicy();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="execution"></param>
        public override void Execute(IExecutionEntity execution)
        {
            if (!GetLocalLoopVariable(execution, CollectionElementIndexVariable).HasValue)
            {
                int nrOfInstances = 0;

                try
                {
                    nrOfInstances = CreateInstances(execution);
                }
                catch (BpmnError error)
                {
                    ErrorPropagation.PropagateError(error, execution);
                }

                if (nrOfInstances == 0)
                {
                    base.Leave(execution);
                }
            }
            else
            {
                Context.CommandContext.HistoryManager.RecordActivityStart(execution);

                innerActivityBehavior.Execute(execution);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="execution"></param>
        /// <returns></returns>
        protected internal abstract int CreateInstances(IExecutionEntity execution);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="flowElement"></param>
        /// <param name="execution"></param>
        protected internal virtual void ExecuteCompensationBoundaryEvents(FlowElement flowElement, IExecutionEntity execution)
        {
            //Execute compensation boundary events
            ICollection<BoundaryEvent> boundaryEvents = FindBoundaryEventsForFlowNode(execution.ProcessDefinitionId, flowElement);
            if (CollectionUtil.IsNotEmpty(boundaryEvents))
            {
                // The parent execution becomes a scope, and a child execution is created for each of the boundary events
                foreach (BoundaryEvent boundaryEvent in boundaryEvents)
                {
                    if (CollectionUtil.IsEmpty(boundaryEvent.EventDefinitions))
                    {
                        continue;
                    }

                    if (boundaryEvent.EventDefinitions[0] is CompensateEventDefinition)
                    {
                        IExecutionEntity childExecutionEntity = Context.CommandContext.ExecutionEntityManager.CreateChildExecution(execution);
                        childExecutionEntity.ParentId = execution.Id;
                        childExecutionEntity.CurrentFlowElement = boundaryEvent;
                        childExecutionEntity.IsScope = false;

                        IActivityBehavior boundaryEventBehavior = ((IActivityBehavior)boundaryEvent.Behavior);
                        boundaryEventBehavior.Execute(childExecutionEntity);
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="processDefinitionId"></param>
        /// <param name="flowElement"></param>
        /// <returns></returns>
        protected internal override ICollection<BoundaryEvent> FindBoundaryEventsForFlowNode(string processDefinitionId, FlowElement flowElement)
        {
            Process process = GetProcessDefinition(processDefinitionId);

            // This could be cached or could be done at parsing time
            IList<BoundaryEvent> results = new List<BoundaryEvent>(1);
            ICollection<BoundaryEvent> boundaryEvents = process.FindFlowElementsOfType<BoundaryEvent>(true);
            foreach (BoundaryEvent boundaryEvent in boundaryEvents)
            {
                if (boundaryEvent.AttachedToRefId is not null && boundaryEvent.AttachedToRefId.Equals(flowElement.Id))
                {
                    results.Add(boundaryEvent);
                }
            }
            return results;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="processDefinitionId"></param>
        /// <returns></returns>
        protected internal override Process GetProcessDefinition(string processDefinitionId)
        {
            return ProcessDefinitionUtil.GetProcess(processDefinitionId);
        }

        // Intercepts signals, and delegates it to the wrapped {@link ActivityBehavior}.
        /// <summary>
        /// 
        /// </summary>
        /// <param name="execution"></param>
        /// <param name="signalName"></param>
        /// <param name="signalData"></param>
        /// <param name="throwError"></param>
        public override void Trigger(IExecutionEntity execution, string signalName, object signalData, bool throwError = true)
        {
            innerActivityBehavior.Trigger(execution, signalName, signalData, throwError);
        }

        // required for supporting embedded subprocesses
        /// <summary>
        /// 
        /// </summary>
        /// <param name="execution"></param>
        public virtual void LastExecutionEnded(IExecutionEntity execution)
        {
            //ScopeUtil.createEventScopeExecution((ExecutionEntity) execution);
            Leave(execution);
        }

        // required for supporting external subprocesses
        /// <summary>
        /// 
        /// </summary>
        /// <param name="execution"></param>
        /// <param name="subProcessInstance"></param>
        public virtual void Completing(IExecutionEntity execution, IExecutionEntity subProcessInstance)
        {
        }

        // required for supporting external subprocesses
        /// <summary>
        /// 
        /// </summary>
        /// <param name="execution"></param>
        public virtual void Completed(IExecutionEntity execution)
        {
            Leave(execution);
        }

        // Helpers
        // //////////////////////////////////////////////////////////////////////
        /// <summary>
        /// 
        /// </summary>
        /// <param name="execution"></param>
        /// <returns></returns>
        protected internal virtual int ResolveNrOfInstances(IExecutionEntity execution)
        {
            if (loopCardinalityExpression is object)
            {
                return ResolveLoopCardinality(execution);

            }
            else if (UsesCollection())
            {
                ICollection collection = ResolveAndValidateCollection(execution);
                return collection.Count;

            }
            else
            {
                throw new ActivitiIllegalArgumentException("Couldn't resolve collection expression nor variable reference");
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="execution"></param>
        /// <param name="loopCounter"></param>
        protected internal virtual void ExecuteOriginalBehavior(IExecutionEntity execution, int loopCounter)
        {
            if (UsesCollection() && collectionElementVariable is not null)
            {
                ICollection collection = (ICollection)ResolveCollection(execution);

                object value = null;
                int index = 0;
                IEnumerator it = collection.GetEnumerator();
                while (it.MoveNext())
                {
                    value = it.Current;
                    if (loopCounter == index)
                    {
                        break;
                    }
                    index++;
                }
                SetLoopVariable(execution, collectionElementVariable, value);
            }

            execution.CurrentFlowElement = activity;
            Context.Agenda.PlanContinueMultiInstanceOperation(execution);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="execution"></param>
        /// <returns></returns>
        protected internal virtual ICollection ResolveAndValidateCollection(IExecutionEntity execution)
        {
            object obj = ResolveCollection(execution);
            if (collectionExpression is object)
            {
                if (obj is not ICollection)
                {
                    throw new ActivitiIllegalArgumentException(collectionExpression.ExpressionText + "' didn't resolve to a Collection");
                }

            }
            else if (collectionVariable is not null)
            {
                if (obj is null)
                {
                    throw new ActivitiIllegalArgumentException("Variable " + collectionVariable + " is not found");
                }

                if (obj is not ICollection)
                {
                    throw new ActivitiIllegalArgumentException("Variable " + collectionVariable + "' is not a Collection");
                }

            }
            else
            {
                throw new ActivitiIllegalArgumentException("Couldn't resolve collection expression nor variable reference");
            }
            return (ICollection)obj;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="execution"></param>
        /// <returns></returns>
        protected internal virtual object ResolveCollection(IExecutionEntity execution)
        {
            if (collectionExpression is object)
            {
                return collectionExpression.GetValue(execution);
            }
            else if (collectionVariable is not null)
            {
                return execution.GetVariable(collectionVariable);
            }
            return null;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected internal virtual bool UsesCollection()
        {
            return collectionExpression is object || collectionVariable is not null;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="flowNode"></param>
        /// <returns></returns>
        protected internal virtual bool IsExtraScopeNeeded(FlowNode flowNode)
        {
            return flowNode.SubProcess is not null;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="execution"></param>
        /// <returns></returns>
        protected internal virtual int ResolveLoopCardinality(IExecutionEntity execution)
        {
            // Using Number since expr can evaluate to eg. Long (which is also the default for Juel)
            object value = loopCardinalityExpression.GetValue(execution);
            if (value is int || value is short || value is long)
            {
                return (int)value;

            }
            else if (value is string)
            {
                return Convert.ToInt32(value.ToString());
            }
            else
            {
                throw new ActivitiIllegalArgumentException("Could not resolve loopCardinality expression '" + loopCardinalityExpression.ExpressionText + "': not a number nor number String");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="execution"></param>
        /// <param name="signalData"></param>
        /// <returns></returns>
        protected internal virtual bool CompletionConditionSatisfied(IExecutionEntity execution, object signalData)
        {
            return completedPolicy.CompletionConditionSatisfied(execution, this, signalData);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="execution"></param>
        /// <param name="variableName"></param>
        /// <param name="value"></param>
        protected internal virtual void SetLoopVariable(IExecutionEntity execution, string variableName, object value)
        {
            execution.SetVariableLocal(variableName, value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="execution"></param>
        /// <param name="variableName"></param>
        /// <returns></returns>
        protected internal virtual int? GetLoopVariable(IExecutionEntity execution, string variableName)
        {
            object value = execution.GetVariableLocal(variableName);
            IExecutionEntity parent = execution.Parent;
            while (value is null && parent is object)
            {
                value = parent.GetVariableLocal(variableName);
                parent = parent.Parent;
            }
            return (int?)value;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="execution"></param>
        /// <param name="variableName"></param>
        /// <returns></returns>
        protected internal virtual int? GetLocalLoopVariable(IExecutionEntity execution, string variableName)
        {
            object value = execution.GetVariableLocal(variableName);
            if (!(value is int?))
            {
                return null;
            }

            return (int?)value;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="execution"></param>
        /// <param name="variableName"></param>
        protected internal virtual void RemoveLocalLoopVariable(IExecutionEntity execution, string variableName)
        {
            execution.RemoveVariableLocal(variableName);
        }

        /// <summary>
        /// Since no transitions are followed when leaving the inner activity, it is needed to call the end listeners yourself.
        /// </summary>
        protected internal virtual void CallActivityEndListeners(IExecutionEntity execution)
        {
            Context.CommandContext.ProcessEngineConfiguration.ListenerNotificationHelper
                .ExecuteExecutionListeners(activity, execution, BaseExecutionListenerFields.EVENTNAME_END);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="execution"></param>
        /// <param name="custom"></param>
        /// <param name="loopCounter"></param>
        /// <param name="nrOfCompletedInstances"></param>
        /// <param name="nrOfActiveInstances"></param>
        /// <param name="nrOfInstances"></param>
        protected internal virtual void LogLoopDetails(IExecutionEntity execution, string custom, int loopCounter, int nrOfCompletedInstances, int nrOfActiveInstances, int nrOfInstances)
        {
            if (log.IsEnabled(LogLevel.Debug))
            {
                log.LogDebug($"Multi-instance '{(execution.CurrentFlowElement is not null ? execution.CurrentFlowElement.Id : "")}' {custom}. Details: loopCounter={loopCounter}, nrOrCompletedInstances={nrOfCompletedInstances},nrOfActiveInstances={nrOfActiveInstances},nrOfInstances={nrOfInstances}");
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="executionEntity"></param>
        /// <returns></returns>
        protected internal virtual IExecutionEntity GetMultiInstanceRootExecution(IExecutionEntity executionEntity)
        {
            return Context.ProcessEngineConfiguration.CommandExecutor.Execute(new GetMultiInstanceRootExecutionCmd(executionEntity));
        }

        // Getters and Setters
        // ///////////////////////////////////////////////////////////
        /// <summary>
        /// 
        /// </summary>
        public virtual IExpression LoopCardinalityExpression
        {
            get
            {
                return loopCardinalityExpression;
            }
            set
            {
                this.loopCardinalityExpression = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IExpression CompletionConditionExpression
        {
            get
            {
                return completionConditionExpression;
            }
            set
            {
                this.completionConditionExpression = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IExpression CollectionExpression
        {
            get
            {
                return collectionExpression;
            }
            set
            {
                this.collectionExpression = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual string CollectionVariable
        {
            get
            {
                return collectionVariable;
            }
            set
            {
                this.collectionVariable = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual string CollectionElementVariable
        {
            get
            {
                return collectionElementVariable;
            }
            set
            {
                this.collectionElementVariable = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual string CollectionElementIndexVariable
        {
            get
            {
                return collectionElementIndexVariable;
            }
            set
            {
                this.collectionElementIndexVariable = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual AbstractBpmnActivityBehavior InnerActivityBehavior
        {
            set
            {
                this.innerActivityBehavior = value;
                this.innerActivityBehavior.MultiInstanceActivityBehavior = this;
            }
            get
            {
                return innerActivityBehavior;
            }
        }
    }
}