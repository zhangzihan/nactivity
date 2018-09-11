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
    public abstract class FlowNode : FlowElement
    {

        protected internal bool asynchronous;
        protected internal bool notExclusive;

        protected internal IList<SequenceFlow> incomingFlows = new List<SequenceFlow>();
        protected internal IList<SequenceFlow> outgoingFlows = new List<SequenceFlow>();

        [JsonIgnore]
        protected internal object behavior;

        public FlowNode()
        {

        }

        public virtual bool Asynchronous
        {
            get
            {
                return asynchronous;
            }
            set
            {
                this.asynchronous = value;
            }
        }


        public virtual bool Exclusive
        {
            get
            {
                return !notExclusive;
            }
            set
            {
                this.notExclusive = !value;
            }
        }


        public virtual bool NotExclusive
        {
            get
            {
                return notExclusive;
            }
            set
            {
                this.notExclusive = value;
            }
        }

        [JsonIgnore]
        public virtual object Behavior
        {
            get
            {
                return behavior;
            }
            set
            {
                this.behavior = value;
            }
        }


        public virtual IList<SequenceFlow> IncomingFlows
        {
            get
            {
                return incomingFlows;
            }
            set
            {
                this.incomingFlows = value;
            }
        }


        public virtual IList<SequenceFlow> OutgoingFlows
        {
            get
            {
                return outgoingFlows;
            }
            set
            {
                this.outgoingFlows = value;
            }
        }


        public override BaseElement Values
        {
            set
            {
                base.Values = value;
                var val = value as FlowNode;

                Asynchronous = val.Asynchronous;
                NotExclusive = val.NotExclusive;
            }
        }
    }

}