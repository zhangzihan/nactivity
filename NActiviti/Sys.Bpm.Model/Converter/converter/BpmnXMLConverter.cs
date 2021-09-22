using Microsoft.Extensions.Logging;
using Sys.Workflow.Bpmn.Constants;
using Sys.Workflow.Bpmn.Converters.Alfresco;
using Sys.Workflow.Bpmn.Converters.Childs;
using Sys.Workflow.Bpmn.Converters.Exports;
using Sys.Workflow.Bpmn.Converters.Parsers;
using Sys.Workflow.Bpmn.Converters.Utils;
using Sys.Workflow.Bpmn.Exceptions;
using Sys.Workflow.Bpmn.Models;
using Sys.Workflow;
using Sys.Workflow.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

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
namespace Sys.Workflow.Bpmn.Converters
{
    /// 
    /// 
    public class BpmnXMLConverter : IBpmnXMLConstants
    {
        private static readonly ILogger log = BpmnModelLoggerFactory.LoggerService<BpmnXMLConverter>();

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
            AddConverter(new EndEventXMLConverter());
            AddConverter(new StartEventXMLConverter());

            // tasks
            AddConverter(new BusinessRuleTaskXMLConverter());
            AddConverter(new ManualTaskXMLConverter());
            AddConverter(new ReceiveTaskXMLConverter());
            AddConverter(new ScriptTaskXMLConverter());
            AddConverter(new ServiceTaskXMLConverter());
            AddConverter(new SendTaskXMLConverter());
            AddConverter(new UserTaskXMLConverter());
            AddConverter(new TaskXMLConverter());
            AddConverter(new CallActivityXMLConverter());

            // gateways
            AddConverter(new EventGatewayXMLConverter());
            AddConverter(new ExclusiveGatewayXMLConverter());
            AddConverter(new InclusiveGatewayXMLConverter());
            AddConverter(new ParallelGatewayXMLConverter());
            AddConverter(new ComplexGatewayXMLConverter());

            // connectors
            AddConverter(new SequenceFlowXMLConverter());

            // catch, throw and boundary event
            AddConverter(new CatchEventXMLConverter());
            AddConverter(new ThrowEventXMLConverter());
            AddConverter(new BoundaryEventXMLConverter());

            // artifacts
            AddConverter(new TextAnnotationXMLConverter());
            AddConverter(new AssociationXMLConverter());

            // data store reference
            AddConverter(new DataStoreReferenceXMLConverter());

            // data objects
            AddConverter(new ValuedDataObjectXMLConverter(), typeof(StringDataObject));
            AddConverter(new ValuedDataObjectXMLConverter(), typeof(BooleanDataObject));
            AddConverter(new ValuedDataObjectXMLConverter(), typeof(IntegerDataObject));
            AddConverter(new ValuedDataObjectXMLConverter(), typeof(LongDataObject));
            AddConverter(new ValuedDataObjectXMLConverter(), typeof(DoubleDataObject));
            AddConverter(new ValuedDataObjectXMLConverter(), typeof(DateDataObject));

            // Alfresco types
            AddConverter(new AlfrescoStartEventXMLConverter());
            AddConverter(new AlfrescoUserTaskXMLConverter());
        }

        public static void AddConverter(BaseBpmnXMLConverter converter)
        {
            AddConverter(converter, converter.BpmnElementType);
        }

        public static void AddConverter(BaseBpmnXMLConverter converter, Type elementType)
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

        public virtual void ValidateModel(IInputStreamProvider inputStreamProvider)
        {
            Schema schema = CreateSchema();

            //Validator validator = schema.newValidator();
            //validator.validate(new StreamSource(inputStreamProvider.InputStream));
        }

        public virtual void ValidateModel(XMLStreamReader xmlStreamReader)
        {
            Schema schema = CreateSchema();

            //Validator validator = schema.newValidator();
            //validator.validate(new StAXSource(xmlStreamReader));
        }

        protected internal virtual Schema CreateSchema()
        {
            throw new NotImplementedException();
            //SchemaFactory factory = SchemaFactory.newInstance(XMLConstants.W3C_XML_SCHEMA_NS_URI);
            //Schema schema = null;
            //if (classloader is object)
            //{
            //    schema = factory.newSchema(classloader.getResource(BPMN_XSD));
            //}

            //if (schema is null)
            //{
            //    schema = factory.newSchema(typeof(BpmnXMLConverter).ClassLoader.getResource(BPMN_XSD));
            //}

            //if (schema is null)
            //{
            //    throw new XMLException("BPMN XSD could not be found");
            //}
            //return schema;
        }

        public virtual BpmnModel ConvertToBpmnModel(IInputStreamProvider inputStreamProvider, bool validateSchema, bool enableSafeBpmnXml)
        {
            return ConvertToBpmnModel(inputStreamProvider, validateSchema, enableSafeBpmnXml, DEFAULT_ENCODING);
        }

        public virtual BpmnModel ConvertToBpmnModel(IInputStreamProvider inputStreamProvider, bool validateSchema, bool enableSafeBpmnXml, string encoding)
        {
            inputStreamProvider.InputStream.Seek(0, System.IO.SeekOrigin.Begin);

            return ConvertToBpmnModel(new XMLStreamReader(inputStreamProvider.InputStream));
        }

        public virtual BpmnModel ConvertToBpmnModel(Stream stream)
        {
            return ConvertToBpmnModel(new XMLStreamReader(stream));
        }

        public virtual BpmnModel ConvertToBpmnModel(XMLStreamReader xtr)
        {
            BpmnModel model = new BpmnModel
            {
                StartEventFormTypes = startEventFormTypes,
                UserTaskFormTypes = userTaskFormTypes
            };
            try
            {
                Process activeProcess = null;
                IList<SubProcess> activeSubProcessList = new List<SubProcess>();
                while (xtr.HasNext())
                {
                    // xtr.next();
                    if (xtr.EndElement && (BpmnXMLConstants.ELEMENT_SUBPROCESS.Equals(xtr.LocalName) || BpmnXMLConstants.ELEMENT_TRANSACTION.Equals(xtr.LocalName) || BpmnXMLConstants.ELEMENT_ADHOC_SUBPROCESS.Equals(xtr.LocalName)))
                    {
                        activeSubProcessList.RemoveAt(activeSubProcessList.Count - 1);
                    }

                    if (!xtr.IsStartElement())
                    {
                        if (BpmnXMLConstants.ELEMENT_DI_DIAGRAM == xtr.LocalName)
                        {
                            xtr.Skip();
                        }
                        else
                        {
                            continue;
                        }
                    }

                    if (xtr.IsStartElement() && BpmnXMLConstants.ELEMENT_DEFINITIONS.Equals(xtr.LocalName))
                    {
                        definitionsParser.Parse(xtr, model);

                    }
                    else if (BpmnXMLConstants.ELEMENT_RESOURCE.Equals(xtr.LocalName))
                    {
                        resourceParser.Parse(xtr, model);

                    }
                    else if (BpmnXMLConstants.ELEMENT_SIGNAL.Equals(xtr.LocalName))
                    {
                        signalParser.Parse(xtr, model);

                    }
                    else if (BpmnXMLConstants.ELEMENT_MESSAGE.Equals(xtr.LocalName))
                    {
                        messageParser.Parse(xtr, model);

                    }
                    else if (BpmnXMLConstants.ELEMENT_ERROR.Equals(xtr.LocalName))
                    {

                        if (!string.IsNullOrWhiteSpace(xtr.GetAttributeValue(BpmnXMLConstants.ATTRIBUTE_ID)))
                        {
                            model.AddError(xtr.GetAttributeValue(BpmnXMLConstants.ATTRIBUTE_ID), xtr.GetAttributeValue(BpmnXMLConstants.ATTRIBUTE_ERROR_CODE));
                        }

                    }
                    else if (BpmnXMLConstants.ELEMENT_IMPORT.Equals(xtr.LocalName))
                    {
                        importParser.Parse(xtr, model);

                    }
                    else if (BpmnXMLConstants.ELEMENT_ITEM_DEFINITION.Equals(xtr.LocalName))
                    {
                        itemDefinitionParser.Parse(xtr, model);

                    }
                    else if (BpmnXMLConstants.ELEMENT_DATA_STORE.Equals(xtr.LocalName))
                    {
                        dataStoreParser.Parse(xtr, model);

                    }
                    else if (BpmnXMLConstants.ELEMENT_INTERFACE.Equals(xtr.LocalName))
                    {
                        interfaceParser.Parse(xtr, model);

                    }
                    else if (BpmnXMLConstants.ELEMENT_IOSPECIFICATION.Equals(xtr.LocalName))
                    {
                        ioSpecificationParser.ParseChildElement(xtr, activeProcess, model);

                    }
                    else if (BpmnXMLConstants.ELEMENT_PARTICIPANT.Equals(xtr.LocalName))
                    {
                        participantParser.Parse(xtr, model);

                    }
                    else if (BpmnXMLConstants.ELEMENT_MESSAGE_FLOW.Equals(xtr.LocalName))
                    {
                        messageFlowParser.Parse(xtr, model);

                    }
                    else if (BpmnXMLConstants.ELEMENT_PROCESS.Equals(xtr.LocalName))
                    {

                        Process process = processParser.Parse(xtr, model);
                        if (process is not null)
                        {
                            activeProcess = process;
                        }

                    }
                    else if (BpmnXMLConstants.ELEMENT_POTENTIAL_STARTER.Equals(xtr.LocalName))
                    {
                        potentialStarterParser.Parse(xtr, activeProcess);

                    }
                    else if (BpmnXMLConstants.ELEMENT_LANE.Equals(xtr.LocalName))
                    {
                        laneParser.Parse(xtr, activeProcess, model);

                    }
                    else if (BpmnXMLConstants.ELEMENT_DOCUMENTATION.Equals(xtr.LocalName))
                    {

                        BaseElement parentElement = null;
                        if (activeSubProcessList.Count > 0)
                        {
                            parentElement = activeSubProcessList[activeSubProcessList.Count - 1];
                        }
                        else if (activeProcess is not null)
                        {
                            parentElement = activeProcess;
                        }
                        documentationParser.ParseChildElement(xtr, parentElement, model);

                    }
                    else if (activeProcess is null && BpmnXMLConstants.ELEMENT_TEXT_ANNOTATION.Equals(xtr.LocalName))
                    {
                        string elementId = xtr.GetAttributeValue(BpmnXMLConstants.ATTRIBUTE_ID);
                        TextAnnotation textAnnotation = (TextAnnotation)(new TextAnnotationXMLConverter()).ConvertXMLToElement(xtr, model);
                        textAnnotation.Id = elementId;
                        model.GlobalArtifacts.Add(textAnnotation);

                    }
                    else if (activeProcess is null && BpmnXMLConstants.ELEMENT_ASSOCIATION.Equals(xtr.LocalName))
                    {
                        string elementId = xtr.GetAttributeValue(BpmnXMLConstants.ATTRIBUTE_ID);
                        Association association = (Association)(new AssociationXMLConverter()).ConvertXMLToElement(xtr, model);
                        association.Id = elementId;
                        model.GlobalArtifacts.Add(association);

                    }
                    else if (BpmnXMLConstants.ELEMENT_EXTENSIONS.Equals(xtr.LocalName))
                    {
                        extensionElementsParser.Parse(xtr, activeSubProcessList, activeProcess, model);

                    }
                    else if (BpmnXMLConstants.ELEMENT_SUBPROCESS.Equals(xtr.LocalName) || BpmnXMLConstants.ELEMENT_TRANSACTION.Equals(xtr.LocalName) || BpmnXMLConstants.ELEMENT_ADHOC_SUBPROCESS.Equals(xtr.LocalName))
                    {
                        subProcessParser.Parse(xtr, activeSubProcessList, activeProcess);

                    }
                    else if (BpmnXMLConstants.ELEMENT_COMPLETION_CONDITION.Equals(xtr.LocalName))
                    {
                        if (activeSubProcessList.Count > 0)
                        {
                            SubProcess subProcess = activeSubProcessList[activeSubProcessList.Count - 1];
                            if (subProcess is AdhocSubProcess adhocSubProcess)
                            {
                                adhocSubProcess.CompletionCondition = xtr.ElementText;
                            }
                        }

                    }
                    else if (BpmnXMLConstants.ELEMENT_DI_SHAPE.Equals(xtr.LocalName))
                    {
                        bpmnShapeParser.Parse(xtr, model);

                    }
                    else if (BpmnXMLConstants.ELEMENT_DI_EDGE.Equals(xtr.LocalName))
                    {
                        bpmnEdgeParser.Parse(xtr, model);

                    }
                    else
                    {

                        if (activeSubProcessList.Count > 0 && BpmnXMLConstants.ELEMENT_MULTIINSTANCE.Equals(xtr.LocalName, StringComparison.CurrentCultureIgnoreCase))
                        {

                            multiInstanceParser.ParseChildElement(xtr, activeSubProcessList[activeSubProcessList.Count - 1], model);

                        }
                        else if (convertersToBpmnMap.ContainsKey(xtr.LocalName))
                        {
                            if (activeProcess is not null)
                            {
                                BaseBpmnXMLConverter converter = convertersToBpmnMap[xtr.LocalName];
                                converter.ConvertToBpmnModel(xtr, model, activeProcess, activeSubProcessList);
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
                    ProcessFlowElements(process.FlowElements, process);
                }
            }
            catch (Exception e)
            {
                log.LogError(e, "Error processing BPMN document");
                throw new XMLException("Error processing BPMN document", e);
            }
            return model;
        }

        public virtual BpmnModel ConvertToBpmnModel(string docfile)
        {
            return ConvertToBpmnModel(new XMLStreamReader(File.Open(docfile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)));
        }

        protected internal virtual void ProcessFlowElements(ICollection<FlowElement> flowElementList, BaseElement parentScope)
        {
            foreach (FlowElement flowElement in flowElementList)
            {
                if (flowElement is SequenceFlow sequenceFlow)
                {
                    FlowNode sourceNode = GetFlowNodeFromScope(sequenceFlow.SourceRef, parentScope);
                    if (sourceNode is not null)
                    {
                        sourceNode.OutgoingFlows.Add(sequenceFlow);
                        sequenceFlow.SourceFlowElement = sourceNode;
                    }

                    FlowNode targetNode = GetFlowNodeFromScope(sequenceFlow.TargetRef, parentScope);
                    if (targetNode is not null)
                    {
                        targetNode.IncomingFlows.Add(sequenceFlow);
                        sequenceFlow.TargetFlowElement = targetNode;
                    }

                }
                else if (flowElement is BoundaryEvent boundaryEvent)
                {
                    FlowElement attachedToElement = GetFlowNodeFromScope(boundaryEvent.AttachedToRefId, parentScope);
                    if (attachedToElement is Activity attachedActivity)
                    {
                        boundaryEvent.AttachedToRef = attachedActivity;
                        attachedActivity.BoundaryEvents.Add(boundaryEvent);
                    }

                }
                else if (flowElement is SubProcess subProcess)
                {
                    ProcessFlowElements(subProcess.FlowElements, subProcess);
                }
            }
        }

        protected internal virtual FlowNode GetFlowNodeFromScope(string elementId, BaseElement scope)
        {
            FlowNode flowNode = null;
            if (!string.IsNullOrWhiteSpace(elementId))
            {
                if (scope is Process)
                {
                    flowNode = (FlowNode)((Process)scope).FindFlowElement(elementId);
                }
                else if (scope is SubProcess)
                {
                    flowNode = (FlowNode)((SubProcess)scope).FindFlowElement(elementId);
                }
            }
            return flowNode;
        }

        public virtual byte[] ConvertToXML(BpmnModel model)
        {
            return ConvertToXML(model, DEFAULT_ENCODING);
        }

        public virtual byte[] ConvertToXML(BpmnModel model, string encoding)
        {
            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    XmlWriterSettings settings = new XmlWriterSettings
                    {
                        //要求缩进
                        Indent = true,
                        //注意如果不设置encoding默认将输出utf-16
                        //注意这儿不能直接用Encoding.UTF8如果用Encoding.UTF8将在输出文本的最前面添加4个字节的非xml内容
                        Encoding = new UTF8Encoding(false),

                        //设置换行符
                        NewLineChars = Environment.NewLine,
                        NamespaceHandling = NamespaceHandling.OmitDuplicates,
                        OmitXmlDeclaration = true
                    };

                    using (XmlWriter xmlWriter = XmlWriter.Create(ms, settings))
                    {
                        XMLStreamWriter writer = new XMLStreamWriter(xmlWriter);

                        DefinitionsRootExport.WriteRootElement(model, writer, encoding);
                        CollaborationExport.WritePools(model, writer);
                        DataStoreExport.WriteDataStores(model, writer);
                        SignalAndMessageDefinitionExport.WriteSignalsAndMessages(model, writer);

                        foreach (Process process in model.Processes)
                        {
                            if (process.FlowElements?.Count == 0 && process.Lanes?.Count == 0)
                            {
                                // empty process, ignore it
                                continue;
                            }

                            ProcessExport.WriteProcess(process, writer);

                            foreach (FlowElement flowElement in process.FlowElements)
                            {
                                CreateXML(flowElement, model, writer);
                            }

                            foreach (Artifact artifact in process.Artifacts)
                            {
                                CreateXML(artifact, model, writer);
                            }

                            // end process element
                            writer.WriteEndElement();
                        }

                        try
                        {
                            BPMNDIExport.WriteBPMNDI(model, writer);
                        }
                        catch(Exception ex)
                        {
                            ///TODO: 不支持仅模型没有坐标位置的流程图
                        }
                        writer.WriteEndElement();
                    }

                    return ms.ToArray();
                }
            }
            catch (Exception e)
            {
                log.LogError("Error writing BPMN XML", e);
                throw new XMLException("Error writing BPMN XML", e);
            }
        }

        public virtual void CreateXML(FlowElement flowElement, BpmnModel model, XMLStreamWriter xtw)
        {

            if (flowElement is SubProcess subProcess)
            {
                if (flowElement is Transaction)
                {
                    xtw.WriteStartElement(BpmnXMLConstants.BPMN_PREFIX, BpmnXMLConstants.ELEMENT_TRANSACTION, BpmnXMLConstants.BPMN2_NAMESPACE);
                }
                else if (flowElement is AdhocSubProcess)
                {
                    xtw.WriteStartElement(BpmnXMLConstants.BPMN_PREFIX, BpmnXMLConstants.ELEMENT_ADHOC_SUBPROCESS, BpmnXMLConstants.BPMN2_NAMESPACE);
                }
                else
                {
                    xtw.WriteStartElement(BpmnXMLConstants.BPMN_PREFIX, BpmnXMLConstants.ELEMENT_SUBPROCESS, BpmnXMLConstants.BPMN2_NAMESPACE);
                }

                xtw.WriteAttribute(BpmnXMLConstants.ATTRIBUTE_ID, subProcess.Id);
                if (!string.IsNullOrWhiteSpace(subProcess.Name))
                {
                    xtw.WriteAttribute(BpmnXMLConstants.ATTRIBUTE_NAME, subProcess.Name);
                }
                else
                {
                    xtw.WriteAttribute(BpmnXMLConstants.ATTRIBUTE_NAME, "subProcess");
                }

                if (subProcess is EventSubProcess)
                {
                    xtw.WriteAttribute(BpmnXMLConstants.ATTRIBUTE_TRIGGERED_BY, BpmnXMLConstants.ATTRIBUTE_VALUE_TRUE);

                }
                else if (!(subProcess is Transaction))
                {
                    if (subProcess.Asynchronous)
                    {
                        BpmnXMLUtil.WriteQualifiedAttribute(BpmnXMLConstants.ATTRIBUTE_ACTIVITY_ASYNCHRONOUS, BpmnXMLConstants.ATTRIBUTE_VALUE_TRUE, xtw);
                        if (subProcess.NotExclusive)
                        {
                            BpmnXMLUtil.WriteQualifiedAttribute(BpmnXMLConstants.ATTRIBUTE_ACTIVITY_EXCLUSIVE, BpmnXMLConstants.ATTRIBUTE_VALUE_FALSE, xtw);
                        }
                    }

                }
                else if (subProcess is AdhocSubProcess hocSubProcess)
                {
                    BpmnXMLUtil.WriteDefaultAttribute(BpmnXMLConstants.ATTRIBUTE_CANCEL_REMAINING_INSTANCES, hocSubProcess.CancelRemainingInstances.ToString(), xtw);
                    if (!string.IsNullOrWhiteSpace(hocSubProcess.Ordering))
                    {
                        BpmnXMLUtil.WriteDefaultAttribute(BpmnXMLConstants.ATTRIBUTE_ORDERING, hocSubProcess.Ordering, xtw);
                    }
                }

                if (!string.IsNullOrWhiteSpace(subProcess.Documentation))
                {

                    xtw.WriteStartElement(BpmnXMLConstants.BPMN_PREFIX, BpmnXMLConstants.ELEMENT_DOCUMENTATION, BpmnXMLConstants.BPMN2_NAMESPACE);
                    xtw.WriteCharacters(subProcess.Documentation);
                    xtw.WriteEndElement();
                }

                bool didWriteExtensionStartElement = ActivitiListenerExport.WriteListeners(subProcess, false, xtw);

                didWriteExtensionStartElement = BpmnXMLUtil.WriteExtensionElements(subProcess, didWriteExtensionStartElement, model.Namespaces, xtw);
                if (didWriteExtensionStartElement)
                {
                    // closing extensions element
                    xtw.WriteEndElement();
                }

                MultiInstanceExport.WriteMultiInstance(subProcess, xtw);

                if (subProcess is AdhocSubProcess adhocSubProcess)
                {
                    if (!string.IsNullOrWhiteSpace(adhocSubProcess.CompletionCondition))
                    {
                        xtw.WriteStartElement(BpmnXMLConstants.BPMN_PREFIX, BpmnXMLConstants.ELEMENT_COMPLETION_CONDITION, BpmnXMLConstants.BPMN2_NAMESPACE);
                        xtw.WriteCharacters(adhocSubProcess.CompletionCondition);
                        xtw.WriteEndElement();
                    }
                }

                foreach (FlowElement subElement in subProcess.FlowElements)
                {
                    CreateXML(subElement, model, xtw);
                }

                foreach (Artifact artifact in subProcess.Artifacts)
                {
                    CreateXML(artifact, model, xtw);
                }

                xtw.WriteEndElement();

            }
            else
            {

                BaseBpmnXMLConverter converter = convertersToXMLMap[flowElement.GetType()];

                if (converter is null)
                {
                    throw new XMLException("No converter for " + flowElement.GetType() + " found");
                }

                converter.ConvertToXML(xtw, flowElement, model);
            }
        }

        protected internal virtual void CreateXML(Artifact artifact, BpmnModel model, XMLStreamWriter xtw)
        {

            BaseBpmnXMLConverter converter = convertersToXMLMap[artifact.GetType()];

            if (converter is null)
            {
                throw new XMLException("No converter for " + artifact.GetType() + " found");
            }

            converter.ConvertToXML(xtw, artifact, model);
        }
    }
}