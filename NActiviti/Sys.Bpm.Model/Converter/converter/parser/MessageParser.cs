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
namespace org.activiti.bpmn.converter.parser
{
    using org.activiti.bpmn.constants;
    using org.activiti.bpmn.converter.util;
    using org.activiti.bpmn.model;

    /// 
    public class MessageParser : IBpmnXMLConstants
    {
        public virtual void parse(XMLStreamReader xtr, BpmnModel model)
        {
            if (!string.IsNullOrWhiteSpace(xtr.getAttributeValue(org.activiti.bpmn.constants.BpmnXMLConstants.ATTRIBUTE_ID)))
            {
                string messageId = xtr.getAttributeValue(org.activiti.bpmn.constants.BpmnXMLConstants.ATTRIBUTE_ID);
                string messageName = xtr.getAttributeValue(org.activiti.bpmn.constants.BpmnXMLConstants.ATTRIBUTE_NAME);
                string itemRef = parseItemRef(xtr.getAttributeValue(org.activiti.bpmn.constants.BpmnXMLConstants.ATTRIBUTE_ITEM_REF), model);
                Message message = new Message(messageId, messageName, itemRef);
                BpmnXMLUtil.addXMLLocation(message, xtr);
                BpmnXMLUtil.parseChildElements(org.activiti.bpmn.constants.BpmnXMLConstants.ELEMENT_MESSAGE, message, xtr, model);
                model.addMessage(message);
            }
        }

        protected internal virtual string parseItemRef(string itemRef, BpmnModel model)
        {
            string result = null;
            if (!string.IsNullOrWhiteSpace(itemRef))
            {
                int indexOfP = itemRef.IndexOf(':');
                if (indexOfP != -1)
                {
                    string prefix = itemRef.Substring(0, indexOfP);
                    string resolvedNamespace = model.getNamespace(prefix);
                    result = resolvedNamespace + ":" + itemRef.Substring(indexOfP + 1);
                }
                else
                {
                    result = itemRef;
                }
            }
            return result;
        }
    }

}