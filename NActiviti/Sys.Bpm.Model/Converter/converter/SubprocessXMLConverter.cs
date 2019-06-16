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
namespace Sys.Workflow.Bpmn.Converters
{
    using Microsoft.Extensions.Logging;
    using Sys.Workflow.Bpmn.Converters.Exports;
    using Sys.Workflow.Bpmn.Exceptions;
    using Sys.Workflow.Bpmn.Models;
    using Sys.Workflow;
    using Sys.Workflow.Model;
    using System;
    using System.Linq;
    using System.Xml.Linq;

    /// 
    public class SubprocessXMLConverter : BpmnXMLConverter
    {
        private static readonly ILogger logger = BpmnModelLoggerFactory.LoggerService<SubprocessXMLConverter>();

        public override byte[] ConvertToXML(BpmnModel model, string encoding)
        {
            try
            {
                System.IO.MemoryStream outputStream = new System.IO.MemoryStream();

                //XMLOutputFactory xof = XMLOutputFactory.newInstance();
                //System.IO.StreamWriter @out = new System.IO.StreamWriter(outputStream, encoding);
                XDocument doc = XDocument.Load(outputStream);
                XElement writer = doc.Root; //xof.createXElement(@out);
                XMLStreamWriter xtw = new IndentingXMLStreamWriter(writer);

                DefinitionsRootExport.WriteRootElement(model, xtw, encoding);
                CollaborationExport.WritePools(model, xtw);
                DataStoreExport.WriteDataStores(model, xtw);
                SignalAndMessageDefinitionExport.WriteSignalsAndMessages(model, xtw);

                foreach (Process process in model.Processes)
                {

                    if (process.FlowElements.Count == 0 && process.Lanes.Count == 0)
                    {
                        // empty process, ignore it
                        continue;
                    }

                    ProcessExport.WriteProcess(process, xtw);

                    foreach (FlowElement flowElement in process.FlowElements)
                    {
                        CreateXML(flowElement, model, xtw);
                    }

                    foreach (Artifact artifact in process.Artifacts)
                    {
                        CreateXML(artifact, model, xtw);
                    }

                    // end process element
                    xtw.WriteEndElement();
                }

                // refactor each subprocess into a separate Diagram
                IList<BpmnModel> subModels = ParseSubModels(model);
                foreach (BpmnModel tempModel in subModels)
                {
                    if (tempModel.FlowLocationMap.Count > 0 || tempModel.LocationMap.Count > 0)
                    {
                        BPMNDIExport.WriteBPMNDI(tempModel, xtw);
                    }
                }

                // end definitions root element
                xtw.WriteEndElement();
                xtw.WriteEndDocument();

                xtw.Flush();
                outputStream.Seek(0, System.IO.SeekOrigin.Begin);
                byte[] bytes = outputStream.ToArray();

                // cleanup
                outputStream.Close();
                //XElementHelper.Close(xtw);
                xtw.Close();

                return bytes;
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error writing BPMN XML");
                throw new XMLException("Error writing BPMN XML", e);
            }
        }

        protected internal virtual IList<BpmnModel> ParseSubModels(BpmnModel model)
        {
            IList<BpmnModel> subModels = new List<BpmnModel>();

            // find all subprocesses
            ICollection<FlowElement> flowElements = model.MainProcess.FlowElements;
            IDictionary<string, GraphicInfo> locations = new Dictionary<string, GraphicInfo>();
            IDictionary<string, IList<GraphicInfo>> flowLocations = new Dictionary<string, IList<GraphicInfo>>();
            IDictionary<string, GraphicInfo> labelLocations = new Dictionary<string, GraphicInfo>();

            locations.PutAll(model.LocationMap);
            flowLocations.PutAll(model.FlowLocationMap);
            labelLocations.PutAll(model.LabelLocationMap);

            // include main process as separate model
            BpmnModel mainModel = new BpmnModel();
            // set main process in submodel to subprocess
            mainModel.AddProcess(model.MainProcess);
            foreach (FlowElement element in flowElements)
            {
                string elementId = element.Id;
                if (element is SubProcess)
                {
                    ((List<BpmnModel>)subModels).AddRange(ParseSubModels(element, locations, flowLocations, labelLocations));
                }

                if (element is SequenceFlow && null != flowLocations[elementId])
                {
                    // must be an edge
                    mainModel.FlowLocationMap[elementId] = flowLocations[elementId];
                }
                else
                {
                    // do not include data objects because they do not have a corresponding shape in the BPMNDI data
                    if (!(element is DataObject) && null != locations[elementId])
                    {
                        // must be a shape
                        mainModel.LocationMap[elementId] = locations[elementId];
                    }
                }
                // also check for any labels
                if (null != labelLocations[elementId])
                {
                    mainModel.LabelLocationMap[elementId] = labelLocations[elementId];
                }
            }
            // add main process model to list
            subModels.Add(mainModel);

            return subModels;
        }

        private IList<BpmnModel> ParseSubModels(FlowElement subElement, IDictionary<string, GraphicInfo> locations, IDictionary<string, IList<GraphicInfo>> flowLocations, IDictionary<string, GraphicInfo> labelLocations)
        {
            IList<BpmnModel> subModels = new List<BpmnModel>();
            BpmnModel subModel = new BpmnModel();

            // find nested subprocess models
            ICollection<FlowElement> subFlowElements = ((SubProcess)subElement).FlowElements;
            // set main process in submodel to subprocess
            Process newMainProcess = new Process
            {
                Id = subElement.Id
            };
            newMainProcess.FlowElements.ToList().AddRange(subFlowElements);
            newMainProcess.Artifacts.ToList().AddRange(((SubProcess)subElement).Artifacts);
            subModel.AddProcess(newMainProcess);

            foreach (FlowElement element in subFlowElements)
            {
                string elementId = element.Id;
                if (element is SubProcess)
                {
                    ((List<BpmnModel>)subModels).AddRange(ParseSubModels(element, locations, flowLocations, labelLocations));
                }

                if (element is SequenceFlow && null != flowLocations[elementId])
                {
                    // must be an edge
                    subModel.FlowLocationMap[elementId] = flowLocations[elementId];
                }
                else
                {
                    // do not include data objects because they do not have a corresponding shape in the BPMNDI data
                    if (!(element is DataObject) && null != locations[elementId])
                    {
                        // must be a shape
                        subModel.LocationMap[elementId] = locations[elementId];
                    }
                }
                // also check for any labels
                if (null != labelLocations[elementId])
                {
                    subModel.LabelLocationMap[elementId] = labelLocations[elementId];
                }
            }
            subModels.Add(subModel);

            return subModels;
        }
    }
}