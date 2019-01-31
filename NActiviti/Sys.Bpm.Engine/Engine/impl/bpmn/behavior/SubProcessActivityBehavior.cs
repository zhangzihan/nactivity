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
    using org.activiti.engine.impl.context;
    using org.activiti.engine.impl.persistence.entity;
    using org.activiti.engine.impl.util;


    /// <summary>
    /// Implementation of the BPMN 2.0 subprocess (formally known as 'embedded' subprocess): a subprocess defined within another process definition.
    /// 
    /// 
    /// </summary>
    [Serializable]
    public class SubProcessActivityBehavior : AbstractBpmnActivityBehavior
    {
        private const long serialVersionUID = 1L;

        public override void execute(IExecutionEntity execution)
        {
            SubProcess subProcess = getSubProcessFromExecution(execution);

            FlowElement startElement = null;
            if (CollectionUtil.IsNotEmpty(subProcess.FlowElements))
            {
                foreach (FlowElement subElement in subProcess.FlowElements)
                {
                    if (subElement is StartEvent)
                    {
                        StartEvent startEvent = (StartEvent)subElement;

                        // start none event
                        if (CollectionUtil.IsEmpty(startEvent.EventDefinitions))
                        {
                            startElement = startEvent;
                            break;
                        }
                    }
                }
            }

            if (startElement == null)
            {
                throw new ActivitiException("No initial activity found for subprocess " + subProcess.Id);
            }

            execution.IsScope = true;

            // initialize the template-defined data objects as variables
            IDictionary<string, object> dataObjectVars = processDataObjects(subProcess.DataObjects);
            if (dataObjectVars != null)
            {
                execution.VariablesLocal = dataObjectVars;
            }

            IExecutionEntity startSubProcessExecution = Context.CommandContext.ExecutionEntityManager.createChildExecution(execution);
            startSubProcessExecution.CurrentFlowElement = startElement;
            Context.Agenda.planContinueProcessOperation(startSubProcessExecution);
        }

        protected internal virtual SubProcess getSubProcessFromExecution(IExecutionEntity execution)
        {
            FlowElement flowElement = execution.CurrentFlowElement;
            SubProcess subProcess = null;
            if (flowElement is SubProcess)
            {
                subProcess = (SubProcess)flowElement;
            }
            else
            {
                throw new ActivitiException("Programmatic error: sub process behaviour can only be applied" + " to a SubProcess instance, but got an instance of " + flowElement);
            }
            return subProcess;
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