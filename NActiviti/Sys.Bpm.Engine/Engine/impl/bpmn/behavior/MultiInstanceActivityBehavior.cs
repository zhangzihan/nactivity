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

namespace org.activiti.engine.impl.bpmn.behavior
{
    using Microsoft.Extensions.Logging;
    using org.activiti.bpmn.model;
    using org.activiti.engine.@delegate;
    using org.activiti.engine.impl.bpmn.helper;
    using org.activiti.engine.impl.context;
    using org.activiti.engine.impl.@delegate;
    using org.activiti.engine.impl.persistence.entity;
    using org.activiti.engine.impl.util;
    using Sys;
    using System.Linq;

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

        // Variable names for outer instance(as described in spec)
        protected internal readonly string NUMBER_OF_INSTANCES = "nrOfInstances";
        protected internal readonly string NUMBER_OF_ACTIVE_INSTANCES = "nrOfActiveInstances";
        protected internal readonly string NUMBER_OF_COMPLETED_INSTANCES = "nrOfCompletedInstances";

        // Instance members
        protected internal Activity activity;
        protected internal AbstractBpmnActivityBehavior innerActivityBehavior;
        protected internal IExpression loopCardinalityExpression;
        protected internal IExpression completionConditionExpression;
        protected internal IExpression collectionExpression;
        protected internal string collectionVariable;
        protected internal string collectionElementVariable;
        // default variable name for loop counter for inner instances (as described in the spec)
        protected internal string collectionElementIndexVariable = "loopCounter";

        /// <param name="innerActivityBehavior">
        ///          The original <seealso cref="IActivityBehavior"/> of the activity that will be wrapped inside this behavior. </param>
        /// <param name="isSequential">
        ///          Indicates whether the multi instance behavior must be sequential or parallel </param>
        public MultiInstanceActivityBehavior(Activity activity, AbstractBpmnActivityBehavior innerActivityBehavior)
        {
            this.activity = activity;
            InnerActivityBehavior = innerActivityBehavior;
        }

        public override void execute(IExecutionEntity execution)
        {
            if (getLocalLoopVariable(execution, CollectionElementIndexVariable) == null)
            {

                int nrOfInstances = 0;

                try
                {
                    nrOfInstances = createInstances(execution);
                }
                catch (BpmnError error)
                {
                    ErrorPropagation.propagateError(error, execution);
                }

                if (nrOfInstances == 0)
                {
                    base.leave(execution);
                }

            }
            else
            {
                Context.CommandContext.HistoryManager.recordActivityStart(execution);

                innerActivityBehavior.execute(execution);
            }
        }

        protected internal abstract int createInstances(IExecutionEntity execution);

        protected internal virtual void executeCompensationBoundaryEvents(FlowElement flowElement, IExecutionEntity execution)
        {

            //Execute compensation boundary events
            ICollection<BoundaryEvent> boundaryEvents = findBoundaryEventsForFlowNode(execution.ProcessDefinitionId, flowElement);
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
                        IExecutionEntity childExecutionEntity = Context.CommandContext.ExecutionEntityManager.createChildExecution(execution);
                        childExecutionEntity.ParentId = execution.Id;
                        childExecutionEntity.CurrentFlowElement = boundaryEvent;
                        childExecutionEntity.IsScope = false;

                        IActivityBehavior boundaryEventBehavior = ((IActivityBehavior)boundaryEvent.Behavior);
                        boundaryEventBehavior.execute(childExecutionEntity);
                    }
                }
            }
        }


        protected internal override ICollection<BoundaryEvent> findBoundaryEventsForFlowNode(string processDefinitionId, FlowElement flowElement)
        {
            Process process = getProcessDefinition(processDefinitionId);

            // This could be cached or could be done at parsing time
            IList<BoundaryEvent> results = new List<BoundaryEvent>(1);
            ICollection<BoundaryEvent> boundaryEvents = process.findFlowElementsOfType<BoundaryEvent>(true);
            foreach (BoundaryEvent boundaryEvent in boundaryEvents)
            {
                if (!ReferenceEquals(boundaryEvent.AttachedToRefId, null) && boundaryEvent.AttachedToRefId.Equals(flowElement.Id))
                {
                    results.Add(boundaryEvent);
                }
            }
            return results;
        }

        protected internal override Process getProcessDefinition(string processDefinitionId)
        {
            return ProcessDefinitionUtil.getProcess(processDefinitionId);
        }

        // Intercepts signals, and delegates it to the wrapped {@link ActivityBehavior}.
        public override void trigger(IExecutionEntity execution, string signalName, object signalData)
        {
            innerActivityBehavior.trigger(execution, signalName, signalData);
        }

        // required for supporting embedded subprocesses
        public virtual void lastExecutionEnded(IExecutionEntity execution)
        {
            //ScopeUtil.createEventScopeExecution((ExecutionEntity) execution);
            leave(execution);
        }

        // required for supporting external subprocesses
        public virtual void completing(IExecutionEntity execution, IExecutionEntity subProcessInstance)
        {
        }

        // required for supporting external subprocesses
        public virtual void completed(IExecutionEntity execution)
        {
            leave(execution);
        }

        // Helpers
        // //////////////////////////////////////////////////////////////////////
        protected internal virtual int resolveNrOfInstances(IExecutionEntity execution)
        {
            if (loopCardinalityExpression != null)
            {
                return resolveLoopCardinality(execution);

            }
            else if (usesCollection())
            {
                System.Collections.ICollection collection = resolveAndValidateCollection(execution);
                return collection.Count;

            }
            else
            {
                throw new ActivitiIllegalArgumentException("Couldn't resolve collection expression nor variable reference");
            }
        }

        protected internal virtual void executeOriginalBehavior(IExecutionEntity execution, int loopCounter)
        {
            if (usesCollection() && !ReferenceEquals(collectionElementVariable, null))
            {
                System.Collections.ICollection collection = (System.Collections.ICollection)resolveCollection(execution);

                object value = null;
                int index = 0;
                System.Collections.IEnumerator it = collection.GetEnumerator();
                while (it.MoveNext())
                {
                    value = it.Current;
                    if (loopCounter == index)
                    {
                        break;
                    }
                    index++;
                }
                setLoopVariable(execution, collectionElementVariable, value);
            }

            execution.CurrentFlowElement = activity;
            Context.Agenda.planContinueMultiInstanceOperation(execution);
        }

        protected internal virtual System.Collections.ICollection resolveAndValidateCollection(IExecutionEntity execution)
        {
            object obj = resolveCollection(execution);
            if (collectionExpression != null)
            {
                if (!(obj is System.Collections.ICollection))
                {
                    throw new ActivitiIllegalArgumentException(collectionExpression.ExpressionText + "' didn't resolve to a Collection");
                }

            }
            else if (!ReferenceEquals(collectionVariable, null))
            {
                if (obj == null)
                {
                    throw new ActivitiIllegalArgumentException("Variable " + collectionVariable + " is not found");
                }

                if (!(obj is System.Collections.ICollection))
                {
                    throw new ActivitiIllegalArgumentException("Variable " + collectionVariable + "' is not a Collection");
                }

            }
            else
            {
                throw new ActivitiIllegalArgumentException("Couldn't resolve collection expression nor variable reference");
            }
            return (System.Collections.ICollection)obj;
        }

        protected internal virtual object resolveCollection(IExecutionEntity execution)
        {
            object collection = null;
            if (collectionExpression != null)
            {
                collection = collectionExpression.getValue(execution);

            }
            else if (!ReferenceEquals(collectionVariable, null))
            {
                collection = execution.getVariable(collectionVariable);
            }
            return collection;
        }

        protected internal virtual bool usesCollection()
        {
            return collectionExpression != null || !ReferenceEquals(collectionVariable, null);
        }

        protected internal virtual bool isExtraScopeNeeded(FlowNode flowNode)
        {
            return flowNode.SubProcess != null;
        }

        protected internal virtual int resolveLoopCardinality(IExecutionEntity execution)
        {
            // Using Number since expr can evaluate to eg. Long (which is also the default for Juel)
            object value = loopCardinalityExpression.getValue(execution);
            if (value is Int32 || value is Int16 || value is Int64)
            {
                return (int)value;

            }
            else if (value is string)
            {
                return Convert.ToInt32((string)value);

            }
            else
            {
                throw new ActivitiIllegalArgumentException("Could not resolve loopCardinality expression '" + loopCardinalityExpression.ExpressionText + "': not a number nor number String");
            }
        }

        protected internal virtual bool completionConditionSatisfied(IExecutionEntity execution)
        {
            if (completionConditionExpression != null)
            {
                object value = completionConditionExpression.getValue(execution);
                if (!(value is bool?))
                {
                    throw new ActivitiIllegalArgumentException("completionCondition '" + completionConditionExpression.ExpressionText + "' does not evaluate to a boolean value");
                }

                bool? booleanValue = (bool?)value;
                if (log.IsEnabled(LogLevel.Debug))
                {
                    log.LogDebug($"Completion condition of multi-instance satisfied: {booleanValue}");
                }
                return booleanValue.Value;
            }
            return false;
        }

        protected internal virtual void setLoopVariable(IExecutionEntity execution, string variableName, object value)
        {
            execution.setVariableLocal(variableName, value);
        }

        protected internal virtual int? getLoopVariable(IExecutionEntity execution, string variableName)
        {
            object value = execution.getVariableLocal(variableName);
            IExecutionEntity parent = execution.Parent;
            while (value == null && parent != null)
            {
                value = parent.getVariableLocal(variableName);
                parent = parent.Parent;
            }
            return (int?)(value != null ? value : 0);
        }

        protected internal virtual int? getLocalLoopVariable(IExecutionEntity execution, string variableName)
        {
            return (int?)execution.getVariableLocal(variableName);
        }

        protected internal virtual void removeLocalLoopVariable(IExecutionEntity execution, string variableName)
        {
            execution.removeVariableLocal(variableName);
        }

        /// <summary>
        /// Since no transitions are followed when leaving the inner activity, it is needed to call the end listeners yourself.
        /// </summary>
        protected internal virtual void callActivityEndListeners(IExecutionEntity execution)
        {
            Context.CommandContext.ProcessEngineConfiguration.ListenerNotificationHelper
                .executeExecutionListeners(activity, execution, BaseExecutionListener_Fields.EVENTNAME_END);
        }

        protected internal virtual void logLoopDetails(IExecutionEntity execution, string custom, int loopCounter, int nrOfCompletedInstances, int nrOfActiveInstances, int nrOfInstances)
        {
            if (log.IsEnabled(LogLevel.Debug))
            {
                log.LogDebug($"Multi-instance '{(execution.CurrentFlowElement != null ? execution.CurrentFlowElement.Id : "")}' {custom}. Details: loopCounter={loopCounter}, nrOrCompletedInstances={nrOfCompletedInstances},nrOfActiveInstances={nrOfActiveInstances},nrOfInstances={nrOfInstances}");
            }
        }

        protected internal virtual IExecutionEntity getMultiInstanceRootExecution(IExecutionEntity executionEntity)
        {
            IExecutionEntity multiInstanceRootExecution = null;
            IExecutionEntity currentExecution = executionEntity;
            while (currentExecution != null && multiInstanceRootExecution == null && currentExecution.Parent != null)
            {
                if (currentExecution.IsMultiInstanceRoot)
                {
                    multiInstanceRootExecution = currentExecution;
                }
                else
                {
                    currentExecution = currentExecution.Parent;
                }
            }
            return multiInstanceRootExecution;
        }

        // Getters and Setters
        // ///////////////////////////////////////////////////////////

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