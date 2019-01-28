using System;
using System.Collections.Generic;
using System.Threading;

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
namespace org.activiti.engine.impl.bpmn.behavior
{

    using org.activiti.bpmn.model;
    using org.activiti.engine.@delegate;
    using org.activiti.engine.impl.bpmn.data;
    using org.activiti.engine.impl.bpmn.helper;
    using org.activiti.engine.impl.bpmn.parser;
    using org.activiti.engine.impl.bpmn.webservice;
    using org.activiti.engine.impl.cfg;
    using org.activiti.engine.impl.context;
    using org.activiti.engine.impl.el;
    using org.activiti.engine.impl.persistence.entity;
    using org.activiti.engine.impl.util;
    using org.activiti.engine.impl.webservice;
    using Sys.Bpm;

    /// <summary>
    /// An activity behavior that allows calling Web services
    /// 
    /// 
    /// 
    /// 
    /// </summary>
    [Serializable]
    public class WebServiceActivityBehavior : AbstractBpmnActivityBehavior
    {

        private const long serialVersionUID = 1L;

        public const string CURRENT_MESSAGE = "org.activiti.engine.impl.bpmn.CURRENT_MESSAGE";

        protected internal IDictionary<string, IXMLImporter> xmlImporterMap = new Dictionary<string, IXMLImporter>();
        protected internal IDictionary<string, WSOperation> wsOperationMap = new Dictionary<string, WSOperation>();
        protected internal IDictionary<string, IStructureDefinition> structureDefinitionMap = new Dictionary<string, IStructureDefinition>();
        protected internal IDictionary<string, WSService> wsServiceMap = new Dictionary<string, WSService>();
        protected internal IDictionary<string, webservice.Operation> operationMap = new Dictionary<string, webservice.Operation>();
        protected internal IDictionary<string, data.ItemDefinition> itemDefinitionMap = new Dictionary<string, data.ItemDefinition>();
        protected internal IDictionary<string, MessageDefinition> messageDefinitionMap = new Dictionary<string, MessageDefinition>();

        public WebServiceActivityBehavior()
        {
            itemDefinitionMap["http://www.w3.org/2001/XMLSchema:string"] = new data.ItemDefinition("http://www.w3.org/2001/XMLSchema:string", new ClassStructureDefinition(typeof(string)));
        }

        public override void execute(IExecutionEntity execution)
        {
            BpmnModel bpmnModel = ProcessDefinitionUtil.getBpmnModel(execution.ProcessDefinitionId);
            FlowElement flowElement = execution.CurrentFlowElement;

            activiti.bpmn.model.IOSpecification ioSpecification = null;
            string operationRef = null;
            IList<DataAssociation> dataInputAssociations = null;
            IList<DataAssociation> dataOutputAssociations = null;

            if (flowElement is SendTask)
            {
                SendTask sendTask = (SendTask)flowElement;
                ioSpecification = sendTask.IoSpecification;
                operationRef = sendTask.OperationRef;
                dataInputAssociations = sendTask.DataInputAssociations;
                dataOutputAssociations = sendTask.DataOutputAssociations;

            }
            else if (flowElement is ServiceTask)
            {
                ServiceTask serviceTask = (ServiceTask)flowElement;
                ioSpecification = serviceTask.IoSpecification;
                operationRef = serviceTask.OperationRef;
                dataInputAssociations = serviceTask.DataInputAssociations;
                dataOutputAssociations = serviceTask.DataOutputAssociations;

            }
            else
            {
                throw new ActivitiException("Unsupported flow element type " + flowElement);
            }

            MessageInstance message = null;

            fillDefinitionMaps(bpmnModel);

            webservice.Operation operation = operationMap[operationRef];

            try
            {

                if (ioSpecification != null)
                {
                    initializeIoSpecification(ioSpecification, execution, bpmnModel);
                    if (ioSpecification.DataInputRefs.Count > 0)
                    {
                        string firstDataInputName = ioSpecification.DataInputRefs[0];
                        ItemInstance inputItem = (ItemInstance)execution.getVariable(firstDataInputName);
                        message = new MessageInstance(operation.InMessage, inputItem);
                    }

                }
                else
                {
                    message = operation.InMessage.createInstance();
                }

                execution.setVariable(CURRENT_MESSAGE, message);

                fillMessage(dataInputAssociations, execution);

                ProcessEngineConfigurationImpl processEngineConfig = Context.ProcessEngineConfiguration;
                MessageInstance receivedMessage = operation.sendMessage(message, processEngineConfig.WsOverridenEndpointAddresses);

                execution.setVariable(CURRENT_MESSAGE, receivedMessage);

                if (ioSpecification != null && ioSpecification.DataOutputRefs.Count > 0)
                {
                    string firstDataOutputName = ioSpecification.DataOutputRefs[0];
                    if (!ReferenceEquals(firstDataOutputName, null))
                    {
                        ItemInstance outputItem = (ItemInstance)execution.getVariable(firstDataOutputName);
                        outputItem.StructureInstance.loadFrom(receivedMessage.StructureInstance.toArray());
                    }
                }

                returnMessage(dataOutputAssociations, execution);

                execution.setVariable(CURRENT_MESSAGE, null);
                leave(execution);
            }
            catch (Exception exc)
            {

                Exception cause = exc;
                BpmnError error = null;
                while (cause != null)
                {
                    if (cause is BpmnError)
                    {
                        error = (BpmnError)cause;
                        break;
                    }
                    cause = cause.InnerException;
                }

                if (error != null)
                {
                    ErrorPropagation.propagateError(error, execution);
                }
                else if (exc is Exception)
                {
                    throw (Exception)exc;
                }
            }
        }

        protected internal virtual void initializeIoSpecification(activiti.bpmn.model.IOSpecification activityIoSpecification, IExecutionEntity execution, BpmnModel bpmnModel)
        {

            foreach (DataSpec dataSpec in activityIoSpecification.DataInputs)
            {
                data.ItemDefinition itemDefinition = itemDefinitionMap[dataSpec.ItemSubjectRef];
                execution.setVariable(dataSpec.Id, itemDefinition.createInstance());
            }

            foreach (DataSpec dataSpec in activityIoSpecification.DataOutputs)
            {
                data.ItemDefinition itemDefinition = itemDefinitionMap[dataSpec.ItemSubjectRef];
                execution.setVariable(dataSpec.Id, itemDefinition.createInstance());
            }
        }

        protected internal virtual void fillDefinitionMaps(BpmnModel bpmnModel)
        {

            foreach (Import theImport in bpmnModel.Imports)
            {
                fillImporterInfo(theImport, bpmnModel.SourceSystemId);
            }

            createItemDefinitions(bpmnModel);
            createMessages(bpmnModel);
            createOperations(bpmnModel);
        }

        protected internal virtual void createItemDefinitions(BpmnModel bpmnModel)
        {

            foreach (org.activiti.bpmn.model.ItemDefinition itemDefinitionElement in bpmnModel.ItemDefinitions.Values)
            {

                if (!itemDefinitionMap.ContainsKey(itemDefinitionElement.Id))
                {
                    IStructureDefinition structure = null;

                    try
                    {
                        // it is a class
                        Type classStructure = ReflectUtil.loadClass(itemDefinitionElement.StructureRef);
                        structure = new ClassStructureDefinition(classStructure);
                    }
                    catch (ActivitiException)
                    {
                        // it is a reference to a different structure
                        structure = structureDefinitionMap[itemDefinitionElement.StructureRef];
                    }

                    data.ItemDefinition itemDefinition = new data.ItemDefinition(itemDefinitionElement.Id, structure);
                    if (!string.IsNullOrWhiteSpace(itemDefinitionElement.ItemKind))
                    {
                        itemDefinition.ItemKind = (ItemKind)Enum.Parse(typeof(ItemKind), itemDefinitionElement.ItemKind);
                    }

                    itemDefinitionMap[itemDefinition.Id] = itemDefinition;
                }
            }
        }

        public virtual void createMessages(BpmnModel bpmnModel)
        {
            foreach (Message messageElement in bpmnModel.Messages)
            {
                if (!messageDefinitionMap.ContainsKey(messageElement.Id))
                {
                    MessageDefinition messageDefinition = new MessageDefinition(messageElement.Id);
                    if (!string.IsNullOrWhiteSpace(messageElement.ItemRef))
                    {
                        if (itemDefinitionMap.ContainsKey(messageElement.ItemRef))
                        {
                            data.ItemDefinition itemDefinition = itemDefinitionMap[messageElement.ItemRef];
                            messageDefinition.ItemDefinition = itemDefinition;
                        }
                    }

                    messageDefinitionMap[messageDefinition.Id] = messageDefinition;
                }
            }
        }

        protected internal virtual void createOperations(BpmnModel bpmnModel)
        {
            foreach (Interface interfaceObject in bpmnModel.Interfaces)
            {
                BpmnInterface bpmnInterface = new BpmnInterface(interfaceObject.Id, interfaceObject.Name);
                bpmnInterface.Implementation = wsServiceMap[interfaceObject.ImplementationRef];

                foreach (org.activiti.bpmn.model.Operation operationObject in interfaceObject.Operations)
                {

                    if (!operationMap.ContainsKey(operationObject.Id))
                    {
                        MessageDefinition inMessage = messageDefinitionMap[operationObject.InMessageRef];
                        webservice.Operation operation = new webservice.Operation(operationObject.Id, operationObject.Name, bpmnInterface, inMessage);
                        operation.Implementation = wsOperationMap[operationObject.ImplementationRef];

                        if (!string.IsNullOrWhiteSpace(operationObject.OutMessageRef))
                        {
                            if (messageDefinitionMap.ContainsKey(operationObject.OutMessageRef))
                            {
                                MessageDefinition outMessage = messageDefinitionMap[operationObject.OutMessageRef];
                                operation.OutMessage = outMessage;
                            }
                        }

                        operationMap[operation.Id] = operation;
                    }
                }
            }
        }

        protected internal virtual void fillImporterInfo(Import theImport, string sourceSystemId)
        {
            if (!xmlImporterMap.ContainsKey(theImport.ImportType))
            {

                if (theImport.ImportType.Equals("http://schemas.xmlsoap.org/wsdl/"))
                {
                    Type wsdlImporterClass;
                    try
                    {
                        wsdlImporterClass = Type.GetType("org.activiti.engine.impl.webservice.CxfWSDLImporter", true);
                        IXMLImporter importerInstance = (IXMLImporter)Activator.CreateInstance(wsdlImporterClass);
                        xmlImporterMap[theImport.ImportType] = importerInstance;
                        importerInstance.importFrom(theImport, sourceSystemId);

                        structureDefinitionMap.putAll(importerInstance.Structures);

                        wsServiceMap.putAll(importerInstance.Services);

                        wsOperationMap.putAll(importerInstance.Operations);

                    }
                    catch (Exception)
                    {
                        throw new ActivitiException("Could not find importer for type " + theImport.ImportType);
                    }

                }
                else
                {
                    throw new ActivitiException("Could not import item of type " + theImport.ImportType);
                }
            }
        }

        protected internal virtual void returnMessage(IList<DataAssociation> dataOutputAssociations, IExecutionEntity execution)
        {
            foreach (DataAssociation dataAssociationElement in dataOutputAssociations)
            {
                AbstractDataAssociation dataAssociation = createDataOutputAssociation(dataAssociationElement);
                dataAssociation.evaluate(execution);
            }
        }

        protected internal virtual void fillMessage(IList<DataAssociation> dataInputAssociations, IExecutionEntity execution)
        {
            foreach (DataAssociation dataAssociationElement in dataInputAssociations)
            {
                AbstractDataAssociation dataAssociation = createDataInputAssociation(dataAssociationElement);
                dataAssociation.evaluate(execution);
            }
        }

        protected internal virtual AbstractDataAssociation createDataInputAssociation(DataAssociation dataAssociationElement)
        {
            if (dataAssociationElement.Assignments.Count == 0)
            {
                return new MessageImplicitDataInputAssociation(dataAssociationElement.SourceRef, dataAssociationElement.TargetRef);
            }
            else
            {
                SimpleDataInputAssociation dataAssociation = new SimpleDataInputAssociation(dataAssociationElement.SourceRef, dataAssociationElement.TargetRef);
                ExpressionManager expressionManager = Context.ProcessEngineConfiguration.ExpressionManager;

                foreach (org.activiti.bpmn.model.Assignment assignmentElement in dataAssociationElement.Assignments)
                {
                    if (!string.IsNullOrWhiteSpace(assignmentElement.From) && !string.IsNullOrWhiteSpace(assignmentElement.To))
                    {
                        IExpression from = expressionManager.createExpression(assignmentElement.From);
                        IExpression to = expressionManager.createExpression(assignmentElement.To);
                        data.Assignment assignment = new data.Assignment(from, to);
                        dataAssociation.addAssignment(assignment);
                    }
                }
                return dataAssociation;
            }
        }

        protected internal virtual AbstractDataAssociation createDataOutputAssociation(DataAssociation dataAssociationElement)
        {
            if (!string.IsNullOrWhiteSpace(dataAssociationElement.SourceRef))
            {
                return new MessageImplicitDataOutputAssociation(dataAssociationElement.TargetRef, dataAssociationElement.SourceRef);
            }
            else
            {
                ExpressionManager expressionManager = Context.ProcessEngineConfiguration.ExpressionManager;
                IExpression transformation = expressionManager.createExpression(dataAssociationElement.Transformation);
                AbstractDataAssociation dataOutputAssociation = new TransformationDataOutputAssociation(null, dataAssociationElement.TargetRef, transformation);
                return dataOutputAssociation;
            }
        }
    }

}