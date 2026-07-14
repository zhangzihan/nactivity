using Sys.Workflow.Bpmn.Constants;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Sys.Workflow.Bpmn.Models
{
    public static class ExtensionElementExtension
    {
        /// <summary>
        /// 获取节点属性
        /// </summary>
        /// <param name="elements"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static ExtensionAttribute GetExtensionElementAttribute(this BaseElement element, string name)
        {
            List<ExtensionElement> pElems = new List<ExtensionElement>();
            if (element.ExtensionElements.TryGetValue(BpmnXMLConstants.ELEMENT_EXTENSIONS_PROPERTY, out var iElems))
            {
                pElems.AddRange(iElems);
            }
            if (element.ExtensionElements.TryGetValue(BpmnXMLConstants.ELEMENT_EXTENSIONS_PROPERTIES, out iElems))
            {
                pElems.AddRange(iElems.SelectMany(x => x.ChildElements.Values.SelectMany(x => x)));
            }

            foreach (var extElement in pElems)
            {
                if (extElement.Attributes.TryGetValue(BpmnXMLConstants.ATTRIBUTE_NAME, out IList<ExtensionAttribute> attributes) && attributes.Count > 0)
                {
                    foreach (ExtensionAttribute attribute in attributes)
                    {
                        if (string.Compare(attribute.Value, name, true) == 0)
                        {
                            if (extElement.Attributes.TryGetValue(BpmnXMLConstants.ELEMENT_VALUE, out IList<ExtensionAttribute> values))
                            {
                                return values.FirstOrDefault();
                            }

                            return null;
                        }
                    }
                }
            }

            return null;
        }
        /// <summary>
        /// 获取节点的值
        /// </summary>
        /// <param name="elements"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string GetExtensionElementAttributeValue(this BaseElement element, string name)
        {
            List<ExtensionElement> pElems = new List<ExtensionElement>();
            if (element.ExtensionElements.TryGetValue(BpmnXMLConstants.ELEMENT_EXTENSIONS_PROPERTY, out var iElems))
            {
                pElems.AddRange(iElems);
            }
            if (element.ExtensionElements.TryGetValue(BpmnXMLConstants.ELEMENT_EXTENSIONS_PROPERTIES, out iElems))
            {
                pElems.AddRange(iElems.SelectMany(x => x.ChildElements.Values.SelectMany(x => x)));
            }

            return pElems.GetAttributeValue(name);
        }

        /// <summary>
        /// 获取节点的值
        /// </summary>
        /// <param name="elements"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string GetAttributeValue(this IList<ExtensionElement> elements, string name)
        {
            foreach (var element in elements)
            {
                string ename = element.GetAttributeValue(null, "name");
                if (string.Equals(ename, name, StringComparison.OrdinalIgnoreCase))
                {
                    return element.GetAttributeValue(null, "value");
                }
            }

            return null;
        }
    }
}