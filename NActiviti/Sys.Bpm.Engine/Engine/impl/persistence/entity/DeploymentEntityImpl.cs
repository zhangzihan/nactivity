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

namespace org.activiti.engine.impl.persistence.entity
{
    using org.activiti.bpmn.constants;
    using org.activiti.engine.@delegate;
    using org.activiti.engine.impl.bpmn.behavior;
    using org.activiti.engine.impl.bpmn.listener;
    using org.activiti.engine.impl.context;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Xml;
    using System.Xml.Linq;

    /// 
    /// 
    [Serializable]
    public class DeploymentEntityImpl : AbstractEntityNoRevision, IDeploymentEntity
    {
        private const long serialVersionUID = 1L;

        protected internal string name;
        protected internal string category;
        protected internal string key;
        protected internal string tenantId = ProcessEngineConfiguration.NO_TENANT_ID;
        protected internal IDictionary<string, IResourceEntity> resources;
        protected internal DateTime deploymentTime;
        protected internal bool isNew;

        // Backwards compatibility
        protected internal string engineVersion;

        private static readonly XNamespace bpmn2 = XNamespace.Get(BpmnXMLConstants.BPMN2_NAMESPACE);
        private static readonly XNamespace camunda = XNamespace.Get(BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE);
        private static readonly XNamespace xsi = XNamespace.Get(BpmnXMLConstants.XSI_NAMESPACE);

        private XmlNamespaceManager namespaceManager;

        /// <summary>
        /// Will only be used during actual deployment to pass deployed artifacts (eg process definitions). Will be null otherwise.
        /// </summary>
        protected internal IDictionary<Type, IList<object>> deployedArtifacts;

        public DeploymentEntityImpl()
        {

        }

        public virtual void addResource(IResourceEntity resource)
        {
            if (resources == null)
            {
                resources = new Dictionary<string, IResourceEntity>();
            }
            resources[resource.Name] = resource;
        }

        // lazy loading ///////////////////////////////////////////////////////////////

        public virtual IDictionary<string, IResourceEntity> GetResources()
        {
            var ctx = Context.CommandContext;
            if (resources == null && id != null && ctx != null)
            {
                IList<IResourceEntity> resourcesList = ctx.ResourceEntityManager.findResourcesByDeploymentId(id);
                resources = new Dictionary<string, IResourceEntity>();
                foreach (IResourceEntity resource in resourcesList)
                {
                    resources[resource.Name] = resource;
                }
            }
            return resources;
        }

        public virtual void SetResources(IDictionary<string, IResourceEntity> value)
        {
            this.resources = value;
        }

        public override PersistentState PersistentState
        {
            get
            {
                PersistentState persistentState = new PersistentState();

                persistentState["category"] = this.category;
                persistentState["key"] = this.key;
                persistentState["tenantId"] = tenantId;

                return persistentState;
            }
        }

        // Deployed artifacts manipulation ////////////////////////////////////////////

        public virtual void addDeployedArtifact(object deployedArtifact)
        {
            if (deployedArtifacts == null)
            {
                deployedArtifacts = new Dictionary<Type, IList<object>>();
            }

            Type clazz = deployedArtifact.GetType();
            deployedArtifacts.TryGetValue(clazz, out IList<object> artifacts);
            if (artifacts == null)
            {
                artifacts = new List<object>();
                deployedArtifacts[clazz] = artifacts;
            }

            artifacts.Add(deployedArtifact);
        }

        public virtual IList<T> getDeployedArtifacts<T>()
        {
            Type clazz = typeof(T);

            List<T> list = new List<T>();
            foreach (Type deployedArtifactsClass in deployedArtifacts.Keys)
            {
                if (clazz.IsAssignableFrom(deployedArtifactsClass))
                {
                    list.AddRange(deployedArtifacts[deployedArtifactsClass].Cast<T>().ToList());
                }
            }

            return list;
        }

        public virtual void unrunable()
        {
            foreach (string key in (resources ?? new Dictionary<string, IResourceEntity>()).Keys)
            {
                IResourceEntity resource = resources[key];
                if (resource.Bytes?.Length == 0)
                {
                    continue;
                }

                runable(resource, false);
            }
        }

        public virtual void runable()
        {
            foreach (string key in (resources ?? new Dictionary<string, IResourceEntity>()).Keys)
            {
                IResourceEntity resource = resources[key];
                if (resource.Bytes?.Length == 0)
                {
                    continue;
                }

                runable(resource, true);
            }
        }

        private static void runable(IResourceEntity resource, bool runable)
        {
            using (MemoryStream ms = new MemoryStream(resource.Bytes))
            {
                XDocument doc = XDocument.Load(ms);
                XElement process = doc.Descendants(XName.Get(BpmnXMLConstants.ELEMENT_PROCESS, BpmnXMLConstants.BPMN2_NAMESPACE)).First();
                process.Attribute(BpmnXMLConstants.ATTRIBUTE_PROCESS_EXECUTABLE).SetValue(runable);
                resource.Bytes = Encoding.UTF8.GetBytes(doc.ToString());
            }
        }


        public void addExecutionAdditional()
        {
            bool changed = false;
            foreach (string key in resources.Keys)
            {
                IResourceEntity resource = resources[key];

                MemoryStream ms = new MemoryStream(resource.Bytes);
                XDocument doc = XDocument.Load(ms, LoadOptions.PreserveWhitespace);
                var userTasks = doc.Descendants(XName.Get(BpmnXMLConstants.ELEMENT_TASK_USER, BpmnXMLConstants.BPMN2_NAMESPACE));

                XmlNameTable nameTable = doc.CreateReader(ReaderOptions.OmitDuplicateNamespaces).NameTable;
                namespaceManager = new XmlNamespaceManager(nameTable);
                namespaceManager.AddNamespace(BpmnXMLConstants.ACTIVITI_EXTENSIONS_PREFIX, BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE);

                changed = changed | ConvertUserTaskToParallel(userTasks);

                changed = changed | AddRuntimeAssigneeExcutionListener(userTasks);

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

        /// <summary>
        /// 修改用户任务节点为并行实例，解决追加节点操作
        /// </summary>
        private bool ConvertUserTaskToParallel(IEnumerable<XElement> userTasks)
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
                var loops = task.Descendants(XName.Get(BpmnXMLConstants.ELEMENT_MULTIINSTANCE, BpmnXMLConstants.BPMN2_NAMESPACE));
                XAttribute assignee = task.Attribute(XName.Get(BpmnXMLConstants.ATTRIBUTE_TASK_USER_ASSIGNEE, BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE));

                var match = new Regex("\\$\\{(.*?)\\}").Match(assignee?.Value);

                if (loops.Count() == 0 && match.Success)
                {
                    var varName = match.Groups[1].Value;

                    XElement completionCondition = new XElement(bpmn2 + BpmnXMLConstants.ELEMENT_COMPLETION_CONDITION,
                            new XAttribute(xsi + "type", "bpmn2:tFormalExpression"));
                    completionCondition.Value = "${" + MultiInstanceActivityBehavior.NUMBER_OF_INSTANCES + "==0}";

                    XElement milc = new XElement(bpmn2 + BpmnXMLConstants.ELEMENT_MULTIINSTANCE,
                        new XAttribute(camunda + BpmnXMLConstants.ATTRIBUTE_MULTIINSTANCE_COLLECTION, "${" + varName + "}"),
                        new XAttribute(camunda + BpmnXMLConstants.ATTRIBUTE_MULTIINSTANCE_VARIABLE, varName + "_"),
                        completionCondition);

                    assignee.Value = $"${{{varName}_}}";

                    task.Add(milc);

                    changed = true;
                }
            }

            return changed;
        }


        /// <summary>
        /// 添加运行时分配人员事件侦听
        /// </summary>

        private bool AddRuntimeAssigneeExcutionListener(IEnumerable<XElement> usertTasks)
        {
            /*<bpmn2:extensionElements>
             * <camunda:properties>
                  <camunda:property name="runtimeAssignee" value="true" />
                  <camunda:property name="assigneeVariable" value="dynamicUsers" />
                </camunda:properties>
                <camunda:executionListener class="org.activiti.engine.impl.bpmn.listener.RuntimeAssigneeExecutionListener,Sys.Bpm.Engine" event="start" />
                <camunda:executionListener class="org.activiti.engine.impl.bpmn.listener.RuntimeAssigneeExecutionEndedListener,Sys.Bpm.Engine" event="end" />
              </bpmn2:extensionElements>
           */
            bool changed = false;

            foreach (XElement task in usertTasks ?? new XElement[0])
            {
                XElement extElem = task.Descendants(XName.Get(BpmnXMLConstants.ELEMENT_EXTENSIONS, BpmnXMLConstants.BPMN2_NAMESPACE)).FirstOrDefault();

                if (extElem != null)
                {
                    changed = AddStartListener(extElem) | AddEndedListener(extElem);
                }
            }

            return changed;
        }

        private bool AddEndedListener(XElement extElem)
        {
            Type endListenerType = typeof(RuntimeAssigneeExecutionEndedListener);
            XElement endListener = (from x in extElem.Descendants(XName.Get(BpmnXMLConstants.ELEMENT_EXECUTION_LISTENER, BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE))
                                    where
               BaseExecutionListener_Fields.EVENTNAME_END.Equals(x.Attribute(BpmnXMLConstants.ATTRIBUTE_LISTENER_EVENT)?.Value, StringComparison.OrdinalIgnoreCase) &&
               (x.Attribute(BpmnXMLConstants.ATTRIBUTE_LISTENER_CLASS)?.Value.Contains(endListenerType.FullName)).GetValueOrDefault()
                                    select x).FirstOrDefault();

            if (endListener != null)
            {
                return false;
            }

            IEnumerable<XElement> eProps = extElem.Descendants(XName.Get("properties", BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE));

            XElement assignee = (from x in eProps.Descendants(XName.Get("property", BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE))
                                 where BpmnXMLConstants.ACTIITI_RUNTIME_ASSIGNEE.Equals(x.Attribute(BpmnXMLConstants.ATTRIBUTE_DATA_NAME)?.Value, StringComparison.OrdinalIgnoreCase)
                                 select x).FirstOrDefault();

            if (assignee != null && bool.TryParse(assignee.Attribute(BpmnXMLConstants.ELEMENT_DATA_VALUE)?.Value, out bool isRuntime) && isRuntime)
            {
                endListener = new XElement(camunda + BpmnXMLConstants.ELEMENT_EXECUTION_LISTENER,
                    new XAttribute(BpmnXMLConstants.ATTRIBUTE_LISTENER_CLASS, $"{endListenerType.FullName},{endListenerType.Assembly.GetName().Name}"),
                    new XAttribute(BpmnXMLConstants.ATTRIBUTE_LISTENER_EVENT, BaseExecutionListener_Fields.EVENTNAME_END));
                extElem.Add(endListener);

                return true;
            }

            return false;
        }

        private bool AddStartListener(XElement extElem)
        {
            Type runListenerType = typeof(RuntimeAssigneeExecutionListener);
            XElement startListener = (from x in extElem.Descendants(XName.Get(BpmnXMLConstants.ELEMENT_EXECUTION_LISTENER, BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE))
                                      where
                 BaseExecutionListener_Fields.EVENTNAME_START.Equals(x.Attribute(BpmnXMLConstants.ATTRIBUTE_LISTENER_EVENT)?.Value, StringComparison.OrdinalIgnoreCase) &&
                 (x.Attribute(BpmnXMLConstants.ATTRIBUTE_LISTENER_CLASS)?.Value.Contains(runListenerType.FullName)).GetValueOrDefault()
                                      select x).FirstOrDefault();

            if (startListener != null)
            {
                return false;
            }

            IEnumerable<XElement> eProps = extElem.Descendants(XName.Get("properties", BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE));

            XElement assignee = (from x in eProps.Descendants(XName.Get("property", BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE))
                                 where BpmnXMLConstants.ACTIITI_RUNTIME_ASSIGNEE.Equals(x.Attribute(BpmnXMLConstants.ATTRIBUTE_DATA_NAME)?.Value, StringComparison.OrdinalIgnoreCase)
                                 select x).FirstOrDefault();

            if (assignee != null && bool.TryParse(assignee.Attribute(BpmnXMLConstants.ELEMENT_DATA_VALUE)?.Value, out bool isRuntime) && isRuntime)
            {
                startListener = new XElement(camunda + BpmnXMLConstants.ELEMENT_EXECUTION_LISTENER,
                    new XAttribute(BpmnXMLConstants.ATTRIBUTE_LISTENER_CLASS, $"{runListenerType.FullName},{runListenerType.Assembly.GetName().Name}"),
                    new XAttribute(BpmnXMLConstants.ATTRIBUTE_LISTENER_EVENT, BaseExecutionListener_Fields.EVENTNAME_START));
                extElem.Add(startListener);

                return true;
            }

            return false;
        }

        // getters and setters ////////////////////////////////////////////////////////

        public virtual string Name
        {
            get
            {
                return name;
            }
            set
            {
                this.name = value;
            }
        }

        public virtual string BusinessKey { get; set; }

        public virtual string BusinessPath { get; set; }

        public string StartForm { get; set; }


        public virtual string Category
        {
            get
            {
                return category;
            }
            set
            {
                this.category = value;
            }
        }


        public virtual string Key
        {
            get
            {
                return key;
            }
            set
            {
                this.key = value;
            }
        }


        public virtual string TenantId
        {
            get
            {
                return tenantId;
            }
            set
            {
                this.tenantId = value;
            }
        }



        public virtual DateTime DeploymentTime
        {
            get
            {
                return deploymentTime;
            }
            set
            {
                this.deploymentTime = value;
            }
        }


        public virtual bool New
        {
            get
            {
                return isNew;
            }
            set
            {
                this.isNew = value;
            }
        }


        public virtual string EngineVersion
        {
            get
            {
                return engineVersion;
            }
            set
            {
                this.engineVersion = value;
            }
        }


        // common methods //////////////////////////////////////////////////////////

        public override string ToString()
        {
            return "DeploymentEntity[id=" + id + ", name=" + name + "]";
        }

    }
}