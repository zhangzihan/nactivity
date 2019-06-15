using org.activiti.bpmn.constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace org.activiti.bpmn.model
{
    /// <summary>
    /// 系统模型扩展属性
    /// </summary>
    public static class BpmnModelExtensions
    {
        public static bool TryGetExtensionElements(this XElement parent, out XElement extElement)
        {
            extElement = parent.Descendants(XName.Get(BpmnXMLConstants.ELEMENT_EXTENSIONS, BpmnXMLConstants.BPMN2_NAMESPACE)).FirstOrDefault();

            return !(extElement is null);
        }

        public static XElement GetOrAddExtensionElements(this XElement parent)
        {
            var extElement = parent.Descendants(XName.Get(BpmnXMLConstants.ELEMENT_EXTENSIONS, BpmnXMLConstants.BPMN2_NAMESPACE)).FirstOrDefault();

            if (extElement is null)
            {
                extElement = new XElement(XName.Get(BpmnXMLConstants.ELEMENT_EXTENSIONS, BpmnXMLConstants.BPMN2_NAMESPACE));

                parent.Add(extElement);
            }

            return extElement;
        }

        public static bool TryGetExtensionPropertiess(this XElement parent, out IEnumerable<XElement> extElements)
        {
            extElements = parent.Descendants(XName.Get(BpmnXMLConstants.ELEMENT_EXTENSIONS_PROPERTY, BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE));

            return !(extElements is null);
        }

        public static bool TryGetUserTaskAssigneeType(this XElement parent, out XElement assigneeElement)
        {
            assigneeElement = parent.Descendants(XName.Get(BpmnXMLConstants.ELEMENT_EXTENSIONS_PROPERTY, BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE))
                .Where(x => string.Compare(x.Attribute(BpmnXMLConstants.ATTRIBUTE_NAME)?.Value, BpmnXMLConstants.ELEMENT_USER_TASK_EXTENSION_ASSIGNE_TYPE, true) == 0)
                .FirstOrDefault();

            return !(assigneeElement is null);
        }

        public static bool TryGetExecutionStartListener(this XElement parent, out IEnumerable<XElement> listeners)
        {
            listeners = parent.Descendants(XName.Get(BpmnXMLConstants.ELEMENT_EXECUTION_LISTENER, BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE))
                                .Where(x => x.Attribute(BpmnXMLConstants.ATTRIBUTE_EVENT)?.Value == BpmnXMLConstants.ATTRIBUTE_EVENT_START_VALUE);

            return !(listeners is null);
        }

        private static IEnumerable<XElement> GetOrAddExecutionStartListener(this XElement parent, string listenerType)
        {
            IList<XElement> listeners = parent.Descendants(XName.Get(BpmnXMLConstants.ELEMENT_EXECUTION_LISTENER, BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE))
                                .Where(x => x.Attribute(BpmnXMLConstants.ATTRIBUTE_EVENT)?.Value == BpmnXMLConstants.ATTRIBUTE_EVENT_START_VALUE)?.ToList() ?? new List<XElement>();

            if (listeners.Count == 0)
            {
                XElement extElement = parent.GetOrAddExtensionElements();
                XElement listener = new XElement(BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE + BpmnXMLConstants.ELEMENT_EXECUTION_LISTENER,
                    new XAttribute(BpmnXMLConstants.ATTRIBUTE_LISTENER_CLASS, listenerType),
                    new XAttribute(BpmnXMLConstants.ATTRIBUTE_LISTENER_EVENT, BpmnXMLConstants.ATTRIBUTE_EVENT_START_VALUE));
                listeners.Add(listener);
                extElement.Add(listener);
            }

            return listeners;
        }

        //string xml = 
        //"<bpmn2:multiInstanceLoopCharacteristics camunda:collection="${0}" camunda:elementVariable="{1}">"
        //    <bpmn2:completionCondition xsi:type="bpmn2:tFormalExpression">
        //      ${nrOfActiveInstances==0}
        //    </bpmn2:completionCondition>
        //</bpmn2:multiInstanceLoopCharacteristics>";
        public static XElement GetOrAddMultiInstanceLoopCharacteristics(this XElement userTask, string collectionName = null, string elementName = null, string formalExpr = null)
        {
            XElement milc = userTask.Descendants(XName.Get(BpmnXMLConstants.ELEMENT_MULTIINSTANCE, BpmnXMLConstants.BPMN2_NAMESPACE)).FirstOrDefault();

            if (milc == null)
            {
                string id = userTask.Attribute(BpmnXMLConstants.ATTRIBUTE_ID).Value;

                collectionName = collectionName ?? $"{id}s";
                elementName = elementName ?? id;

                milc = new XElement(XName.Get(BpmnXMLConstants.ELEMENT_MULTIINSTANCE, BpmnXMLConstants.BPMN2_NAMESPACE),
                    /*添加collection attribute*/
                    new XAttribute(XName.Get(BpmnXMLConstants.ATTRIBUTE_MULTIINSTANCE_COLLECTION, BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE), $"${{{collectionName}}}"),
                    /*添加element variable attribute*/
                    new XAttribute(XName.Get(BpmnXMLConstants.ATTRIBUTE_MULTIINSTANCE_VARIABLE, BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE), $"{elementName}")
                    );

                /* isSequential="true"*/
                if (userTask.IsSequentialUserTask())
                {
                    milc.Add(new XAttribute(BpmnXMLConstants.ATTRIBUTE_MULTIINSTANCE_SEQUENTIAL, true));
                }

                /*添加 assignee attribute*/
                userTask.Add(new XAttribute(XName.Get(BpmnXMLConstants.ATTRIBUTE_TASK_USER_ASSIGNEE, BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE), $"${{{elementName}}}"));

                userTask.Add(milc);
            }

            XElement ccElem = milc.Descendants(XName.Get(BpmnXMLConstants.ELEMENT_COMPLETION_CONDITION, BpmnXMLConstants.BPMN2_NAMESPACE)).FirstOrDefault();

            if (ccElem == null)
            {
                formalExpr = formalExpr ?? "nrOfActiveInstances==0";

                ccElem = new XElement(XName.Get(BpmnXMLConstants.ELEMENT_COMPLETION_CONDITION, BpmnXMLConstants.BPMN2_NAMESPACE),
                    /*添加表达式完成条件*/
                    new XAttribute(XName.Get(BpmnXMLConstants.ATTRIBUTE_TYPE, BpmnXMLConstants.XSI_NAMESPACE), "bpmn2:tFormalExpression"));

                ccElem.Add($"${{{formalExpr}}}");

                milc.Add(ccElem);
            }

            return milc;
        }


        public static bool IsSequentialUserTask(this XElement userTask)
        {
            XElement elem = userTask.Descendants(XName.Get(BpmnXMLConstants.ELEMENT_EXTENSIONS_PROPERTY,
                BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE))
                .Where(x =>
                {
                    string value = x.Attribute(BpmnXMLConstants.ATTRIBUTE_MULTIINSTANCE_SEQUENTIAL)?.Value;

                    if (bool.TryParse(value, out bool isSeq))
                    {
                        return isSeq;
                    }

                    return false;
                }).FirstOrDefault();

            return !(elem is null);
        }
    }
}
