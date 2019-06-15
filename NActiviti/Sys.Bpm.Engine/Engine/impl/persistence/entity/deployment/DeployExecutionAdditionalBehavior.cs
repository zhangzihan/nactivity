using org.activiti.bpmn.constants;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace org.activiti.engine.impl.persistence.entity
{
    /// <inheritdoc />
    public class DeployExecutionAdditionalBehavior : IDeployExecutionBehavior
    {
        private XmlNamespaceManager namespaceManager;

        private static readonly IDeployMultilInstanceExecutionListener deployMultilInstanceExecutionListener = new DeployMultilInstanceExecutionListener();

        private static readonly IDeployUserTaskToParallel deployUserTaskToParallel = new DeployUserTaskToParallel();

        private static readonly IDeployRuntimeAssigneeExecutionListener deployRuntimeAssigneeExecutionListener = new DeployRuntimeAssigneeExecutionListener();

        private static readonly IDeployUserTaskAssigneeListener deployUserTaskAssigneeListener = new DeployUserTaskAssigneeListener();

        private static readonly IDeployServiceTask deployServiceTask = new DeployServiceTask();

        /// <inheritdoc />
        public void Deploy(IDictionary<string, IResourceEntity> resources)
        {
            foreach (string key in resources.Keys)
            {
                IResourceEntity resource = resources[key];

                MemoryStream ms = new MemoryStream(resource.Bytes);
                XDocument doc = XDocument.Load(ms, LoadOptions.PreserveWhitespace);
                var userTasks = doc.Descendants(XName.Get(BpmnXMLConstants.ELEMENT_TASK_USER, BpmnXMLConstants.BPMN2_NAMESPACE));

                XmlNameTable nameTable = doc.CreateReader(ReaderOptions.OmitDuplicateNamespaces).NameTable;
                namespaceManager = new XmlNamespaceManager(nameTable);
                namespaceManager.AddNamespace(BpmnXMLConstants.ACTIVITI_EXTENSIONS_PREFIX, BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE);

                var serviceTasks = doc.Descendants(XName.Get(BpmnXMLConstants.ELEMENT_TASK_SERVICE, BpmnXMLConstants.BPMN2_NAMESPACE));

                bool changed = deployUserTaskToParallel.ConvertUserTaskToParallel(userTasks)
                    | deployMultilInstanceExecutionListener.AddMultilinstanceExcutionListener(userTasks)
                    | deployRuntimeAssigneeExecutionListener.AddRuntimeAssigneeExcutionListener(userTasks)
                    | deployUserTaskAssigneeListener.AddUserTaskAssignmentListener(userTasks)
                    | deployServiceTask.AddDefaultImplementation(serviceTasks);

                if (changed)
                {
                    MemoryStream saved = new MemoryStream();
                    doc.Save(saved);
                    saved.Flush();
                    saved.Seek(0, SeekOrigin.Begin);
                    resource.Bytes = saved.ToArray();
                    saved.Close();
                }

                ms.Close();
            }
        }
    }
}
