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
namespace Sys.Workflow.engine.impl.bpmn.parser.handler
{
    using Sys.Workflow.bpmn.model;

    /// 
    /// 
    public class MessageEventDefinitionParseHandler : AbstractBpmnParseHandler<MessageEventDefinition>
    {

        protected internal override Type HandledType
        {
            get
            {
                return typeof(MessageEventDefinition);
            }
        }

        protected internal override void ExecuteParse(BpmnParse bpmnParse, MessageEventDefinition messageDefinition)
        {
            BpmnModel bpmnModel = bpmnParse.BpmnModel;
            string messageRef = messageDefinition.MessageRef;
            if (bpmnModel.ContainsMessageId(messageRef))
            {
                Message message = bpmnModel.GetMessage(messageRef);
                messageDefinition.MessageRef = message.Name;
                messageDefinition.ExtensionElements = message.ExtensionElements;
            }

            if (bpmnParse.CurrentFlowElement is IntermediateCatchEvent intermediateCatchEvent)
            {
                intermediateCatchEvent.Behavior = bpmnParse.ActivityBehaviorFactory.CreateIntermediateCatchMessageEventActivityBehavior(intermediateCatchEvent, messageDefinition);

            }
            else if (bpmnParse.CurrentFlowElement is BoundaryEvent boundaryEvent)
            {
                boundaryEvent.Behavior = bpmnParse.ActivityBehaviorFactory.CreateBoundaryMessageEventActivityBehavior(boundaryEvent, messageDefinition, boundaryEvent.CancelActivity);
            }

            else
            {
                // What to do here?
            }

        }

    }

}