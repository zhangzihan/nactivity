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
namespace Sys.Workflow.bpmn.converter.parser
{
    using Sys.Workflow.bpmn.constants;
    using Sys.Workflow.bpmn.converter.util;
    using Sys.Workflow.bpmn.model;

    /// 
    public class MessageParser : IBpmnXMLConstants
    {
        public virtual void Parse(XMLStreamReader xtr, BpmnModel model)
        {
            if (!string.IsNullOrWhiteSpace(xtr.GetAttributeValue(BpmnXMLConstants.ATTRIBUTE_ID)))
            {
                string messageId = xtr.GetAttributeValue(BpmnXMLConstants.ATTRIBUTE_ID);
                string messageName = xtr.GetAttributeValue(BpmnXMLConstants.ATTRIBUTE_NAME);
                string itemRef = ParseItemRef(xtr.GetAttributeValue(BpmnXMLConstants.ATTRIBUTE_ITEM_REF), model);
                Message message = new Message(messageId, messageName, itemRef);
                BpmnXMLUtil.AddXMLLocation(message, xtr);
                BpmnXMLUtil.ParseChildElements(BpmnXMLConstants.ELEMENT_MESSAGE, message, xtr, model);
                model.AddMessage(message);
            }
        }

        protected internal virtual string ParseItemRef(string itemRef, BpmnModel model)
        {
            string result = null;
            if (!string.IsNullOrWhiteSpace(itemRef))
            {
                int indexOfP = itemRef.IndexOf(':');
                if (indexOfP != -1)
                {
                    string prefix = itemRef.Substring(0, indexOfP);
                    string resolvedNamespace = model.GetNamespace(prefix);
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