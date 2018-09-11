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
namespace org.activiti.bpmn.converter.export
{

    using org.activiti.bpmn.constants;
    using org.activiti.bpmn.model;

    public class FailedJobRetryCountExport : IBpmnXMLConstants
    {
        public static void writeFailedJobRetryCount(Activity activity, XMLStreamWriter xtw)
        {
            string failedJobRetryCycle = activity.FailedJobRetryTimeCycleValue;
            if (!string.ReferenceEquals(failedJobRetryCycle, null))
            {

                if (!string.IsNullOrWhiteSpace(failedJobRetryCycle))
                {
                    xtw.writeStartElement(org.activiti.bpmn.constants.BpmnXMLConstants.ACTIVITI_EXTENSIONS_PREFIX, org.activiti.bpmn.constants.BpmnXMLConstants.FAILED_JOB_RETRY_TIME_CYCLE, org.activiti.bpmn.constants.BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE);
                    xtw.writeCharacters(failedJobRetryCycle);
                    xtw.writeEndElement();
                }
            }
        }
    }

}