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
namespace Sys.Workflow.Engine.Impl.Bpmn.Parser.Handlers
{
    using Sys.Workflow.Bpmn.Models;
    using Sys.Workflow.Engine.Impl.Util;

    /// 
    /// 
    public class StartEventParseHandler : AbstractActivityBpmnParseHandler<StartEvent>
    {

        protected internal override Type HandledType
        {
            get
            {
                return typeof(StartEvent);
            }
        }

        protected internal override void ExecuteParse(BpmnParse bpmnParse, StartEvent element)
        {
            if (element.SubProcess is not null && element.SubProcess is EventSubProcess)
            {
                if (CollectionUtil.IsNotEmpty(element.EventDefinitions))
                {
                    EventDefinition eventDefinition = element.EventDefinitions[0];
                    if (eventDefinition is MessageEventDefinition messageDefinition)
                    {
                        BpmnModel bpmnModel = bpmnParse.BpmnModel;
                        string messageRef = messageDefinition.MessageRef;
                        if (bpmnModel.ContainsMessageId(messageRef))
                        {
                            Message message = bpmnModel.GetMessage(messageRef);
                            messageDefinition.MessageRef = message.Name;
                            messageDefinition.ExtensionElements = message.ExtensionElements;
                        }
                        element.Behavior = bpmnParse.ActivityBehaviorFactory.CreateEventSubProcessMessageStartEventActivityBehavior(element, messageDefinition);

                    }
                    else if (eventDefinition is ErrorEventDefinition)
                    {
                        element.Behavior = bpmnParse.ActivityBehaviorFactory.CreateEventSubProcessErrorStartEventActivityBehavior(element);
                    }
                }

            }
            else if (CollectionUtil.IsEmpty(element.EventDefinitions))
            {
                element.Behavior = bpmnParse.ActivityBehaviorFactory.CreateNoneStartEventActivityBehavior(element);
            }

            if (element.SubProcess is null && (CollectionUtil.IsEmpty(element.EventDefinitions) || bpmnParse.CurrentProcess.InitialFlowElement is null))
            {

                bpmnParse.CurrentProcess.InitialFlowElement = element;
            }
        }

    }

}