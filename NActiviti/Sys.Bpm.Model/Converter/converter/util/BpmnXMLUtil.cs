using org.activiti.bpmn.constants;
using org.activiti.bpmn.converter.child;
using org.activiti.bpmn.model;
using Sys.Bpm;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace org.activiti.bpmn.converter.util
{

    public class BpmnXMLUtil : IBpmnXMLConstants
    {
        private static IDictionary<string, BaseChildElementParser> genericChildParserMap = new Dictionary<string, BaseChildElementParser>();

        static BpmnXMLUtil()
        {
            addGenericParser(new ActivitiEventListenerParser());
            addGenericParser(new CancelEventDefinitionParser());
            addGenericParser(new CompensateEventDefinitionParser());
            addGenericParser(new ConditionExpressionParser());
            addGenericParser(new DataInputAssociationParser());
            addGenericParser(new DataOutputAssociationParser());
            addGenericParser(new DataStateParser());
            addGenericParser(new DocumentationParser());
            addGenericParser(new ErrorEventDefinitionParser());
            addGenericParser(new ExecutionListenerParser());
            addGenericParser(new FieldExtensionParser());
            addGenericParser(new FormPropertyParser());
            addGenericParser(new IOSpecificationParser());
            addGenericParser(new MessageEventDefinitionParser());
            addGenericParser(new MultiInstanceParser());
            addGenericParser(new SignalEventDefinitionParser());
            addGenericParser(new TaskListenerParser());
            addGenericParser(new TerminateEventDefinitionParser());
            addGenericParser(new TimerEventDefinitionParser());
            addGenericParser(new TimeDateParser());
            addGenericParser(new TimeCycleParser());
            addGenericParser(new TimeDurationParser());
            addGenericParser(new FlowNodeRefParser());
            addGenericParser(new ActivitiFailedjobRetryParser());
            addGenericParser(new ActivitiMapExceptionParser());
        }

        private static void addGenericParser(BaseChildElementParser parser)
        {
            genericChildParserMap[parser.ElementName] = parser;
        }

        public static void addXMLLocation(BaseElement element, XMLStreamReader xtr)
        {
            element.XmlRowNumber = xtr.LineInfo.LineNumber;
            element.XmlColumnNumber = xtr.LineInfo.LinePosition;
        }

        public static void addXMLLocation(GraphicInfo graphicInfo, XMLStreamReader xtr)
        {
            graphicInfo.XmlRowNumber = xtr.LineInfo.LineNumber;
            graphicInfo.XmlColumnNumber = xtr.LineInfo.LinePosition;
        }

        public static void parseChildElements(string elementName, BaseElement parentElement, XMLStreamReader xtr, BpmnModel model)
        {
            parseChildElements(elementName, parentElement, xtr, null, model);
        }

        public static void parseChildElements(string elementName, BaseElement parentElement, XMLStreamReader xtr, IDictionary<string, BaseChildElementParser> childParsers, BpmnModel model)
        {
            IDictionary<string, BaseChildElementParser> localParserMap = new Dictionary<string, BaseChildElementParser>(genericChildParserMap);
            if (childParsers != null)
            {
                localParserMap.putAll(childParsers);
            }

            bool inExtensionElements = false;
            bool readyWithChildElements = false;
            while (!readyWithChildElements && xtr.hasNext())
            {
                //xtr.next();

                if (xtr.StartElement)
                {
                    if (BpmnXMLConstants.ELEMENT_EXTENSIONS.Equals(xtr.LocalName))
                    {
                        inExtensionElements = true;
                    }
                    else if (localParserMap.ContainsKey(xtr.LocalName))
                    {
                        BaseChildElementParser childParser = localParserMap[xtr.LocalName];
                        //if we're into an extension element but the current element is not accepted by this parentElement then is read as a custom extension element
                        if (inExtensionElements && !childParser.accepts(parentElement))
                        {
                            ExtensionElement extensionElement = BpmnXMLUtil.parseExtensionElement(xtr);
                            parentElement.addExtensionElement(extensionElement);
                            continue;
                        }
                        localParserMap[xtr.LocalName].parseChildElement(xtr, parentElement, model);
                    }
                    else if (inExtensionElements)
                    {
                        ExtensionElement extensionElement = BpmnXMLUtil.parseExtensionElement(xtr);
                        parentElement.addExtensionElement(extensionElement);
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

        public static ExtensionElement parseExtensionElement(XMLStreamReader xtr)
        {
            ExtensionElement extensionElement = CreateExtensionElement(xtr);

            bool readyWithExtensionElement = false;
            while (!readyWithExtensionElement && xtr.hasNext())
            {
                //xtr.next();

                if (xtr.NodeType == XmlNodeType.Text || xtr.NodeType == XmlNodeType.CDATA)
                {
                    if (!string.IsNullOrWhiteSpace(xtr.Value?.Trim()))
                    {
                        extensionElement.ElementText = xtr.Value?.Trim();
                    }
                }
                else if (xtr.StartElement)
                {
                    if (xtr.IsEmptyElement)
                    {
                        ExtensionElement childExtensionElement = CreateExtensionElement(xtr);
                        extensionElement.addChildElement(childExtensionElement);
                        xtr.next();
                        return childExtensionElement;
                    }
                    else
                    {
                        ExtensionElement childExtensionElement = parseExtensionElement(xtr);
                        extensionElement.addChildElement(childExtensionElement);
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
            ExtensionElement extensionElement = new ExtensionElement();
            extensionElement.Name = xtr.LocalName;
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
                ExtensionAttribute extensionAttribute = new ExtensionAttribute();
                extensionAttribute.Name = attr.Name.LocalName;
                extensionAttribute.Value = attr.Value;
                if (!string.IsNullOrWhiteSpace(attr.Name.NamespaceName))
                {
                    extensionAttribute.Namespace = attr.Name.NamespaceName;
                }
                if (!string.IsNullOrWhiteSpace(xtr.element.GetPrefixOfNamespace(attr.Name.NamespaceName)))
                {
                    extensionAttribute.NamespacePrefix = xtr.element.GetPrefixOfNamespace(attr.Name.NamespaceName);
                }
                extensionElement.addAttribute(extensionAttribute);
            }

            return extensionElement;
        }

        public static void writeDefaultAttribute(string attributeName, string value, XMLStreamWriter xtw)
        {
            if (!string.IsNullOrWhiteSpace(value) && !"null".Equals(value, StringComparison.CurrentCultureIgnoreCase))
            {
                xtw.writeAttribute(attributeName, value);
            }
        }

        public static void writeQualifiedAttribute(string attributeName, string value, XMLStreamWriter xtw)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                xtw.writeAttribute(BpmnXMLConstants.ACTIVITI_EXTENSIONS_PREFIX, BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE, attributeName, value);
            }
        }

        public static bool writeExtensionElements(BaseElement baseElement, bool didWriteExtensionStartElement, XMLStreamWriter xtw)
        {
            return didWriteExtensionStartElement = writeExtensionElements(baseElement, didWriteExtensionStartElement, null, xtw);
        }

        public static bool writeExtensionElements(BaseElement baseElement, bool didWriteExtensionStartElement, IDictionary<string, string> namespaceMap, XMLStreamWriter xtw)
        {
            if (baseElement.ExtensionElements.Count > 0)
            {
                if (!didWriteExtensionStartElement)
                {
                    xtw.writeStartElement(BpmnXMLConstants.ELEMENT_EXTENSIONS);
                    didWriteExtensionStartElement = true;
                }

                if (namespaceMap == null)
                {
                    namespaceMap = new Dictionary<string, string>();
                }

                foreach (IList<ExtensionElement> extensionElements in baseElement.ExtensionElements.Values)
                {
                    foreach (ExtensionElement extensionElement in extensionElements)
                    {
                        writeExtensionElement(extensionElement, namespaceMap, xtw);
                    }
                }
            }
            return didWriteExtensionStartElement;
        }

        protected internal static void writeExtensionElement(ExtensionElement extensionElement, IDictionary<string, string> namespaceMap, XMLStreamWriter xtw)
        {
            if (!string.IsNullOrWhiteSpace(extensionElement.Name))
            {
                IDictionary<string, string> localNamespaceMap = new Dictionary<string, string>();
                if (!string.IsNullOrWhiteSpace(extensionElement.Namespace))
                {
                    if (!string.IsNullOrWhiteSpace(extensionElement.NamespacePrefix))
                    {
                        xtw.writeStartElement(extensionElement.NamespacePrefix, extensionElement.Name, extensionElement.Namespace);

                        if (!namespaceMap.ContainsKey(extensionElement.NamespacePrefix) || !namespaceMap[extensionElement.NamespacePrefix].Equals(extensionElement.Namespace))
                        {

                            xtw.writeNamespace(extensionElement.NamespacePrefix, extensionElement.Namespace);
                            namespaceMap[extensionElement.NamespacePrefix] = extensionElement.Namespace;
                            localNamespaceMap[extensionElement.NamespacePrefix] = extensionElement.Namespace;
                        }
                    }
                    else
                    {
                        xtw.writeStartElement(extensionElement.Namespace, extensionElement.Name);
                    }
                }
                else
                {
                    xtw.writeStartElement(extensionElement.Name);
                }

                foreach (IList<ExtensionAttribute> attributes in extensionElement.Attributes.Values)
                {
                    foreach (ExtensionAttribute attribute in attributes)
                    {
                        if (!string.IsNullOrWhiteSpace(attribute.Name) && attribute.Value != null)
                        {
                            if (!string.IsNullOrWhiteSpace(attribute.Namespace))
                            {
                                if (!string.IsNullOrWhiteSpace(attribute.NamespacePrefix))
                                {

                                    if (!namespaceMap.ContainsKey(attribute.NamespacePrefix) || !namespaceMap[attribute.NamespacePrefix].Equals(attribute.Namespace))
                                    {

                                        xtw.writeNamespace(attribute.NamespacePrefix, attribute.Namespace);
                                        namespaceMap[attribute.NamespacePrefix] = attribute.Namespace;
                                    }

                                    xtw.writeAttribute(attribute.NamespacePrefix, attribute.Namespace, attribute.Name, attribute.Value);
                                }
                                else
                                {
                                    xtw.writeAttribute(attribute.Namespace, attribute.Name, attribute.Value);
                                }
                            }
                            else
                            {
                                xtw.writeAttribute(attribute.Name, attribute.Value);
                            }
                        }
                    }
                }

                if (extensionElement.ElementText != null)
                {
                    xtw.writeCData(extensionElement.ElementText);
                }
                else
                {
                    foreach (IList<ExtensionElement> childElements in extensionElement.ChildElements.Values)
                    {
                        foreach (ExtensionElement childElement in childElements)
                        {
                            writeExtensionElement(childElement, namespaceMap, xtw);
                        }
                    }
                }

                foreach (string prefix in localNamespaceMap.Keys)
                {
                    namespaceMap.Remove(prefix);
                }

                xtw.writeEndElement();
            }
        }

        public static IList<string> parseDelimitedList(string s)
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

        public static string convertToDelimitedString(IList<string> stringList)
        {
            StringBuilder resultString = new StringBuilder();

            if (stringList != null)
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
        public static void addCustomAttributes(XMLStreamReader xtr, BaseElement element, params IList<ExtensionAttribute>[] blackLists)
        {
            foreach (var attr in xtr.element.Attributes())
            {
                ExtensionAttribute extensionAttribute = new ExtensionAttribute();
                extensionAttribute.Name = attr.Name.LocalName;
                extensionAttribute.Value = attr.Value;
                if (!string.IsNullOrWhiteSpace(attr.Name.NamespaceName))
                {
                    extensionAttribute.Namespace = attr.Name.NamespaceName;
                }
                if (!string.IsNullOrWhiteSpace(xtr.element.GetPrefixOfNamespace(attr.Name.Namespace)))
                {
                    extensionAttribute.NamespacePrefix = xtr.element.GetPrefixOfNamespace(attr.Name.Namespace);
                }
                if (!isBlacklisted(extensionAttribute, blackLists))
                {
                    element.addAttribute(extensionAttribute);
                }
            }
        }

        public static void writeCustomAttributes(ICollection<IList<ExtensionAttribute>> attributes, XMLStreamWriter xtw, params IList<ExtensionAttribute>[] blackLists)
        {
            writeCustomAttributes(attributes, xtw, new Dictionary<string, string>(), blackLists);
        }

        /// <summary>
        /// write attributes to xtw (except blacklisted)
        /// </summary>
        /// <param name="attributes"> </param>
        /// <param name="xtw"> </param>
        /// <param name="blackLists"> </param>
        public static void writeCustomAttributes(ICollection<IList<ExtensionAttribute>> attributes, XMLStreamWriter xtw, IDictionary<string, string> namespaceMap, params IList<ExtensionAttribute>[] blackLists)
        {
            foreach (IList<ExtensionAttribute> attributeList in attributes)
            {
                if (attributeList != null && attributeList.Count > 0)
                {
                    foreach (ExtensionAttribute attribute in attributeList)
                    {
                        if (!isBlacklisted(attribute, blackLists))
                        {
                            if (attribute.NamespacePrefix == null)
                            {
                                if (attribute.Namespace == null)
                                {
                                    xtw.writeAttribute(attribute.Name, attribute.Value);
                                }
                                else
                                {
                                    xtw.writeAttribute(attribute.Namespace, attribute.Name, attribute.Value);
                                }
                            }
                            else
                            {
                                if (!namespaceMap.ContainsKey(attribute.NamespacePrefix))
                                {
                                    namespaceMap[attribute.NamespacePrefix] = attribute.Namespace;
                                    xtw.writeNamespace(attribute.NamespacePrefix, attribute.Namespace);
                                }
                                xtw.writeAttribute(attribute.NamespacePrefix, attribute.Namespace, attribute.Name, attribute.Value);
                            }
                        }
                    }
                }
            }
        }

        public static bool isBlacklisted(ExtensionAttribute attribute, params IList<ExtensionAttribute>[] blackLists)
        {
            if (blackLists != null)
            {
                foreach (IList<ExtensionAttribute> blackList in blackLists)
                {
                    foreach (ExtensionAttribute blackAttribute in blackList)
                    {
                        if (blackAttribute.Name.Equals(attribute.Name))
                        {
                            if (blackAttribute.Namespace != null && attribute.Namespace != null && blackAttribute.Namespace.Equals(attribute.Namespace))
                            {
                                return true;
                            }
                            if (blackAttribute.Namespace == null && attribute.Namespace == null)
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