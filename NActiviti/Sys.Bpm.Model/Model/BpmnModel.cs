using Newtonsoft.Json;
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
namespace Sys.Workflow.Bpmn.Models
{
    public class BpmnModel
    {

        protected internal IDictionary<string, IList<ExtensionAttribute>> definitionsAttributes = new Dictionary<string, IList<ExtensionAttribute>>();
        protected internal IList<Process> processes = new List<Process>();
        protected internal IDictionary<string, GraphicInfo> locationMap = new Dictionary<string, GraphicInfo>();
        protected internal IDictionary<string, GraphicInfo> labelLocationMap = new Dictionary<string, GraphicInfo>();
        protected internal IDictionary<string, IList<GraphicInfo>> flowLocationMap = new Dictionary<string, IList<GraphicInfo>>();
        protected internal IList<Signal> signals = new List<Signal>();
        protected internal IDictionary<string, MessageFlow> messageFlowMap = new Dictionary<string, MessageFlow>();
        protected internal IDictionary<string, Message> messageMap = new Dictionary<string, Message>();
        protected internal IDictionary<string, string> errorMap = new Dictionary<string, string>();
        protected internal IDictionary<string, ItemDefinition> itemDefinitionMap = new Dictionary<string, ItemDefinition>();
        protected internal IDictionary<string, DataStore> dataStoreMap = new Dictionary<string, DataStore>();
        protected internal IList<Pool> pools = new List<Pool>();
        protected internal IList<Import> imports = new List<Import>();
        protected internal IList<Interface> interfaces = new List<Interface>();
        protected internal IList<Artifact> globalArtifacts = new List<Artifact>();
        protected internal IList<Resource> resources = new List<Resource>();
        protected internal IDictionary<string, string> namespaceMap = new Dictionary<string, string>();
        protected internal string targetNamespace;
        protected internal string sourceSystemId;
        protected internal IList<string> userTaskFormTypes;
        protected internal IList<string> startEventFormTypes;
        protected internal int nextFlowIdCounter = 1;

        [JsonIgnore]
        protected internal object eventSupport;

        public virtual IDictionary<string, IList<ExtensionAttribute>> DefinitionsAttributes
        {
            get
            {
                return definitionsAttributes;
            }
            set
            {
                this.definitionsAttributes = value;
            }
        }

        public virtual string GetDefinitionsAttributeValue(string @namespace, string name)
        {
            IList<ExtensionAttribute> attributes = DefinitionsAttributes[name];
            if (attributes != null && attributes.Count > 0)
            {
                foreach (ExtensionAttribute attribute in attributes)
                {
                    if (@namespace.Equals(attribute.Namespace))
                    {
                        return attribute.Value;
                    }
                }
            }
            return null;
        }

        public virtual void AddDefinitionsAttribute(ExtensionAttribute attribute)
        {
            if (attribute != null && !string.IsNullOrWhiteSpace(attribute.Name))
            {
                if (!this.definitionsAttributes.ContainsKey(attribute.Name))
                {
                    List<ExtensionAttribute> attributeList = new List<ExtensionAttribute>();
                    this.definitionsAttributes[attribute.Name] = attributeList;
                }
                this.definitionsAttributes[attribute.Name].Add(attribute);
            }
        }


        public virtual Process MainProcess
        {
            get
            {
                if (Pools.Count > 0)
                {
                    return GetProcess(Pools[0].Id);
                }
                else
                {
                    return GetProcess(null);
                }
            }
        }

        public virtual Process GetProcess(string poolRef)
        {
            foreach (Process process in processes)
            {
                bool foundPool = false;
                foreach (Pool pool in pools)
                {
                    if (!string.IsNullOrWhiteSpace(pool.ProcessRef) && pool.ProcessRef.Equals(process.Id, StringComparison.CurrentCultureIgnoreCase))
                    {

                        if (!(poolRef is null))
                        {
                            if (pool.Id.Equals(poolRef, StringComparison.CurrentCultureIgnoreCase))
                            {
                                foundPool = true;
                            }
                        }
                        else
                        {
                            foundPool = true;
                        }
                    }
                }

                if (poolRef is null && !foundPool)
                {
                    return process;
                }
                else if (!(poolRef is null) && foundPool)
                {
                    return process;
                }
            }

            return null;
        }

        public virtual Process GetProcessById(string id)
        {
            foreach (Process process in processes)
            {
                if (process.Id.Equals(id))
                {
                    return process;
                }
            }
            return null;
        }

        public virtual IList<Process> Processes
        {
            get
            {
                return processes;
            }
        }

        public virtual void AddProcess(Process process)
        {
            processes.Add(process);
        }

        public virtual Pool GetPool(string id)
        {
            Pool foundPool = null;
            if (!string.IsNullOrWhiteSpace(id))
            {
                foreach (Pool pool in pools)
                {
                    if (id.Equals(pool.Id))
                    {
                        foundPool = pool;
                        break;
                    }
                }
            }
            return foundPool;
        }

        public virtual Lane GetLane(string id)
        {
            Lane foundLane = null;
            if (!string.IsNullOrWhiteSpace(id))
            {
                foreach (Process process in processes)
                {
                    foreach (Lane lane in process.Lanes)
                    {
                        if (id.Equals(lane.Id))
                        {
                            foundLane = lane;
                            break;
                        }
                    }
                    if (foundLane != null)
                    {
                        break;
                    }
                }
            }
            return foundLane;
        }

        public virtual FlowElement GetFlowElement(string id)
        {
            FlowElement foundFlowElement = null;
            foreach (Process process in processes)
            {
                foundFlowElement = process.FindFlowElement(id);
                if (foundFlowElement != null)
                {
                    break;
                }
            }

            if (foundFlowElement == null)
            {
                foreach (Process process in processes)
                {
                    foreach (FlowElement flowElement in process.FindFlowElementsOfType<SubProcess>())
                    {
                        foundFlowElement = GetFlowElementInSubProcess(id, (SubProcess)flowElement);
                        if (foundFlowElement != null)
                        {
                            break;
                        }
                    }
                    if (foundFlowElement != null)
                    {
                        break;
                    }
                }
            }

            return foundFlowElement;
        }

        protected internal virtual FlowElement GetFlowElementInSubProcess(string id, SubProcess subProcess)
        {
            FlowElement foundFlowElement = subProcess.FindFlowElement(id);
            if (foundFlowElement == null)
            {
                foreach (FlowElement flowElement in subProcess.FlowElements)
                {
                    if (flowElement is SubProcess)
                    {
                        foundFlowElement = GetFlowElementInSubProcess(id, (SubProcess)flowElement);
                        if (foundFlowElement != null)
                        {
                            break;
                        }
                    }
                }
            }
            return foundFlowElement;
        }

        public virtual Artifact GetArtifact(string id)
        {
            Artifact foundArtifact = null;
            foreach (Process process in processes)
            {
                foundArtifact = process.GetArtifact(id);
                if (foundArtifact != null)
                {
                    break;
                }
            }

            if (foundArtifact == null)
            {
                foreach (Process process in processes)
                {
                    foreach (FlowElement flowElement in process.FindFlowElementsOfType<SubProcess>())
                    {
                        foundArtifact = GetArtifactInSubProcess(id, (SubProcess)flowElement);
                        if (foundArtifact != null)
                        {
                            break;
                        }
                    }
                    if (foundArtifact != null)
                    {
                        break;
                    }
                }
            }

            return foundArtifact;
        }

        protected internal virtual Artifact GetArtifactInSubProcess(string id, SubProcess subProcess)
        {
            Artifact foundArtifact = subProcess.GetArtifact(id);
            if (foundArtifact == null)
            {
                foreach (FlowElement flowElement in subProcess.FlowElements)
                {
                    if (flowElement is SubProcess)
                    {
                        foundArtifact = GetArtifactInSubProcess(id, (SubProcess)flowElement);
                        if (foundArtifact != null)
                        {
                            break;
                        }
                    }
                }
            }
            return foundArtifact;
        }

        public virtual void AddGraphicInfo(string key, GraphicInfo graphicInfo)
        {
            locationMap[key] = graphicInfo;
        }

        public virtual GraphicInfo GetGraphicInfo(string key)
        {
            locationMap.TryGetValue(key, out var gi);
            return gi;
        }

        public virtual void RemoveGraphicInfo(string key)
        {
            locationMap.Remove(key);
        }

        public virtual IList<GraphicInfo> GetFlowLocationGraphicInfo(string key)
        {
            flowLocationMap.TryGetValue(key, out var list);
            return list;
        }

        public virtual void RemoveFlowGraphicInfoList(string key)
        {
            flowLocationMap.Remove(key);
        }

        public virtual IDictionary<string, GraphicInfo> LocationMap
        {
            get
            {
                return locationMap;
            }
        }

        public virtual bool HasDiagramInterchangeInfo()
        {
            return locationMap.Count > 0;
        }

        public virtual IDictionary<string, IList<GraphicInfo>> FlowLocationMap
        {
            get
            {
                return flowLocationMap;
            }
        }

        public virtual GraphicInfo GetLabelGraphicInfo(string key)
        {
            labelLocationMap.TryGetValue(key, out var gi);
            return gi;
        }

        public virtual void AddLabelGraphicInfo(string key, GraphicInfo graphicInfo)
        {
            labelLocationMap[key] = graphicInfo;
        }

        public virtual void RemoveLabelGraphicInfo(string key)
        {
            labelLocationMap.Remove(key);
        }

        public virtual IDictionary<string, GraphicInfo> LabelLocationMap
        {
            get
            {
                return labelLocationMap;
            }
        }

        public virtual void AddFlowGraphicInfoList(string key, IList<GraphicInfo> graphicInfoList)
        {
            flowLocationMap[key] = graphicInfoList;
        }

        public virtual ICollection<Resource> Resources
        {
            get
            {
                return resources;
            }
            set
            {
                if (value != null)
                {
                    resources.Clear();
                    ((List<Resource>)resources).AddRange(value);
                }
            }
        }


        public virtual void AddResource(Resource resource)
        {
            if (resource != null)
            {
                resources.Add(resource);
            }
        }

        public virtual bool ContainsResourceId(string resourceId)
        {
            return GetResource(resourceId) != null;
        }

        public virtual Resource GetResource(string id)
        {
            foreach (Resource resource in resources)
            {
                if (id.Equals(resource.Id))
                {
                    return resource;
                }
            }
            return null;
        }

        public virtual ICollection<Signal> Signals
        {
            get
            {
                return signals;
            }
            set
            {
                if (value != null)
                {
                    signals.Clear();
                    ((List<Signal>)signals).AddRange(value);
                }
            }
        }


        public virtual void AddSignal(Signal signal)
        {
            if (signal != null)
            {
                signals.Add(signal);
            }
        }

        public virtual bool ContainsSignalId(string signalId)
        {
            return GetSignal(signalId) != null;
        }

        public virtual Signal GetSignal(string id)
        {
            foreach (Signal signal in signals)
            {
                if (id.Equals(signal.Id))
                {
                    return signal;
                }
            }
            return null;
        }

        public virtual IDictionary<string, MessageFlow> MessageFlows
        {
            get
            {
                return messageFlowMap;
            }
            set
            {
                this.messageFlowMap = value;
            }
        }


        public virtual void AddMessageFlow(MessageFlow messageFlow)
        {
            if (messageFlow != null && !string.IsNullOrWhiteSpace(messageFlow.Id))
            {
                messageFlowMap[messageFlow.Id] = messageFlow;
            }
        }

        public virtual MessageFlow GetMessageFlow(string id)
        {
            return messageFlowMap[id];
        }

        public virtual bool ContainsMessageFlowId(string messageFlowId)
        {
            return messageFlowMap.ContainsKey(messageFlowId);
        }

        public virtual ICollection<Message> Messages
        {
            get
            {
                return messageMap.Values;
            }
            set
            {
                if (value != null)
                {
                    messageMap.Clear();
                    foreach (Message message in value)
                    {
                        AddMessage(message);
                    }
                }
            }
        }


        public virtual void AddMessage(Message message)
        {
            if (message != null && !string.IsNullOrWhiteSpace(message.Id))
            {
                messageMap[message.Id] = message;
            }
        }

        public virtual Message GetMessage(string id)
        {
            Message result = messageMap[id];
            if (result == null)
            {
                int indexOfNS = id.IndexOf(":", StringComparison.Ordinal);
                if (indexOfNS > 0)
                {
                    string idNamespace = id.Substring(0, indexOfNS);
                    if (idNamespace.Equals(this.TargetNamespace, StringComparison.CurrentCultureIgnoreCase))
                    {
                        id = id.Substring(indexOfNS + 1);
                    }
                    result = messageMap[id];
                }
            }
            return result;
        }

        public virtual bool ContainsMessageId(string messageId)
        {
            return messageMap.ContainsKey(messageId);
        }

        public virtual IDictionary<string, string> Errors
        {
            get
            {
                return errorMap;
            }
            set
            {
                this.errorMap = value;
            }
        }


        public virtual void AddError(string errorRef, string errorCode)
        {
            if (!string.IsNullOrWhiteSpace(errorRef))
            {
                errorMap[errorRef] = errorCode;
            }
        }

        public virtual bool ContainsErrorRef(string errorRef)
        {
            return errorMap.ContainsKey(errorRef);
        }

        public virtual IDictionary<string, ItemDefinition> ItemDefinitions
        {
            get
            {
                return itemDefinitionMap;
            }
            set
            {
                this.itemDefinitionMap = value;
            }
        }


        public virtual void AddItemDefinition(string id, ItemDefinition item)
        {
            if (!string.IsNullOrWhiteSpace(id))
            {
                itemDefinitionMap[id] = item;
            }
        }

        public virtual bool ContainsItemDefinitionId(string id)
        {
            return itemDefinitionMap.ContainsKey(id);
        }

        public virtual IDictionary<string, DataStore> DataStores
        {
            get
            {
                return dataStoreMap;
            }
            set
            {
                this.dataStoreMap = value;
            }
        }


        public virtual DataStore GetDataStore(string id)
        {
            DataStore dataStore = null;
            if (dataStoreMap.ContainsKey(id))
            {
                dataStore = dataStoreMap[id];
            }
            return dataStore;
        }

        public virtual void AddDataStore(string id, DataStore dataStore)
        {
            if (!string.IsNullOrWhiteSpace(id))
            {
                dataStoreMap[id] = dataStore;
            }
        }

        public virtual bool ContainsDataStore(string id)
        {
            return dataStoreMap.ContainsKey(id);
        }

        public virtual IList<Pool> Pools
        {
            get
            {
                return pools;
            }
            set
            {
                this.pools = value ?? new List<Pool>();
            }
        }


        public virtual IList<Import> Imports
        {
            get
            {
                return imports;
            }
            set
            {
                this.imports = value ?? new List<Import>();
            }
        }


        public virtual IList<Interface> Interfaces
        {
            get
            {
                return interfaces;
            }
            set
            {
                this.interfaces = value ?? new List<Interface>();
            }
        }


        public virtual IList<Artifact> GlobalArtifacts
        {
            get
            {
                return globalArtifacts;
            }
            set
            {
                this.globalArtifacts = value ?? new List<Artifact>();
            }
        }


        public virtual void AddNamespace(string prefix, string uri)
        {
            namespaceMap[prefix] = uri;
        }

        public virtual bool ContainsNamespacePrefix(string prefix)
        {
            return namespaceMap.ContainsKey(prefix);
        }

        public virtual string GetNamespace(string prefix)
        {
            return namespaceMap[prefix];
        }

        public virtual IDictionary<string, string> Namespaces
        {
            get
            {
                return namespaceMap;
            }
        }

        public virtual string TargetNamespace
        {
            get
            {
                return targetNamespace;
            }
            set
            {
                this.targetNamespace = value;
            }
        }


        public virtual string SourceSystemId
        {
            get
            {
                return sourceSystemId;
            }
            set
            {
                this.sourceSystemId = value;
            }
        }


        public virtual IList<string> UserTaskFormTypes
        {
            get
            {
                return userTaskFormTypes;
            }
            set
            {
                this.userTaskFormTypes = value ?? new List<string>();
            }
        }


        public virtual IList<string> StartEventFormTypes
        {
            get
            {
                return startEventFormTypes;
            }
            set
            {
                this.startEventFormTypes = value ?? new List<string>();
            }
        }


        [JsonIgnore]
        public virtual object EventSupport
        {
            get
            {
                return eventSupport;
            }
            set
            {
                this.eventSupport = value;
            }
        }

    }

}