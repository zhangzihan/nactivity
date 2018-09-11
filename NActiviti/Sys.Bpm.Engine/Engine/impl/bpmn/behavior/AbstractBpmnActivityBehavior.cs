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

    using org.activiti.bpmn.model;
    using org.activiti.engine.@delegate;
    using org.activiti.engine.impl.context;
    using org.activiti.engine.impl.@delegate;
    using org.activiti.engine.impl.persistence.entity;
    using org.activiti.engine.impl.util;

    /// <summary>
    /// Denotes an 'activity' in the sense of BPMN 2.0: a parent class for all tasks, subprocess and callActivity.
    /// 
    /// 
    /// </summary>
    [Serializable]
    public class AbstractBpmnActivityBehavior : FlowNodeActivityBehavior
    {

        private const long serialVersionUID = 1L;

        protected internal MultiInstanceActivityBehavior multiInstanceActivityBehavior;

        /// <summary>
        /// Subclasses that call leave() will first pass through this method, before the regular <seealso cref="FlowNodeActivityBehavior#leave(ActivityExecution)"/> is called. This way, we can check if the activity
        /// has loop characteristics, and delegate to the behavior if this is the case.
        /// </summary>
        public override void leave(IExecutionEntity execution)
        {
            FlowElement currentFlowElement = execution.CurrentFlowElement;
            ICollection<BoundaryEvent> boundaryEvents = findBoundaryEventsForFlowNode(execution.ProcessDefinitionId, currentFlowElement);
            if (CollectionUtil.IsNotEmpty(boundaryEvents))
            {
                executeCompensateBoundaryEvents(boundaryEvents, execution);
            }
            if (!hasLoopCharacteristics())
            {
                base.leave(execution);
            }
            else if (hasMultiInstanceCharacteristics())
            {
                multiInstanceActivityBehavior.leave(execution);
            }
        }

        protected internal virtual void executeCompensateBoundaryEvents(ICollection<BoundaryEvent> boundaryEvents, IExecutionEntity execution)
        {

            // The parent execution becomes a scope, and a child execution is created for each of the boundary events
            foreach (BoundaryEvent boundaryEvent in boundaryEvents)
            {

                if (CollectionUtil.IsEmpty(boundaryEvent.EventDefinitions))
                {
                    continue;
                }

                if (boundaryEvent.EventDefinitions[0] is CompensateEventDefinition == false)
                {
                    continue;
                }

                IExecutionEntity childExecutionEntity = Context.CommandContext.ExecutionEntityManager.createChildExecution(execution);
                childExecutionEntity.ParentId = execution.Id;
                childExecutionEntity.CurrentFlowElement = boundaryEvent;
                childExecutionEntity.IsScope = false;

                IActivityBehavior boundaryEventBehavior = ((IActivityBehavior)boundaryEvent.Behavior);
                boundaryEventBehavior.execute(childExecutionEntity);
            }

        }

        protected internal virtual ICollection<BoundaryEvent> findBoundaryEventsForFlowNode(string processDefinitionId, FlowElement flowElement)
        {
            Process process = getProcessDefinition(processDefinitionId);

            // This could be cached or could be done at parsing time
            IList<BoundaryEvent> results = new List<BoundaryEvent>(1);
            ICollection<BoundaryEvent> boundaryEvents = process.findFlowElementsOfType<BoundaryEvent>(true);
            foreach (BoundaryEvent boundaryEvent in boundaryEvents)
            {
                if (!string.ReferenceEquals(boundaryEvent.AttachedToRefId, null) && boundaryEvent.AttachedToRefId.Equals(flowElement.Id))
                {
                    results.Add(boundaryEvent);
                }
            }
            return results;
        }

        protected internal virtual Process getProcessDefinition(string processDefinitionId)
        {
            // TODO: must be extracted / cache should be accessed in another way
            return ProcessDefinitionUtil.getProcess(processDefinitionId);
        }

        protected internal virtual bool hasLoopCharacteristics()
        {
            return hasMultiInstanceCharacteristics();
        }

        protected internal virtual bool hasMultiInstanceCharacteristics()
        {
            return multiInstanceActivityBehavior != null;
        }

        public virtual MultiInstanceActivityBehavior MultiInstanceActivityBehavior
        {
            get
            {
                return multiInstanceActivityBehavior;
            }
            set
            {
                this.multiInstanceActivityBehavior = value;
            }
        }
    }

}