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
namespace org.activiti.bpmn.converter
{
    using org.activiti.bpmn.constants;
    using org.activiti.bpmn.converter.child;
    using org.activiti.bpmn.converter.util;
    using org.activiti.bpmn.model;
    using org.activiti.bpmn.model.alfresco;
    using System.Linq;

    /// 
    public class UserTaskXMLConverter : BaseBpmnXMLConverter
    {

        protected internal IDictionary<string, BaseChildElementParser> childParserMap = new Dictionary<string, BaseChildElementParser>();

        /// <summary>
        /// default attributes taken from bpmn spec and from activiti extension </summary>
        protected static IList<ExtensionAttribute> defaultUserTaskAttributes = new List<ExtensionAttribute>()
        {
            new ExtensionAttribute(BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE, BpmnXMLConstants.ATTRIBUTE_FORM_FORMKEY),
            new ExtensionAttribute(BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE, BpmnXMLConstants.ATTRIBUTE_TASK_USER_DUEDATE),
            new ExtensionAttribute(BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE, BpmnXMLConstants.ATTRIBUTE_TASK_USER_BUSINESS_CALENDAR_NAME),
            new ExtensionAttribute(BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE, BpmnXMLConstants.ATTRIBUTE_TASK_USER_ASSIGNEE),
            new ExtensionAttribute(BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE, BpmnXMLConstants.ATTRIBUTE_TASK_USER_OWNER),
            new ExtensionAttribute(BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE, BpmnXMLConstants.ATTRIBUTE_TASK_USER_PRIORITY),
            new ExtensionAttribute(BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE, BpmnXMLConstants.ATTRIBUTE_TASK_USER_CANDIDATEUSERS),
            new ExtensionAttribute(BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE, BpmnXMLConstants.ATTRIBUTE_TASK_USER_CANDIDATEGROUPS),
            new ExtensionAttribute(BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE, BpmnXMLConstants.ATTRIBUTE_TASK_USER_CATEGORY),
            new ExtensionAttribute(BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE, BpmnXMLConstants.ATTRIBUTE_TASK_SERVICE_EXTENSIONID),
            new ExtensionAttribute(BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE, BpmnXMLConstants.ATTRIBUTE_TASK_USER_SKIP_EXPRESSION)
        };

        public UserTaskXMLConverter()
        {
            HumanPerformerParser humanPerformerParser = new HumanPerformerParser();
            childParserMap[humanPerformerParser.ElementName] = humanPerformerParser;
            PotentialOwnerParser potentialOwnerParser = new PotentialOwnerParser();
            childParserMap[potentialOwnerParser.ElementName] = potentialOwnerParser;
            CustomIdentityLinkParser customIdentityLinkParser = new CustomIdentityLinkParser();
            childParserMap[customIdentityLinkParser.ElementName] = customIdentityLinkParser;
        }

        public override Type BpmnElementType => typeof(UserTask);

        public override string XMLElementName => BpmnXMLConstants.ELEMENT_TASK_USER;

        protected internal override BaseElement convertXMLToElement(XMLStreamReader xtr, BpmnModel model)
        {
            string formKey = xtr.getAttributeValue(BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE, BpmnXMLConstants.ATTRIBUTE_FORM_FORMKEY);
            UserTask userTask = null;
            if (!string.IsNullOrWhiteSpace(formKey))
            {
                if (model.UserTaskFormTypes != null && model.UserTaskFormTypes.Contains(formKey))
                {
                    userTask = new AlfrescoUserTask();
                }
            }
            if (userTask == null)
            {
                userTask = new UserTask();
            }
            BpmnXMLUtil.addXMLLocation(userTask, xtr);
            userTask.DueDate = xtr.getAttributeValue(BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE, BpmnXMLConstants.ATTRIBUTE_TASK_USER_DUEDATE);
            userTask.BusinessCalendarName = xtr.getAttributeValue(BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE, BpmnXMLConstants.ATTRIBUTE_TASK_USER_BUSINESS_CALENDAR_NAME);
            userTask.Category = xtr.getAttributeValue(BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE, BpmnXMLConstants.ATTRIBUTE_TASK_USER_CATEGORY);
            userTask.FormKey = formKey;
            userTask.Assignee = xtr.getAttributeValue(BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE, BpmnXMLConstants.ATTRIBUTE_TASK_USER_ASSIGNEE);
            userTask.Owner = xtr.getAttributeValue(BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE, BpmnXMLConstants.ATTRIBUTE_TASK_USER_OWNER);
            userTask.Priority = xtr.getAttributeValue(BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE, BpmnXMLConstants.ATTRIBUTE_TASK_USER_PRIORITY);

            if (!string.IsNullOrWhiteSpace(xtr.getAttributeValue(BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE, BpmnXMLConstants.ATTRIBUTE_TASK_USER_CANDIDATEUSERS)))
            {
                string expression = xtr.getAttributeValue(BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE, BpmnXMLConstants.ATTRIBUTE_TASK_USER_CANDIDATEUSERS);
                (userTask.CandidateUsers as List<string>).AddRange(parseDelimitedList(expression));
            }

            if (!string.IsNullOrWhiteSpace(xtr.getAttributeValue(BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE, BpmnXMLConstants.ATTRIBUTE_TASK_USER_CANDIDATEGROUPS)))
            {
                string expression = xtr.getAttributeValue(BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE, BpmnXMLConstants.ATTRIBUTE_TASK_USER_CANDIDATEGROUPS);
                (userTask.CandidateGroups as List<string>).AddRange(parseDelimitedList(expression));
            }

            userTask.ExtensionId = xtr.getAttributeValue(BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE, BpmnXMLConstants.ATTRIBUTE_TASK_SERVICE_EXTENSIONID);

            if (!string.IsNullOrWhiteSpace(xtr.getAttributeValue(BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE, BpmnXMLConstants.ATTRIBUTE_TASK_USER_SKIP_EXPRESSION)))
            {
                string expression = xtr.getAttributeValue(BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE, BpmnXMLConstants.ATTRIBUTE_TASK_USER_SKIP_EXPRESSION);
                userTask.SkipExpression = expression;
            }

            BpmnXMLUtil.addCustomAttributes(xtr, userTask, defaultElementAttributes, defaultActivityAttributes, defaultUserTaskAttributes);

            parseChildElements(XMLElementName, userTask, childParserMap, model, xtr);

            return userTask;
        }

        protected internal override void writeAdditionalAttributes(BaseElement element, BpmnModel model, XMLStreamWriter xtw)
        {
            UserTask userTask = (UserTask)element;
            writeQualifiedAttribute(BpmnXMLConstants.ATTRIBUTE_TASK_USER_ASSIGNEE, userTask.Assignee, xtw);
            writeQualifiedAttribute(BpmnXMLConstants.ATTRIBUTE_TASK_USER_OWNER, userTask.Owner, xtw);
            writeQualifiedAttribute(BpmnXMLConstants.ATTRIBUTE_TASK_USER_CANDIDATEUSERS, convertToDelimitedString(userTask.CandidateUsers), xtw);
            writeQualifiedAttribute(BpmnXMLConstants.ATTRIBUTE_TASK_USER_CANDIDATEGROUPS, convertToDelimitedString(userTask.CandidateGroups), xtw);
            writeQualifiedAttribute(BpmnXMLConstants.ATTRIBUTE_TASK_USER_DUEDATE, userTask.DueDate, xtw);
            writeQualifiedAttribute(BpmnXMLConstants.ATTRIBUTE_TASK_USER_BUSINESS_CALENDAR_NAME, userTask.BusinessCalendarName, xtw);
            writeQualifiedAttribute(BpmnXMLConstants.ATTRIBUTE_TASK_USER_CATEGORY, userTask.Category, xtw);
            writeQualifiedAttribute(BpmnXMLConstants.ATTRIBUTE_FORM_FORMKEY, userTask.FormKey, xtw);
            if (userTask.Priority != null)

            {
                writeQualifiedAttribute(BpmnXMLConstants.ATTRIBUTE_TASK_USER_PRIORITY, userTask.Priority.ToString(), xtw);
            }
            if (!string.IsNullOrWhiteSpace(userTask.ExtensionId))

            {
                writeQualifiedAttribute(BpmnXMLConstants.ATTRIBUTE_TASK_SERVICE_EXTENSIONID, userTask.ExtensionId, xtw);
            }
            if (userTask.SkipExpression != null)

            {
                writeQualifiedAttribute(BpmnXMLConstants.ATTRIBUTE_TASK_USER_SKIP_EXPRESSION, userTask.SkipExpression, xtw);
            }
            // write custom attributes
            BpmnXMLUtil.writeCustomAttributes(userTask.Attributes.Values, xtw, defaultElementAttributes, defaultActivityAttributes, defaultUserTaskAttributes);
        }

        protected internal override bool writeExtensionChildElements(BaseElement element, bool didWriteExtensionStartElement, XMLStreamWriter xtw)
        {
            UserTask userTask = (UserTask)element;
            didWriteExtensionStartElement = writeFormProperties(userTask, didWriteExtensionStartElement, xtw);
            didWriteExtensionStartElement = writeCustomIdentities(element, didWriteExtensionStartElement, xtw);
            if (userTask.CustomProperties.Count() > 0)
            {
                foreach (CustomProperty customProperty in userTask.CustomProperties)
                {
                    if (string.IsNullOrWhiteSpace(customProperty.SimpleValue))
                    {
                        continue;
                    }

                    if (!didWriteExtensionStartElement)
                    {
                        xtw.writeStartElement(BpmnXMLConstants.ELEMENT_EXTENSIONS);
                        didWriteExtensionStartElement = true;
                    }
                    xtw.writeStartElement(BpmnXMLConstants.ACTIVITI_EXTENSIONS_PREFIX, customProperty.Name, BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE);
                    xtw.writeCharacters(customProperty.SimpleValue);
                    xtw.writeEndElement();
                }
            }
            return didWriteExtensionStartElement;
        }

        protected bool writeCustomIdentities(BaseElement element, bool didWriteExtensionStartElement, XMLStreamWriter xtw)
        {
            UserTask userTask = (UserTask)element;
            if (userTask.CustomUserIdentityLinks.Count == 0 && userTask.CustomGroupIdentityLinks.Count == 0)

            {
                return didWriteExtensionStartElement;
            }

            if (!didWriteExtensionStartElement)

            {
                xtw.writeStartElement(BpmnXMLConstants.ELEMENT_EXTENSIONS);
                didWriteExtensionStartElement = true;
            }
            List<string> identityLinkTypes = new List<string>();
            identityLinkTypes.AddRange(userTask.CustomUserIdentityLinks.Keys);
            identityLinkTypes.AddRange(userTask.CustomGroupIdentityLinks.Keys);
            foreach (string identityType in identityLinkTypes)
            {
                writeCustomIdentities(userTask, identityType, userTask.CustomUserIdentityLinks[identityType], userTask.CustomGroupIdentityLinks[identityType], xtw);
            }

            return didWriteExtensionStartElement;
        }

        protected void writeCustomIdentities(UserTask userTask, string identityType, ISet<string> users, ISet<string> groups, XMLStreamWriter xtw)
        {
            xtw.writeStartElement(BpmnXMLConstants.ACTIVITI_EXTENSIONS_PREFIX, BpmnXMLConstants.ELEMENT_CUSTOM_RESOURCE, BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE);
            writeDefaultAttribute(BpmnXMLConstants.ATTRIBUTE_NAME, identityType, xtw);

            IList<string> identityList = new List<string>();

            if (users != null)
            {
                foreach (string userId in users)
                {
                    identityList.Add("user(" + userId + ")");
                }
            }

            if (groups != null)
            {
                foreach (string groupId in groups)
                {
                    identityList.Add("group(" + groupId + ")");
                }
            }

            string delimitedString = convertToDelimitedString(identityList);

            xtw.writeStartElement(BpmnXMLConstants.ELEMENT_RESOURCE_ASSIGNMENT);
            xtw.writeStartElement(BpmnXMLConstants.ELEMENT_FORMAL_EXPRESSION);
            xtw.writeCharacters(delimitedString);
            xtw.writeEndElement(); // End ELEMENT_FORMAL_EXPRESSION
            xtw.writeEndElement(); // End ELEMENT_RESOURCE_ASSIGNMENT

            xtw.writeEndElement(); // End ELEMENT_CUSTOM_RESOURCE
        }

        protected internal override void writeAdditionalChildElements(BaseElement element, BpmnModel model, XMLStreamWriter xtw)
        {
        }

        public class HumanPerformerParser : BaseChildElementParser
        {
            public override string ElementName => "humanPerformer";

            public override void parseChildElement(XMLStreamReader xtr, BaseElement parentElement, BpmnModel model)
            {

                string resourceElement = XMLStreamReaderUtil.moveDown(xtr);
                if (!string.IsNullOrWhiteSpace(resourceElement) && BpmnXMLConstants.ELEMENT_RESOURCE_ASSIGNMENT.Equals(resourceElement))

                {
                    string expression = XMLStreamReaderUtil.moveDown(xtr);
                    if (!string.IsNullOrWhiteSpace(expression) && BpmnXMLConstants.ELEMENT_FORMAL_EXPRESSION.Equals(expression))
                    {
                        ((UserTask)parentElement).Assignee = xtr.ElementText;
                    }
                }
            }
        }

        public class PotentialOwnerParser : BaseChildElementParser
        {

            public override string ElementName => "potentialOwner";

            public override void parseChildElement(XMLStreamReader xtr, BaseElement parentElement, BpmnModel model)
            {

                string resourceElement = XMLStreamReaderUtil.moveDown(xtr);
                if (!string.IsNullOrWhiteSpace(resourceElement) && BpmnXMLConstants.ELEMENT_RESOURCE_ASSIGNMENT.Equals(resourceElement))

                {
                    string expression = XMLStreamReaderUtil.moveDown(xtr);
                    if (!string.IsNullOrWhiteSpace(expression) && BpmnXMLConstants.ELEMENT_FORMAL_EXPRESSION.Equals(expression))
                    {

                        IList<string> assignmentList = CommaSplitter.splitCommas(xtr.ElementText);

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
                                ((UserTask)parentElement).CandidateUsers.Add(assignmentValue);
                            }
                            else if (assignmentValue.StartsWith(groupPrefix, StringComparison.Ordinal))
                            {
                                assignmentValue = assignmentList[idx] = StringHelper.SubstringSpecial(assignmentValue, groupPrefix.Length, assignmentValue.Length - 1).Trim();
                                ((UserTask)parentElement).CandidateGroups.Add(assignmentValue);
                            }
                            else
                            {
                                ((UserTask)parentElement).CandidateGroups.Add(assignmentValue);
                            }
                        }
                    }
                }
                else if (!string.IsNullOrWhiteSpace(resourceElement) && BpmnXMLConstants.ELEMENT_RESOURCE_REF.Equals(resourceElement))

                {
                    string resourceId = xtr.ElementText;
                    if (model.containsResourceId(resourceId))
                    {
                        Resource resource = model.getResource(resourceId);
                        ((UserTask)parentElement).CandidateGroups.Add(resource.Name);
                    }
                    else
                    {
                        Resource resource = new Resource(resourceId, resourceId);
                        model.addResource(resource);
                        ((UserTask)parentElement).CandidateGroups.Add(resource.Name);
                    }
                }
            }
        }

        public class CustomIdentityLinkParser : BaseChildElementParser
        {


            public override string ElementName => constants.BpmnXMLConstants.ELEMENT_CUSTOM_RESOURCE;

            public override void parseChildElement(XMLStreamReader xtr, BaseElement parentElement, BpmnModel model)
            {

                string identityLinkType = xtr.getAttributeValue(BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE, BpmnXMLConstants.ATTRIBUTE_NAME);

                // the attribute value may be unqualified
                if (string.ReferenceEquals(identityLinkType, null))

                {
                    identityLinkType = xtr.getAttributeValue(BpmnXMLConstants.ATTRIBUTE_NAME);
                }

                if (string.ReferenceEquals(identityLinkType, null))

                {
                    return;
                }

                string resourceElement = XMLStreamReaderUtil.moveDown(xtr);
                if (!string.IsNullOrWhiteSpace(resourceElement) && BpmnXMLConstants.ELEMENT_RESOURCE_ASSIGNMENT.Equals(resourceElement))

                {
                    string expression = XMLStreamReaderUtil.moveDown(xtr);
                    if (!string.IsNullOrWhiteSpace(expression) && BpmnXMLConstants.ELEMENT_FORMAL_EXPRESSION.Equals(expression))
                    {

                        IList<string> assignmentList = CommaSplitter.splitCommas(xtr.ElementText);

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
                                ((UserTask)parentElement).addCustomUserIdentityLink(assignmentValue, identityLinkType);
                            }
                            else if (assignmentValue.StartsWith(groupPrefix, StringComparison.Ordinal))
                            {
                                assignmentValue = assignmentList[idx] = StringHelper.SubstringSpecial(assignmentValue, groupPrefix.Length, assignmentValue.Length - 1).Trim();
                                ((UserTask)parentElement).addCustomGroupIdentityLink(assignmentValue, identityLinkType);
                            }
                            else
                            {
                                ((UserTask)parentElement).addCustomGroupIdentityLink(assignmentValue, identityLinkType);
                            }
                        }
                    }
                }
            }
        }
    }

}