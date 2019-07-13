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
namespace Sys.Workflow.Bpmn.Converters.Parsers
{

    using Sys.Workflow.Bpmn.Constants;
    using Sys.Workflow.Bpmn.Converters.Utils;
    using Sys.Workflow.Bpmn.Models;

    /// 
    public class SignalParser : IBpmnXMLConstants
    {
        public virtual void Parse(XMLStreamReader xtr, BpmnModel model)
        {
            string signalId = xtr.GetAttributeValue(BpmnXMLConstants.ATTRIBUTE_ID);
            string signalName = xtr.GetAttributeValue(BpmnXMLConstants.ATTRIBUTE_NAME);

            Signal signal = new Signal(signalId, signalName);

            string scope = xtr.GetAttributeValue(BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE, BpmnXMLConstants.ATTRIBUTE_SCOPE);
            if (scope is object)
            {
                signal.Scope = scope;
            }

            BpmnXMLUtil.AddXMLLocation(signal, xtr);
            BpmnXMLUtil.ParseChildElements(BpmnXMLConstants.ELEMENT_SIGNAL, signal, xtr, model);
            model.AddSignal(signal);
        }
    }
}