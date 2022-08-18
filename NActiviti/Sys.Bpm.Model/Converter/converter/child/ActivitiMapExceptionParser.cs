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
namespace Sys.Workflow.Bpmn.Converters.Childs
{
    using Sys.Workflow.Bpmn.Constants;
    using Sys.Workflow.Bpmn.Exceptions;
    using Sys.Workflow.Bpmn.Models;

    /// 

    public class ActivitiMapExceptionParser : BaseChildElementParser
    {

        public override string ElementName
        {
            get
            {
                return BpmnXMLConstants.MAP_EXCEPTION;
            }
        }
        public override void ParseChildElement(XMLStreamReader xtr, BaseElement parentElement, BpmnModel model)
        {
            if (parentElement is not Activity)
            {
                return;
            }

            string errorCode = xtr.GetAttributeValue(BpmnXMLConstants.MAP_EXCEPTION_ERRORCODE);
            string andChildren = xtr.GetAttributeValue(BpmnXMLConstants.MAP_EXCEPTION_ANDCHILDREN);
            string exceptionClass = xtr.ElementText;
            bool hasChildrenBool = false;

            if (string.IsNullOrWhiteSpace(andChildren) || andChildren.ToLower().Equals("false"))
            {
                hasChildrenBool = false;
            }
            else if (andChildren.ToLower().Equals("true"))
            {
                hasChildrenBool = true;
            }
            else
            {
                throw new XMLException("'" + andChildren + "' is not valid boolean in mapException with errorCode=" + errorCode + " and class=" + exceptionClass);
            }

            if (string.IsNullOrWhiteSpace(errorCode) || string.IsNullOrWhiteSpace(errorCode.Trim()))
            {
                throw new XMLException("No errorCode defined mapException with errorCode=" + errorCode + " and class=" + exceptionClass);
            }

          ((Activity)parentElement).MapExceptions.Add(new MapExceptionEntry(errorCode, exceptionClass, hasChildrenBool));
        }
    }

}