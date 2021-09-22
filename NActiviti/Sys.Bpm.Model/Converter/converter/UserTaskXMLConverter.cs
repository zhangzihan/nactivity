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
namespace Sys.Workflow.Bpmn.Converters
{
    using Sys.Workflow.Bpmn.Constants;
    using Sys.Workflow.Bpmn.Converters.Childs;
    using Sys.Workflow.Bpmn.Converters.Utils;
    using Sys.Workflow.Bpmn.Models;
    using Sys.Workflow.Bpmn.Models.Alfresco;
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

        protected internal override BaseElement ConvertXMLToElement(XMLStreamReader xtr, BpmnModel model)
        {
            string formKey = xtr.GetAttributeValue(BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE, BpmnXMLConstants.ATTRIBUTE_FORM_FORMKEY);
            UserTask userTask = null;
            if (!string.IsNullOrWhiteSpace(formKey))
            {
                if (model.UserTaskFormTypes is object && model.UserTaskFormTypes.Contains(formKey))
                {
                    userTask = new AlfrescoUserTask();
                }
            }
            if (userTask is null)
            {
                userTask = new UserTask();
            }
            BpmnXMLUtil.AddXMLLocation(userTask, xtr);
            userTask.DueDate = xtr.GetAttributeValue(BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE, BpmnXMLConstants.ATTRIBUTE_TASK_USER_DUEDATE);
            userTask.BusinessCalendarName = xtr.GetAttributeValue(BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE, BpmnXMLConstants.ATTRIBUTE_TASK_USER_BUSINESS_CALENDAR_NAME);
            userTask.Category = xtr.GetAttributeValue(BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE, BpmnXMLConstants.ATTRIBUTE_TASK_USER_CATEGORY);
            userTask.FormKey = formKey;
            userTask.Assignee = xtr.GetAttributeValue(BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE, BpmnXMLConstants.ATTRIBUTE_TASK_USER_ASSIGNEE);
            userTask.Owner = xtr.GetAttributeValue(BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE, BpmnXMLConstants.ATTRIBUTE_TASK_USER_OWNER);
            userTask.Priority = xtr.GetAttributeValue(BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE, BpmnXMLConstants.ATTRIBUTE_TASK_USER_PRIORITY);

            if (!string.IsNullOrWhiteSpace(xtr.GetAttributeValue(BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE, BpmnXMLConstants.ATTRIBUTE_TASK_USER_CANDIDATEUSERS)))
            {
                string expression = xtr.GetAttributeValue(BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE, BpmnXMLConstants.ATTRIBUTE_TASK_USER_CANDIDATEUSERS);
                (userTask.CandidateUsers as List<string>).AddRange(ParseDelimitedList(expression));
            }

            if (!string.IsNullOrWhiteSpace(xtr.GetAttributeValue(BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE, BpmnXMLConstants.ATTRIBUTE_TASK_USER_CANDIDATEGROUPS)))
            {
                string expression = xtr.GetAttributeValue(BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE, BpmnXMLConstants.ATTRIBUTE_TASK_USER_CANDIDATEGROUPS);
                (userTask.CandidateGroups as List<string>).AddRange(ParseDelimitedList(expression));
            }

            userTask.ExtensionId = xtr.GetAttributeValue(BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE, BpmnXMLConstants.ATTRIBUTE_TASK_SERVICE_EXTENSIONID);

            if (!string.IsNullOrWhiteSpace(xtr.GetAttributeValue(BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE, BpmnXMLConstants.ATTRIBUTE_TASK_USER_SKIP_EXPRESSION)))
            {
                string expression = xtr.GetAttributeValue(BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE, BpmnXMLConstants.ATTRIBUTE_TASK_USER_SKIP_EXPRESSION);
                userTask.SkipExpression = expression;
            }

            BpmnXMLUtil.AddCustomAttributes(xtr, userTask, defaultElementAttributes, defaultActivityAttributes, defaultUserTaskAttributes);

            ParseChildElements(XMLElementName, userTask, childParserMap, model, xtr);

            return userTask;
        }

        protected internal override void WriteAdditionalAttributes(BaseElement element, BpmnModel model, XMLStreamWriter xtw)
        {
            UserTask userTask = (UserTask)element;
            WriteQualifiedAttribute(BpmnXMLConstants.ATTRIBUTE_TASK_USER_ASSIGNEE, userTask.Assignee, xtw);
            WriteQualifiedAttribute(BpmnXMLConstants.ATTRIBUTE_TASK_USER_OWNER, userTask.Owner, xtw);
            WriteQualifiedAttribute(BpmnXMLConstants.ATTRIBUTE_TASK_USER_CANDIDATEUSERS, ConvertToDelimitedString(userTask.CandidateUsers), xtw);
            WriteQualifiedAttribute(BpmnXMLConstants.ATTRIBUTE_TASK_USER_CANDIDATEGROUPS, ConvertToDelimitedString(userTask.CandidateGroups), xtw);
            WriteQualifiedAttribute(BpmnXMLConstants.ATTRIBUTE_TASK_USER_DUEDATE, userTask.DueDate, xtw);
            WriteQualifiedAttribute(BpmnXMLConstants.ATTRIBUTE_TASK_USER_BUSINESS_CALENDAR_NAME, userTask.BusinessCalendarName, xtw);
            WriteQualifiedAttribute(BpmnXMLConstants.ATTRIBUTE_TASK_USER_CATEGORY, userTask.Category, xtw);
            WriteQualifiedAttribute(BpmnXMLConstants.ATTRIBUTE_FORM_FORMKEY, userTask.FormKey, xtw);
            if (userTask.Priority is not null)

            {
                WriteQualifiedAttribute(BpmnXMLConstants.ATTRIBUTE_TASK_USER_PRIORITY, userTask.Priority.ToString(), xtw);
            }
            if (!string.IsNullOrWhiteSpace(userTask.ExtensionId))

            {
                WriteQualifiedAttribute(BpmnXMLConstants.ATTRIBUTE_TASK_SERVICE_EXTENSIONID, userTask.ExtensionId, xtw);
            }
            if (userTask.SkipExpression is not null)

            {
                WriteQualifiedAttribute(BpmnXMLConstants.ATTRIBUTE_TASK_USER_SKIP_EXPRESSION, userTask.SkipExpression, xtw);
            }
            // write custom attributes
            BpmnXMLUtil.WriteCustomAttributes(userTask.Attributes.Values, xtw, defaultElementAttributes, defaultActivityAttributes, defaultUserTaskAttributes);
        }

        protected internal override bool WriteExtensionChildElements(BaseElement element, bool didWriteExtensionStartElement, XMLStreamWriter xtw)
        {
            UserTask userTask = (UserTask)element;
            didWriteExtensionStartElement = WriteFormProperties(userTask, didWriteExtensionStartElement, xtw);
            didWriteExtensionStartElement = WriteCustomIdentities(element, didWriteExtensionStartElement, xtw);
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
                        xtw.WriteStartElement(BpmnXMLConstants.BPMN_PREFIX, BpmnXMLConstants.ELEMENT_EXTENSIONS, BpmnXMLConstants.BPMN2_NAMESPACE);
                        didWriteExtensionStartElement = true;
                    }
                    xtw.WriteStartElement(BpmnXMLConstants.ACTIVITI_EXTENSIONS_PREFIX, customProperty.Name, BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE);
                    xtw.WriteCharacters(customProperty.SimpleValue);
                    xtw.WriteEndElement();
                }
            }
            return didWriteExtensionStartElement;
        }

        protected bool WriteCustomIdentities(BaseElement element, bool didWriteExtensionStartElement, XMLStreamWriter xtw)
        {
            UserTask userTask = (UserTask)element;
            if (userTask.CustomUserIdentityLinks.Count == 0 && userTask.CustomGroupIdentityLinks.Count == 0)

            {
                return didWriteExtensionStartElement;
            }

            if (!didWriteExtensionStartElement)
            {
                xtw.WriteStartElement(BpmnXMLConstants.BPMN_PREFIX, BpmnXMLConstants.ELEMENT_EXTENSIONS, BpmnXMLConstants.BPMN2_NAMESPACE);
                didWriteExtensionStartElement = true;
            }
            List<string> identityLinkTypes = new List<string>();
            identityLinkTypes.AddRange(userTask.CustomUserIdentityLinks.Keys);
            identityLinkTypes.AddRange(userTask.CustomGroupIdentityLinks.Keys);
            foreach (string identityType in identityLinkTypes)
            {
                WriteCustomIdentities(userTask, identityType, userTask.CustomUserIdentityLinks[identityType], userTask.CustomGroupIdentityLinks[identityType], xtw);
            }

            return didWriteExtensionStartElement;
        }

        protected void WriteCustomIdentities(UserTask userTask, string identityType, ISet<string> users, ISet<string> groups, XMLStreamWriter xtw)
        {
            xtw.WriteStartElement(BpmnXMLConstants.ACTIVITI_EXTENSIONS_PREFIX, BpmnXMLConstants.ELEMENT_CUSTOM_RESOURCE, BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE);
            WriteDefaultAttribute(BpmnXMLConstants.ATTRIBUTE_NAME, identityType, xtw);

            IList<string> identityList = new List<string>();

            if (users is object)
            {
                foreach (string userId in users)
                {
                    identityList.Add("user(" + userId + ")");
                }
            }

            if (groups is object)
            {
                foreach (string groupId in groups)
                {
                    identityList.Add("group(" + groupId + ")");
                }
            }

            string delimitedString = ConvertToDelimitedString(identityList);

            xtw.WriteStartElement(BpmnXMLConstants.BPMN_PREFIX, BpmnXMLConstants.ELEMENT_RESOURCE_ASSIGNMENT, BpmnXMLConstants.BPMN2_NAMESPACE);
            xtw.WriteStartElement(BpmnXMLConstants.BPMN_PREFIX, BpmnXMLConstants.ELEMENT_FORMAL_EXPRESSION, BpmnXMLConstants.BPMN2_NAMESPACE);
            xtw.WriteCharacters(delimitedString);
            xtw.WriteEndElement(); // End ELEMENT_FORMAL_EXPRESSION
            xtw.WriteEndElement(); // End ELEMENT_RESOURCE_ASSIGNMENT

            xtw.WriteEndElement(); // End ELEMENT_CUSTOM_RESOURCE
        }

        protected internal override void WriteAdditionalChildElements(BaseElement element, BpmnModel model, XMLStreamWriter xtw)
        {
        }

        public class HumanPerformerParser : BaseChildElementParser
        {
            public override string ElementName => "humanPerformer";

            public override void ParseChildElement(XMLStreamReader xtr, BaseElement parentElement, BpmnModel model)
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

            public override void ParseChildElement(XMLStreamReader xtr, BaseElement parentElement, BpmnModel model)
            {

                string resourceElement = XMLStreamReaderUtil.moveDown(xtr);
                if (!string.IsNullOrWhiteSpace(resourceElement) && BpmnXMLConstants.ELEMENT_RESOURCE_ASSIGNMENT.Equals(resourceElement))

                {
                    string expression = XMLStreamReaderUtil.moveDown(xtr);
                    if (!string.IsNullOrWhiteSpace(expression) && BpmnXMLConstants.ELEMENT_FORMAL_EXPRESSION.Equals(expression))
                    {

                        IList<string> assignmentList = CommaSplitter.SplitCommas(xtr.ElementText);

                        for (var idx = 0; idx < assignmentList.Count; idx++)
                        {
                            string assignmentValue = assignmentList[idx];

                            if (assignmentValue is null)
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
                    if (model.ContainsResourceId(resourceId))
                    {
                        Resource resource = model.GetResource(resourceId);
                        ((UserTask)parentElement).CandidateGroups.Add(resource.Name);
                    }
                    else
                    {
                        Resource resource = new Resource(resourceId, resourceId);
                        model.AddResource(resource);
                        ((UserTask)parentElement).CandidateGroups.Add(resource.Name);
                    }
                }
            }
        }

        public class CustomIdentityLinkParser : BaseChildElementParser
        {


            public override string ElementName => BpmnXMLConstants.ELEMENT_CUSTOM_RESOURCE;

            public override void ParseChildElement(XMLStreamReader xtr, BaseElement parentElement, BpmnModel model)
            {

                string identityLinkType = xtr.GetAttributeValue(BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE, BpmnXMLConstants.ATTRIBUTE_NAME);

                // the attribute value may be unqualified
                if (identityLinkType is null)

                {
                    identityLinkType = xtr.GetAttributeValue(BpmnXMLConstants.ATTRIBUTE_NAME);
                }

                if (identityLinkType is null)

                {
                    return;
                }

                string resourceElement = XMLStreamReaderUtil.moveDown(xtr);
                if (!string.IsNullOrWhiteSpace(resourceElement) && BpmnXMLConstants.ELEMENT_RESOURCE_ASSIGNMENT.Equals(resourceElement))

                {
                    string expression = XMLStreamReaderUtil.moveDown(xtr);
                    if (!string.IsNullOrWhiteSpace(expression) && BpmnXMLConstants.ELEMENT_FORMAL_EXPRESSION.Equals(expression))
                    {

                        IList<string> assignmentList = CommaSplitter.SplitCommas(xtr.ElementText);

                        for (var idx = 0; idx < assignmentList.Count; idx++)
                        {
                            string assignmentValue = assignmentList[idx];
                            if (assignmentValue is null)
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
                                ((UserTask)parentElement).AddCustomUserIdentityLink(assignmentValue, identityLinkType);
                            }
                            else if (assignmentValue.StartsWith(groupPrefix, StringComparison.Ordinal))
                            {
                                assignmentValue = assignmentList[idx] = StringHelper.SubstringSpecial(assignmentValue, groupPrefix.Length, assignmentValue.Length - 1).Trim();
                                ((UserTask)parentElement).AddCustomGroupIdentityLink(assignmentValue, identityLinkType);
                            }
                            else
                            {
                                ((UserTask)parentElement).AddCustomGroupIdentityLink(assignmentValue, identityLinkType);
                            }
                        }
                    }
                }
            }
        }
    }
}