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
    using Microsoft.Extensions.Logging;
    using org.activiti.bpmn.model;
    using Sys;

    /// 
    /// 
    public class EndEventParseHandler : AbstractActivityBpmnParseHandler<EndEvent>
    {
        private new static readonly ILogger logger = ProcessEngineServiceProvider.LoggerService<EndEventParseHandler>();

        protected internal override Type HandledType
        {
            get
            {
                return typeof(EndEvent);
            }
        }

        protected internal override void executeParse(BpmnParse bpmnParse, EndEvent endEvent)
        {

            EventDefinition eventDefinition = null;
            if (endEvent.EventDefinitions.Count > 0)
            {
                eventDefinition = endEvent.EventDefinitions[0];

                if (eventDefinition is ErrorEventDefinition)
                {
                    ErrorEventDefinition errorDefinition = (ErrorEventDefinition)eventDefinition;
                    if (bpmnParse.BpmnModel.containsErrorRef(errorDefinition.ErrorCode))
                    {
                        string errorCode = bpmnParse.BpmnModel.Errors[errorDefinition.ErrorCode];
                        if (string.IsNullOrWhiteSpace(errorCode))
                        {
                            logger.LogWarning("errorCode is required for an error event " + endEvent.Id);
                        }
                    }
                    endEvent.Behavior = bpmnParse.ActivityBehaviorFactory.createErrorEndEventActivityBehavior(endEvent, errorDefinition);
                }
                else if (eventDefinition is TerminateEventDefinition)
                {
                    endEvent.Behavior = bpmnParse.ActivityBehaviorFactory.createTerminateEndEventActivityBehavior(endEvent);
                }
                else if (eventDefinition is CancelEventDefinition)
                {
                    endEvent.Behavior = bpmnParse.ActivityBehaviorFactory.createCancelEndEventActivityBehavior(endEvent);
                }
                else
                {
                    endEvent.Behavior = bpmnParse.ActivityBehaviorFactory.createNoneEndEventActivityBehavior(endEvent);
                }

            }
            else
            {
                endEvent.Behavior = bpmnParse.ActivityBehaviorFactory.createNoneEndEventActivityBehavior(endEvent);
            }
        }

    }

}