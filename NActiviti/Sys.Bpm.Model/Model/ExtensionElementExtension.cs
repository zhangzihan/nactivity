using Sys.Workflow.Bpmn.Constants;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

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
            foreach (ExtensionElement extElement in EnumeratePropertyElements(element))
            {
                if (extElement.Attributes.TryGetValue(BpmnXMLConstants.ATTRIBUTE_NAME, out IList<ExtensionAttribute> attributes) && attributes.Count > 0)
                {
                    foreach (ExtensionAttribute attribute in attributes)
                    {
                        if (string.Equals(attribute.Value, name, StringComparison.OrdinalIgnoreCase))
                        {
                            if (extElement.Attributes.TryGetValue(BpmnXMLConstants.ELEMENT_VALUE, out IList<ExtensionAttribute> values) && values.Count > 0)
                            {
                                return values[0];
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
            foreach (ExtensionElement extElement in EnumeratePropertyElements(element))
            {
                string ename = extElement.GetAttributeValue(null, "name");
                if (string.Equals(ename, name, StringComparison.OrdinalIgnoreCase))
                {
                    return extElement.GetAttributeValue(null, "value");
                }
            }

            return null;
        }

        /// <summary>
        /// 依次枚举直属 property 扩展元素及 properties 下的子元素，
        /// 避免每次访问都分配临时 List 与 LINQ 迭代链
        /// </summary>
        private static IEnumerable<ExtensionElement> EnumeratePropertyElements(BaseElement element)
        {
            if (element.ExtensionElements.TryGetValue(BpmnXMLConstants.ELEMENT_EXTENSIONS_PROPERTY, out IList<ExtensionElement> propertyElements))
            {
                foreach (ExtensionElement extElement in propertyElements)
                {
                    yield return extElement;
                }
            }
            if (element.ExtensionElements.TryGetValue(BpmnXMLConstants.ELEMENT_EXTENSIONS_PROPERTIES, out IList<ExtensionElement> propertiesElements))
            {
                foreach (ExtensionElement propertiesElement in propertiesElements)
                {
                    foreach (IList<ExtensionElement> childList in propertiesElement.ChildElements.Values)
                    {
                        foreach (ExtensionElement child in childList)
                        {
                            yield return child;
                        }
                    }
                }
            }
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