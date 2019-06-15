using System.Collections.Generic;
using System.Text;

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
namespace org.activiti.engine.impl.bpmn.parser
{
    using Microsoft.Extensions.Logging;
    using org.activiti.bpmn.constants;
    using org.activiti.bpmn.converter;
    using org.activiti.bpmn.exceptions;
    using org.activiti.bpmn.model;
    using org.activiti.engine.@delegate.@event.impl;
    using org.activiti.engine.exceptions;
    using org.activiti.engine.impl.bpmn.parser.factory;
    using org.activiti.engine.impl.cfg;
    using org.activiti.engine.impl.context;
    using org.activiti.engine.impl.persistence.entity;
    using org.activiti.engine.impl.util.io;
    using org.activiti.validation;
    using Sys;
    using Sys.Workflow;
    using System;
    using System.Linq;

    /// <summary>
    /// Specific parsing of one BPMN 2.0 XML file, created by the <seealso cref="BpmnParser"/>.
    /// 
    /// 
    /// 
    /// </summary>
    public class BpmnParse : IBpmnXMLConstants
    {
        private static readonly ILogger logger = ProcessEngineServiceProvider.LoggerService<BpmnParse>();

        public const string PROPERTYNAME_INITIAL = "initial";
        public const string PROPERTYNAME_INITIATOR_VARIABLE_NAME = "initiatorVariableName";
        public const string PROPERTYNAME_CONDITION = "condition";
        public const string PROPERTYNAME_CONDITION_TEXT = "conditionText";
        public const string PROPERTYNAME_TIMER_DECLARATION = "timerDeclarations";
        public const string PROPERTYNAME_ISEXPANDED = "isExpanded";
        public const string PROPERTYNAME_START_TIMER = "timerStart";
        public const string PROPERTYNAME_COMPENSATION_HANDLER_ID = "compensationHandler";
        public const string PROPERTYNAME_IS_FOR_COMPENSATION = "isForCompensation";
        public const string PROPERTYNAME_ERROR_EVENT_DEFINITIONS = "errorEventDefinitions";
        public const string PROPERTYNAME_EVENT_SUBSCRIPTION_DECLARATION = "eventDefinitions";

        protected internal string name;

        protected internal bool validateSchema = true;
        protected internal bool validateProcess = true;

        protected internal IStreamSource streamSource;
        protected internal string sourceSystemId;

        protected internal BpmnModel bpmnModel;

        protected internal string targetNamespace;

        /// <summary>
        /// The deployment to which the parsed process definitions will be added. </summary>
        protected internal IDeploymentEntity deployment;

        /// <summary>
        /// The end result of the parsing: a list of process definition. </summary>
        protected internal ISet<IProcessDefinitionEntity> processDefinitions = new HashSet<IProcessDefinitionEntity>();

        /// <summary>
        /// A map for storing sequence flow based on their id during parsing. </summary>
        protected internal IDictionary<string, SequenceFlow> sequenceFlows;

        protected internal BpmnParseHandlers bpmnParserHandlers;

        protected internal IProcessDefinitionEntity currentProcessDefinition;

        protected internal Process currentProcess;

        protected internal FlowElement currentFlowElement;

        protected internal LinkedList<SubProcess> currentSubprocessStack = new LinkedList<SubProcess>();

        /// <summary>
        /// Mapping containing values stored during the first phase of parsing since other elements can reference these messages.
        /// 
        /// All the map's elements are defined outside the process definition(s), which means that this map doesn't need to be re-initialized for each new process definition.
        /// </summary>
        protected internal IDictionary<string, string> prefixs = new Dictionary<string, string>();

        // Factories
        protected internal IActivityBehaviorFactory activityBehaviorFactory;
        protected internal IListenerFactory listenerFactory;

        /// <summary>
        /// Constructor to be called by the <seealso cref="BpmnParser"/>.
        /// </summary>
        public BpmnParse(BpmnParser parser)
        {
            this.activityBehaviorFactory = parser.ActivityBehaviorFactory;
            this.listenerFactory = parser.ListenerFactory;
            this.bpmnParserHandlers = parser.BpmnParserHandlers;
        }

        public virtual BpmnParse Execute()
        {
            try
            {
                ProcessEngineConfigurationImpl processEngineConfiguration = Context.ProcessEngineConfiguration;
                BpmnXMLConverter converter = new BpmnXMLConverter();

                bool enableSafeBpmnXml = false;
                string encoding = null;
                if (processEngineConfiguration != null)
                {
                    enableSafeBpmnXml = processEngineConfiguration.EnableSafeBpmnXml;
                    encoding = processEngineConfiguration.XmlEncoding;
                }

                if (!(encoding is null))
                {
                    bpmnModel = converter.ConvertToBpmnModel(streamSource, validateSchema, enableSafeBpmnXml, encoding);
                }
                else
                {
                    bpmnModel = converter.ConvertToBpmnModel(streamSource, validateSchema, enableSafeBpmnXml);
                }

                // XSD validation goes first, then process/semantic validation
                if (validateProcess)
                {
                    IProcessValidator processValidator = processEngineConfiguration.ProcessValidator;
                    if (processValidator == null)
                    {
                        logger.LogWarning("Process should be validated, but no process validator is configured on the process engine configuration!");
                    }
                    else
                    {
                        IList<ValidationError> validationErrors = processValidator.Validate(bpmnModel);
                        if (validationErrors != null && validationErrors.Count > 0)
                        {

                            StringBuilder warningBuilder = new StringBuilder();
                            StringBuilder errorBuilder = new StringBuilder();

                            foreach (ValidationError error in validationErrors)
                            {
                                if (error.Warning)
                                {
                                    warningBuilder.Append(error.ToString());
                                    warningBuilder.Append("\n");
                                }
                                else
                                {
                                    errorBuilder.Append(error.ToString());
                                    errorBuilder.Append("\n");
                                }
                            }

                            // Throw exception if there is any error
                            if (validationErrors.Any(x => x.Warning == false))
                            {
                                logger.LogError($"Following errors encounted during process validation:\r\n{errorBuilder.ToString()}");

                                throw new ActivitiValidationException(validationErrors);
                            }

                            // Write out warnings (if any)
                            if (warningBuilder.Length > 0)
                            {
                                logger.LogWarning($"Following warnings encountered during process validation: {warningBuilder.ToString()}");
                            }

                        }
                    }
                }

                bpmnModel.SourceSystemId = sourceSystemId;
                bpmnModel.EventSupport = new ActivitiEventSupport();

                // Validation successful (or no validation)

                // Attach logic to the processes (eg. map ActivityBehaviors to bpmn model elements)
                ApplyParseHandlers();

                // Finally, process the diagram interchange info
                ProcessDI();

            }
            catch (Exception e)
            {
                if (e is ActivitiException)
                {
                    throw;
                }
                else if (e is XMLException)
                {
                    throw;
                }
                else if (e is ActivitiValidationException)
                {
                    throw;
                }
                else
                {
                    throw new ActivitiException("Error parsing XML", e);
                }
            }

            return this;
        }

        public virtual BpmnParse SetName(string name)
        {
            this.name = name;
            return this;
        }

        public virtual BpmnParse SourceInputStream(System.IO.Stream inputStream)
        {
            if (name is null)
            {
                SetName("inputStream");
            }
            StreamSource = new InputStreamSource(inputStream);
            return this;
        }

        public virtual BpmnParse SourceResource(string resource)
        {
            return SourceResource(resource, null);
        }

        public virtual BpmnParse SourceUrl(Uri url)
        {
            if (name is null)
            {
                SetName(url.ToString());
            }
            StreamSource = new UrlStreamSource(url);
            return this;
        }

        public virtual BpmnParse SourceUrl(string url)
        {
            try
            {
                return SourceUrl(new Uri(url));
            }
            catch (Exception e)
            {
                throw new ActivitiIllegalArgumentException("malformed url: " + url, e);
            }
        }

        public virtual BpmnParse SourceResource(string resource, ClassLoader classLoader)
        {
            if (name is null)
            {
                SetName(resource);
            }
            StreamSource = new ResourceStreamSource(resource, classLoader);
            return this;
        }

        public virtual BpmnParse SourceString(string @string)
        {
            if (name is null)
            {
                SetName("string");
            }
            StreamSource = new StringStreamSource(@string);
            return this;
        }

        protected internal virtual IStreamSource StreamSource
        {
            set
            {
                if (this.streamSource != null)
                {
                    throw new ActivitiIllegalArgumentException("invalid: multiple sources " + this.streamSource + " and " + value);
                }
                this.streamSource = value;
            }
        }

        public virtual BpmnParse SetSourceSystemId(string sourceSystemId)
        {
            this.sourceSystemId = sourceSystemId;
            return this;
        }

        /// <summary>
        /// Parses the 'definitions' root element
        /// </summary>
        protected internal virtual void ApplyParseHandlers()
        {
            sequenceFlows = new Dictionary<string, SequenceFlow>();
            foreach (Process process in bpmnModel.Processes)
            {
                currentProcess = process;
                if (process.Executable)
                {
                    bpmnParserHandlers.ParseElement(this, process);
                }
            }
        }

        public virtual void ProcessFlowElements(ICollection<FlowElement> flowElements)
        {
            // Parsing the elements is done in a strict order of types,
            // as otherwise certain information might not be available when parsing
            // a certain type.

            // Using lists as we want to keep the order in which they are defined
            IList<SequenceFlow> sequenceFlowToParse = new List<SequenceFlow>();
            IList<BoundaryEvent> boundaryEventsToParse = new List<BoundaryEvent>();

            // Flow elements that depend on other elements are parse after the first run-through
            IList<FlowElement> defferedFlowElementsToParse = new List<FlowElement>();

            // Activities are parsed first
            foreach (FlowElement flowElement in flowElements)
            {
                // Sequence flow are also flow elements, but are only parsed once every activity is found
                if (flowElement is SequenceFlow)
                {
                    sequenceFlowToParse.Add((SequenceFlow)flowElement);
                }
                else if (flowElement is BoundaryEvent)
                {
                    boundaryEventsToParse.Add((BoundaryEvent)flowElement);
                }
                else if (flowElement is Event)
                {
                    defferedFlowElementsToParse.Add(flowElement);
                }
                else
                {
                    bpmnParserHandlers.ParseElement(this, flowElement);
                }
            }

            // Deferred elements
            foreach (FlowElement flowElement in defferedFlowElementsToParse)
            {
                bpmnParserHandlers.ParseElement(this, flowElement);
            }

            // Boundary events are parsed after all the regular activities are parsed
            foreach (BoundaryEvent boundaryEvent in boundaryEventsToParse)
            {
                bpmnParserHandlers.ParseElement(this, boundaryEvent);
            }

            // sequence flows
            foreach (SequenceFlow sequenceFlow in sequenceFlowToParse)
            {
                bpmnParserHandlers.ParseElement(this, sequenceFlow);
            }
        }

        // Diagram interchange
        // /////////////////////////////////////////////////////////////////

        public virtual void ProcessDI()
        {
            if (processDefinitions.Count == 0)
            {
                return;
            }

            if (bpmnModel.LocationMap.Count > 0)
            {
                // Verify if all referenced elements exist
                foreach (string bpmnReference in bpmnModel.LocationMap.Keys)
                {
                    if (bpmnModel.GetFlowElement(bpmnReference) == null)
                    {
                        // ACT-1625: don't warn when artifacts are referenced from DI
                        if (bpmnModel.GetArtifact(bpmnReference) == null)
                        {
                            // Check if it's a Pool or Lane, then DI is ok
                            if (bpmnModel.GetPool(bpmnReference) == null && bpmnModel.GetLane(bpmnReference) == null)
                            {
                                logger.LogWarning($"Invalid reference in diagram interchange definition: could not find {bpmnReference}");
                            }
                        }
                    }
                    else if (!(bpmnModel.GetFlowElement(bpmnReference) is FlowNode))
                    {
                        logger.LogWarning($"Invalid reference in diagram interchange definition: {bpmnReference} does not reference a flow node");
                    }
                }

                foreach (string bpmnReference in bpmnModel.FlowLocationMap.Keys)
                {
                    if (bpmnModel.GetFlowElement(bpmnReference) == null)
                    {
                        // ACT-1625: don't warn when artifacts are referenced from DI
                        if (bpmnModel.GetArtifact(bpmnReference) == null)
                        {
                            logger.LogWarning($"Invalid reference in diagram interchange definition: could not find {bpmnReference}");
                        }
                    }
                    else if (!(bpmnModel.GetFlowElement(bpmnReference) is SequenceFlow))
                    {
                        logger.LogWarning($"Invalid reference in diagram interchange definition: {bpmnReference} does not reference a sequence flow");
                    }
                }

                foreach (Process process in bpmnModel.Processes)
                {
                    if (!process.Executable)
                    {
                        continue;
                    }

                    // Parse diagram interchange information
                    IProcessDefinitionEntity processDefinition = GetProcessDefinition(process.Id);
                    if (processDefinition != null)
                    {
                        processDefinition.IsGraphicalNotationDefined = true;

                        foreach (string edgeId in bpmnModel.FlowLocationMap.Keys)
                        {
                            if (bpmnModel.GetFlowElement(edgeId) != null)
                            {
                                CreateBPMNEdge(edgeId, bpmnModel.GetFlowLocationGraphicInfo(edgeId));
                            }
                        }
                    }
                }
            }
        }

        public virtual void CreateBPMNEdge(string key, IList<GraphicInfo> graphicList)
        {
            FlowElement flowElement = bpmnModel.GetFlowElement(key);
            if (flowElement is SequenceFlow sequenceFlow)
            {
                IList<int> waypoints = new List<int>();
                foreach (GraphicInfo waypointInfo in graphicList)
                {
                    waypoints.Add((int)waypointInfo.X);
                    waypoints.Add((int)waypointInfo.Y);
                }
                sequenceFlow.Waypoints = waypoints;
            }
            else if (bpmnModel.GetArtifact(key) != null)
            {
                // it's an association, so nothing to do
            }
            else
            {
                logger.LogWarning($"Invalid reference in 'bpmnElement' attribute, sequenceFlow {key} not found");
            }
        }

        public virtual IProcessDefinitionEntity GetProcessDefinition(string processDefinitionKey)
        {
            foreach (IProcessDefinitionEntity processDefinition in processDefinitions)
            {
                if (processDefinition.Key.Equals(processDefinitionKey))
                {
                    return processDefinition;
                }
            }
            return null;
        }

        /*
         * ------------------- GETTERS AND SETTERS -------------------
         */
        public virtual bool ValidateSchema
        {
            get
            {
                return validateSchema;
            }
            set
            {
                this.validateSchema = value;
            }
        }


        public virtual bool ValidateProcess
        {
            get
            {
                return validateProcess;
            }
            set
            {
                this.validateProcess = value;
            }
        }


        public virtual ISet<IProcessDefinitionEntity> ProcessDefinitions
        {
            get
            {
                return processDefinitions;
            }
        }

        public virtual string TargetNamespace
        {
            get
            {
                return targetNamespace;
            }
        }

        public virtual BpmnParseHandlers BpmnParserHandlers
        {
            get
            {
                return bpmnParserHandlers;
            }
            set
            {
                this.bpmnParserHandlers = value;
            }
        }


        public virtual IDeploymentEntity Deployment
        {
            get
            {
                return deployment;
            }
            set
            {
                this.deployment = value;
            }
        }


        public virtual BpmnModel BpmnModel
        {
            get
            {
                return bpmnModel;
            }
            set
            {
                this.bpmnModel = value;
            }
        }


        public virtual IActivityBehaviorFactory ActivityBehaviorFactory
        {
            get
            {
                return activityBehaviorFactory;
            }
            set
            {
                this.activityBehaviorFactory = value;
            }
        }


        public virtual IListenerFactory ListenerFactory
        {
            get
            {
                return listenerFactory;
            }
            set
            {
                this.listenerFactory = value;
            }
        }


        public virtual IDictionary<string, SequenceFlow> SequenceFlows
        {
            get
            {
                return sequenceFlows;
            }
        }

        public virtual IProcessDefinitionEntity CurrentProcessDefinition
        {
            get
            {
                return currentProcessDefinition;
            }
            set
            {
                this.currentProcessDefinition = value;
            }
        }


        public virtual FlowElement CurrentFlowElement
        {
            get
            {
                return currentFlowElement;
            }
            set
            {
                this.currentFlowElement = value;
            }
        }


        public virtual Process CurrentProcess
        {
            get
            {
                return currentProcess;
            }
            set
            {
                this.currentProcess = value;
            }
        }


        public virtual SubProcess CurrentSubProcess
        {
            set
            {
                currentSubprocessStack.AddLast(value);
            }
            get
            {
                return currentSubprocessStack.First.Value;
            }
        }


        public virtual void RemoveCurrentSubProcess()
        {
            currentSubprocessStack.Remove(CurrentSubProcess);
        }
    }
}