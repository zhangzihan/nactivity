using Sys.Workflow.Bpmn.Constants;
using Sys.Workflow.Bpmn.Models;
using Sys.Workflow.Engine.Impl.Bpmn.Behavior;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Sys.Workflow.Engine.Impl.Persistence.Entity
{
    /// <inheritdoc />
    class DeployUserTaskToParallel : IDeployUserTaskToParallel
    {
        private static readonly Regex EXPR_PATTERN = new Regex(@"\$\{(.*?)\}");

        /// <inheritdoc />
        public bool ConvertUserTaskToParallel(IEnumerable<XElement> userTasks)
        {
            //string xml = 
            //"<bpmn2:multiInstanceLoopCharacteristics camunda:collection="${0}" camunda:elementVariable="{1}">"
            //    <bpmn2:completionCondition xsi:type="bpmn2:tFormalExpression">
            //      ${nrOfActiveInstances==0}
            //    </bpmn2:completionCondition>
            //</bpmn2:multiInstanceLoopCharacteristics>";

            bool changed = false;

            foreach (XElement task in userTasks ?? new XElement[0])
            {
                string id = task.Attribute("id").Value;
                XAttribute assignee = task.Attribute(XName.Get(BpmnXMLConstants.ATTRIBUTE_TASK_USER_ASSIGNEE, BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE));

                string user = assignee?.Value;

                //如果对 assignee 没有赋值，调用默认方法将节点转为多任务节点
                //否则 assignee 如果如果是表达式变量，则将源变量名做为collection变量名，将节点转为多任务节点
                if (string.IsNullOrWhiteSpace(user))
                {
                    XElement milc = task.GetOrAddMultiInstanceLoopCharacteristics();
                }
                else
                {
                    var loops = task.Descendants(XName.Get(BpmnXMLConstants.ELEMENT_MULTIINSTANCE, BpmnXMLConstants.BPMN2_NAMESPACE));

                    Match match = EXPR_PATTERN.Match(user);

                    if (loops.Count() == 0 && match.Success)
                    {
                        string[] varNames = match.Groups[1].Value.Split('.');
                        string objName = string.Join(".", varNames.Take(varNames.Length));

                        XElement completionCondition = new XElement(XName.Get(BpmnXMLConstants.ELEMENT_COMPLETION_CONDITION, BpmnXMLConstants.BPMN2_NAMESPACE),
                                /*添加任务节点完成条件,默认为所有人都执行完成*/
                                new XAttribute(XName.Get(BpmnXMLConstants.ATTRIBUTE_TYPE, BpmnXMLConstants.XSI_NAMESPACE), "bpmn2:tFormalExpression"))
                        {
                            Value = $"${{{MultiInstanceActivityBehavior.NUMBER_OF_ACTIVE_INSTANCES}==0}}"
                        };

                        XElement milc = new XElement(XName.Get(BpmnXMLConstants.ELEMENT_MULTIINSTANCE, BpmnXMLConstants.BPMN2_NAMESPACE),
                            /*添加 collection attribute*/
                            new XAttribute(XName.Get(BpmnXMLConstants.ATTRIBUTE_MULTIINSTANCE_COLLECTION, BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE), $"${{{objName}}}"),
                            /*添加实例变量 elementVariable*/
                            new XAttribute(XName.Get(BpmnXMLConstants.ATTRIBUTE_MULTIINSTANCE_VARIABLE, BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE), id),
                            completionCondition);

                        /*赋值 assignee 变量*/
                        assignee.Value = "${" + id + (varNames.Length > 1 ? $"{string.Join(".", varNames.Skip(1))}" : "") + "}";

                        task.Add(milc);

                        changed = true;
                    }
                }
            }

            return changed;
        }
    }
}
