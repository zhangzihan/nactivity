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
namespace org.activiti.engine.impl.repository
{
    using java.util.zip;
    using org.activiti.bpmn.constants;
    using org.activiti.bpmn.converter;
    using org.activiti.bpmn.model;
    using org.activiti.engine.impl.context;
    using org.activiti.engine.impl.persistence.entity;
    using org.activiti.engine.impl.util;
    using org.activiti.engine.repository;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml.Linq;

    /// 
    /// 
    [Serializable]
    public class DeploymentBuilderImpl : IDeploymentBuilder
    {

        private const long serialVersionUID = 1L;
        protected internal const string DEFAULT_ENCODING = "UTF-8";

        [NonSerialized]
        protected internal RepositoryServiceImpl repositoryService;
        [NonSerialized]
        protected internal IResourceEntityManager resourceEntityManager;

        protected internal IDeploymentEntity deployment;
        protected internal bool isBpmn20XsdValidationEnabled = true;
        protected internal bool isProcessValidationEnabled = true;
        protected internal bool isDuplicateFilterEnabled;
        protected internal bool isDuplicateStartFormEnabled = true;
        protected internal DateTime? processDefinitionsActivationDate;
        protected internal IDictionary<string, object> deploymentProperties = new Dictionary<string, object>();

        private readonly object syncRoot = new object();

        public DeploymentBuilderImpl(RepositoryServiceImpl repositoryService)
        {
            this.repositoryService = repositoryService;
            this.deployment = Context.ProcessEngineConfiguration.DeploymentEntityManager.Create();
            this.resourceEntityManager = Context.ProcessEngineConfiguration.ResourceEntityManager;
        }

        public virtual IDeploymentBuilder AddInputStream(string resourceName, Stream inputStream)
        {
            if (inputStream == null)
            {
                throw new ActivitiIllegalArgumentException("inputStream for resource '" + resourceName + "' is null");
            }
            byte[] bytes = IoUtil.ReadInputStream(inputStream, resourceName);
            IResourceEntity resource = resourceEntityManager.Create();
            resource.Name = resourceName;
            resource.Bytes = bytes;
            deployment.AddResource(resource);
            return this;
        }

        public virtual IDeploymentBuilder AddClasspathResource(string resource)
        {
            System.IO.Stream inputStream = ReflectUtil.GetResourceAsStream(resource);
            if (inputStream == null)
            {
                throw new ActivitiIllegalArgumentException("resource '" + resource + "' not found");
            }
            return AddInputStream(resource, inputStream);
        }

        public virtual IDeploymentBuilder AddString(string resourceName, string text)
        {
            if (text is null)
            {
                throw new ActivitiIllegalArgumentException("text is null");
            }
            IResourceEntity resource = resourceEntityManager.Create();
            resource.Name = resourceName;
            try
            {
                resource.Bytes = text.GetBytes(DEFAULT_ENCODING);
            }
            catch (Exception e)
            {
                throw new ActivitiException("Unable to get process bytes.", e);
            }
            deployment.AddResource(resource);
            return this;
        }

        public virtual IDeploymentBuilder AddBytes(string resourceName, byte[] bytes)
        {
            IResourceEntity resource = resourceEntityManager.Create();
            resource.Name = resourceName;
            resource.Bytes = bytes ?? throw new ActivitiIllegalArgumentException("bytes is null");
            deployment.AddResource(resource);
            return this;
        }

        public virtual IDeploymentBuilder AddZipInputStream(ZipInputStream zipInputStream)
        {
            throw new NotImplementedException();
            //try
            //{
            //    ZipEntry entry = zipInputStream.NextEntry;
            //    while (entry != null)
            //    {
            //        if (!entry.Directory)
            //        {
            //            string entryName = entry.Name;
            //            byte[] bytes = IoUtil.readInputStream(zipInputStream, entryName);
            //            IResourceEntity resource = resourceEntityManager.create();
            //            resource.Name = entryName;
            //            resource.Bytes = bytes;
            //            deployment.addResource(resource);
            //        }
            //        entry = zipInputStream.NextEntry;
            //    }
            //}
            //catch (Exception e)
            //{
            //    throw new ActivitiException("problem reading zip input stream", e);
            //}
            //return this;
        }

        public virtual IDeploymentBuilder AddBpmnModel(string resourceName, BpmnModel bpmnModel)
        {
            BpmnXMLConverter bpmnXMLConverter = new BpmnXMLConverter();
            try
            {
                string bpmn20Xml = StringHelper.NewString(bpmnXMLConverter.ConvertToXML(bpmnModel), "UTF-8");
                AddString(resourceName, bpmn20Xml);
            }
            catch (Exception e)
            {
                throw new ActivitiException("Error while transforming BPMN model to xml: not UTF-8 encoded", e);
            }
            return this;
        }

        public virtual IDeploymentBuilder Name(string name)
        {
            deployment.Name = name;
            return this;
        }

        public virtual IDeploymentBuilder Category(string category)
        {
            deployment.Category = category;
            return this;
        }

        public virtual IDeploymentBuilder BusinessKey(string businessKey)
        {
            deployment.BusinessKey = businessKey;
            return this;
        }

        public virtual IDeploymentBuilder BusinessPath(string businessPath)
        {
            deployment.BusinessPath = businessPath;
            return this;
        }


        private string VerifyStartForm(string name, string startForm)
        {
            if (this.isDuplicateStartFormEnabled)
            {
                IList<IProcessDefinition> processes = repositoryService.CreateProcessDefinitionQuery()
                    .SetProcessDefinitionStartForm(startForm)
                    .SetLatestVersion()
                    .List();

                return processes.FirstOrDefault(x =>
                    string.IsNullOrWhiteSpace(x.StartForm) == false &&
                    !string.Equals(x.Name.Trim(), name, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(x.StartForm, startForm, StringComparison.OrdinalIgnoreCase))?.Name;
            }

            return null;
        }

        public virtual IDeploymentBuilder StartForm(string startForm, string bpmnXML)
        {
            lock (syncRoot)
            {
                if (string.IsNullOrWhiteSpace(startForm) == false)
                {
                    deployment.StartForm = startForm;
                }
                else
                {
                    BpmnXMLConverter bpm = new BpmnXMLConverter();

                    using (MemoryStream ms = new MemoryStream())
                    {
                        StreamWriter sw = new StreamWriter(ms);
                        sw.Write(bpmnXML);
                        sw.Flush();
                        ms.Seek(0, SeekOrigin.Begin);
                        XDocument doc = XDocument.Load(ms, LoadOptions.PreserveWhitespace);

                        XElement start = doc.Descendants(XName.Get(BpmnXMLConstants.ELEMENT_PROCESS, BpmnXMLConstants.BPMN2_NAMESPACE))
                             .Descendants(XName.Get(BpmnXMLConstants.ELEMENT_EVENT_START, BpmnXMLConstants.BPMN2_NAMESPACE))
                             .FirstOrDefault();

                        if (start != null)
                        {
                            string formKey = start.Attribute(XName.Get(BpmnXMLConstants.ATTRIBUTE_FORM_FORMKEY, BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE))?.Value;

                            deployment.StartForm = formKey;
                        }
                    }
                }

                //校验启动表单唯一性
                //if (string.IsNullOrWhiteSpace(deployment.StartForm) == false)
                //{
                //    string procName = VerifyStartForm(deployment.Name, deployment.StartForm);

                //    if (string.IsNullOrWhiteSpace(procName) == false)
                //    {
                //        throw new StartFormUniqueException(procName, deployment.StartForm);
                //    }
                //}

                //if (string.IsNullOrWhiteSpace(deployment.StartForm))
                //{
                //    throw new StartFormNullException(deployment.Name);
                //}
                return this;
            }
        }

        public virtual IDeploymentBuilder DisableDuplicateStartForm()
        {
            this.isDuplicateStartFormEnabled = false;
            return this;
        }

        public virtual IDeploymentBuilder Key(string key)
        {
            deployment.Key = key;
            return this;
        }

        public virtual IDeploymentBuilder DisableBpmnValidation()
        {
            this.isProcessValidationEnabled = false;
            return this;
        }

        public virtual IDeploymentBuilder DisableSchemaValidation()
        {
            this.isBpmn20XsdValidationEnabled = false;
            return this;
        }

        public virtual IDeploymentBuilder TenantId(string tenantId)
        {
            deployment.TenantId = tenantId;
            return this;
        }

        public virtual IDeploymentBuilder EnableDuplicateFiltering()
        {
            this.isDuplicateFilterEnabled = true;
            return this;
        }

        public virtual IDeploymentBuilder ActivateProcessDefinitionsOn(DateTime date)
        {
            this.processDefinitionsActivationDate = date;
            return this;
        }

        public virtual IDeploymentBuilder DeploymentProperty(string propertyKey, object propertyValue)
        {
            deploymentProperties[propertyKey] = propertyValue;
            return this;
        }

        public virtual IDeployment Deploy()
        {
            return repositoryService.Deploy(this);
        }

        public virtual IDeployment Save()
        {
            return repositoryService.Save(this);
        }

        public string Copy(string id, bool fullCopy)
        {
            IList<string> names = repositoryService.GetDeploymentResourceNames(id);

            Stream resourceStream = repositoryService.GetResourceAsStream(id, names[0]);

            if (fullCopy)
            {
                resourceStream.Seek(0, SeekOrigin.Begin);
                byte[] data = new byte[resourceStream.Length];
                resourceStream.Read(data, 0, data.Length);

                return new UTF8Encoding(false).GetString(data);
            }

            BpmnXMLConverter bpmnXMLConverter = new BpmnXMLConverter();

            BpmnModel model = bpmnXMLConverter.ConvertToBpmnModel(new XMLStreamReader(resourceStream));

            //return bpmnXMLConverter.convertToXML(model);
            return null;
        }

        // getters and setters
        // //////////////////////////////////////////////////////

        public virtual IDeploymentEntity Deployment
        {
            get
            {
                return deployment;
            }
        }

        public virtual bool ProcessValidationEnabled
        {
            get
            {
                return isProcessValidationEnabled;
            }
        }

        public virtual bool Bpmn20XsdValidationEnabled
        {
            get
            {
                return isBpmn20XsdValidationEnabled;
            }
        }

        public virtual bool DuplicateFilterEnabled
        {
            get
            {
                return isDuplicateFilterEnabled;
            }
        }

        public virtual DateTime? ProcessDefinitionsActivationDate
        {
            get
            {
                return processDefinitionsActivationDate;
            }
        }

        public virtual IDictionary<string, object> DeploymentProperties
        {
            get
            {
                return deploymentProperties;
            }
        }
    }
}