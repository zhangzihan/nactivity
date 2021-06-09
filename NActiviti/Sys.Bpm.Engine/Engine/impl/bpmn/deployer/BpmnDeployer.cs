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
namespace Sys.Workflow.Engine.Impl.Bpmn.Deployers
{
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json.Linq;
    using Sys.Workflow.Bpmn.Constants;
    using Sys.Workflow.Bpmn.Models;
    using Sys.Workflow.Engine.Delegate.Events;
    using Sys.Workflow.Engine.Delegate.Events.Impl;
    using Sys.Workflow.Engine.Impl.Cfg;
    using Sys.Workflow.Engine.Impl.Contexts;
    using Sys.Workflow.Engine.Impl.Interceptor;
    using Sys.Workflow.Engine.Impl.Persistence.Deploies;
    using Sys.Workflow.Engine.Impl.Persistence.Entity;
    using Sys.Workflow;

    public class BpmnDeployer : IDeployer
    {
        protected internal IIdGenerator idGenerator;
        protected internal ParsedDeploymentBuilderFactory parsedDeploymentBuilderFactory;
        protected internal BpmnDeploymentHelper bpmnDeploymentHelper;
        protected internal CachingAndArtifactsManager cachingAndArtifactsManager;

        private static readonly ILogger<BpmnDeployer> log = ProcessEngineServiceProvider.LoggerService<BpmnDeployer>();

        public virtual void Deploy(IDeploymentEntity deployment, IDictionary<string, object> deploymentSettings)
        {
            log.LogDebug($"Processing deployment {deployment.Name}");

            // The ParsedDeployment represents the deployment, the process definitions, and the BPMN
            // resource, parse, and model associated with each process definition.
            ParsedDeployment parsedDeployment = parsedDeploymentBuilderFactory.GetBuilderForDeploymentAndSettings(deployment, deploymentSettings).Build(bpmnDeploymentHelper);

            bpmnDeploymentHelper.VerifyProcessDefinitionsDoNotShareKeys(parsedDeployment.AllProcessDefinitions);

            bpmnDeploymentHelper.CopyDeploymentValuesToProcessDefinitions(parsedDeployment.Deployment, parsedDeployment.AllProcessDefinitions);
            bpmnDeploymentHelper.ResourceNamesOnProcessDefinitions(parsedDeployment);

            //    createAndPersistNewDiagramsIfNeeded(parsedDeployment);
            ProcessDefinitionDiagramNames(parsedDeployment);

            if (deployment.New)
            {
                IDictionary<IProcessDefinitionEntity, IProcessDefinitionEntity> mapOfNewProcessDefinitionToPreviousVersion = GetPreviousVersionsOfProcessDefinitions(parsedDeployment);
                SetProcessDefinitionVersionsAndIds(parsedDeployment, mapOfNewProcessDefinitionToPreviousVersion);
                PersistProcessDefinitionsAndAuthorizations(parsedDeployment);
                UpdateTimersAndEvents(parsedDeployment, mapOfNewProcessDefinitionToPreviousVersion);
                DispatchProcessDefinitionEntityInitializedEvent(parsedDeployment);
            }
            else
            {
                MakeProcessDefinitionsConsistentWithPersistedVersions(parsedDeployment);
            }

            cachingAndArtifactsManager.UpdateCachingAndArtifacts(parsedDeployment);

            foreach (IProcessDefinitionEntity processDefinition in parsedDeployment.AllProcessDefinitions)
            {
                BpmnModel bpmnModel = parsedDeployment.GetBpmnModelForProcessDefinition(processDefinition);
                CreateLocalizationValues(processDefinition.Id, bpmnModel.GetProcessById(processDefinition.Key));
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
        //        if (resource is object) {
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
                string diagramResourceName = ResourceNameUtil.GetProcessDiagramResourceNameFromDeployment(processDefinition, resources);
                processDefinition.DiagramResourceName = diagramResourceName;
            }

            return value;
        }

        /// <summary>
        /// Constructs a map from new ProcessDefinitionEntities to the previous version by key and tenant.
        /// If no previous version exists, no map entry is created.
        /// </summary>
        protected internal virtual IDictionary<IProcessDefinitionEntity, IProcessDefinitionEntity> GetPreviousVersionsOfProcessDefinitions(ParsedDeployment parsedDeployment)
        {
            IDictionary<IProcessDefinitionEntity, IProcessDefinitionEntity> result = new Dictionary<IProcessDefinitionEntity, IProcessDefinitionEntity>();

            foreach (IProcessDefinitionEntity newDefinition in parsedDeployment.AllProcessDefinitions)
            {
                IProcessDefinitionEntity existingDefinition = bpmnDeploymentHelper.GetMostRecentVersionOfProcessDefinition(newDefinition);

                if (existingDefinition is object)
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
        protected internal virtual void SetProcessDefinitionVersionsAndIds(ParsedDeployment parsedDeployment, IDictionary<IProcessDefinitionEntity, IProcessDefinitionEntity> mapNewToOldProcessDefinitions)
        {
            ICommandContext commandContext = Context.CommandContext;

            foreach (IProcessDefinitionEntity processDefinition in parsedDeployment.AllProcessDefinitions)
            {
                int version = 1;

                mapNewToOldProcessDefinitions.TryGetValue(processDefinition, out IProcessDefinitionEntity latest);
                if (latest is object)
                {
                    version = latest.Version + 1;
                }

                processDefinition.Version = version;
                processDefinition.Id = GetIdForNewProcessDefinition(processDefinition);

                if (commandContext.ProcessEngineConfiguration.EventDispatcher.Enabled)
                {
                    commandContext.ProcessEngineConfiguration.EventDispatcher.DispatchEvent(ActivitiEventBuilder.CreateEntityEvent(ActivitiEventType.ENTITY_CREATED, processDefinition));
                }
            }
        }

        /// <summary>
        /// Saves each process definition.  It is assumed that the deployment is new, the definitions
        /// have never been saved before, and that they have all their values properly set up.
        /// </summary>
        protected internal virtual void PersistProcessDefinitionsAndAuthorizations(ParsedDeployment parsedDeployment)
        {
            ICommandContext commandContext = Context.CommandContext;
            IProcessDefinitionEntityManager processDefinitionManager = commandContext.ProcessDefinitionEntityManager;

            foreach (IProcessDefinitionEntity processDefinition in parsedDeployment.AllProcessDefinitions)
            {
                processDefinitionManager.Insert(processDefinition, false);
                bpmnDeploymentHelper.AddAuthorizationsForNewProcessDefinition(parsedDeployment.GetProcessModelForProcessDefinition(processDefinition), processDefinition);
            }
        }

        protected internal virtual void UpdateTimersAndEvents(ParsedDeployment parsedDeployment, IDictionary<IProcessDefinitionEntity, IProcessDefinitionEntity> mapNewToOldProcessDefinitions)
        {
            foreach (IProcessDefinitionEntity processDefinition in parsedDeployment.AllProcessDefinitions)
            {
                if (mapNewToOldProcessDefinitions.TryGetValue(processDefinition, out var item))
                {
                    bpmnDeploymentHelper.UpdateTimersAndEvents(processDefinition, item, parsedDeployment);
                }
            }
        }

        protected internal virtual void DispatchProcessDefinitionEntityInitializedEvent(ParsedDeployment parsedDeployment)
        {
            ICommandContext commandContext = Context.CommandContext;
            foreach (IProcessDefinitionEntity processDefinitionEntity in parsedDeployment.AllProcessDefinitions)
            {
                if (commandContext.ProcessEngineConfiguration.EventDispatcher.Enabled)
                {
                    commandContext.ProcessEngineConfiguration.EventDispatcher.DispatchEvent(ActivitiEventBuilder.CreateEntityEvent(ActivitiEventType.ENTITY_INITIALIZED, processDefinitionEntity));
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
        protected internal virtual string GetIdForNewProcessDefinition(IProcessDefinitionEntity processDefinition)
        {
            string nextId = idGenerator.GetNextId();

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
        protected internal virtual void MakeProcessDefinitionsConsistentWithPersistedVersions(ParsedDeployment parsedDeployment)
        {
            foreach (IProcessDefinitionEntity processDefinition in parsedDeployment.AllProcessDefinitions)
            {
                IProcessDefinitionEntity persistedProcessDefinition = bpmnDeploymentHelper.GetPersistedInstanceOfProcessDefinition(processDefinition);

                if (persistedProcessDefinition is object)
                {
                    processDefinition.Id = persistedProcessDefinition.Id;
                    processDefinition.Version = persistedProcessDefinition.Version;
                    processDefinition.SuspensionState = persistedProcessDefinition.SuspensionState;
                }
            }
        }

        protected internal virtual void CreateLocalizationValues(string processDefinitionId, Process process)
        {
            if (process is null)
            {
                return;
            }

            ICommandContext commandContext = Context.CommandContext;
            IDynamicBpmnService dynamicBpmnService = commandContext.ProcessEngineConfiguration.DynamicBpmnService;
            JToken infoNode = dynamicBpmnService.GetProcessDefinitionInfo(processDefinitionId);

            bool localizationValuesChanged = false;
            process.ExtensionElements.TryGetValue("localization", out IList<ExtensionElement> localizationElements);
            if (localizationElements is object)
            {
                foreach (ExtensionElement localizationElement in localizationElements)
                {
                    if (BpmnXMLConstants.ACTIVITI_EXTENSIONS_PREFIX.Equals(localizationElement.NamespacePrefix))
                    {
                        string locale = localizationElement.GetAttributeValue(null, "locale");
                        string name = localizationElement.GetAttributeValue(null, "name");
                        string documentation = null;
                        localizationElement.ChildElements.TryGetValue("documentation", out IList<ExtensionElement> documentationElements);
                        if (documentationElements is object)
                        {
                            foreach (ExtensionElement documentationElement in documentationElements)
                            {
                                documentation = documentationElement.ElementText.Trim();
                                break;
                            }
                        }

                        string processId = process.Id;
                        if (!IsEqualToCurrentLocalizationValue(locale, processId, "name", name, infoNode))
                        {
                            dynamicBpmnService.ChangeLocalizationName(locale, processId, name, infoNode);
                            localizationValuesChanged = true;
                        }

                        if (documentation is object && !IsEqualToCurrentLocalizationValue(locale, processId, "description", documentation, infoNode))
                        {
                            dynamicBpmnService.ChangeLocalizationDescription(locale, processId, documentation, infoNode);
                            localizationValuesChanged = true;
                        }

                        break;
                    }
                }
            }

            bool isFlowElementLocalizationChanged = LocalizeFlowElements(process.FlowElements, infoNode);
            bool isDataObjectLocalizationChanged = LocalizeDataObjectElements(process.DataObjects, infoNode);
            if (isFlowElementLocalizationChanged || isDataObjectLocalizationChanged)
            {
                localizationValuesChanged = true;
            }

            if (localizationValuesChanged)
            {
                dynamicBpmnService.SaveProcessDefinitionInfo(processDefinitionId, infoNode);
            }
        }

        protected internal virtual bool LocalizeFlowElements(ICollection<FlowElement> flowElements, JToken infoNode)
        {
            bool localizationValuesChanged = false;

            if (flowElements is null)
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
                    if (localizationElements is object)
                    {
                        foreach (ExtensionElement localizationElement in localizationElements)
                        {
                            if (BpmnXMLConstants.ACTIVITI_EXTENSIONS_PREFIX.Equals(localizationElement.NamespacePrefix))
                            {
                                string locale = localizationElement.GetAttributeValue(null, "locale");
                                string name = localizationElement.GetAttributeValue(null, "name");
                                string documentation = null;
                                localizationElement.ChildElements.TryGetValue("documentation", out IList<ExtensionElement> documentationElements);
                                if (documentationElements is object)
                                {
                                    foreach (ExtensionElement documentationElement in documentationElements)
                                    {
                                        documentation = documentationElement.ElementText.Trim();
                                        break;
                                    }
                                }

                                string flowElementId = flowElement.Id;
                                if (IsEqualToCurrentLocalizationValue(locale, flowElementId, "name", name, infoNode) == false)
                                {
                                    dynamicBpmnService.ChangeLocalizationName(locale, flowElementId, name, infoNode);
                                    localizationValuesChanged = true;
                                }

                                if (documentation is object && IsEqualToCurrentLocalizationValue(locale, flowElementId, "description", documentation, infoNode) == false)
                                {
                                    dynamicBpmnService.ChangeLocalizationDescription(locale, flowElementId, documentation, infoNode);
                                    localizationValuesChanged = true;
                                }

                                break;
                            }
                        }
                    }

                    if (flowElement is SubProcess subprocess)
                    {
                        bool isFlowElementLocalizationChanged = LocalizeFlowElements(subprocess.FlowElements, infoNode);
                        bool isDataObjectLocalizationChanged = LocalizeDataObjectElements(subprocess.DataObjects, infoNode);
                        if (isFlowElementLocalizationChanged || isDataObjectLocalizationChanged)
                        {
                            localizationValuesChanged = true;
                        }
                    }
                }
            }

            return localizationValuesChanged;
        }

        protected internal virtual bool IsEqualToCurrentLocalizationValue(string language, string id, string propertyName, string propertyValue, JToken infoNode)
        {
            bool isEqual = false;
            JToken localizationNode = infoNode.SelectToken("localization.language.id.propertyName");
            if (localizationNode is object && localizationNode.ToString().Equals(propertyValue))
            {
                isEqual = true;
            }
            return isEqual;
        }

        protected internal virtual bool LocalizeDataObjectElements(IList<ValuedDataObject> dataObjects, JToken infoNode)
        {
            bool localizationValuesChanged = false;
            ICommandContext commandContext = Context.CommandContext;
            IDynamicBpmnService dynamicBpmnService = commandContext.ProcessEngineConfiguration.DynamicBpmnService;

            foreach (ValuedDataObject dataObject in dataObjects)
            {
                dataObject.ExtensionElements.TryGetValue("localization", out IList<ExtensionElement> localizationElements);
                if (localizationElements is object)
                {
                    foreach (ExtensionElement localizationElement in localizationElements)
                    {
                        if (BpmnXMLConstants.ACTIVITI_EXTENSIONS_PREFIX.Equals(localizationElement.NamespacePrefix))
                        {
                            string locale = localizationElement.GetAttributeValue(null, "locale");
                            string name = localizationElement.GetAttributeValue(null, "name");
                            string documentation = null;

                            localizationElement.ChildElements.TryGetValue("documentation", out IList<ExtensionElement> documentationElements);
                            if (documentationElements is object)
                            {
                                foreach (ExtensionElement documentationElement in documentationElements)
                                {
                                    documentation = documentationElement.ElementText.Trim();
                                    break;
                                }
                            }

                            if (name is object && IsEqualToCurrentLocalizationValue(locale, dataObject.Id, DynamicBpmnConstants.LOCALIZATION_NAME, name, infoNode) == false)
                            {
                                dynamicBpmnService.ChangeLocalizationName(locale, dataObject.Id, name, infoNode);
                                localizationValuesChanged = true;
                            }

                            if (documentation is object && IsEqualToCurrentLocalizationValue(locale, dataObject.Id, DynamicBpmnConstants.LOCALIZATION_DESCRIPTION, documentation, infoNode) == false)
                            {

                                dynamicBpmnService.ChangeLocalizationDescription(locale, dataObject.Id, documentation, infoNode);
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