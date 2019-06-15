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
using System.Text.RegularExpressions;

namespace org.activiti.bpmn.model
{
    public class MultiInstanceLoopCharacteristics : BaseElement
    {

        protected internal string inputDataItem;
        protected internal string loopCardinality;
        protected internal string completionCondition;
        protected internal string elementVariable;
        protected internal string elementIndexVariable;
        protected internal bool sequential;

        public virtual string InputDataItem
        {
            get
            {
                return inputDataItem;
            }
            set
            {
                this.inputDataItem = value;
            }
        }


        public virtual string LoopCardinality
        {
            get
            {
                return loopCardinality;
            }
            set
            {
                this.loopCardinality = value;
            }
        }


        public virtual string CompletionCondition
        {
            get
            {
                return completionCondition;
            }
            set
            {
                this.completionCondition = value;
            }
        }


        public virtual string ElementVariable
        {
            get
            {
                return elementVariable;
            }
            set
            {
                this.elementVariable = value;
            }
        }


        public virtual string ElementIndexVariable
        {
            get
            {
                return elementIndexVariable;
            }
            set
            {
                this.elementIndexVariable = value;
            }
        }


        public virtual bool Sequential
        {
            get
            {
                return sequential;
            }
            set
            {
                this.sequential = value;
            }
        }

        public string GetCollectionVarName()
        {
            var regex = new Regex("\\$\\{(.*?)\\}");

            if (regex.IsMatch(this.inputDataItem))
            {
                return regex.Match(this.inputDataItem).Groups[1].Value;
            }

            return this.inputDataItem;
        }


        public override BaseElement Clone()
        {
            MultiInstanceLoopCharacteristics clone = new MultiInstanceLoopCharacteristics
            {
                Values = this
            };
            return clone;
        }

        public override BaseElement Values
        {
            set
            {
                var val = value as MultiInstanceLoopCharacteristics;

                InputDataItem = val.InputDataItem;
                LoopCardinality = val.LoopCardinality;
                CompletionCondition = val.CompletionCondition;
                ElementVariable = val.ElementVariable;
                ElementIndexVariable = val.ElementIndexVariable;
                Sequential = val.Sequential;
            }
        }
    }

}