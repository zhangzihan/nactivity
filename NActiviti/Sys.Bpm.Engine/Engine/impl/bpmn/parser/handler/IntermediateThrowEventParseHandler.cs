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
    public class IntermediateThrowEventParseHandler : AbstractActivityBpmnParseHandler<ThrowEvent>
    {
        //private static readonly Logger logger = LoggerFactory.getLogger(typeof(IntermediateThrowEventParseHandler));

        protected internal override Type HandledType
        {
            get
            {
                return typeof(ThrowEvent);
            }
        }

        protected internal override void executeParse(BpmnParse bpmnParse, ThrowEvent intermediateEvent)
        {

            EventDefinition eventDefinition = null;
            if (intermediateEvent.EventDefinitions.Count > 0)
            {
                eventDefinition = intermediateEvent.EventDefinitions[0];
            }

            if (eventDefinition is SignalEventDefinition)
            {
                SignalEventDefinition signalEventDefinition = (SignalEventDefinition)eventDefinition;
                intermediateEvent.Behavior = bpmnParse.ActivityBehaviorFactory.createIntermediateThrowSignalEventActivityBehavior(intermediateEvent, signalEventDefinition, bpmnParse.BpmnModel.getSignal(signalEventDefinition.SignalRef));

            }
            else if (eventDefinition is CompensateEventDefinition)
            {
                CompensateEventDefinition compensateEventDefinition = (CompensateEventDefinition)eventDefinition;
                intermediateEvent.Behavior = bpmnParse.ActivityBehaviorFactory.createIntermediateThrowCompensationEventActivityBehavior(intermediateEvent, compensateEventDefinition);

            }
            else if (eventDefinition == null)
            {
                intermediateEvent.Behavior = bpmnParse.ActivityBehaviorFactory.createIntermediateThrowNoneEventActivityBehavior(intermediateEvent);
            }
            else
            {
                //logger.warn("Unsupported intermediate throw event type for throw event " + intermediateEvent.Id);
            }
        }

        //
        // Seems not to be used anymore?
        //
        // protected CompensateEventDefinition
        // createCompensateEventDefinition(BpmnParse bpmnParse,
        // org.activiti.bpmn.model.CompensateEventDefinition eventDefinition,
        // ScopeImpl scopeElement) {
        // if(!string.IsNullOrWhiteSpace(eventDefinition.getActivityRef())) {
        // if(scopeElement.findActivity(eventDefinition.getActivityRef()) == null) {
        // bpmnParse.getBpmnModel().addProblem("Invalid attribute value for 'activityRef': no activity with id '"
        // + eventDefinition.getActivityRef() +
        // "' in current scope " + scopeElement.getId(), eventDefinition);
        // }
        // }
        //
        // CompensateEventDefinition compensateEventDefinition = new
        // CompensateEventDefinition();
        // compensateEventDefinition.setActivityRef(eventDefinition.getActivityRef());
        // compensateEventDefinition.setWaitForCompletion(eventDefinition.isWaitForCompletion());
        //
        // return compensateEventDefinition;
        // }

    }

}