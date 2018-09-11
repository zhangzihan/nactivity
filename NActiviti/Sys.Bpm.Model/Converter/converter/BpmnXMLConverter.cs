using org.activiti.bpmn.constants;
using org.activiti.bpmn.converter.alfresco;
using org.activiti.bpmn.converter.child;
using org.activiti.bpmn.converter.export;
using org.activiti.bpmn.converter.parser;
using org.activiti.bpmn.converter.util;
using org.activiti.bpmn.exceptions;
using org.activiti.bpmn.model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

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
namespace org.activiti.bpmn.converter
{
    /// 
    /// 
    public class BpmnXMLConverter : IBpmnXMLConstants
    {
        protected internal const string BPMN_XSD = "org/activiti/impl/bpmn/parser/BPMN20.xsd";
        protected internal const string DEFAULT_ENCODING = "UTF-8";

        protected internal static IDictionary<string, BaseBpmnXMLConverter> convertersToBpmnMap = new Dictionary<string, BaseBpmnXMLConverter>();
        protected internal static IDictionary<Type, BaseBpmnXMLConverter> convertersToXMLMap = new Dictionary<Type, BaseBpmnXMLConverter>();

        protected internal IList<string> userTaskFormTypes;
        protected internal IList<string> startEventFormTypes;

        protected internal BpmnEdgeParser bpmnEdgeParser = new BpmnEdgeParser();
        protected internal BpmnShapeParser bpmnShapeParser = new BpmnShapeParser();
        protected internal DefinitionsParser definitionsParser = new DefinitionsParser();
        protected internal DocumentationParser documentationParser = new DocumentationParser();
        protected internal ExtensionElementsParser extensionElementsParser = new ExtensionElementsParser();
        protected internal ImportParser importParser = new ImportParser();
        protected internal InterfaceParser interfaceParser = new InterfaceParser();
        protected internal ItemDefinitionParser itemDefinitionParser = new ItemDefinitionParser();
        protected internal IOSpecificationParser ioSpecificationParser = new IOSpecificationParser();
        protected internal DataStoreParser dataStoreParser = new DataStoreParser();
        protected internal LaneParser laneParser = new LaneParser();
        protected internal MessageParser messageParser = new MessageParser();
        protected internal MessageFlowParser messageFlowParser = new MessageFlowParser();
        protected internal MultiInstanceParser multiInstanceParser = new MultiInstanceParser();
        protected internal ParticipantParser participantParser = new ParticipantParser();
        protected internal PotentialStarterParser potentialStarterParser = new PotentialStarterParser();
        protected internal ProcessParser processParser = new ProcessParser();
        protected internal ResourceParser resourceParser = new ResourceParser();
        protected internal SignalParser signalParser = new SignalParser();
        protected internal SubProcessParser subProcessParser = new SubProcessParser();

        static BpmnXMLConverter()
        {
            // events
            addConverter(new EndEventXMLConverter());
            addConverter(new StartEventXMLConverter());

            // tasks
            addConverter(new BusinessRuleTaskXMLConverter());
            addConverter(new ManualTaskXMLConverter());
            addConverter(new ReceiveTaskXMLConverter());
            addConverter(new ScriptTaskXMLConverter());
            addConverter(new ServiceTaskXMLConverter());
            addConverter(new SendTaskXMLConverter());
            addConverter(new UserTaskXMLConverter());
            addConverter(new TaskXMLConverter());
            addConverter(new CallActivityXMLConverter());

            // gateways
            addConverter(new EventGatewayXMLConverter());
            addConverter(new ExclusiveGatewayXMLConverter());
            addConverter(new InclusiveGatewayXMLConverter());
            addConverter(new ParallelGatewayXMLConverter());
            addConverter(new ComplexGatewayXMLConverter());

            // connectors
            addConverter(new SequenceFlowXMLConverter());

            // catch, throw and boundary event
            addConverter(new CatchEventXMLConverter());
            addConverter(new ThrowEventXMLConverter());
            addConverter(new BoundaryEventXMLConverter());

            // artifacts
            addConverter(new TextAnnotationXMLConverter());
            addConverter(new AssociationXMLConverter());

            // data store reference
            addConverter(new DataStoreReferenceXMLConverter());

            // data objects
            addConverter(new ValuedDataObjectXMLConverter(), typeof(StringDataObject));
            addConverter(new ValuedDataObjectXMLConverter(), typeof(BooleanDataObject));
            addConverter(new ValuedDataObjectXMLConverter(), typeof(IntegerDataObject));
            addConverter(new ValuedDataObjectXMLConverter(), typeof(LongDataObject));
            addConverter(new ValuedDataObjectXMLConverter(), typeof(DoubleDataObject));
            addConverter(new ValuedDataObjectXMLConverter(), typeof(DateDataObject));

            // Alfresco types
            addConverter(new AlfrescoStartEventXMLConverter());
            addConverter(new AlfrescoUserTaskXMLConverter());
        }

        public static void addConverter(BaseBpmnXMLConverter converter)
        {
            addConverter(converter, converter.BpmnElementType);
        }

        public static void addConverter(BaseBpmnXMLConverter converter, Type elementType)
        {
            convertersToBpmnMap[converter.XMLElementName] = converter;
            convertersToXMLMap[elementType] = converter;
        }

        public virtual IList<string> UserTaskFormTypes
        {
            set
            {
                this.userTaskFormTypes = value;
            }
        }

        public virtual IList<string> StartEventFormTypes
        {
            set
            {
                this.startEventFormTypes = value;
            }
        }

        public virtual void validateModel(IInputStreamProvider inputStreamProvider)
        {
            Schema schema = createSchema();

            //Validator validator = schema.newValidator();
            //validator.validate(new StreamSource(inputStreamProvider.InputStream));
        }

        public virtual void validateModel(XMLStreamReader xmlStreamReader)
        {
            Schema schema = createSchema();

            //Validator validator = schema.newValidator();
            //validator.validate(new StAXSource(xmlStreamReader));
        }

        protected internal virtual Schema createSchema()
        {
            throw new NotImplementedException();
            //SchemaFactory factory = SchemaFactory.newInstance(XMLConstants.W3C_XML_SCHEMA_NS_URI);
            //Schema schema = null;
            //if (classloader != null)
            //{
            //    schema = factory.newSchema(classloader.getResource(BPMN_XSD));
            //}

            //if (schema == null)
            //{
            //    schema = factory.newSchema(typeof(BpmnXMLConverter).ClassLoader.getResource(BPMN_XSD));
            //}

            //if (schema == null)
            //{
            //    throw new XMLException("BPMN XSD could not be found");
            //}
            //return schema;
        }

        public virtual BpmnModel convertToBpmnModel(IInputStreamProvider inputStreamProvider, bool validateSchema, bool enableSafeBpmnXml)
        {
            return convertToBpmnModel(inputStreamProvider, validateSchema, enableSafeBpmnXml, DEFAULT_ENCODING);
        }

        public virtual BpmnModel convertToBpmnModel(IInputStreamProvider inputStreamProvider, bool validateSchema, bool enableSafeBpmnXml, string encoding)
        {
            inputStreamProvider.InputStream.Seek(0, System.IO.SeekOrigin.Begin);

            return convertToBpmnModel(new XMLStreamReader(inputStreamProvider.InputStream));
        }

        public virtual BpmnModel convertToBpmnModel(XMLStreamReader xtr)
        {
            BpmnModel model = new BpmnModel();
            model.StartEventFormTypes = startEventFormTypes;
            model.UserTaskFormTypes = userTaskFormTypes;
            try
            {
                Process activeProcess = null;
                IList<SubProcess> activeSubProcessList = new List<SubProcess>();
                while (xtr.hasNext())
                {
                    // xtr.next();

                    if (xtr.EndElement && (BpmnXMLConstants.ELEMENT_SUBPROCESS.Equals(xtr.LocalName) || BpmnXMLConstants.ELEMENT_TRANSACTION.Equals(xtr.LocalName) || BpmnXMLConstants.ELEMENT_ADHOC_SUBPROCESS.Equals(xtr.LocalName)))
                    {
                        activeSubProcessList.RemoveAt(activeSubProcessList.Count - 1);
                    }

                    if (!xtr.StartElement)
                    {
                        if (constants.BpmnXMLConstants.ELEMENT_DI_DIAGRAM == xtr.LocalName)
                        {
                            xtr.Skip();
                        }
                        else
                        {
                            continue;
                        }
                    }

                    if (BpmnXMLConstants.ELEMENT_DEFINITIONS.Equals(xtr.LocalName))
                    {
                        definitionsParser.parse(xtr, model);

                    }
                    else if (BpmnXMLConstants.ELEMENT_RESOURCE.Equals(xtr.LocalName))
                    {
                        resourceParser.parse(xtr, model);

                    }
                    else if (BpmnXMLConstants.ELEMENT_SIGNAL.Equals(xtr.LocalName))
                    {
                        signalParser.parse(xtr, model);

                    }
                    else if (BpmnXMLConstants.ELEMENT_MESSAGE.Equals(xtr.LocalName))
                    {
                        messageParser.parse(xtr, model);

                    }
                    else if (BpmnXMLConstants.ELEMENT_ERROR.Equals(xtr.LocalName))
                    {

                        if (!string.IsNullOrWhiteSpace(xtr.getAttributeValue(BpmnXMLConstants.ATTRIBUTE_ID)))
                        {
                            model.addError(xtr.getAttributeValue(BpmnXMLConstants.ATTRIBUTE_ID), xtr.getAttributeValue(BpmnXMLConstants.ATTRIBUTE_ERROR_CODE));
                        }

                    }
                    else if (BpmnXMLConstants.ELEMENT_IMPORT.Equals(xtr.LocalName))
                    {
                        importParser.parse(xtr, model);

                    }
                    else if (BpmnXMLConstants.ELEMENT_ITEM_DEFINITION.Equals(xtr.LocalName))
                    {
                        itemDefinitionParser.parse(xtr, model);

                    }
                    else if (BpmnXMLConstants.ELEMENT_DATA_STORE.Equals(xtr.LocalName))
                    {
                        dataStoreParser.parse(xtr, model);

                    }
                    else if (BpmnXMLConstants.ELEMENT_INTERFACE.Equals(xtr.LocalName))
                    {
                        interfaceParser.parse(xtr, model);

                    }
                    else if (BpmnXMLConstants.ELEMENT_IOSPECIFICATION.Equals(xtr.LocalName))
                    {
                        ioSpecificationParser.parseChildElement(xtr, activeProcess, model);

                    }
                    else if (BpmnXMLConstants.ELEMENT_PARTICIPANT.Equals(xtr.LocalName))
                    {
                        participantParser.parse(xtr, model);

                    }
                    else if (BpmnXMLConstants.ELEMENT_MESSAGE_FLOW.Equals(xtr.LocalName))
                    {
                        messageFlowParser.parse(xtr, model);

                    }
                    else if (BpmnXMLConstants.ELEMENT_PROCESS.Equals(xtr.LocalName))
                    {

                        Process process = processParser.parse(xtr, model);
                        if (process != null)
                        {
                            activeProcess = process;
                        }

                    }
                    else if (BpmnXMLConstants.ELEMENT_POTENTIAL_STARTER.Equals(xtr.LocalName))
                    {
                        potentialStarterParser.parse(xtr, activeProcess);

                    }
                    else if (BpmnXMLConstants.ELEMENT_LANE.Equals(xtr.LocalName))
                    {
                        laneParser.parse(xtr, activeProcess, model);

                    }
                    else if (BpmnXMLConstants.ELEMENT_DOCUMENTATION.Equals(xtr.LocalName))
                    {

                        BaseElement parentElement = null;
                        if (activeSubProcessList.Count > 0)
                        {
                            parentElement = activeSubProcessList[activeSubProcessList.Count - 1];
                        }
                        else if (activeProcess != null)
                        {
                            parentElement = activeProcess;
                        }
                        documentationParser.parseChildElement(xtr, parentElement, model);

                    }
                    else if (activeProcess == null && BpmnXMLConstants.ELEMENT_TEXT_ANNOTATION.Equals(xtr.LocalName))
                    {
                        string elementId = xtr.getAttributeValue(BpmnXMLConstants.ATTRIBUTE_ID);
                        TextAnnotation textAnnotation = (TextAnnotation)(new TextAnnotationXMLConverter()).convertXMLToElement(xtr, model);
                        textAnnotation.Id = elementId;
                        model.GlobalArtifacts.Add(textAnnotation);

                    }
                    else if (activeProcess == null && BpmnXMLConstants.ELEMENT_ASSOCIATION.Equals(xtr.LocalName))
                    {
                        string elementId = xtr.getAttributeValue(BpmnXMLConstants.ATTRIBUTE_ID);
                        Association association = (Association)(new AssociationXMLConverter()).convertXMLToElement(xtr, model);
                        association.Id = elementId;
                        model.GlobalArtifacts.Add(association);

                    }
                    else if (BpmnXMLConstants.ELEMENT_EXTENSIONS.Equals(xtr.LocalName))
                    {
                        extensionElementsParser.parse(xtr, activeSubProcessList, activeProcess, model);

                    }
                    else if (BpmnXMLConstants.ELEMENT_SUBPROCESS.Equals(xtr.LocalName) || BpmnXMLConstants.ELEMENT_TRANSACTION.Equals(xtr.LocalName) || BpmnXMLConstants.ELEMENT_ADHOC_SUBPROCESS.Equals(xtr.LocalName))
                    {
                        subProcessParser.parse(xtr, activeSubProcessList, activeProcess);

                    }
                    else if (BpmnXMLConstants.ELEMENT_COMPLETION_CONDITION.Equals(xtr.LocalName))
                    {
                        if (activeSubProcessList.Count > 0)
                        {
                            SubProcess subProcess = activeSubProcessList[activeSubProcessList.Count - 1];
                            if (subProcess is AdhocSubProcess)
                            {
                                AdhocSubProcess adhocSubProcess = (AdhocSubProcess)subProcess;
                                adhocSubProcess.CompletionCondition = xtr.ElementText;
                            }
                        }

                    }
                    else if (BpmnXMLConstants.ELEMENT_DI_SHAPE.Equals(xtr.LocalName))
                    {
                        bpmnShapeParser.parse(xtr, model);

                    }
                    else if (BpmnXMLConstants.ELEMENT_DI_EDGE.Equals(xtr.LocalName))
                    {
                        bpmnEdgeParser.parse(xtr, model);

                    }
                    else
                    {

                        if (activeSubProcessList.Count > 0 && BpmnXMLConstants.ELEMENT_MULTIINSTANCE.Equals(xtr.LocalName, StringComparison.CurrentCultureIgnoreCase))
                        {

                            multiInstanceParser.parseChildElement(xtr, activeSubProcessList[activeSubProcessList.Count - 1], model);

                        }
                        else if (convertersToBpmnMap.ContainsKey(xtr.LocalName))
                        {
                            if (activeProcess != null)
                            {
                                BaseBpmnXMLConverter converter = convertersToBpmnMap[xtr.LocalName];
                                converter.convertToBpmnModel(xtr, model, activeProcess, activeSubProcessList);
                            }
                        }
                    }
                    //try
                    //{
                    //    xtr.next();
                    //}
                    //catch (Exception e)
                    //{
                    //    //LOGGER.debug("Error reading XML document", e);
                    //    throw new XMLException("Error reading XML", e);
                    //}
                }

                foreach (Process process in model.Processes)
                {
                    foreach (Pool pool in model.Pools)
                    {
                        if (process.Id.Equals(pool.ProcessRef))
                        {
                            pool.Executable = process.Executable;
                        }
                    }
                    processFlowElements(process.FlowElements, process);
                }

            }
            catch (XMLException e)
            {
                throw e;

            }
            catch (Exception e)
            {
                //LOGGER.error("Error processing BPMN document", e);
                throw new XMLException("Error processing BPMN document", e);
            }
            return model;
        }

        public virtual BpmnModel convertToBpmnModel(string docfile)
        {
            return convertToBpmnModel(new XMLStreamReader(File.Open(docfile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)));
        }

        protected internal virtual void processFlowElements(ICollection<FlowElement> flowElementList, BaseElement parentScope)
        {
            foreach (FlowElement flowElement in flowElementList)
            {
                if (flowElement is SequenceFlow)
                {
                    SequenceFlow sequenceFlow = (SequenceFlow)flowElement;
                    FlowNode sourceNode = getFlowNodeFromScope(sequenceFlow.SourceRef, parentScope);
                    if (sourceNode != null)
                    {
                        sourceNode.OutgoingFlows.Add(sequenceFlow);
                        sequenceFlow.SourceFlowElement = sourceNode;
                    }

                    FlowNode targetNode = getFlowNodeFromScope(sequenceFlow.TargetRef, parentScope);
                    if (targetNode != null)
                    {
                        targetNode.IncomingFlows.Add(sequenceFlow);
                        sequenceFlow.TargetFlowElement = targetNode;
                    }

                }
                else if (flowElement is BoundaryEvent)
                {
                    BoundaryEvent boundaryEvent = (BoundaryEvent)flowElement;
                    FlowElement attachedToElement = getFlowNodeFromScope(boundaryEvent.AttachedToRefId, parentScope);
                    if (attachedToElement is Activity)
                    {
                        Activity attachedActivity = (Activity)attachedToElement;
                        boundaryEvent.AttachedToRef = attachedActivity;
                        attachedActivity.BoundaryEvents.Add(boundaryEvent);
                    }

                }
                else if (flowElement is SubProcess)
                {
                    SubProcess subProcess = (SubProcess)flowElement;
                    processFlowElements(subProcess.FlowElements, subProcess);
                }
            }
        }

        protected internal virtual FlowNode getFlowNodeFromScope(string elementId, BaseElement scope)
        {
            FlowNode flowNode = null;
            if (!string.IsNullOrWhiteSpace(elementId))
            {
                if (scope is Process)
                {
                    flowNode = (FlowNode)((Process)scope).getFlowElement(elementId);
                }
                else if (scope is SubProcess)
                {
                    flowNode = (FlowNode)((SubProcess)scope).getFlowElement(elementId);
                }
            }
            return flowNode;
        }

        public virtual byte[] convertToXML(BpmnModel model)
        {
            return convertToXML(model, DEFAULT_ENCODING);
        }

        public virtual byte[] convertToXML(BpmnModel model, string encoding)
        {
            throw new NotImplementedException();
            //try
            //{

            //    System.IO.MemoryStream outputStream = new System.IO.MemoryStream();

            //    XMLOutputFactory xof = XMLOutputFactory.newInstance();
            //    System.IO.StreamWriter @out = new System.IO.StreamWriter(outputStream, encoding);

            //    XMLStreamWriter writer = xof.createXMLStreamWriter(@out);
            //    XMLStreamWriter xtw = new IndentingXMLStreamWriter(writer);

            //    DefinitionsRootExport.writeRootElement(model, xtw, encoding);
            //    CollaborationExport.writePools(model, xtw);
            //    DataStoreExport.writeDataStores(model, xtw);
            //    SignalAndMessageDefinitionExport.writeSignalsAndMessages(model, xtw);

            //    foreach (Process process in model.Processes)
            //    {

            //        if (process.FlowElements.Empty && process.Lanes.Empty)
            //        {
            //            // empty process, ignore it
            //            continue;
            //        }

            //        ProcessExport.writeProcess(process, xtw);

            //        foreach (FlowElement flowElement in process.FlowElements)
            //        {
            //            createXML(flowElement, model, xtw);
            //        }

            //        foreach (Artifact artifact in process.Artifacts)
            //        {
            //            createXML(artifact, model, xtw);
            //        }

            //        // end process element
            //        xtw.writeEndElement();
            //    }

            //    BPMNDIExport.writeBPMNDI(model, xtw);

            //    // end definitions root element
            //    xtw.writeEndElement();
            //    xtw.writeEndDocument();

            //    xtw.flush();

            //    outputStream.Close();

            //    xtw.close();

            //    return outputStream.toByteArray();

            //}
            //catch (Exception e)
            //{
            //    LOGGER.error("Error writing BPMN XML", e);
            //    throw new XMLException("Error writing BPMN XML", e);
            //}
        }

        public virtual void createXML(FlowElement flowElement, BpmnModel model, XMLStreamWriter xtw)
        {

            if (flowElement is SubProcess)
            {

                SubProcess subProcess = (SubProcess)flowElement;
                if (flowElement is Transaction)
                {
                    xtw.writeStartElement(BpmnXMLConstants.ELEMENT_TRANSACTION);
                }
                else if (flowElement is AdhocSubProcess)
                {
                    xtw.writeStartElement(BpmnXMLConstants.ELEMENT_ADHOC_SUBPROCESS);
                }
                else
                {
                    xtw.writeStartElement(BpmnXMLConstants.ELEMENT_SUBPROCESS);
                }

                xtw.writeAttribute(BpmnXMLConstants.ATTRIBUTE_ID, subProcess.Id);
                if (!string.IsNullOrWhiteSpace(subProcess.Name))
                {
                    xtw.writeAttribute(BpmnXMLConstants.ATTRIBUTE_NAME, subProcess.Name);
                }
                else
                {
                    xtw.writeAttribute(BpmnXMLConstants.ATTRIBUTE_NAME, "subProcess");
                }

                if (subProcess is EventSubProcess)
                {
                    xtw.writeAttribute(BpmnXMLConstants.ATTRIBUTE_TRIGGERED_BY, BpmnXMLConstants.ATTRIBUTE_VALUE_TRUE);

                }
                else if (!(subProcess is Transaction))
                {
                    if (subProcess.Asynchronous)
                    {
                        BpmnXMLUtil.writeQualifiedAttribute(BpmnXMLConstants.ATTRIBUTE_ACTIVITY_ASYNCHRONOUS, BpmnXMLConstants.ATTRIBUTE_VALUE_TRUE, xtw);
                        if (subProcess.NotExclusive)
                        {
                            BpmnXMLUtil.writeQualifiedAttribute(BpmnXMLConstants.ATTRIBUTE_ACTIVITY_EXCLUSIVE, BpmnXMLConstants.ATTRIBUTE_VALUE_FALSE, xtw);
                        }
                    }

                }
                else if (subProcess is AdhocSubProcess)
                {
                    AdhocSubProcess adhocSubProcess = (AdhocSubProcess)subProcess;
                    BpmnXMLUtil.writeDefaultAttribute(BpmnXMLConstants.ATTRIBUTE_CANCEL_REMAINING_INSTANCES, adhocSubProcess.CancelRemainingInstances.ToString(), xtw);
                    if (!string.IsNullOrWhiteSpace(adhocSubProcess.Ordering))
                    {
                        BpmnXMLUtil.writeDefaultAttribute(BpmnXMLConstants.ATTRIBUTE_ORDERING, adhocSubProcess.Ordering, xtw);
                    }
                }

                if (!string.IsNullOrWhiteSpace(subProcess.Documentation))
                {

                    xtw.writeStartElement(BpmnXMLConstants.ELEMENT_DOCUMENTATION);
                    xtw.writeCharacters(subProcess.Documentation);
                    xtw.writeEndElement();
                }

                bool didWriteExtensionStartElement = ActivitiListenerExport.writeListeners(subProcess, false, xtw);

                didWriteExtensionStartElement = BpmnXMLUtil.writeExtensionElements(subProcess, didWriteExtensionStartElement, model.Namespaces, xtw);
                if (didWriteExtensionStartElement)
                {
                    // closing extensions element
                    xtw.writeEndElement();
                }

                MultiInstanceExport.writeMultiInstance(subProcess, xtw);

                if (subProcess is AdhocSubProcess)
                {
                    AdhocSubProcess adhocSubProcess = (AdhocSubProcess)subProcess;
                    if (!string.IsNullOrWhiteSpace(adhocSubProcess.CompletionCondition))
                    {
                        xtw.writeStartElement(BpmnXMLConstants.ELEMENT_COMPLETION_CONDITION);
                        xtw.writeCData(adhocSubProcess.CompletionCondition);
                        xtw.writeEndElement();
                    }
                }

                foreach (FlowElement subElement in subProcess.FlowElements)
                {
                    createXML(subElement, model, xtw);
                }

                foreach (Artifact artifact in subProcess.Artifacts)
                {
                    createXML(artifact, model, xtw);
                }

                xtw.writeEndElement();

            }
            else
            {

                BaseBpmnXMLConverter converter = convertersToXMLMap[flowElement.GetType()];

                if (converter == null)
                {
                    throw new XMLException("No converter for " + flowElement.GetType() + " found");
                }

                converter.convertToXML(xtw, flowElement, model);
            }
        }

        protected internal virtual void createXML(Artifact artifact, BpmnModel model, XMLStreamWriter xtw)
        {

            BaseBpmnXMLConverter converter = convertersToXMLMap[artifact.GetType()];

            if (converter == null)
            {
                throw new XMLException("No converter for " + artifact.GetType() + " found");
            }

            converter.convertToXML(xtw, artifact, model);
        }
    }
}