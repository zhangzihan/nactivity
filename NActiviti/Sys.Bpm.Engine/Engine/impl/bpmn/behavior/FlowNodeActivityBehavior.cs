using System;

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
    using org.activiti.engine.impl.@delegate;
    using org.activiti.engine.impl.persistence.entity;
    using System.Collections.Generic;

    /// <summary>
    /// Superclass for all 'connectable' BPMN 2.0 process elements: tasks, gateways and events. This means that any subclass can be the source or target of a sequenceflow.
    /// 
    /// Corresponds with the notion of the 'flownode' in BPMN 2.0.
    /// 
    /// 
    /// </summary>
    [Serializable]
    public abstract class FlowNodeActivityBehavior : ITriggerableActivityBehavior
    {

        private const long serialVersionUID = 1L;

        protected internal BpmnActivityBehavior bpmnActivityBehavior = new BpmnActivityBehavior();

        /// <summary>
        /// Default behaviour: just leave the activity with no extra functionality.
        /// </summary>
        public virtual void Execute(IExecutionEntity execution)
        {
            Leave(execution);
        }

        /// <summary>
        /// Default way of leaving a BPMN 2.0 activity: evaluate the conditions on the outgoing sequence flow and take those that evaluate to true.
        /// </summary>
        public virtual void Leave(IExecutionEntity execution)
        {
            Leave(execution, null);
        }
        public virtual void Leave(IExecutionEntity execution, object signalData)
        {
            bpmnActivityBehavior.PerformDefaultOutgoingBehavior(execution);
        }

        public virtual void LeaveIgnoreConditions(IExecutionEntity execution)
        {
            bpmnActivityBehavior.PerformIgnoreConditionsOutgoingBehavior(execution);
        }

        public virtual void Trigger(IExecutionEntity execution, string signalName, object signalData, bool throwError = true)
        {
            // concrete activity behaviours that do accept signals should override this method;
            throw new ActivitiException("this activity isn't waiting for a trigger");
        }

        protected internal virtual string ParseActivityType(FlowNode flowNode)
        {
            string elementType = flowNode.GetType().Name;
            elementType = elementType.Substring(0, 1).ToLower() + elementType.Substring(1);
            return elementType;
        }

    }

}