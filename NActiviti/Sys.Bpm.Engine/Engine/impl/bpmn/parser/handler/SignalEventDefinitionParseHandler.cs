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
namespace org.activiti.engine.impl.bpmn.parser.handler
{
    using org.activiti.bpmn.model;

    /// 
    /// 
    public class SignalEventDefinitionParseHandler : AbstractBpmnParseHandler<SignalEventDefinition>
    {

        protected internal override Type HandledType
        {
            get
            {
                return typeof(SignalEventDefinition);
            }
        }

        protected internal override void ExecuteParse(BpmnParse bpmnParse, SignalEventDefinition signalDefinition)
        {

            Signal signal = null;
            if (bpmnParse.BpmnModel.ContainsSignalId(signalDefinition.SignalRef))
            {
                signal = bpmnParse.BpmnModel.GetSignal(signalDefinition.SignalRef);
            }

            if (bpmnParse.CurrentFlowElement is IntermediateCatchEvent intermediateCatchEvent)
            {
                intermediateCatchEvent.Behavior = bpmnParse.ActivityBehaviorFactory.CreateIntermediateCatchSignalEventActivityBehavior(intermediateCatchEvent, signalDefinition, signal);

            }
            else if (bpmnParse.CurrentFlowElement is BoundaryEvent boundaryEvent)
            {
                boundaryEvent.Behavior = bpmnParse.ActivityBehaviorFactory.CreateBoundarySignalEventActivityBehavior(boundaryEvent, signalDefinition, signal, boundaryEvent.CancelActivity);
            }
        }
    }

}