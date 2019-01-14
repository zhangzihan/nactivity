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
namespace org.activiti.engine.impl.bpmn.deployer
{
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json.Linq;
    using org.activiti.bpmn.constants;
    using org.activiti.bpmn.model;
    using org.activiti.engine.@delegate.@event;
    using org.activiti.engine.@delegate.@event.impl;
    using org.activiti.engine.impl.cfg;
    using org.activiti.engine.impl.context;
    using org.activiti.engine.impl.interceptor;
    using org.activiti.engine.impl.persistence.deploy;
    using org.activiti.engine.impl.persistence.entity;
    using Sys;

    public class BpmnDeployer : IDeployer
    {
        protected internal IIdGenerator idGenerator;
        protected internal ParsedDeploymentBuilderFactory parsedDeploymentBuilderFactory;
        protected internal BpmnDeploymentHelper bpmnDeploymentHelper;
        protected internal CachingAndArtifactsManager cachingAndArtifactsManager;

        private static readonly ILogger<BpmnDeployer> log = ProcessEngineServiceProvider.LoggerService<BpmnDeployer>();

        public virtual void deploy(IDeploymentEntity deployment, IDictionary<string, object> deploymentSettings)
        {
            log.LogDebug($"Processing deployment {deployment.Name}");

            // The ParsedDeployment represents the deployment, the process definitions, and the BPMN
            // resource, parse, and model associated with each process definition.
            ParsedDeployment parsedDeployment = parsedDeploymentBuilderFactory.getBuilderForDeploymentAndSettings(deployment, deploymentSettings).build();

            bpmnDeploymentHelper.verifyProcessDefinitionsDoNotShareKeys(parsedDeployment.AllProcessDefinitions);

            bpmnDeploymentHelper.copyDeploymentValuesToProcessDefinitions(parsedDeployment.Deployment, parsedDeployment.AllProcessDefinitions);
            bpmnDeploymentHelper.ResourceNamesOnProcessDefinitions(parsedDeployment);

            //    createAndPersistNewDiagramsIfNeeded(parsedDeployment);
            ProcessDefinitionDiagramNames(parsedDeployment);

            if (deployment.New)
            {
                IDictionary<IProcessDefinitionEntity, IProcessDefinitionEntity> mapOfNewProcessDefinitionToPreviousVersion = getPreviousVersionsOfProcessDefinitions(parsedDeployment);
                setProcessDefinitionVersionsAndIds(parsedDeployment, mapOfNewProcessDefinitionToPreviousVersion);
                persistProcessDefinitionsAndAuthorizations(parsedDeployment);
                updateTimersAndEvents(parsedDeployment, mapOfNewProcessDefinitionToPreviousVersion);
                dispatchProcessDefinitionEntityInitializedEvent(parsedDeployment);
            }
            else
            {
                makeProcessDefinitionsConsistentWithPersistedVersions(parsedDeployment);
            }

            cachingAndArtifactsManager.updateCachingAndArtifacts(parsedDeployment);

            foreach (IProcessDefinitionEntity processDefinition in parsedDeployment.AllProcessDefinitions)
            {
                BpmnModel bpmnModel = parsedDeployment.getBpmnModelForProcessDefinition(processDefinition);
                createLocalizationValues(processDefinition.Id, bpmnModel.getProcessById(processDefinition.Key));
            }
        }
        //
        //  /**
        //   * Creates new diagrams for process definitions if the deployment is new, the process definition in
        //   * question supports it, and the engine is configured to make new diagrams.
        //   *
        //   * When this method creates a new diagram, it also persists it via the ResourceEntityManager
        //   * and adds it to the resources of the deployment.
        //   */
        //  protected void createAndPersistNewDiagramsIfNeeded(ParsedDeployment parsedDeployment) {
        //
        //    final ProcessEngineConfigurationImpl processEngineConfiguration = Context.getProcessEngineConfiguration();
        //    final DeploymentEntity deploymentEntity = parsedDeployment.getDeployment();
        //
        //    final ResourceEntityManager resourceEntityManager = processEngineConfiguration.getResourceEntityManager();
        //
        //    for (ProcessDefinitionEntity processDefinition : parsedDeployment.getAllProcessDefinitions()) {
        //      if (processDefinitionDiagramHelper.shouldCreateDiagram(processDefinition, deploymentEntity)) {
        //        ResourceEntity resource = processDefinitionDiagramHelper.createDiagramForProcessDefinition(
        //            processDefinition, parsedDeployment.getBpmnParseForProcessDefinition(processDefinition));
        //        if (resource != null) {
        //          resourceEntityManager.insert(resource, false);
        //          deploymentEntity.addResource(resource);  // now we'll find it if we look for the diagram name later.
        //        }
        //      }
        //    }
        //  }

        /// <summary>
        /// Updates all the process definition entities to have the correct diagram resource name.  Must
        /// be called after createAndPersistNewDiagramsAsNeeded to ensure that any newly-created diagrams
        /// already have their resources attached to the deployment.
        /// </summary>
        protected internal virtual ParsedDeployment ProcessDefinitionDiagramNames(ParsedDeployment value)
        {
            IDictionary<string, IResourceEntity> resources = value.Deployment.GetResources();

            foreach (IProcessDefinitionEntity processDefinition in value.AllProcessDefinitions)
            {
                string diagramResourceName = ResourceNameUtil.getProcessDiagramResourceNameFromDeployment(processDefinition, resources);
                processDefinition.DiagramResourceName = diagramResourceName;
            }

            return value;
        }

        /// <summary>
        /// Constructs a map from new ProcessDefinitionEntities to the previous version by key and tenant.
        /// If no previous version exists, no map entry is created.
        /// </summary>
        protected internal virtual IDictionary<IProcessDefinitionEntity, IProcessDefinitionEntity> getPreviousVersionsOfProcessDefinitions(ParsedDeployment parsedDeployment)
        {
            IDictionary<IProcessDefinitionEntity, IProcessDefinitionEntity> result = new Dictionary<IProcessDefinitionEntity, IProcessDefinitionEntity>();

            foreach (IProcessDefinitionEntity newDefinition in parsedDeployment.AllProcessDefinitions)
            {
                IProcessDefinitionEntity existingDefinition = bpmnDeploymentHelper.getMostRecentVersionOfProcessDefinition(newDefinition);

                if (existingDefinition != null)
                {
                    result[newDefinition] = existingDefinition;
                }
            }

            return result;
        }

        /// <summary>
        /// Sets the version on each process definition entity, and the identifier.  If the map contains
        /// an older version for a process definition, then the version is set to that older entity's
        /// version plus one; otherwise it is set to 1.  Also dispatches an ENTITY_CREATED event.
        /// </summary>
        protected internal virtual void setProcessDefinitionVersionsAndIds(ParsedDeployment parsedDeployment, IDictionary<IProcessDefinitionEntity, IProcessDefinitionEntity> mapNewToOldProcessDefinitions)
        {
            ICommandContext commandContext = Context.CommandContext;

            foreach (IProcessDefinitionEntity processDefinition in parsedDeployment.AllProcessDefinitions)
            {
                int version = 1;

                mapNewToOldProcessDefinitions.TryGetValue(processDefinition, out IProcessDefinitionEntity latest);
                if (latest != null)
                {
                    version = latest.Version + 1;
                }

                processDefinition.Version = version;
                processDefinition.Id = getIdForNewProcessDefinition(processDefinition);

                if (commandContext.ProcessEngineConfiguration.EventDispatcher.Enabled)
                {
                    commandContext.ProcessEngineConfiguration.EventDispatcher.dispatchEvent(ActivitiEventBuilder.createEntityEvent(ActivitiEventType.ENTITY_CREATED, processDefinition));
                }
            }
        }

        /// <summary>
        /// Saves each process definition.  It is assumed that the deployment is new, the definitions
        /// have never been saved before, and that they have all their values properly set up.
        /// </summary>
        protected internal virtual void persistProcessDefinitionsAndAuthorizations(ParsedDeployment parsedDeployment)
        {
            ICommandContext commandContext = Context.CommandContext;
            IProcessDefinitionEntityManager processDefinitionManager = commandContext.ProcessDefinitionEntityManager;

            foreach (IProcessDefinitionEntity processDefinition in parsedDeployment.AllProcessDefinitions)
            {
                processDefinitionManager.insert(processDefinition, false);
                bpmnDeploymentHelper.addAuthorizationsForNewProcessDefinition(parsedDeployment.getProcessModelForProcessDefinition(processDefinition), processDefinition);
            }
        }

        protected internal virtual void updateTimersAndEvents(ParsedDeployment parsedDeployment, IDictionary<IProcessDefinitionEntity, IProcessDefinitionEntity> mapNewToOldProcessDefinitions)
        {
            foreach (IProcessDefinitionEntity processDefinition in parsedDeployment.AllProcessDefinitions)
            {
                if (mapNewToOldProcessDefinitions.TryGetValue(processDefinition, out var item))
                {
                    bpmnDeploymentHelper.updateTimersAndEvents(processDefinition, item, parsedDeployment);
                }
            }
        }

        protected internal virtual void dispatchProcessDefinitionEntityInitializedEvent(ParsedDeployment parsedDeployment)
        {
            ICommandContext commandContext = Context.CommandContext;
            foreach (IProcessDefinitionEntity processDefinitionEntity in parsedDeployment.AllProcessDefinitions)
            {
                if (commandContext.ProcessEngineConfiguration.EventDispatcher.Enabled)
                {
                    commandContext.ProcessEngineConfiguration.EventDispatcher.dispatchEvent(ActivitiEventBuilder.createEntityEvent(ActivitiEventType.ENTITY_INITIALIZED, processDefinitionEntity));
                }
            }
        }

        /// <summary>
        /// Returns the ID to use for a new process definition; subclasses may override this to provide
        /// their own identification scheme.
        /// <para>
        /// Process definition ids NEED to be unique accross the whole engine!
        /// </para>
        /// </summary>
        protected internal virtual string getIdForNewProcessDefinition(IProcessDefinitionEntity processDefinition)
        {
            string nextId = idGenerator.NextId;

            //string result = processDefinition.Key + ":" + processDefinition.Version + ":" + nextId; // ACT-505
            // ACT-115: maximum id length is 64 characters
            //if (result.Length > 64)
            //{
            //result = nextId;
            //}

            return nextId;
        }

        /// <summary>
        /// Loads the persisted version of each process definition and set values on the in-memory
        /// version to be consistent.
        /// </summary>
        protected internal virtual void makeProcessDefinitionsConsistentWithPersistedVersions(ParsedDeployment parsedDeployment)
        {
            foreach (IProcessDefinitionEntity processDefinition in parsedDeployment.AllProcessDefinitions)
            {
                IProcessDefinitionEntity persistedProcessDefinition = bpmnDeploymentHelper.getPersistedInstanceOfProcessDefinition(processDefinition);

                if (persistedProcessDefinition != null)
                {
                    processDefinition.Id = persistedProcessDefinition.Id;
                    processDefinition.Version = persistedProcessDefinition.Version;
                    processDefinition.SuspensionState = persistedProcessDefinition.SuspensionState;
                }
            }
        }

        protected internal virtual void createLocalizationValues(string processDefinitionId, Process process)
        {
            if (process == null)
            {
                return;
            }

            ICommandContext commandContext = Context.CommandContext;
            IDynamicBpmnService dynamicBpmnService = commandContext.ProcessEngineConfiguration.DynamicBpmnService;
            JToken infoNode = dynamicBpmnService.getProcessDefinitionInfo(processDefinitionId);

            bool localizationValuesChanged = false;
            process.ExtensionElements.TryGetValue("localization", out IList<ExtensionElement> localizationElements);
            if (localizationElements != null)
            {
                foreach (ExtensionElement localizationElement in localizationElements)
                {
                    if (BpmnXMLConstants.ACTIVITI_EXTENSIONS_PREFIX.Equals(localizationElement.NamespacePrefix))
                    {
                        string locale = localizationElement.getAttributeValue(null, "locale");
                        string name = localizationElement.getAttributeValue(null, "name");
                        string documentation = null;
                        localizationElement.ChildElements.TryGetValue("documentation", out IList<ExtensionElement> documentationElements);
                        if (documentationElements != null)
                        {
                            foreach (ExtensionElement documentationElement in documentationElements)
                            {
                                documentation = documentationElement.ElementText.Trim();
                                break;
                            }
                        }

                        string processId = process.Id;
                        if (!isEqualToCurrentLocalizationValue(locale, processId, "name", name, infoNode))
                        {
                            dynamicBpmnService.changeLocalizationName(locale, processId, name, infoNode);
                            localizationValuesChanged = true;
                        }

                        if (!string.ReferenceEquals(documentation, null) && !isEqualToCurrentLocalizationValue(locale, processId, "description", documentation, infoNode))
                        {
                            dynamicBpmnService.changeLocalizationDescription(locale, processId, documentation, infoNode);
                            localizationValuesChanged = true;
                        }

                        break;
                    }
                }
            }

            bool isFlowElementLocalizationChanged = localizeFlowElements(process.FlowElements, infoNode);
            bool isDataObjectLocalizationChanged = localizeDataObjectElements(process.DataObjects, infoNode);
            if (isFlowElementLocalizationChanged || isDataObjectLocalizationChanged)
            {
                localizationValuesChanged = true;
            }

            if (localizationValuesChanged)
            {
                dynamicBpmnService.saveProcessDefinitionInfo(processDefinitionId, infoNode);
            }
        }

        protected internal virtual bool localizeFlowElements(ICollection<FlowElement> flowElements, JToken infoNode)
        {
            bool localizationValuesChanged = false;

            if (flowElements == null)
            {
                return localizationValuesChanged;
            }

            ICommandContext commandContext = Context.CommandContext;
            IDynamicBpmnService dynamicBpmnService = commandContext.ProcessEngineConfiguration.DynamicBpmnService;

            foreach (FlowElement flowElement in flowElements)
            {
                if (flowElement is UserTask || flowElement is SubProcess)
                {
                    flowElement.ExtensionElements.TryGetValue("localization", out IList<ExtensionElement> localizationElements);
                    if (localizationElements != null)
                    {
                        foreach (ExtensionElement localizationElement in localizationElements)
                        {
                            if (BpmnXMLConstants.ACTIVITI_EXTENSIONS_PREFIX.Equals(localizationElement.NamespacePrefix))
                            {
                                string locale = localizationElement.getAttributeValue(null, "locale");
                                string name = localizationElement.getAttributeValue(null, "name");
                                string documentation = null;
                                localizationElement.ChildElements.TryGetValue("documentation", out IList<ExtensionElement> documentationElements);
                                if (documentationElements != null)
                                {
                                    foreach (ExtensionElement documentationElement in documentationElements)
                                    {
                                        documentation = documentationElement.ElementText.Trim();
                                        break;
                                    }
                                }

                                string flowElementId = flowElement.Id;
                                if (isEqualToCurrentLocalizationValue(locale, flowElementId, "name", name, infoNode) == false)
                                {
                                    dynamicBpmnService.changeLocalizationName(locale, flowElementId, name, infoNode);
                                    localizationValuesChanged = true;
                                }

                                if (!string.ReferenceEquals(documentation, null) && isEqualToCurrentLocalizationValue(locale, flowElementId, "description", documentation, infoNode) == false)
                                {
                                    dynamicBpmnService.changeLocalizationDescription(locale, flowElementId, documentation, infoNode);
                                    localizationValuesChanged = true;
                                }

                                break;
                            }
                        }
                    }

                    if (flowElement is SubProcess)
                    {
                        SubProcess subprocess = (SubProcess)flowElement;
                        bool isFlowElementLocalizationChanged = localizeFlowElements(subprocess.FlowElements, infoNode);
                        bool isDataObjectLocalizationChanged = localizeDataObjectElements(subprocess.DataObjects, infoNode);
                        if (isFlowElementLocalizationChanged || isDataObjectLocalizationChanged)
                        {
                            localizationValuesChanged = true;
                        }
                    }
                }
            }

            return localizationValuesChanged;
        }

        protected internal virtual bool isEqualToCurrentLocalizationValue(string language, string id, string propertyName, string propertyValue, JToken infoNode)
        {
            bool isEqual = false;
            JToken localizationNode = infoNode.SelectToken("localization.language.id.propertyName");
            if (localizationNode != null && localizationNode.ToString().Equals(propertyValue))
            {
                isEqual = true;
            }
            return isEqual;
        }

        protected internal virtual bool localizeDataObjectElements(IList<ValuedDataObject> dataObjects, JToken infoNode)
        {
            bool localizationValuesChanged = false;
            ICommandContext commandContext = Context.CommandContext;
            IDynamicBpmnService dynamicBpmnService = commandContext.ProcessEngineConfiguration.DynamicBpmnService;

            foreach (ValuedDataObject dataObject in dataObjects)
            {
                dataObject.ExtensionElements.TryGetValue("localization", out IList<ExtensionElement> localizationElements);
                if (localizationElements != null)
                {
                    foreach (ExtensionElement localizationElement in localizationElements)
                    {
                        if (org.activiti.bpmn.constants.BpmnXMLConstants.ACTIVITI_EXTENSIONS_PREFIX.Equals(localizationElement.NamespacePrefix))
                        {
                            string locale = localizationElement.getAttributeValue(null, "locale");
                            string name = localizationElement.getAttributeValue(null, "name");
                            string documentation = null;

                            localizationElement.ChildElements.TryGetValue("documentation", out IList<ExtensionElement> documentationElements);
                            if (documentationElements != null)
                            {
                                foreach (ExtensionElement documentationElement in documentationElements)
                                {
                                    documentation = documentationElement.ElementText.Trim();
                                    break;
                                }
                            }

                            if (!string.ReferenceEquals(name, null) && isEqualToCurrentLocalizationValue(locale, dataObject.Id, org.activiti.engine.DynamicBpmnConstants_Fields.LOCALIZATION_NAME, name, infoNode) == false)
                            {
                                dynamicBpmnService.changeLocalizationName(locale, dataObject.Id, name, infoNode);
                                localizationValuesChanged = true;
                            }

                            if (!string.ReferenceEquals(documentation, null) && isEqualToCurrentLocalizationValue(locale, dataObject.Id, org.activiti.engine.DynamicBpmnConstants_Fields.LOCALIZATION_DESCRIPTION, documentation, infoNode) == false)
                            {

                                dynamicBpmnService.changeLocalizationDescription(locale, dataObject.Id, documentation, infoNode);
                                localizationValuesChanged = true;
                            }
                        }
                    }
                }
            }

            return localizationValuesChanged;
        }

        public virtual IIdGenerator IdGenerator
        {
            get
            {
                return idGenerator;
            }
            set
            {
                this.idGenerator = value;
            }
        }


        public virtual ParsedDeploymentBuilderFactory ExParsedDeploymentBuilderFactory
        {
            get
            {
                return parsedDeploymentBuilderFactory;
            }
        }

        public virtual ParsedDeploymentBuilderFactory ParsedDeploymentBuilderFactory
        {
            set
            {
                this.parsedDeploymentBuilderFactory = value;
            }
        }

        public virtual BpmnDeploymentHelper BpmnDeploymentHelper
        {
            get
            {
                return bpmnDeploymentHelper;
            }
            set
            {
                this.bpmnDeploymentHelper = value;
            }
        }


        public virtual CachingAndArtifactsManager CachingAndArtifcatsManager
        {
            get
            {
                return cachingAndArtifactsManager;
            }
        }

        public virtual CachingAndArtifactsManager CachingAndArtifactsManager
        {
            set
            {
                this.cachingAndArtifactsManager = value;
            }
        }
    }

}