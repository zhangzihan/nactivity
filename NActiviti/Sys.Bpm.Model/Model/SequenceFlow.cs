using Newtonsoft.Json;
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
    public class SequenceFlow : FlowElement
    {
        protected internal string conditionExpression;
        protected internal string sourceRef;
        protected internal string targetRef;
        protected internal string skipExpression;

        // Actual flow elements that match the source and target ref
        // Set during process definition parsing
        [JsonIgnore]
        protected internal FlowElement sourceFlowElement;

        [JsonIgnore]
        protected internal FlowElement targetFlowElement;

        /// <summary>
        /// Graphical information: a list of waypoints: x1, y1, x2, y2, x3, y3, ..
        /// 
        /// Added during parsing of a process definition.
        /// </summary>
        protected internal IList<int> waypoints = new List<int>();

        public SequenceFlow()
        {

        }

        public SequenceFlow(string sourceRef, string targetRef)
        {
            this.sourceRef = sourceRef;
            this.targetRef = targetRef;
        }

        public virtual string ConditionExpression
        {
            get
            {
                return conditionExpression;
            }
            set
            {
                this.conditionExpression = value;
            }
        }


        public virtual string SourceRef
        {
            get
            {
                return sourceRef;
            }
            set
            {
                this.sourceRef = value;
            }
        }


        public virtual string TargetRef
        {
            get
            {
                return targetRef;
            }
            set
            {
                this.targetRef = value;
            }
        }


        public virtual string SkipExpression
        {
            get
            {
                return skipExpression;
            }
            set
            {
                this.skipExpression = value;
            }
        }

        [JsonIgnore]
        public virtual FlowElement SourceFlowElement
        {
            get
            {
                return sourceFlowElement;
            }
            set
            {
                this.sourceFlowElement = value;
            }
        }

        [JsonIgnore]
        public virtual FlowElement TargetFlowElement
        {
            get
            {
                return targetFlowElement;
            }
            set
            {
                this.targetFlowElement = value;
            }
        }


        public virtual IList<int> Waypoints
        {
            get
            {
                return waypoints;
            }
            set
            {
                this.waypoints = value;
            }
        }


        public override string ToString()
        {
            return sourceRef + " --> " + targetRef;
        }

        public override BaseElement Clone()
        {
            SequenceFlow clone = new SequenceFlow
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
                var val = value as SequenceFlow;
                ConditionExpression = val.ConditionExpression;
                SourceRef = val.SourceRef;
                TargetRef = val.TargetRef;
                SkipExpression = val.SkipExpression;
            }
        }
    }

}