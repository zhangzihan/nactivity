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
namespace org.activiti.bpmn.converter.parser
{

    using org.activiti.bpmn.constants;
    using org.activiti.bpmn.model;

    /// 
    public class PotentialStarterParser : IBpmnXMLConstants
    {
        public virtual void parse(XMLStreamReader xtr, Process activeProcess)
        {
            string resourceElement = XMLStreamReaderUtil.moveDown(xtr);
            if (!string.IsNullOrWhiteSpace(resourceElement) && "resourceAssignmentExpression".Equals(resourceElement))
            {
                string expression = XMLStreamReaderUtil.moveDown(xtr);
                if (!string.IsNullOrWhiteSpace(expression) && "formalExpression".Equals(expression))
                {
                    IList<string> assignmentList = new List<string>();
                    string assignmentText = xtr.ElementText;
                    if (assignmentText.Contains(","))
                    {
                        string[] assignmentArray = assignmentText.Split(",", true);
                        assignmentList = new List<string>(assignmentArray);
                    }
                    else
                    {
                        assignmentList.Add(assignmentText);
                    }
                    for (var idx = 0; idx < assignmentList.Count; idx++)
                    {
                        string assignmentValue = assignmentList[idx];
                        if (string.ReferenceEquals(assignmentValue, null))
                        {
                            continue;
                        }
                        assignmentValue = assignmentList[idx] = assignmentValue.Trim();
                        if (assignmentValue.Length == 0)
                        {
                            continue;
                        }

                        string userPrefix = "user(";
                        string groupPrefix = "group(";
                        if (assignmentValue.StartsWith(userPrefix, StringComparison.Ordinal))
                        {
                            assignmentValue = assignmentList[idx] = StringHelper.SubstringSpecial(assignmentValue, userPrefix.Length, assignmentValue.Length - 1).Trim();
                            activeProcess.CandidateStarterUsers.Add(assignmentValue);
                        }
                        else if (assignmentValue.StartsWith(groupPrefix, StringComparison.Ordinal))
                        {
                            assignmentValue = assignmentList[idx] = StringHelper.SubstringSpecial(assignmentValue, groupPrefix.Length, assignmentValue.Length - 1).Trim();
                            activeProcess.CandidateStarterGroups.Add(assignmentValue);
                        }
                        else
                        {
                            activeProcess.CandidateStarterGroups.Add(assignmentValue);
                        }
                    }
                }
            }
        }
    }

}