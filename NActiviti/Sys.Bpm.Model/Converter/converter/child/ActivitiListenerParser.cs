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
namespace Sys.Workflow.bpmn.converter.child
{
    using Sys.Workflow.bpmn.constants;
    using Sys.Workflow.bpmn.converter.util;
    using Sys.Workflow.bpmn.model;

    /// 
    /// 
    public abstract class ActivitiListenerParser : BaseChildElementParser
    {
        public override void ParseChildElement(XMLStreamReader xtr, BaseElement parentElement, BpmnModel model)
        {
            ActivitiListener listener = new ActivitiListener();
            BpmnXMLUtil.AddXMLLocation(listener, xtr);
            if (!string.IsNullOrWhiteSpace(xtr.GetAttributeValue(BpmnXMLConstants.ATTRIBUTE_LISTENER_CLASS)))
            {
                listener.Implementation = xtr.GetAttributeValue(BpmnXMLConstants.ATTRIBUTE_LISTENER_CLASS);
                listener.ImplementationType = ImplementationType.IMPLEMENTATION_TYPE_CLASS;
            }
            else if (!string.IsNullOrWhiteSpace(xtr.GetAttributeValue(BpmnXMLConstants.ATTRIBUTE_LISTENER_EXPRESSION)))
            {
                listener.Implementation = xtr.GetAttributeValue(BpmnXMLConstants.ATTRIBUTE_LISTENER_EXPRESSION);
                listener.ImplementationType = ImplementationType.IMPLEMENTATION_TYPE_EXPRESSION;
            }
            else if (!string.IsNullOrWhiteSpace(xtr.GetAttributeValue(BpmnXMLConstants.ATTRIBUTE_LISTENER_DELEGATEEXPRESSION)))
            {
                listener.Implementation = xtr.GetAttributeValue(BpmnXMLConstants.ATTRIBUTE_LISTENER_DELEGATEEXPRESSION);
                listener.ImplementationType = ImplementationType.IMPLEMENTATION_TYPE_DELEGATEEXPRESSION;
            }
            listener.Event = xtr.GetAttributeValue(BpmnXMLConstants.ATTRIBUTE_LISTENER_EVENT);
            listener.OnTransaction = xtr.GetAttributeValue(BpmnXMLConstants.ATTRIBUTE_LISTENER_ON_TRANSACTION);

            if (!string.IsNullOrWhiteSpace((xtr.GetAttributeValue(BpmnXMLConstants.ATTRIBUTE_LISTENER_CUSTOM_PROPERTIES_RESOLVER_CLASS))))
            {
                listener.CustomPropertiesResolverImplementation = xtr.GetAttributeValue(BpmnXMLConstants.ATTRIBUTE_LISTENER_CUSTOM_PROPERTIES_RESOLVER_CLASS);
                listener.CustomPropertiesResolverImplementationType = ImplementationType.IMPLEMENTATION_TYPE_CLASS;
            }
            else if (!string.IsNullOrWhiteSpace(xtr.GetAttributeValue(BpmnXMLConstants.ATTRIBUTE_LISTENER_CUSTOM_PROPERTIES_RESOLVER_EXPRESSION)))
            {
                listener.CustomPropertiesResolverImplementation = xtr.GetAttributeValue(BpmnXMLConstants.ATTRIBUTE_LISTENER_CUSTOM_PROPERTIES_RESOLVER_EXPRESSION);
                listener.CustomPropertiesResolverImplementationType = ImplementationType.IMPLEMENTATION_TYPE_EXPRESSION;
            }
            else if (!string.IsNullOrWhiteSpace(xtr.GetAttributeValue(BpmnXMLConstants.ATTRIBUTE_LISTENER_CUSTOM_PROPERTIES_RESOLVER_DELEGATEEXPRESSION)))
            {
                listener.CustomPropertiesResolverImplementation = xtr.GetAttributeValue(BpmnXMLConstants.ATTRIBUTE_LISTENER_CUSTOM_PROPERTIES_RESOLVER_DELEGATEEXPRESSION);
                listener.CustomPropertiesResolverImplementationType = ImplementationType.IMPLEMENTATION_TYPE_DELEGATEEXPRESSION;
            }
            AddListenerToParent(listener, parentElement);
            ParseChildElements(xtr, listener, model, new FieldExtensionParser());
        }

        public abstract void AddListenerToParent(ActivitiListener listener, BaseElement parentElement);
    }

}