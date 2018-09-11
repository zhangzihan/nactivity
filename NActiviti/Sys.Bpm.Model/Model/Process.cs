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
namespace org.activiti.bpmn.model
{
    using Sys.Bpm;
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
        protected internal IDictionary<string, FlowElement> flowElementMap = new ConcurrentDictionary<string, FlowElement>();

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


        public virtual bool containsFlowElementId(string id)
        {
            return flowElementMap.ContainsKey(id);
        }

        public virtual FlowElement getFlowElement(string flowElementId)
        {
            return getFlowElement(flowElementId, false);
        }

        /// <param name="searchRecurive">: searches the whole process, including subprocesses </param>
        public virtual FlowElement getFlowElement(string flowElementId, bool searchRecurive)
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
                return findFlowElementInList(flowElementId);
            }
        }

        public virtual IList<Association> findAssociationsWithSourceRefRecursive(string sourceRef)
        {
            return findAssociationsWithSourceRefRecursive(this, sourceRef);
        }

        protected internal virtual IList<Association> findAssociationsWithSourceRefRecursive(IFlowElementsContainer flowElementsContainer, string sourceRef)
        {
            IList<Association> associations = new List<Association>();
            foreach (Artifact artifact in flowElementsContainer.Artifacts)
            {
                if (artifact is Association)
                {
                    Association association = (Association)artifact;
                    if (!string.ReferenceEquals(association.SourceRef, null) && !string.ReferenceEquals(association.TargetRef, null) && association.SourceRef.Equals(sourceRef))
                    {
                        associations.Add(association);
                    }
                }
            }

            foreach (FlowElement flowElement in flowElementsContainer.FlowElements)
            {
                if (flowElement is IFlowElementsContainer)
                {
                    ((List<Association>)associations).AddRange(findAssociationsWithSourceRefRecursive((IFlowElementsContainer)flowElement, sourceRef));
                }
            }
            return associations;
        }

        public virtual IList<Association> findAssociationsWithTargetRefRecursive(string targetRef)
        {
            return findAssociationsWithTargetRefRecursive(this, targetRef);
        }

        protected internal virtual IList<Association> findAssociationsWithTargetRefRecursive(IFlowElementsContainer flowElementsContainer, string targetRef)
        {
            IList<Association> associations = new List<Association>();
            foreach (Artifact artifact in flowElementsContainer.Artifacts)
            {
                if (artifact is Association)
                {
                    Association association = (Association)artifact;
                    if (!string.ReferenceEquals(association.TargetRef, null) && association.TargetRef.Equals(targetRef))
                    {
                        associations.Add(association);
                    }
                }
            }

            foreach (FlowElement flowElement in flowElementsContainer.FlowElements)
            {
                if (flowElement is IFlowElementsContainer)
                {
                    ((List<Association>)associations).AddRange(findAssociationsWithTargetRefRecursive((IFlowElementsContainer)flowElement, targetRef));
                }
            }
            return associations;
        }

        /// <summary>
        /// Searches the whole process, including subprocesses
        /// </summary>
        public virtual IFlowElementsContainer getFlowElementsContainer(string flowElementId)
        {
            return getFlowElementsContainer(this, flowElementId);
        }

        protected internal virtual IFlowElementsContainer getFlowElementsContainer(IFlowElementsContainer flowElementsContainer, string flowElementId)
        {
            foreach (FlowElement flowElement in flowElementsContainer.FlowElements)
            {
                if (!string.ReferenceEquals(flowElement.Id, null) && flowElement.Id.Equals(flowElementId))
                {
                    return flowElementsContainer;
                }
                else if (flowElement is IFlowElementsContainer)
                {
                    IFlowElementsContainer result = getFlowElementsContainer((IFlowElementsContainer)flowElement, flowElementId);
                    if (result != null)
                    {
                        return result;
                    }
                }
            }
            return null;
        }

        protected internal virtual FlowElement findFlowElementInList(string flowElementId)
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

        public virtual ICollection<FlowElement> FlowElements
        {
            get
            {
                return flowElementList;
            }
        }

        public virtual void addFlowElement(FlowElement element)
        {
            flowElementList.Add(element);
            element.ParentContainer = this;
            if (!string.IsNullOrWhiteSpace(element.Id))
            {
                flowElementMap[element.Id] = element;
            }
            if (element is IFlowElementsContainer)
            {
                flowElementMap.putAll(((IFlowElementsContainer)element).FlowElementMap);
            }
        }

        public virtual void addFlowElementToMap(FlowElement element)
        {
            if (element != null && !string.IsNullOrWhiteSpace(element.Id))
            {
                flowElementMap[element.Id] = element;
            }
        }

        public virtual void removeFlowElement(string elementId)
        {
            flowElementMap.TryGetValue(elementId, out var element);
            if (element != null)
            {
                flowElementList.Remove(element);
                flowElementMap.Remove(element.Id);
            }
        }

        public virtual void removeFlowElementFromMap(string elementId)
        {
            if (!string.IsNullOrWhiteSpace(elementId))
            {
                flowElementMap.Remove(elementId);
            }
        }

        public virtual Artifact getArtifact(string id)
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

        public virtual ICollection<Artifact> Artifacts
        {
            get
            {
                return artifactList;
            }
        }

        public virtual void addArtifact(Artifact artifact)
        {
            artifactList.Add(artifact);
        }

        public virtual void removeArtifact(string artifactId)
        {
            Artifact artifact = getArtifact(artifactId);
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


        public virtual IList<FlowElementType> findFlowElementsOfType<FlowElementType>() where FlowElementType : FlowElement
        {
            return findFlowElementsOfType<FlowElementType>(true);
        }

        public virtual IList<FlowElementType> findFlowElementsOfType<FlowElementType>(bool goIntoSubprocesses) where FlowElementType : FlowElement
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
                        ((List<FlowElementType>)foundFlowElements).AddRange(findFlowElementsInSubProcessOfType<FlowElementType>((SubProcess)flowElement));
                    }
                }
            }
            return foundFlowElements;
        }

        public virtual IList<FlowElementType> findFlowElementsInSubProcessOfType<FlowElementType>(SubProcess subProcess) where FlowElementType : FlowElement
        {
            return findFlowElementsInSubProcessOfType<FlowElementType>(subProcess, true);
        }

        public virtual IList<FlowElementType> findFlowElementsInSubProcessOfType<FlowElementType>(SubProcess subProcess, bool goIntoSubprocesses) where FlowElementType : FlowElement
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
                        ((List<FlowElementType>)foundFlowElements).AddRange(findFlowElementsInSubProcessOfType<FlowElementType>((SubProcess)flowElement));
                    }
                }
            }
            return foundFlowElements;
        }

        public virtual IFlowElementsContainer findParent(FlowElement childElement)
        {
            return findParent(childElement, this);
        }

        public virtual IFlowElementsContainer findParent(FlowElement childElement, IFlowElementsContainer flowElementsContainer)
        {
            foreach (FlowElement flowElement in flowElementsContainer.FlowElements)
            {
                if (!string.ReferenceEquals(childElement.Id, null) && childElement.Id.Equals(flowElement.Id))
                {
                    return flowElementsContainer;
                }
                if (flowElement is IFlowElementsContainer)
                {
                    IFlowElementsContainer result = findParent(childElement, (IFlowElementsContainer)flowElement);
                    if (result != null)
                    {
                        return result;
                    }
                }
            }
            return null;
        }

        public override BaseElement clone()
        {
            Process clone = new Process();
            clone.Values = this;
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
                    IoSpecification = val.IoSpecification.clone() as IOSpecification;
                }

                executionListeners = new List<ActivitiListener>();
                if (val.ExecutionListeners != null && val.ExecutionListeners.Count > 0)
                {
                    foreach (ActivitiListener listener in val.ExecutionListeners)
                    {
                        executionListeners.Add(listener.clone() as ActivitiListener);
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
                        eventListeners.Add(listener.clone() as EventListener);
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
                        removeFlowElement(thisObject.Id);
                    }
                }

                dataObjects = new List<ValuedDataObject>();
                if (val.DataObjects != null && val.DataObjects.Count > 0)
                {
                    foreach (ValuedDataObject dataObject in val.DataObjects)
                    {
                        ValuedDataObject clone = dataObject.clone() as ValuedDataObject;
                        dataObjects.Add(clone);
                        // add it to the list of FlowElements
                        // if it is already there, remove it first so order is same as
                        // data object list
                        removeFlowElement(clone.Id);
                        addFlowElement(clone);
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