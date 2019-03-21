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

        private object syncRoot = new object();

        public DeploymentBuilderImpl(RepositoryServiceImpl repositoryService)
        {
            this.repositoryService = repositoryService;
            this.deployment = Context.ProcessEngineConfiguration.DeploymentEntityManager.create();
            this.resourceEntityManager = Context.ProcessEngineConfiguration.ResourceEntityManager;
        }

        public virtual IDeploymentBuilder addInputStream(string resourceName, System.IO.Stream inputStream)
        {
            if (inputStream == null)
            {
                throw new ActivitiIllegalArgumentException("inputStream for resource '" + resourceName + "' is null");
            }
            byte[] bytes = IoUtil.readInputStream(inputStream, resourceName);
            IResourceEntity resource = resourceEntityManager.create();
            resource.Name = resourceName;
            resource.Bytes = bytes;
            deployment.addResource(resource);
            return this;
        }

        public virtual IDeploymentBuilder addClasspathResource(string resource)
        {
            System.IO.Stream inputStream = ReflectUtil.getResourceAsStream(resource);
            if (inputStream == null)
            {
                throw new ActivitiIllegalArgumentException("resource '" + resource + "' not found");
            }
            return addInputStream(resource, inputStream);
        }

        public virtual IDeploymentBuilder addString(string resourceName, string text)
        {
            if (ReferenceEquals(text, null))
            {
                throw new ActivitiIllegalArgumentException("text is null");
            }
            IResourceEntity resource = resourceEntityManager.create();
            resource.Name = resourceName;
            try
            {
                resource.Bytes = text.GetBytes(DEFAULT_ENCODING);
            }
            catch (Exception e)
            {
                throw new ActivitiException("Unable to get process bytes.", e);
            }
            deployment.addResource(resource);
            return this;
        }

        public virtual IDeploymentBuilder addBytes(string resourceName, byte[] bytes)
        {
            if (bytes == null)
            {
                throw new ActivitiIllegalArgumentException("bytes is null");
            }

            IResourceEntity resource = resourceEntityManager.create();
            resource.Name = resourceName;
            resource.Bytes = bytes;
            deployment.addResource(resource);
            return this;
        }

        public virtual IDeploymentBuilder addZipInputStream(ZipInputStream zipInputStream)
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

        public virtual IDeploymentBuilder addBpmnModel(string resourceName, BpmnModel bpmnModel)
        {
            BpmnXMLConverter bpmnXMLConverter = new BpmnXMLConverter();
            try
            {
                string bpmn20Xml = StringHelper.NewString(bpmnXMLConverter.convertToXML(bpmnModel), "UTF-8");
                addString(resourceName, bpmn20Xml);
            }
            catch (Exception e)
            {
                throw new ActivitiException("Error while transforming BPMN model to xml: not UTF-8 encoded", e);
            }
            return this;
        }

        public virtual IDeploymentBuilder name(string name)
        {
            deployment.Name = name;
            return this;
        }

        public virtual IDeploymentBuilder category(string category)
        {
            deployment.Category = category;
            return this;
        }

        public virtual IDeploymentBuilder businessKey(string businessKey)
        {
            deployment.BusinessKey = businessKey;
            return this;
        }

        public virtual IDeploymentBuilder businessPath(string businessPath)
        {
            deployment.BusinessPath = businessPath;
            return this;
        }


        private string VerifyStartForm(string name, string startForm)
        {
            if (this.isDuplicateStartFormEnabled)
            {
                IList<IProcessDefinition> processes = repositoryService.createProcessDefinitionQuery()
                    .processDefinitionStartForm(startForm)
                    .latestVersion()
                    .list();

                return processes.FirstOrDefault(x => string.IsNullOrWhiteSpace(x.StartForm) == false &&
                    x.StartForm.Trim().ToLower() == startForm.Trim().ToLower() &&
                    x.Name.Trim() != name.Trim().ToLower())?.Name;
            }

            return null;
        }

        public virtual IDeploymentBuilder startForm(string startForm, string bpmnXML)
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

                if (string.IsNullOrWhiteSpace(deployment.StartForm) == false)
                {
                    string procName = VerifyStartForm(deployment.Name, deployment.StartForm);

                    if (string.IsNullOrWhiteSpace(procName) == false)
                    {
                        throw new StartFormUniqueException(procName, deployment.StartForm);
                    }
                }

                //if (string.IsNullOrWhiteSpace(deployment.StartForm))
                //{
                //    throw new StartFormNullException(deployment.Name);
                //}
                return this;
            }
        }

        public virtual IDeploymentBuilder disableDuplicateStartForm()
        {
            this.isDuplicateStartFormEnabled = false;
            return this;
        }

        public virtual IDeploymentBuilder key(string key)
        {
            deployment.Key = key;
            return this;
        }

        public virtual IDeploymentBuilder disableBpmnValidation()
        {
            this.isProcessValidationEnabled = false;
            return this;
        }

        public virtual IDeploymentBuilder disableSchemaValidation()
        {
            this.isBpmn20XsdValidationEnabled = false;
            return this;
        }

        public virtual IDeploymentBuilder tenantId(string tenantId)
        {
            deployment.TenantId = tenantId;
            return this;
        }

        public virtual IDeploymentBuilder enableDuplicateFiltering()
        {
            this.isDuplicateFilterEnabled = true;
            return this;
        }

        public virtual IDeploymentBuilder activateProcessDefinitionsOn(DateTime date)
        {
            this.processDefinitionsActivationDate = date;
            return this;
        }

        public virtual IDeploymentBuilder deploymentProperty(string propertyKey, object propertyValue)
        {
            deploymentProperties[propertyKey] = propertyValue;
            return this;
        }

        public virtual IDeployment deploy()
        {
            return repositoryService.deploy(this);
        }

        public virtual IDeployment save()
        {
            return repositoryService.save(this);
        }

        public string copy(string id, bool fullCopy)
        {
            IList<string> names = repositoryService.getDeploymentResourceNames(id);

            Stream resourceStream = repositoryService.getResourceAsStream(id, names[0]);

            if (fullCopy)
            {
                resourceStream.Seek(0, SeekOrigin.Begin);
                byte[] data = new byte[resourceStream.Length];
                resourceStream.Read(data, 0, data.Length);

                return Encoding.UTF8.GetString(data);
            }

            BpmnXMLConverter bpmnXMLConverter = new BpmnXMLConverter();

            BpmnModel model = bpmnXMLConverter.convertToBpmnModel(new XMLStreamReader(resourceStream));

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