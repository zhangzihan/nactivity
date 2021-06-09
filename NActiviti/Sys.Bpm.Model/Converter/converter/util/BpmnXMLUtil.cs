using Sys.Workflow.Bpmn.Constants;
using Sys.Workflow.Bpmn.Converters.Childs;
using Sys.Workflow.Bpmn.Models;
using Sys.Workflow;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Sys.Workflow.Bpmn.Converters.Utils
{

    public class BpmnXMLUtil : IBpmnXMLConstants
    {
        private static readonly IDictionary<string, BaseChildElementParser> genericChildParserMap = new Dictionary<string, BaseChildElementParser>();

        static BpmnXMLUtil()
        {
            AddGenericParser(new ActivitiEventListenerParser());
            AddGenericParser(new CancelEventDefinitionParser());
            AddGenericParser(new CompensateEventDefinitionParser());
            AddGenericParser(new ConditionExpressionParser());
            AddGenericParser(new DataInputAssociationParser());
            AddGenericParser(new DataOutputAssociationParser());
            AddGenericParser(new DataStateParser());
            AddGenericParser(new DocumentationParser());
            AddGenericParser(new ErrorEventDefinitionParser());
            AddGenericParser(new ExecutionListenerParser());
            AddGenericParser(new FieldExtensionParser());
            AddGenericParser(new FormPropertyParser());
            AddGenericParser(new IOSpecificationParser());
            AddGenericParser(new MessageEventDefinitionParser());
            AddGenericParser(new MultiInstanceParser());
            AddGenericParser(new SignalEventDefinitionParser());
            AddGenericParser(new TaskListenerParser());
            AddGenericParser(new TerminateEventDefinitionParser());
            AddGenericParser(new TimerEventDefinitionParser());
            AddGenericParser(new TimeDateParser());
            AddGenericParser(new TimeCycleParser());
            AddGenericParser(new TimeDurationParser());
            AddGenericParser(new FlowNodeRefParser());
            AddGenericParser(new ActivitiFailedjobRetryParser());
            AddGenericParser(new ActivitiMapExceptionParser());
        }

        private static void AddGenericParser(BaseChildElementParser parser)
        {
            genericChildParserMap[parser.ElementName] = parser;
        }

        public static void AddXMLLocation(BaseElement element, XMLStreamReader xtr)
        {
            element.XmlRowNumber = xtr.LineInfo.LineNumber;
            element.XmlColumnNumber = xtr.LineInfo.LinePosition;
        }

        public static void AddXMLLocation(GraphicInfo graphicInfo, XMLStreamReader xtr)
        {
            graphicInfo.XmlRowNumber = xtr.LineInfo.LineNumber;
            graphicInfo.XmlColumnNumber = xtr.LineInfo.LinePosition;
        }

        public static void ParseChildElements(string elementName, BaseElement parentElement, XMLStreamReader xtr, BpmnModel model)
        {
            ParseChildElements(elementName, parentElement, xtr, null, model);
        }

        public static void ParseChildElements(string elementName, BaseElement parentElement, XMLStreamReader xtr, IDictionary<string, BaseChildElementParser> childParsers, BpmnModel model)
        {
            IDictionary<string, BaseChildElementParser> localParserMap = new Dictionary<string, BaseChildElementParser>(genericChildParserMap);
            if (childParsers is object)
            {
                localParserMap.PutAll(childParsers);
            }

            bool inExtensionElements = false;
            bool readyWithChildElements = false;
            while (!readyWithChildElements && xtr.HasNext())
            {
                //xtr.next();

                if (xtr.IsStartElement())
                {
                    if (BpmnXMLConstants.ELEMENT_EXTENSIONS.Equals(xtr.LocalName))
                    {
                        inExtensionElements = true;
                    }
                    else if (localParserMap.ContainsKey(xtr.LocalName))
                    {
                        BaseChildElementParser childParser = localParserMap[xtr.LocalName];
                        //if we're into an extension element but the current element is not accepted by this parentElement then is read as a custom extension element
                        if (inExtensionElements && !childParser.Accepts(parentElement))
                        {
                            ExtensionElement extensionElement = BpmnXMLUtil.ParseExtensionElement(xtr);
                            parentElement.AddExtensionElement(extensionElement);
                            continue;
                        }
                        localParserMap[xtr.LocalName].ParseChildElement(xtr, parentElement, model);
                    }
                    else if (inExtensionElements)
                    {
                        ExtensionElement extensionElement = BpmnXMLUtil.ParseExtensionElement(xtr);
                        parentElement.AddExtensionElement(extensionElement);
                    }

                }
                else if (xtr.EndElement)
                {
                    if (BpmnXMLConstants.ELEMENT_EXTENSIONS.Equals(xtr.LocalName))
                    {
                        inExtensionElements = false;
                    }
                    else if (elementName.Equals(xtr.LocalName, StringComparison.CurrentCultureIgnoreCase))
                    {
                        readyWithChildElements = true;
                    }
                }

                if (xtr.IsEmptyElement && elementName.Equals(xtr.LocalName, StringComparison.OrdinalIgnoreCase))
                {
                    readyWithChildElements = true;
                }
            }
        }

        public static ExtensionElement ParseExtensionElement(XMLStreamReader xtr)
        {
            ExtensionElement extensionElement = CreateExtensionElement(xtr);

            bool readyWithExtensionElement = false;
            while (!readyWithExtensionElement && xtr.HasNext())
            {
                //xtr.next();

                if (xtr.NodeType == XmlNodeType.Text || xtr.NodeType == XmlNodeType.CDATA)
                {
                    if (!string.IsNullOrWhiteSpace(xtr.Value?.Trim()))
                    {
                        extensionElement.ElementText = xtr.Value?.Trim();
                    }
                }
                else if (xtr.IsStartElement())
                {
                    if (xtr.IsEmptyElement)
                    {
                        ExtensionElement childExtensionElement = CreateExtensionElement(xtr);
                        extensionElement.AddChildElement(childExtensionElement);
                        xtr.isEmpty = xtr.IsStartElement() && xtr.EndElement;
                        xtr.Next();
                        return childExtensionElement;
                    }
                    else
                    {
                        ExtensionElement childExtensionElement = ParseExtensionElement(xtr);
                        extensionElement.AddChildElement(childExtensionElement);
                    }
                }
                else if (xtr.EndElement && string.Compare(extensionElement.Name, xtr.LocalName, true) == 0)
                {
                    readyWithExtensionElement = true;
                }

                if (xtr.IsEmptyElement && string.Compare(extensionElement.Name, xtr.LocalName, true) == 0)
                {
                    readyWithExtensionElement = true;
                }
            }
            return extensionElement;
        }

        private static ExtensionElement CreateExtensionElement(XMLStreamReader xtr)
        {
            ExtensionElement extensionElement = new ExtensionElement
            {
                Name = xtr.LocalName
            };
            if (!string.IsNullOrWhiteSpace(xtr.NamespaceURI))
            {
                extensionElement.Namespace = xtr.NamespaceURI;
            }
            if (!string.IsNullOrWhiteSpace(xtr.Prefix))
            {
                extensionElement.NamespacePrefix = xtr.Prefix;
            }

            foreach (var attr in xtr.element.Attributes())
            {
                ExtensionAttribute extensionAttribute = new ExtensionAttribute
                {
                    Name = attr.Name.LocalName,
                    Value = attr.Value
                };
                if (!string.IsNullOrWhiteSpace(attr.Name.NamespaceName))
                {
                    extensionAttribute.Namespace = attr.Name.NamespaceName;
                }
                if (!string.IsNullOrWhiteSpace(xtr.element.GetPrefixOfNamespace(attr.Name.NamespaceName)))
                {
                    extensionAttribute.NamespacePrefix = xtr.element.GetPrefixOfNamespace(attr.Name.NamespaceName);
                }
                extensionElement.AddAttribute(extensionAttribute);
            }

            return extensionElement;
        }

        public static void WriteDefaultAttribute(string attributeName, string value, XMLStreamWriter xtw)
        {
            if (!string.IsNullOrWhiteSpace(value) && !"null".Equals(value, StringComparison.CurrentCultureIgnoreCase))
            {
                xtw.WriteAttribute(attributeName, value);
            }
        }

        public static void WriteQualifiedAttribute(string attributeName, string value, XMLStreamWriter xtw)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                xtw.WriteAttribute(BpmnXMLConstants.ACTIVITI_EXTENSIONS_PREFIX, BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE, attributeName, value);
            }
        }

        public static bool WriteExtensionElements(BaseElement baseElement, bool didWriteExtensionStartElement, XMLStreamWriter xtw)
        {
            return WriteExtensionElements(baseElement, didWriteExtensionStartElement, null, xtw);
        }

        public static bool WriteExtensionElements(BaseElement baseElement, bool didWriteExtensionStartElement, IDictionary<string, string> namespaceMap, XMLStreamWriter xtw)
        {
            if (baseElement.ExtensionElements.Count > 0)
            {
                if (!didWriteExtensionStartElement)
                {
                    xtw.WriteStartElement(BpmnXMLConstants.BPMN_PREFIX, BpmnXMLConstants.ELEMENT_EXTENSIONS, BpmnXMLConstants.BPMN2_NAMESPACE);
                    didWriteExtensionStartElement = true;
                }

                if (namespaceMap is null)
                {
                    namespaceMap = new Dictionary<string, string>();
                }

                foreach (IList<ExtensionElement> extensionElements in baseElement.ExtensionElements.Values)
                {
                    foreach (ExtensionElement extensionElement in extensionElements)
                    {
                        WriteExtensionElement(extensionElement, namespaceMap, xtw);
                    }
                }
            }
            return didWriteExtensionStartElement;
        }

        protected internal static void WriteExtensionElement(ExtensionElement extensionElement, IDictionary<string, string> namespaceMap, XMLStreamWriter xtw)
        {
            if (!string.IsNullOrWhiteSpace(extensionElement.Name))
            {
                IDictionary<string, string> localNamespaceMap = new Dictionary<string, string>();
                if (!string.IsNullOrWhiteSpace(extensionElement.Namespace))
                {
                    if (!string.IsNullOrWhiteSpace(extensionElement.NamespacePrefix))
                    {
                        xtw.WriteStartElement(extensionElement.NamespacePrefix, extensionElement.Name, extensionElement.Namespace);

                        if (!namespaceMap.ContainsKey(extensionElement.NamespacePrefix) || !namespaceMap[extensionElement.NamespacePrefix].Equals(extensionElement.Namespace))
                        {

                            xtw.WriteNamespace(extensionElement.NamespacePrefix, extensionElement.Namespace);
                            namespaceMap[extensionElement.NamespacePrefix] = extensionElement.Namespace;
                            localNamespaceMap[extensionElement.NamespacePrefix] = extensionElement.Namespace;
                        }
                    }
                    else
                    {
                        xtw.WriteStartElement(extensionElement.Namespace, extensionElement.Name);
                    }
                }
                else
                {
                    xtw.WriteStartElement(BpmnXMLConstants.BPMN_PREFIX, extensionElement.Name, BpmnXMLConstants.BPMN2_NAMESPACE);
                }

                foreach (IList<ExtensionAttribute> attributes in extensionElement.Attributes.Values)
                {
                    foreach (ExtensionAttribute attribute in attributes)
                    {
                        if (!string.IsNullOrWhiteSpace(attribute.Name) && attribute.Value is object)
                        {
                            if (!string.IsNullOrWhiteSpace(attribute.Namespace))
                            {
                                if (!string.IsNullOrWhiteSpace(attribute.NamespacePrefix))
                                {

                                    if (!namespaceMap.ContainsKey(attribute.NamespacePrefix) || !namespaceMap[attribute.NamespacePrefix].Equals(attribute.Namespace))
                                    {

                                        xtw.WriteNamespace(attribute.NamespacePrefix, attribute.Namespace);
                                        namespaceMap[attribute.NamespacePrefix] = attribute.Namespace;
                                    }

                                    xtw.WriteAttribute(attribute.NamespacePrefix, attribute.Namespace, attribute.Name, attribute.Value);
                                }
                                else
                                {
                                    xtw.WriteAttribute(attribute.Namespace, attribute.Name, attribute.Value);
                                }
                            }
                            else
                            {
                                xtw.WriteAttribute(attribute.Name, attribute.Value);
                            }
                        }
                    }
                }

                if (extensionElement.ElementText is object)
                {
                    xtw.WriteCData(extensionElement.ElementText);
                }
                else
                {
                    foreach (IList<ExtensionElement> childElements in extensionElement.ChildElements.Values)
                    {
                        foreach (ExtensionElement childElement in childElements)
                        {
                            WriteExtensionElement(childElement, namespaceMap, xtw);
                        }
                    }
                }

                foreach (string prefix in localNamespaceMap.Keys)
                {
                    namespaceMap.Remove(prefix);
                }

                xtw.WriteEndElement();
            }
        }

        public static IList<string> ParseDelimitedList(string s)
        {
            IList<string> result = new List<string>();
            if (!string.IsNullOrWhiteSpace(s))
            {

                CharEnumerator iterator = s.GetEnumerator();
                if (iterator.MoveNext() == false)
                {
                    return result;
                }

                StringBuilder strb = new StringBuilder();
                bool insideExpression = false;

                do
                {
                    char c = iterator.Current;

                    if (c == '{' || c == '$')
                    {
                        insideExpression = true;
                    }
                    else if (c == '}')
                    {
                        insideExpression = false;
                    }
                    else if (c == ',' && !insideExpression)
                    {
                        result.Add(strb.ToString().Trim());
                        strb.Remove(0, strb.Length);
                    }

                    if (c != ',' || (insideExpression))
                    {
                        strb.Append(c);
                    }

                } while (iterator.MoveNext());

                if (strb.Length > 0)
                {
                    result.Add(strb.ToString().Trim());
                }

            }
            return result;
        }

        public static string ConvertToDelimitedString(IList<string> stringList)
        {
            StringBuilder resultString = new StringBuilder();

            if (stringList is object)
            {
                foreach (string result in stringList)
                {
                    if (resultString.Length > 0)
                    {
                        resultString.Append(",");
                    }
                    resultString.Append(result);
                }
            }
            return resultString.ToString();
        }

        /// <summary>
        /// add all attributes from XML to element extensionAttributes (except blackListed).
        /// </summary>
        /// <param name="xtr"> </param>
        /// <param name="element"> </param>
        /// <param name="blackLists"> </param>
        public static void AddCustomAttributes(XMLStreamReader xtr, BaseElement element, params IList<ExtensionAttribute>[] blackLists)
        {
            foreach (var attr in xtr.element.Attributes())
            {
                ExtensionAttribute extensionAttribute = new ExtensionAttribute
                {
                    Name = attr.Name.LocalName,
                    Value = attr.Value
                };
                if (!string.IsNullOrWhiteSpace(attr.Name.NamespaceName))
                {
                    extensionAttribute.Namespace = attr.Name.NamespaceName;
                }
                if (!string.IsNullOrWhiteSpace(xtr.element.GetPrefixOfNamespace(attr.Name.Namespace)))
                {
                    extensionAttribute.NamespacePrefix = xtr.element.GetPrefixOfNamespace(attr.Name.Namespace);
                }
                if (!IsBlacklisted(extensionAttribute, blackLists))
                {
                    element.AddAttribute(extensionAttribute);
                }
            }
        }

        public static void WriteCustomAttributes(ICollection<IList<ExtensionAttribute>> attributes, XMLStreamWriter xtw, params IList<ExtensionAttribute>[] blackLists)
        {
            WriteCustomAttributes(attributes, xtw, new Dictionary<string, string>(), blackLists);
        }

        /// <summary>
        /// write attributes to xtw (except blacklisted)
        /// </summary>
        /// <param name="attributes"> </param>
        /// <param name="xtw"> </param>
        /// <param name="blackLists"> </param>
        public static void WriteCustomAttributes(ICollection<IList<ExtensionAttribute>> attributes, XMLStreamWriter xtw, IDictionary<string, string> namespaceMap, params IList<ExtensionAttribute>[] blackLists)
        {
            foreach (IList<ExtensionAttribute> attributeList in attributes)
            {
                if (attributeList is object && attributeList.Count > 0)
                {
                    foreach (ExtensionAttribute attribute in attributeList)
                    {
                        if (!IsBlacklisted(attribute, blackLists))
                        {
                            if (attribute.NamespacePrefix is null)
                            {
                                if (attribute.Namespace is null)
                                {
                                    xtw.WriteAttribute(attribute.Name, attribute.Value);
                                }
                                else
                                {
                                    xtw.WriteAttribute(attribute.Namespace, attribute.Name, attribute.Value);
                                }
                            }
                            else
                            {
                                if (!namespaceMap.ContainsKey(attribute.Name))
                                {
                                    namespaceMap[attribute.Name] = attribute.Namespace;
                                    xtw.WriteNamespace(attribute.NamespacePrefix, attribute.Namespace);
                                }
                                xtw.WriteAttribute(attribute.NamespacePrefix, attribute.Namespace, attribute.Name, attribute.Value);
                            }
                        }
                    }
                }
            }
        }

        public static bool IsBlacklisted(ExtensionAttribute attribute, params IList<ExtensionAttribute>[] blackLists)
        {
            if (blackLists is object)
            {
                foreach (IList<ExtensionAttribute> blackList in blackLists)
                {
                    foreach (ExtensionAttribute blackAttribute in blackList)
                    {
                        if (blackAttribute.Name.Equals(attribute.Name))
                        {
                            if (blackAttribute.Namespace is object && attribute.Namespace is object && blackAttribute.Namespace.Equals(attribute.Namespace))
                            {
                                return true;
                            }
                            if (blackAttribute.Namespace is null && attribute.Namespace is null)
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 'safe' is here reflecting:
        /// http://activiti.org/userguide/index.html#advanced.safe.bpmn.xml
        /// </summary>
        //public static XMLInputFactory createSafeXmlInputFactory()
        //{
        //    XMLInputFactory xif = XMLInputFactory.newInstance();
        //    if (xif.isPropertySupported(XMLInputFactory.IS_REPLACING_ENTITY_REFERENCES))
        //    {
        //        xif.setProperty(XMLInputFactory.IS_REPLACING_ENTITY_REFERENCES, false);
        //    }

        //    if (xif.isPropertySupported(XMLInputFactory.IS_SUPPORTING_EXTERNAL_ENTITIES))
        //    {
        //        xif.setProperty(XMLInputFactory.IS_SUPPORTING_EXTERNAL_ENTITIES, false);
        //    }

        //    if (xif.isPropertySupported(XMLInputFactory.SUPPORT_DTD))
        //    {
        //        xif.setProperty(XMLInputFactory.SUPPORT_DTD, false);
        //    }
        //    return xif;
        //}
    }

}