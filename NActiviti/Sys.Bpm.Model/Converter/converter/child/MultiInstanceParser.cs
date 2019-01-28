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
namespace org.activiti.bpmn.converter.child
{
    using org.activiti.bpmn.constants;
    using org.activiti.bpmn.converter.util;
    using org.activiti.bpmn.model;

    public class MultiInstanceParser : BaseChildElementParser
    {

        public override string ElementName
        {
            get
            {
                return BpmnXMLConstants.ELEMENT_MULTIINSTANCE;
            }
        }

        public override void parseChildElement(XMLStreamReader xtr, BaseElement parentElement, BpmnModel model)
        {
            if (!(parentElement is Activity))
            {
                return;
            }
            MultiInstanceLoopCharacteristics multiInstanceDef = new MultiInstanceLoopCharacteristics();
            BpmnXMLUtil.addXMLLocation(multiInstanceDef, xtr);
            if (xtr.getAttributeValue(BpmnXMLConstants.ATTRIBUTE_MULTIINSTANCE_SEQUENTIAL) != null)
            {
                multiInstanceDef.Sequential = Convert.ToBoolean(xtr.getAttributeValue(BpmnXMLConstants.ATTRIBUTE_MULTIINSTANCE_SEQUENTIAL));
            }
            multiInstanceDef.InputDataItem = xtr.getAttributeValue(BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE, BpmnXMLConstants.ATTRIBUTE_MULTIINSTANCE_COLLECTION);
            multiInstanceDef.ElementVariable = xtr.getAttributeValue(BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE, BpmnXMLConstants.ATTRIBUTE_MULTIINSTANCE_VARIABLE);
            multiInstanceDef.ElementIndexVariable = xtr.getAttributeValue(BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE, BpmnXMLConstants.ATTRIBUTE_MULTIINSTANCE_INDEX_VARIABLE);

            bool readyWithMultiInstance = false;
            try
            {
                while (!readyWithMultiInstance && xtr.hasNext())
                {
                    //xtr.next();

                    if (xtr.IsStartElement() && BpmnXMLConstants.ELEMENT_MULTIINSTANCE_CARDINALITY.Equals(xtr.LocalName, StringComparison.CurrentCultureIgnoreCase))
                    {
                        multiInstanceDef.LoopCardinality = xtr.ElementText;
                    }
                    else if (xtr.IsStartElement() && BpmnXMLConstants.ELEMENT_MULTIINSTANCE_DATAINPUT.Equals(xtr.LocalName, StringComparison.CurrentCultureIgnoreCase))
                    {
                        multiInstanceDef.InputDataItem = xtr.ElementText;
                    }
                    else if (xtr.IsStartElement() && BpmnXMLConstants.ELEMENT_MULTIINSTANCE_DATAITEM.Equals(xtr.LocalName, StringComparison.CurrentCultureIgnoreCase))
                    {
                        if (xtr.getAttributeValue(BpmnXMLConstants.ATTRIBUTE_NAME) != null)
                        {
                            multiInstanceDef.ElementVariable = xtr.getAttributeValue(BpmnXMLConstants.ATTRIBUTE_NAME);
                        }
                    }
                    else if (xtr.IsStartElement() && BpmnXMLConstants.ELEMENT_MULTIINSTANCE_CONDITION.Equals(xtr.LocalName, StringComparison.CurrentCultureIgnoreCase))
                    {
                        multiInstanceDef.CompletionCondition = xtr.ElementText;
                    }
                    else if (xtr.EndElement && ElementName.Equals(xtr.LocalName, StringComparison.CurrentCultureIgnoreCase))
                    {
                        readyWithMultiInstance = true;
                    }

                    if (xtr.IsEmptyElement && ElementName.Equals(xtr.LocalName, StringComparison.OrdinalIgnoreCase))
                    {
                        readyWithMultiInstance = true;
                    }
                } 
            }
            catch (Exception e)
            {
                throw;
                //LOGGER.warn("Error parsing multi instance definition", e);
            }
            ((Activity)parentElement).LoopCharacteristics = multiInstanceDef;
        }
    }

}