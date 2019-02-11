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
    using Microsoft.Extensions.Logging;
    using org.activiti.bpmn.constants;
    using org.activiti.bpmn.converter.util;
    using org.activiti.bpmn.model;
    using Sys.Bpm;
    using System;

    public class DataAssociationParser : IBpmnXMLConstants
    {
        private static readonly ILogger log = BpmnModelLoggerFactory.LoggerService<DataAssociationParser>();

        public static void parseDataAssociation(DataAssociation dataAssociation, string elementName, XMLStreamReader xtr)
        {
            bool readyWithDataAssociation = false;
            Assignment assignment = null;
            try
            {

                dataAssociation.Id = xtr.getAttributeValue("id");

                while (!readyWithDataAssociation && xtr.hasNext())
                {
                    //xtr.next();

                    if (xtr.IsStartElement() && BpmnXMLConstants.ELEMENT_SOURCE_REF.Equals(xtr.LocalName))
                    {
                        string sourceRef = xtr.ElementText;
                        if (!string.IsNullOrWhiteSpace(sourceRef))
                        {
                            dataAssociation.SourceRef = sourceRef.Trim();
                        }

                    }
                    else if (xtr.IsStartElement() && BpmnXMLConstants.ELEMENT_TARGET_REF.Equals(xtr.LocalName))
                    {
                        string targetRef = xtr.ElementText;
                        if (!string.IsNullOrWhiteSpace(targetRef))
                        {
                            dataAssociation.TargetRef = targetRef.Trim();
                        }

                    }
                    else if (xtr.IsStartElement() && BpmnXMLConstants.ELEMENT_TRANSFORMATION.Equals(xtr.LocalName))
                    {
                        string transformation = xtr.ElementText;
                        if (!string.IsNullOrWhiteSpace(transformation))
                        {
                            dataAssociation.Transformation = transformation.Trim();
                        }

                    }
                    else if (xtr.IsStartElement() && BpmnXMLConstants.ELEMENT_ASSIGNMENT.Equals(xtr.LocalName))
                    {
                        assignment = new Assignment();
                        BpmnXMLUtil.addXMLLocation(assignment, xtr);

                    }
                    else if (xtr.IsStartElement() && BpmnXMLConstants.ELEMENT_FROM.Equals(xtr.LocalName))
                    {
                        string from = xtr.ElementText;
                        if (assignment != null && !string.IsNullOrWhiteSpace(from))
                        {
                            assignment.From = from.Trim();
                        }

                    }
                    else if (xtr.IsStartElement() && BpmnXMLConstants.ELEMENT_TO.Equals(xtr.LocalName))
                    {
                        string to = xtr.ElementText;
                        if (assignment != null && !string.IsNullOrWhiteSpace(to))
                        {
                            assignment.To = to.Trim();
                        }

                    }
                    else if (xtr.EndElement && BpmnXMLConstants.ELEMENT_ASSIGNMENT.Equals(xtr.LocalName))
                    {
                        if (!string.IsNullOrWhiteSpace(assignment.From) && !string.IsNullOrWhiteSpace(assignment.To))
                        {
                            dataAssociation.Assignments.Add(assignment);
                        }

                    }
                    else if (xtr.EndElement && elementName.Equals(xtr.LocalName))
                    {
                        readyWithDataAssociation = true;
                    }

                    if (xtr.IsEmptyElement && elementName.Equals(xtr.LocalName, StringComparison.OrdinalIgnoreCase))
                    {
                        readyWithDataAssociation = true;
                    }
                }
            }
            catch (Exception e)
            {
                log.LogWarning(e, "Error parsing data association child elements");
            }
        }
    }

}