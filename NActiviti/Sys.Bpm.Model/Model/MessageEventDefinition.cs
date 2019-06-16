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
    public class MessageEventDefinition : EventDefinition
    {

        protected internal string messageRef;
        protected internal string messageExpression;

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


        public virtual string MessageExpression
        {
            get
            {
                return messageExpression;
            }
            set
            {
                this.messageExpression = value;
            }
        }


        public override BaseElement Clone()
        {
            MessageEventDefinition clone = new MessageEventDefinition
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
                var val = value as MessageEventDefinition;
                MessageRef = val.MessageRef;
                MessageExpression = val.MessageExpression;
            }
        }
    }

}