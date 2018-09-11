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
namespace org.activiti.engine.impl.bpmn.parser.handler
{

    using org.activiti.bpmn.model;
    using org.activiti.engine.@delegate;
    using org.activiti.engine.parse;

    public abstract class AbstractBpmnParseHandler<T> : IBpmnParseHandler where T : org.activiti.bpmn.model.BaseElement
    {

        public virtual ICollection<Type> HandledTypes
        {
            get
            {
                ISet<Type> types = new HashSet<Type>();
                types.Add(HandledType);
                return types;
            }
        }

        protected internal abstract Type HandledType { get; }

        public virtual void parse(BpmnParse bpmnParse, BaseElement element)
        {
            T baseElement = (T)element;
            executeParse(bpmnParse, baseElement);
        }

        protected internal abstract void executeParse(BpmnParse bpmnParse, T element);

        protected internal virtual IExecutionListener createExecutionListener(BpmnParse bpmnParse, ActivitiListener activitiListener)
        {
            IExecutionListener executionListener = null;

            if (ImplementationType.IMPLEMENTATION_TYPE_CLASS.Equals(activitiListener.ImplementationType, StringComparison.CurrentCultureIgnoreCase))
            {
                executionListener = bpmnParse.ListenerFactory.createClassDelegateExecutionListener(activitiListener);
            }
            else if (ImplementationType.IMPLEMENTATION_TYPE_EXPRESSION.Equals(activitiListener.ImplementationType, StringComparison.CurrentCultureIgnoreCase))
            {
                executionListener = bpmnParse.ListenerFactory.createExpressionExecutionListener(activitiListener);
            }
            else if (ImplementationType.IMPLEMENTATION_TYPE_DELEGATEEXPRESSION.Equals(activitiListener.ImplementationType, StringComparison.CurrentCultureIgnoreCase))
            {
                executionListener = bpmnParse.ListenerFactory.createDelegateExpressionExecutionListener(activitiListener);
            }
            return executionListener;
        }

        protected internal virtual string getPrecedingEventBasedGateway(BpmnParse bpmnParse, IntermediateCatchEvent @event)
        {
            string eventBasedGatewayId = null;
            foreach (SequenceFlow sequenceFlow in @event.IncomingFlows)
            {
                FlowElement sourceElement = bpmnParse.BpmnModel.getFlowElement(sequenceFlow.SourceRef);
                if (sourceElement is EventGateway)
                {
                    eventBasedGatewayId = sourceElement.Id;
                    break;
                }
            }
            return eventBasedGatewayId;
        }

        protected internal virtual void processArtifacts(BpmnParse bpmnParse, ICollection<Artifact> artifacts)
        {
            // associations
            foreach (Artifact artifact in artifacts)
            {
                if (artifact is Association)
                {
                    createAssociation(bpmnParse, (Association)artifact);
                }
            }
        }

        protected internal virtual void createAssociation(BpmnParse bpmnParse, Association association)
        {
            BpmnModel bpmnModel = bpmnParse.BpmnModel;
            if (bpmnModel.getArtifact(association.SourceRef) != null || bpmnModel.getArtifact(association.TargetRef) != null)
            {

                // connected to a text annotation so skipping it
                return;
            }

            // ActivityImpl sourceActivity =
            // parentScope.findActivity(association.getSourceRef());
            // ActivityImpl targetActivity =
            // parentScope.findActivity(association.getTargetRef());

            // an association may reference elements that are not parsed as
            // activities (like for instance
            // text annotations so do not throw an exception if sourceActivity or
            // targetActivity are null)
            // However, we make sure they reference 'something':
            // if (sourceActivity == null) {
            // bpmnModel.addProblem("Invalid reference sourceRef '" +
            // association.getSourceRef() + "' of association element ",
            // association.getId());
            // } else if (targetActivity == null) {
            // bpmnModel.addProblem("Invalid reference targetRef '" +
            // association.getTargetRef() + "' of association element ",
            // association.getId());
            /*
             * } else { if (sourceActivity.getProperty("type").equals("compensationBoundaryCatch" )) { Object isForCompensation = targetActivity.getProperty(PROPERTYNAME_IS_FOR_COMPENSATION); if
             * (isForCompensation == null || !(Boolean) isForCompensation) { logger.warn( "compensation boundary catch must be connected to element with isForCompensation=true" ); } else { ActivityImpl
             * compensatedActivity = sourceActivity.getParentActivity(); compensatedActivity.setProperty(BpmnParse .PROPERTYNAME_COMPENSATION_HANDLER_ID, targetActivity.getId()); } } }
             */
        }
    }

}