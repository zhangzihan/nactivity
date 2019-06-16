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
    public class MessageFlow : BaseElement
    {

        protected internal string name;
        protected internal string sourceRef;
        protected internal string targetRef;
        protected internal string messageRef;

        public MessageFlow()
        {

        }

        public MessageFlow(string sourceRef, string targetRef)
        {
            this.sourceRef = sourceRef;
            this.targetRef = targetRef;
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


        public virtual string MessageRef
        {
            get
            {
                return messageRef;
            }
            set
            {
                this.messageRef = value;
            }
        }


        public override string ToString()
        {
            return sourceRef + " --> " + targetRef;
        }

        public override BaseElement Clone()
        {
            MessageFlow clone = new MessageFlow
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
                var val = value as MessageFlow;
                Name = val.Name;
                SourceRef = val.SourceRef;
                TargetRef = val.TargetRef;
                MessageRef = val.MessageRef;
            }
        }
    }

}