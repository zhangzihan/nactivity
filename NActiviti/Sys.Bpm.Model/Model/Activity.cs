using Sys.Workflow.bpmn.constants;
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
namespace Sys.Workflow.bpmn.model
{

    public abstract class Activity : FlowNode
    {

        protected internal string defaultFlow;
        protected internal bool forCompensation;
        protected internal MultiInstanceLoopCharacteristics loopCharacteristics;
        protected internal IOSpecification ioSpecification;
        protected internal IList<DataAssociation> dataInputAssociations = new List<DataAssociation>();
        protected internal IList<DataAssociation> dataOutputAssociations = new List<DataAssociation>();
        protected internal IList<BoundaryEvent> boundaryEvents = new List<BoundaryEvent>();
        protected internal string failedJobRetryTimeCycleValue;
        protected internal IList<MapExceptionEntry> mapExceptions = new List<MapExceptionEntry>();

        public virtual string FailedJobRetryTimeCycleValue
        {
            get
            {
                return failedJobRetryTimeCycleValue;
            }
            set
            {
                this.failedJobRetryTimeCycleValue = value;
            }
        }

        public virtual bool ForCompensation
        {
            get
            {
                return forCompensation;
            }
            set
            {
                this.forCompensation = value;
            }
        }


        public virtual IList<BoundaryEvent> BoundaryEvents
        {
            get
            {
                return boundaryEvents;
            }
            set
            {
                this.boundaryEvents = value;
            }
        }


        public virtual string DefaultFlow
        {
            get
            {
                return defaultFlow;
            }
            set
            {
                this.defaultFlow = value;
            }
        }

        public virtual ExtensionAttribute AssigneeType
        {
            get
            {
                return this.GetExtensionElementAttribute(BpmnXMLConstants.ELEMENT_USER_TASK_EXTENSION_ASSIGNE_TYPE);
            }
        }


        public virtual MultiInstanceLoopCharacteristics LoopCharacteristics
        {
            get
            {
                return loopCharacteristics;
            }
            set
            {
                this.loopCharacteristics = value;
            }
        }


        public virtual bool HasMultiInstanceLoopCharacteristics()
        {
            return LoopCharacteristics != null;
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


        public virtual IList<DataAssociation> DataInputAssociations
        {
            get
            {
                return dataInputAssociations;
            }
            set
            {
                this.dataInputAssociations = value;
            }
        }


        public virtual IList<DataAssociation> DataOutputAssociations
        {
            get
            {
                return dataOutputAssociations;
            }
            set
            {
                this.dataOutputAssociations = value;
            }
        }

        public virtual IList<MapExceptionEntry> MapExceptions
        {
            get
            {
                return mapExceptions;
            }
            set
            {
                this.mapExceptions = value;
            }
        }

        public override BaseElement Values
        {
            set
            {
                base.Values = value;
                var val = value as Activity;

                FailedJobRetryTimeCycleValue = val.FailedJobRetryTimeCycleValue;
                DefaultFlow = val.DefaultFlow;
                ForCompensation = val.ForCompensation;
                if (val.LoopCharacteristics != null)
                {
                    LoopCharacteristics = val.LoopCharacteristics.Clone() as MultiInstanceLoopCharacteristics;
                }
                if (val.IoSpecification != null)
                {
                    IoSpecification = val.IoSpecification.Clone() as IOSpecification;
                }

                dataInputAssociations = new List<DataAssociation>();
                if (val.DataInputAssociations != null && val.DataInputAssociations.Count > 0)
                {
                    foreach (DataAssociation association in val.DataInputAssociations)
                    {
                        dataInputAssociations.Add(association.Clone() as DataAssociation);
                    }
                }

                dataOutputAssociations = new List<DataAssociation>();
                if (val.DataOutputAssociations != null && val.DataOutputAssociations.Count > 0)
                {
                    foreach (DataAssociation association in val.DataOutputAssociations)
                    {
                        dataOutputAssociations.Add(association.Clone() as DataAssociation);
                    }
                }

                boundaryEvents.Clear();
                foreach (BoundaryEvent @event in val.BoundaryEvents)
                {
                    boundaryEvents.Add(@event);
                }
            }
        }
    }

}