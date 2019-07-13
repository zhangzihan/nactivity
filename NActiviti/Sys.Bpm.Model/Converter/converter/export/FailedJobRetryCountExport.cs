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
namespace Sys.Workflow.Bpmn.Converters.Exports
{

    using Sys.Workflow.Bpmn.Constants;
    using Sys.Workflow.Bpmn.Models;

    public class FailedJobRetryCountExport : IBpmnXMLConstants
    {
        public static void WriteFailedJobRetryCount(Activity activity, XMLStreamWriter xtw)
        {
            string failedJobRetryCycle = activity.FailedJobRetryTimeCycleValue;
            if (failedJobRetryCycle is object)
            {

                if (!string.IsNullOrWhiteSpace(failedJobRetryCycle))
                {
                    xtw.WriteStartElement(BpmnXMLConstants.ACTIVITI_EXTENSIONS_PREFIX, BpmnXMLConstants.FAILED_JOB_RETRY_TIME_CYCLE, BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE);
                    xtw.WriteCharacters(failedJobRetryCycle);
                    xtw.WriteEndElement();
                }
            }
        }
    }
}