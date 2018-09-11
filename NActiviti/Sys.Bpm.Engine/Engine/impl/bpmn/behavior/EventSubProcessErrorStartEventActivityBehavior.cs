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
    using org.activiti.engine.impl.persistence.entity;


    /// <summary>
    /// Implementation of the BPMN 2.0 event subprocess start event.
    /// 
    /// 
    /// </summary>
    [Serializable]
    public class EventSubProcessErrorStartEventActivityBehavior : AbstractBpmnActivityBehavior
    {

        private const long serialVersionUID = 1L;

        public override void execute(IExecutionEntity execution)
        {
            StartEvent startEvent = (StartEvent)execution.CurrentFlowElement;
            EventSubProcess eventSubProcess = (EventSubProcess)startEvent.SubProcess;
            execution.CurrentFlowElement = eventSubProcess;
            execution.IsScope = true;

            // initialize the template-defined data objects as variables
            IDictionary<string, object> dataObjectVars = processDataObjects(eventSubProcess.DataObjects);
            if (dataObjectVars != null)
            {
                execution.VariablesLocal = dataObjectVars;
            }

            IExecutionEntity startSubProcessExecution = Context.CommandContext.ExecutionEntityManager.createChildExecution(execution);
            startSubProcessExecution.CurrentFlowElement = startEvent;
            Context.Agenda.planTakeOutgoingSequenceFlowsOperation(startSubProcessExecution, true);
        }

        protected internal virtual IDictionary<string, object> processDataObjects(ICollection<ValuedDataObject> dataObjects)
        {
            IDictionary<string, object> variablesMap = new Dictionary<string, object>();
            // convert data objects to process variables
            if (dataObjects != null)
            {
                foreach (ValuedDataObject dataObject in dataObjects)
                {
                    variablesMap[dataObject.Name] = dataObject.Value;
                }
            }
            return variablesMap;
        }
    }

}