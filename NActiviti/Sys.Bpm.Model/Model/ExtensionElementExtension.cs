using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace org.activiti.bpmn.model
{
    public static class ExtensionElementExtension
    {
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
                string ename = element.getAttributeValue(null, "name");
                if (string.Equals(ename, name, StringComparison.OrdinalIgnoreCase))
                {
                    return element.getAttributeValue(null, "value");
                }
            }

            return null;
        }
    }
}