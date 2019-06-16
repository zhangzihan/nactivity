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
    public class SignalEventDefinition : EventDefinition
    {

        protected internal string signalRef;
        protected internal string signalExpression;
        protected internal bool async;

        public virtual string SignalRef
        {
            get
            {
                return signalRef;
            }
            set
            {
                this.signalRef = value;
            }
        }


        public virtual string SignalExpression
        {
            get
            {
                return signalExpression;
            }
            set
            {
                this.signalExpression = value;
            }
        }


        public virtual bool Async
        {
            get
            {
                return async;
            }
            set
            {
                this.async = value;
            }
        }


        public override BaseElement Clone()
        {
            SignalEventDefinition clone = new SignalEventDefinition
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
                var val = value as SignalEventDefinition;

                SignalRef = val.SignalRef;
                SignalExpression = val.SignalExpression;
                Async = val.Async;
            }
        }
    }

}