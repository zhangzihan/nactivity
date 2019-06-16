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
namespace Sys.Workflow.Engine.Impl.Bpmn.Behavior
{

    using Sys.Workflow.Bpmn.Models;
    using Sys.Workflow.Engine.Delegate;
    using Sys.Workflow.Engine.Impl.Bpmn.Datas;
    using Sys.Workflow.Engine.Impl.Bpmn.Helper;
    using Sys.Workflow.Engine.Impl.Bpmn.Parser;
    using Sys.Workflow.Engine.Impl.Bpmn.Webservice;
    using Sys.Workflow.Engine.Impl.Cfg;
    using Sys.Workflow.Engine.Impl.Contexts;
    using Sys.Workflow.Engine.Impl.EL;
    using Sys.Workflow.Engine.Impl.Persistence.Entity;
    using Sys.Workflow.Engine.Impl.Util;
    using Sys.Workflow.Engine.Impl.Webservice;
    using Sys.Workflow;
    using IOSpecification = Workflow.Bpmn.Models.IOSpecification;
    using ItemDefinition = Workflow.Bpmn.Models.ItemDefinition;
    using Operation = Workflow.Bpmn.Models.Operation;
    using Assignment = Workflow.Bpmn.Models.Assignment;

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

        public const string CURRENT_MESSAGE = "Sys.Workflow.Engine.Impl.Bpmn.CURRENT_MESSAGE";

        protected internal IDictionary<string, IXMLImporter> xmlImporterMap = new Dictionary<string, IXMLImporter>();
        protected internal IDictionary<string, WSOperation> wsOperationMap = new Dictionary<string, WSOperation>();
        protected internal IDictionary<string, IStructureDefinition> structureDefinitionMap = new Dictionary<string, IStructureDefinition>();
        protected internal IDictionary<string, WSService> wsServiceMap = new Dictionary<string, WSService>();
        protected internal IDictionary<string, Webservice.Operation> operationMap = new Dictionary<string, Webservice.Operation>();
        protected internal IDictionary<string, Datas.ItemDefinition> itemDefinitionMap = new Dictionary<string, Datas.ItemDefinition>();
        protected internal IDictionary<string, MessageDefinition> messageDefinitionMap = new Dictionary<string, MessageDefinition>();

        public WebServiceActivityBehavior()
        {
            itemDefinitionMap["http://www.w3.org/2001/XMLSchema:string"] = new Datas.ItemDefinition("http://www.w3.org/2001/XMLSchema:string", new ClassStructureDefinition(typeof(string)));
        }

        public override void Execute(IExecutionEntity execution)
        {
            BpmnModel bpmnModel = ProcessDefinitionUtil.GetBpmnModel(execution.ProcessDefinitionId);
            FlowElement flowElement = execution.CurrentFlowElement;
            IOSpecification ioSpecification;
            string operationRef;
            IList<DataAssociation> dataInputAssociations;
            IList<DataAssociation> dataOutputAssociations;
            if (flowElement is SendTask sendTask)
            {
                ioSpecification = sendTask.IoSpecification;
                operationRef = sendTask.OperationRef;
                dataInputAssociations = sendTask.DataInputAssociations;
                dataOutputAssociations = sendTask.DataOutputAssociations;
            }
            else if (flowElement is ServiceTask serviceTask)
            {
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

            FillDefinitionMaps(bpmnModel);

            Webservice.Operation operation = operationMap[operationRef];

            try
            {

                if (ioSpecification != null)
                {
                    InitializeIoSpecification(ioSpecification, execution, bpmnModel);
                    if (ioSpecification.DataInputRefs.Count > 0)
                    {
                        string firstDataInputName = ioSpecification.DataInputRefs[0];
                        ItemInstance inputItem = (ItemInstance)execution.GetVariable(firstDataInputName);
                        message = new MessageInstance(operation.InMessage, inputItem);
                    }

                }
                else
                {
                    message = operation.InMessage.CreateInstance();
                }

                execution.SetVariable(CURRENT_MESSAGE, message);

                FillMessage(dataInputAssociations, execution);

                ProcessEngineConfigurationImpl processEngineConfig = Context.ProcessEngineConfiguration;
                MessageInstance receivedMessage = operation.SendMessage(message, processEngineConfig.WsOverridenEndpointAddresses);

                execution.SetVariable(CURRENT_MESSAGE, receivedMessage);

                if (ioSpecification != null && ioSpecification.DataOutputRefs.Count > 0)
                {
                    string firstDataOutputName = ioSpecification.DataOutputRefs[0];
                    if (!(firstDataOutputName is null))
                    {
                        ItemInstance outputItem = (ItemInstance)execution.GetVariable(firstDataOutputName);
                        outputItem.StructureInstance.LoadFrom(receivedMessage.StructureInstance.ToArray());
                    }
                }

                ReturnMessage(dataOutputAssociations, execution);

                execution.SetVariable(CURRENT_MESSAGE, null);
                Leave(execution);
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
                    ErrorPropagation.PropagateError(error, execution);
                }
                else if (exc is Exception)
                {
                    throw (Exception)exc;
                }
            }
        }

        protected internal virtual void InitializeIoSpecification(IOSpecification activityIoSpecification, IExecutionEntity execution, BpmnModel bpmnModel)
        {
            foreach (DataSpec dataSpec in activityIoSpecification.DataInputs)
            {
                Datas.ItemDefinition itemDefinition = itemDefinitionMap[dataSpec.ItemSubjectRef];
                execution.SetVariable(dataSpec.Id, itemDefinition.CreateInstance());
            }

            foreach (DataSpec dataSpec in activityIoSpecification.DataOutputs)
            {
                Datas.ItemDefinition itemDefinition = itemDefinitionMap[dataSpec.ItemSubjectRef];
                execution.SetVariable(dataSpec.Id, itemDefinition.CreateInstance());
            }
        }

        protected internal virtual void FillDefinitionMaps(BpmnModel bpmnModel)
        {
            foreach (Import theImport in bpmnModel.Imports)
            {
                FillImporterInfo(theImport, bpmnModel.SourceSystemId);
            }

            CreateItemDefinitions(bpmnModel);
            CreateMessages(bpmnModel);
            CreateOperations(bpmnModel);
        }

        protected internal virtual void CreateItemDefinitions(BpmnModel bpmnModel)
        {
            foreach (ItemDefinition itemDefinitionElement in bpmnModel.ItemDefinitions.Values)
            {

                if (!itemDefinitionMap.ContainsKey(itemDefinitionElement.Id))
                {
                    IStructureDefinition structure = null;

                    try
                    {
                        // it is a class
                        Type classStructure = ReflectUtil.LoadClass(itemDefinitionElement.StructureRef);
                        structure = new ClassStructureDefinition(classStructure);
                    }
                    catch (ActivitiException)
                    {
                        // it is a reference to a different structure
                        structure = structureDefinitionMap[itemDefinitionElement.StructureRef];
                    }

                    Datas.ItemDefinition itemDefinition = new Datas.ItemDefinition(itemDefinitionElement.Id, structure);
                    if (!string.IsNullOrWhiteSpace(itemDefinitionElement.ItemKind))
                    {
                        itemDefinition.ItemKind = (ItemKind)Enum.Parse(typeof(ItemKind), itemDefinitionElement.ItemKind);
                    }

                    itemDefinitionMap[itemDefinition.Id] = itemDefinition;
                }
            }
        }

        public virtual void CreateMessages(BpmnModel bpmnModel)
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
                            Datas.ItemDefinition itemDefinition = itemDefinitionMap[messageElement.ItemRef];
                            messageDefinition.ItemDefinition = itemDefinition;
                        }
                    }

                    messageDefinitionMap[messageDefinition.Id] = messageDefinition;
                }
            }
        }

        protected internal virtual void CreateOperations(BpmnModel bpmnModel)
        {
            foreach (Interface interfaceObject in bpmnModel.Interfaces)
            {
                BpmnInterface bpmnInterface = new BpmnInterface(interfaceObject.Id, interfaceObject.Name)
                {
                    Implementation = wsServiceMap[interfaceObject.ImplementationRef]
                };

                foreach (Operation operationObject in interfaceObject.Operations)
                {
                    if (!operationMap.ContainsKey(operationObject.Id))
                    {
                        MessageDefinition inMessage = messageDefinitionMap[operationObject.InMessageRef];
                        Webservice.Operation operation = new Webservice.Operation(operationObject.Id, operationObject.Name, bpmnInterface, inMessage)
                        {
                            Implementation = wsOperationMap[operationObject.ImplementationRef]
                        };

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

        protected internal virtual void FillImporterInfo(Import theImport, string sourceSystemId)
        {
            if (!xmlImporterMap.ContainsKey(theImport.ImportType))
            {

                if (theImport.ImportType.Equals("http://schemas.xmlsoap.org/wsdl/"))
                {
                    Type wsdlImporterClass;
                    try
                    {
                        wsdlImporterClass = Type.GetType("Sys.Workflow.Engine.Impl.Webservice.CxfWSDLImporter", true);
                        IXMLImporter importerInstance = (IXMLImporter)Activator.CreateInstance(wsdlImporterClass);
                        xmlImporterMap[theImport.ImportType] = importerInstance;
                        importerInstance.ImportFrom(theImport, sourceSystemId);

                        structureDefinitionMap.PutAll(importerInstance.Structures);

                        wsServiceMap.PutAll(importerInstance.Services);

                        wsOperationMap.PutAll(importerInstance.Operations);

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

        protected internal virtual void ReturnMessage(IList<DataAssociation> dataOutputAssociations, IExecutionEntity execution)
        {
            foreach (DataAssociation dataAssociationElement in dataOutputAssociations)
            {
                AbstractDataAssociation dataAssociation = CreateDataOutputAssociation(dataAssociationElement);
                dataAssociation.Evaluate(execution);
            }
        }

        protected internal virtual void FillMessage(IList<DataAssociation> dataInputAssociations, IExecutionEntity execution)
        {
            foreach (DataAssociation dataAssociationElement in dataInputAssociations)
            {
                AbstractDataAssociation dataAssociation = CreateDataInputAssociation(dataAssociationElement);
                dataAssociation.Evaluate(execution);
            }
        }

        protected internal virtual AbstractDataAssociation CreateDataInputAssociation(DataAssociation dataAssociationElement)
        {
            if (dataAssociationElement.Assignments.Count == 0)
            {
                return new MessageImplicitDataInputAssociation(dataAssociationElement.SourceRef, dataAssociationElement.TargetRef);
            }
            else
            {
                SimpleDataInputAssociation dataAssociation = new SimpleDataInputAssociation(dataAssociationElement.SourceRef, dataAssociationElement.TargetRef);
                ExpressionManager expressionManager = Context.ProcessEngineConfiguration.ExpressionManager;

                foreach (Assignment assignmentElement in dataAssociationElement.Assignments)
                {
                    if (!string.IsNullOrWhiteSpace(assignmentElement.From) && !string.IsNullOrWhiteSpace(assignmentElement.To))
                    {
                        IExpression from = expressionManager.CreateExpression(assignmentElement.From);
                        IExpression to = expressionManager.CreateExpression(assignmentElement.To);
                        Datas.Assignment assignment = new Datas.Assignment(from, to);
                        dataAssociation.AddAssignment(assignment);
                    }
                }
                return dataAssociation;
            }
        }

        protected internal virtual AbstractDataAssociation CreateDataOutputAssociation(DataAssociation dataAssociationElement)
        {
            if (!string.IsNullOrWhiteSpace(dataAssociationElement.SourceRef))
            {
                return new MessageImplicitDataOutputAssociation(dataAssociationElement.TargetRef, dataAssociationElement.SourceRef);
            }
            else
            {
                ExpressionManager expressionManager = Context.ProcessEngineConfiguration.ExpressionManager;
                IExpression transformation = expressionManager.CreateExpression(dataAssociationElement.Transformation);
                AbstractDataAssociation dataOutputAssociation = new TransformationDataOutputAssociation(null, dataAssociationElement.TargetRef, transformation);
                return dataOutputAssociation;
            }
        }
    }
}