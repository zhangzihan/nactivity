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
    using Sys.Workflow;
    using System.Collections.Concurrent;

    public class Process : BaseElement, IFlowElementsContainer, IHasExecutionListeners
    {

        protected internal string name;
        protected internal bool executable = true;
        protected internal string documentation;
        protected internal IOSpecification ioSpecification;
        protected internal IList<ActivitiListener> executionListeners = new List<ActivitiListener>();
        protected internal IList<Lane> lanes = new List<Lane>();
        protected internal IList<FlowElement> flowElementList = new List<FlowElement>();
        protected internal IList<ValuedDataObject> dataObjects = new List<ValuedDataObject>();
        protected internal IList<Artifact> artifactList = new List<Artifact>();
        protected internal IList<string> candidateStarterUsers = new List<string>();
        protected internal IList<string> candidateStarterGroups = new List<string>();
        protected internal IList<EventListener> eventListeners = new List<EventListener>();
        protected internal IDictionary<string, FlowElement> flowElementMap = new Dictionary<string, FlowElement>();

        // Added during process definition parsing
        protected internal FlowElement initialFlowElement;

        public Process()
        {

        }

        public virtual string Documentation
        {
            get
            {
                return documentation;
            }
            set
            {
                this.documentation = value;
            }
        }


        public virtual string Name
        {
            get
            {
                return name;
            }
            set
            {
                this.name = value;
            }
        }


        public virtual bool Executable
        {
            get
            {
                return executable;
            }
            set
            {
                this.executable = value;
            }
        }


        public virtual IOSpecification IoSpecification
        {
            get
            {
                return ioSpecification;
            }
            set
            {
                this.ioSpecification = value;
            }
        }


        public virtual IList<ActivitiListener> ExecutionListeners
        {
            get
            {
                return executionListeners;
            }
            set
            {
                this.executionListeners = value;
            }
        }


        public virtual IList<Lane> Lanes
        {
            get
            {
                return lanes;
            }
            set
            {
                this.lanes = value;
            }
        }


        public virtual IDictionary<string, FlowElement> FlowElementMap
        {
            get
            {
                return flowElementMap;
            }
            set
            {
                this.flowElementMap = value;
            }
        }


        public virtual bool ContainsFlowElementId(string id)
        {
            return flowElementMap.ContainsKey(id);
        }

        public virtual FlowElement FindFlowElement(string flowElementId)
        {
            return GetFlowElement(flowElementId, false);
        }

        /// <param name="searchRecurive">: searches the whole process, including subprocesses </param>
        public virtual FlowElement GetFlowElement(string flowElementId, bool searchRecurive)
        {
            if (searchRecurive)
            {
                if (flowElementId == null)
                {
                    return null;
                }

                flowElementMap.TryGetValue(flowElementId, out var element);

                return element;
            }
            else
            {
                return FindFlowElementInList(flowElementId);
            }
        }

        public virtual IList<Association> FindAssociationsWithSourceRefRecursive(string sourceRef)
        {
            return FindAssociationsWithSourceRefRecursive(this, sourceRef);
        }

        protected internal virtual IList<Association> FindAssociationsWithSourceRefRecursive(IFlowElementsContainer flowElementsContainer, string sourceRef)
        {
            IList<Association> associations = new List<Association>();
            foreach (Artifact artifact in flowElementsContainer.Artifacts)
            {
                if (artifact is Association association)
                {
                    if (!(association.SourceRef is null) && !(association.TargetRef is null) && association.SourceRef.Equals(sourceRef))
                    {
                        associations.Add(association);
                    }
                }
            }

            foreach (FlowElement flowElement in flowElementsContainer.FlowElements)
            {
                if (flowElement is IFlowElementsContainer)
                {
                    ((List<Association>)associations).AddRange(FindAssociationsWithSourceRefRecursive((IFlowElementsContainer)flowElement, sourceRef));
                }
            }
            return associations;
        }

        public virtual IList<Association> FindAssociationsWithTargetRefRecursive(string targetRef)
        {
            return FindAssociationsWithTargetRefRecursive(this, targetRef);
        }

        protected internal virtual IList<Association> FindAssociationsWithTargetRefRecursive(IFlowElementsContainer flowElementsContainer, string targetRef)
        {
            IList<Association> associations = new List<Association>();
            foreach (Artifact artifact in flowElementsContainer.Artifacts)
            {
                if (artifact is Association association)
                {
                    if (!(association.TargetRef is null) && association.TargetRef.Equals(targetRef))
                    {
                        associations.Add(association);
                    }
                }
            }

            foreach (FlowElement flowElement in flowElementsContainer.FlowElements)
            {
                if (flowElement is IFlowElementsContainer)
                {
                    ((List<Association>)associations).AddRange(FindAssociationsWithTargetRefRecursive((IFlowElementsContainer)flowElement, targetRef));
                }
            }
            return associations;
        }

        /// <summary>
        /// Searches the whole process, including subprocesses
        /// </summary>
        public virtual IFlowElementsContainer GetFlowElementsContainer(string flowElementId)
        {
            return GetFlowElementsContainer(this, flowElementId);
        }

        protected internal virtual IFlowElementsContainer GetFlowElementsContainer(IFlowElementsContainer flowElementsContainer, string flowElementId)
        {
            foreach (FlowElement flowElement in flowElementsContainer.FlowElements)
            {
                if (!(flowElement.Id is null) && flowElement.Id.Equals(flowElementId))
                {
                    return flowElementsContainer;
                }
                else if (flowElement is IFlowElementsContainer)
                {
                    IFlowElementsContainer result = GetFlowElementsContainer((IFlowElementsContainer)flowElement, flowElementId);
                    if (result != null)
                    {
                        return result;
                    }
                }
            }
            return null;
        }

        protected internal virtual FlowElement FindFlowElementInList(string flowElementId)
        {
            foreach (FlowElement f in flowElementList)
            {
                if (!string.IsNullOrWhiteSpace(f.Id) && string.Compare(f.Id, flowElementId, true) == 0)
                {
                    return f;
                }
            }
            return null;
        }

        public virtual IList<FlowElement> FlowElements
        {
            get
            {
                return flowElementList;
            }
        }

        public virtual void AddFlowElement(FlowElement element)
        {
            flowElementList.Add(element);
            element.ParentContainer = this;
            if (!string.IsNullOrWhiteSpace(element.Id))
            {
                flowElementMap[element.Id] = element;
            }
            if (element is IFlowElementsContainer)
            {
                flowElementMap.PutAll(((IFlowElementsContainer)element).FlowElementMap);
            }
        }

        public virtual void AddFlowElementToMap(FlowElement element)
        {
            if (element != null && !string.IsNullOrWhiteSpace(element.Id))
            {
                flowElementMap[element.Id] = element;
            }
        }

        public virtual void RemoveFlowElement(string elementId)
        {
            flowElementMap.TryGetValue(elementId, out var element);
            if (element != null)
            {
                flowElementList.Remove(element);
                flowElementMap.Remove(element.Id);
            }
        }

        public virtual void RemoveFlowElementFromMap(string elementId)
        {
            if (!string.IsNullOrWhiteSpace(elementId))
            {
                flowElementMap.Remove(elementId);
            }
        }

        public virtual Artifact GetArtifact(string id)
        {
            Artifact foundArtifact = null;
            foreach (Artifact artifact in artifactList)
            {
                if (id.Equals(artifact.Id))
                {
                    foundArtifact = artifact;
                    break;
                }
            }
            return foundArtifact;
        }

        public virtual IList<Artifact> Artifacts
        {
            get
            {
                return artifactList;
            }
        }

        public virtual void AddArtifact(Artifact artifact)
        {
            artifactList.Add(artifact);
        }

        public virtual void RemoveArtifact(string artifactId)
        {
            Artifact artifact = GetArtifact(artifactId);
            if (artifact != null)
            {
                artifactList.Remove(artifact);
            }
        }

        public virtual IList<string> CandidateStarterUsers
        {
            get
            {
                return candidateStarterUsers;
            }
            set
            {
                this.candidateStarterUsers = value;
            }
        }


        public virtual IList<string> CandidateStarterGroups
        {
            get
            {
                return candidateStarterGroups;
            }
            set
            {
                this.candidateStarterGroups = value;
            }
        }


        public virtual IList<EventListener> EventListeners
        {
            get
            {
                return eventListeners;
            }
            set
            {
                this.eventListeners = value;
            }
        }


        public virtual IList<FlowElementType> FindFlowElementsOfType<FlowElementType>() where FlowElementType : FlowElement
        {
            return FindFlowElementsOfType<FlowElementType>(true);
        }

        public virtual IList<FlowElementType> FindFlowElementsOfType<FlowElementType>(bool goIntoSubprocesses) where FlowElementType : FlowElement
        {
            IList<FlowElementType> foundFlowElements = new List<FlowElementType>();
            foreach (FlowElement flowElement in this.FlowElements)
            {
                if (flowElement is FlowElementType)
                {
                    foundFlowElements.Add((FlowElementType)flowElement);
                }
                if (flowElement is SubProcess)
                {
                    if (goIntoSubprocesses)
                    {
                        ((List<FlowElementType>)foundFlowElements).AddRange(FindFlowElementsInSubProcessOfType<FlowElementType>((SubProcess)flowElement));
                    }
                }
            }
            return foundFlowElements;
        }

        public virtual IList<FlowElementType> FindFlowElementsInSubProcessOfType<FlowElementType>(SubProcess subProcess) where FlowElementType : FlowElement
        {
            return FindFlowElementsInSubProcessOfType<FlowElementType>(subProcess, true);
        }

        public virtual IList<FlowElementType> FindFlowElementsInSubProcessOfType<FlowElementType>(SubProcess subProcess, bool goIntoSubprocesses) where FlowElementType : FlowElement
        {

            IList<FlowElementType> foundFlowElements = new List<FlowElementType>();
            foreach (FlowElement flowElement in subProcess.FlowElements)
            {
                if (flowElement is FlowElementType)
                {
                    foundFlowElements.Add((FlowElementType)flowElement);
                }
                if (flowElement is SubProcess)
                {
                    if (goIntoSubprocesses)
                    {
                        ((List<FlowElementType>)foundFlowElements).AddRange(FindFlowElementsInSubProcessOfType<FlowElementType>((SubProcess)flowElement));
                    }
                }
            }
            return foundFlowElements;
        }

        public virtual IFlowElementsContainer FindParent(FlowElement childElement)
        {
            return FindParent(childElement, this);
        }

        public virtual IFlowElementsContainer FindParent(FlowElement childElement, IFlowElementsContainer flowElementsContainer)
        {
            foreach (FlowElement flowElement in flowElementsContainer.FlowElements)
            {
                if (!(childElement.Id is null) && childElement.Id.Equals(flowElement.Id))
                {
                    return flowElementsContainer;
                }
                if (flowElement is IFlowElementsContainer)
                {
                    IFlowElementsContainer result = FindParent(childElement, (IFlowElementsContainer)flowElement);
                    if (result != null)
                    {
                        return result;
                    }
                }
            }
            return null;
        }

        public override BaseElement Clone()
        {
            Process clone = new Process()
            {
                Values = this
            };
            return clone;
        }

        public override BaseElement Values
        {
            set
            {
                base.Values = value;

                var val = value as Process;

                //    setBpmnModel(bpmnModel);
                Name = val.Name;
                Executable = val.Executable;
                Documentation = val.Documentation;
                if (val.IoSpecification != null)
                {
                    IoSpecification = val.IoSpecification.Clone() as IOSpecification;
                }

                executionListeners = new List<ActivitiListener>();
                if (val.ExecutionListeners != null && val.ExecutionListeners.Count > 0)
                {
                    foreach (ActivitiListener listener in val.ExecutionListeners)
                    {
                        executionListeners.Add(listener.Clone() as ActivitiListener);
                    }
                }

                candidateStarterUsers = new List<string>();
                if (val.CandidateStarterUsers != null && val.CandidateStarterUsers.Count > 0)
                {
                    ((List<string>)candidateStarterUsers).AddRange(val.CandidateStarterUsers);
                }

                candidateStarterGroups = new List<string>();
                if (val.CandidateStarterGroups != null && val.CandidateStarterGroups.Count > 0)
                {
                    ((List<string>)candidateStarterGroups).AddRange(val.CandidateStarterGroups);
                }

                eventListeners = new List<EventListener>();
                if (val.EventListeners != null && val.EventListeners.Count > 0)
                {
                    foreach (EventListener listener in val.EventListeners)
                    {
                        eventListeners.Add(listener.Clone() as EventListener);
                    }
                }

                /*
                 * This is required because data objects in Designer have no DI info and are added as properties, not flow elements
                 * 
                 * Determine the differences between the 2 elements' data object
                 */
                foreach (ValuedDataObject thisObject in DataObjects)
                {
                    bool exists = false;
                    foreach (ValuedDataObject otherObject in val.DataObjects)
                    {
                        if (thisObject.Id.Equals(otherObject.Id))
                        {
                            exists = true;
                        }
                    }
                    if (!exists)
                    {
                        // missing object
                        RemoveFlowElement(thisObject.Id);
                    }
                }

                dataObjects = new List<ValuedDataObject>();
                if (val.DataObjects != null && val.DataObjects.Count > 0)
                {
                    foreach (ValuedDataObject dataObject in val.DataObjects)
                    {
                        ValuedDataObject clone = dataObject.Clone() as ValuedDataObject;
                        dataObjects.Add(clone);
                        // add it to the list of FlowElements
                        // if it is already there, remove it first so order is same as
                        // data object list
                        RemoveFlowElement(clone.Id);
                        AddFlowElement(clone);
                    }
                }
            }
        }

        public virtual IList<ValuedDataObject> DataObjects
        {
            get
            {
                return dataObjects;
            }
            set
            {
                this.dataObjects = value;
            }
        }


        public virtual FlowElement InitialFlowElement
        {
            get
            {
                return initialFlowElement;
            }
            set
            {
                this.initialFlowElement = value;
            }
        }


    }

}