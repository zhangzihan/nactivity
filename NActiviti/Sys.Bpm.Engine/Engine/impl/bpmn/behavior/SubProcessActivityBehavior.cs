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
    using Sys.Workflow.Bpmn.Models;
    using Sys.Workflow.Engine.Impl.Contexts;
    using Sys.Workflow.Engine.Impl.Persistence.Entity;
    using Sys.Workflow.Engine.Impl.Util;


    /// <summary>
    /// Implementation of the BPMN 2.0 subprocess (formally known as 'embedded' subprocess): a subprocess defined within another process definition.
    /// 
    /// 
    /// </summary>
    [Serializable]
    public class SubProcessActivityBehavior : AbstractBpmnActivityBehavior
    {
        private const long serialVersionUID = 1L;

        public override void Execute(IExecutionEntity execution)
        {
            SubProcess subProcess = GetSubProcessFromExecution(execution);

            FlowElement startElement = null;
            if (CollectionUtil.IsNotEmpty(subProcess.FlowElements))
            {
                foreach (FlowElement subElement in subProcess.FlowElements)
                {
                    if (subElement is StartEvent startEvent)
                    {
                        // start none event
                        if (CollectionUtil.IsEmpty(startEvent.EventDefinitions))
                        {
                            startElement = startEvent;
                            break;
                        }
                    }
                }
            }

            execution.IsScope = true;

            // initialize the template-defined data objects as variables
            IDictionary<string, object> dataObjectVars = ProcessDataObjects(subProcess.DataObjects);
            if (dataObjectVars != null)
            {
                execution.VariablesLocal = dataObjectVars;
            }

            IExecutionEntity startSubProcessExecution = Context.CommandContext.ExecutionEntityManager.CreateChildExecution(execution);
            startSubProcessExecution.CurrentFlowElement = startElement ?? throw new ActivitiException("No initial activity found for subprocess " + subProcess.Id);
            Context.Agenda.PlanContinueProcessOperation(startSubProcessExecution);
        }

        protected internal virtual SubProcess GetSubProcessFromExecution(IExecutionEntity execution)
        {
            FlowElement flowElement = execution.CurrentFlowElement;
            SubProcess subProcess;
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

        protected internal virtual IDictionary<string, object> ProcessDataObjects(ICollection<ValuedDataObject> dataObjects)
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